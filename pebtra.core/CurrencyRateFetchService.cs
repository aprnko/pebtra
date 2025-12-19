using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using pebtra.DAL;
using pebtra.core.Utils;
using pebtra.core.Dto;

namespace pebtra.core;

public class CurrencyRateFetchService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly FinContext _dbContext;

    public CurrencyRateFetchService(ILogger logger, IConfiguration configuration, HttpClient httpClient, FinContext dbContext)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _dbContext = dbContext;
    }

    public async Task Fetch()
    {
        var baseCurrency = await _dbContext.Currencies.Where(c => c.IsBase).Select(c => c.Id)
            .SingleOrDefaultAsync() ?? throw new InvalidOperationException("Base currency not found");

        var targetCurrencies = await _dbContext.Currencies.Where(c => !c.IsBase).Select(c => c.Id)
            .ToArrayAsync();

        if (!targetCurrencies.Any())
        {
            throw new InvalidOperationException("No quote currencies found");
        }

        var perCurrencyMaxDates = await _dbContext.CurrencyRates.GroupBy(r => r.QuoteCurrencyId).Select(g => g.Max(r => r.Date))
            .ToListAsync();

        DateOnly startDate = perCurrencyMaxDates.Any()
            ? DateOnly.FromDateTime(perCurrencyMaxDates.Min())
            : DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        if (startDate == endDate)
        {
            _logger.LogInformation("No new currency rates to fetch");
            return;
        }

        _logger.LogInformation(
            "Fetching rates with baseCurrency={Base}, targets={Targets}, startDate={Start}, endDate={End}",
            baseCurrency,
            string.Join(",", targetCurrencies),
            startDate.Formatted(),
            endDate.Formatted());

        var ratesToSave = await GetCurrencyRates(startDate, endDate, baseCurrency, targetCurrencies).ToListAsync();

        if (ratesToSave.Any())
        {
            var dates = ratesToSave.Select(r => r.Date.Date).Distinct().ToList();
            var existingRates = await _dbContext.CurrencyRates
                .Where(r => dates.Contains(r.Date.Date))
                .Select(r => new { r.Date.Date, r.BaseCurrencyId, r.QuoteCurrencyId })
                .ToListAsync();

            var existingKeys = existingRates
                .Select(r => (r.Date, r.BaseCurrencyId, r.QuoteCurrencyId)).ToHashSet();

            var newRates = ratesToSave
                .Where(rate => !existingKeys.Contains((rate.Date.Date, rate.BaseCurrency, rate.QuoteCurrency)))
                .Select(rate => new CurrencyRate
                {
                    Date = rate.Date,
                    BaseCurrencyId = rate.BaseCurrency,
                    QuoteCurrencyId = rate.QuoteCurrency,
                    Rate = rate.Rate
                }).ToList();

            if (newRates.Any())
            {
                await _dbContext.CurrencyRates.AddRangeAsync(newRates);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Saved {Count} new currency rates to database", newRates.Count);
            }
            else
            {
                _logger.LogInformation("All {Count} currency rates already exist in database", ratesToSave.Count);
            }
        }        
    }

    public async IAsyncEnumerable<CurrencyRateDto> GetCurrencyRates(DateOnly startDate, DateOnly endDate, string baseCurrency, string[] targetCurrencies)
    {
        _logger.LogInformation("Fetching currency rates...");

        var apiKey = _configuration.GetSection("CurrencyRate")["ApiKey"];

        var targetCurrenciesString = string.Join(",", targetCurrencies);
        var startDateString = startDate.ToString("yyyy-MM-dd");
        var endDateString = endDate.ToString("yyyy-MM-dd");

        var url =            
            $"https://api.exchangerate.host/timeframe?start_date={startDateString}&end_date={endDateString}&source={baseCurrency}&currencies={targetCurrenciesString}&access_key={apiKey}";

        CurrencyRateResponseDto? result;
        try
        {
            _logger.LogInformation("Requesting URL: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Currency rates raw JSON: {Json}", jsonContent);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            result = JsonSerializer.Deserialize<CurrencyRateResponseDto>(jsonContent, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching currency rates: {Message}", ex.Message);
            throw;
        }
        
        if (result == null)
        {
            _logger.LogError("Failed to deserialize API response");
            yield break;
        }

        if (!result.Success && result.Error != null)
        {
            _logger.LogError("API Error - Code: {Code}, Type: {Type}, Info: {Info}", 
                result.Error.Code, result.Error.Type, result.Error.Info);
            yield break;
        }

        if (!result.Success || result.Quotes == null)
        {
            yield break;
        }

        var count = 0;

        foreach (var dateQuote in result.Quotes)
        {
            if (!DateTime.TryParse(dateQuote.Key, out var date))
            {
                _logger.LogWarning("Failed to parse date: {Date}", dateQuote.Key);
                continue;
            }

            foreach (var quote in dateQuote.Value)
            {
                // Parse currency pair: assume 6 characters total, 3 for each currency
                if (quote.Key.Length != 6)
                {
                    throw new InvalidOperationException($"Unexpected currency pair length: expected 6 characters, got {quote.Key.Length} for pair '{quote.Key}'");
                }

                var baseCurrencyFromPair = quote.Key.Substring(0, 3);
                var quoteCurrencyFromPair = quote.Key.Substring(3, 3);

                if (baseCurrencyFromPair != baseCurrency)
                {
                    throw new InvalidOperationException($"Base currency mismatch: expected '{baseCurrency}', got '{baseCurrencyFromPair}' from pair '{quote.Key}'");
                }

                if (!targetCurrencies.Contains(quoteCurrencyFromPair))
                {
                    throw new InvalidOperationException($"Quote currency '{quoteCurrencyFromPair}' from pair '{quote.Key}' is not in the target currencies list: {targetCurrenciesString}");
                }

                count++;
                yield return new CurrencyRateDto
                {
                    Date = date,
                    BaseCurrency = baseCurrencyFromPair,
                    QuoteCurrency = quoteCurrencyFromPair,
                    Rate = 1/quote.Value
                };
            }
        }

        var formattedStartDate = result.StartDate != null && DateTime.TryParse(result.StartDate, out var parsedStartDate)
            ? parsedStartDate.FormattedDate()
            : result.StartDate ?? "";
        var formattedEndDate = result.EndDate != null && DateTime.TryParse(result.EndDate, out var parsedEndDate)
            ? parsedEndDate.FormattedDate()
            : result.EndDate ?? "";

        _logger.LogInformation(
            "Successfully fetched {Count} currency rates from {StartDate} to {EndDate} for source {Source} and currencies {Currencies}", 
            count, formattedStartDate, formattedEndDate, result.Source, targetCurrenciesString);
    }
}

