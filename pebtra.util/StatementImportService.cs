using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using pebtra.DAL.Repositories;
using pebtra.DAL;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pebtra.Util;

public class StatementImportService
{
    private readonly StatementFormatProvider _formatProvider;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;

    public StatementImportService(ITransactionRepository transactionRepository, ILogger logger, ILoggerFactory loggerFactory)
    {
        _formatProvider = new StatementFormatProvider();
        _transactionRepository = transactionRepository;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    private IStatementFileReader GetReaderForFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".pdf" => new PdfFileReader(),
            ".xls" or ".xlsx" => new XlsFileReader(),
            _ => throw new NotSupportedException($"File format {extension} is not supported.")
        };
    }

    public void ExtractText(string filePath)
    {
        var reader = GetReaderForFile(filePath);
        var lines = reader.Read(filePath);
        _logger.LogInformation(string.Join("\n", lines));
    }

    public async Task ImportAsync(string filePath, bool forceImportSkipDuplicates = false)
    {
        _logger.LogInformation("Importing file: {FilePath}", filePath);

        bool isCommited = false;
        using var dbContextTransaction = await _transactionRepository.BeginTransactionAsync();        
        try
        {
            // Read the file lines first to detect the account
            var reader = GetReaderForFile(filePath);
            var lines = reader.Read(filePath);
            
            // Get all accounts with non-null UniqueStatementString
            var accountsWithUniqueString = await _transactionRepository.GetAccountsWithUniqueStatementStringAsync();
            
            string? accountId = null;
            var matchingAccounts = new List<Account>();
            
            // Check each account's UniqueStatementString against the file lines
            foreach (var accountWithString in accountsWithUniqueString)
            {
                if (accountWithString.UniqueStatementString != null && 
                    lines.Any(line => line.Contains(accountWithString.UniqueStatementString)))
                {
                    matchingAccounts.Add(accountWithString);
                }
            }
            
            // Validate that exactly one account matches
            if (matchingAccounts.Count == 0)
            {
                _logger.LogError("No account found that matches the statement file content.");
                _logger.LogInformation("Available unique statement strings:");
                foreach (var accountWithString in accountsWithUniqueString)
                {
                    _logger.LogInformation("  Account {AccountId}: {UniqueStatementString}", accountWithString.Id, accountWithString.UniqueStatementString);
                }
                return;
            }
            
            if (matchingAccounts.Count > 1)
            {
                _logger.LogError("Multiple accounts match the statement file content:");
                foreach (var accountWithString in matchingAccounts)
                {
                    _logger.LogError("  Account {AccountId}: {UniqueStatementString}", accountWithString.Id, accountWithString.UniqueStatementString);
                }
                throw new InvalidOperationException("Statement file matches multiple accounts. Please ensure UniqueStatementString values are unique.");
            }
            
            accountId = matchingAccounts[0].Id;
            var account = matchingAccounts[0];
            
            _logger.LogInformation("Detected account: {AccountId}", accountId);

            if (string.IsNullOrEmpty(account.FormatName))
            {
                _logger.LogError("No statement format found for account {AccountId}", accountId);
                return;
            }

            if (!_formatProvider.GetAvailableFormats().Contains(account.FormatName))
            {
                _logger.LogError("Format '{FormatName}' not found. Available formats: {AvailableFormats}", account.FormatName, string.Join(", ", _formatProvider.GetAvailableFormats()));
                return;
            }
            
            var format = _formatProvider.Get(account.FormatName);
            ILogger parserLogger = _loggerFactory.CreateLogger(String.Empty);            
            var parser = new StatementParser { Format = format, Logger = parserLogger };

            _logger.LogInformation("-------------------------------- Parsing the statement file --------------------------------");

            var importedTransactions = parser.Parse(lines);

            _logger.LogInformation("-------------------------------- Importing transactions --------------------------------");

            var newTransactions = new List<Transaction>();
            var alreadyExistingCount = 0;
            
            foreach (var importedTransaction in importedTransactions)
            {
                var transaction = new Transaction
                {
                    Date = importedTransaction.Date.ToDateTime(TimeOnly.MinValue),
                    Amount = importedTransaction.Amount,
                    Details = importedTransaction.Details,
                    ExtraDetails = importedTransaction.ExtraDetails,
                    CurrencyDetails = importedTransaction.CurrencyDetails,
                    AccountId = accountId,
                    IsActive = true
                };
                                
                // Check if transaction already exists
                if (!await _transactionRepository.ExistsAsync(transaction))
                {
                    newTransactions.Add(transaction);
                    var message = $"New transaction: {transaction.Date} -- {transaction.Amount} -- {transaction.Details} -- {transaction.Comment}" + 
                        (importedTransaction.CurrencyDetails != null ? $" [{importedTransaction.CurrencyDetails}]" : "");
                    _logger.LogInformation(message);
                }
                else
                {
                    alreadyExistingCount ++;
                    _logger.LogWarning("Transaction already exists: {Date} -- {Amount} -- {Details}", transaction.Date, transaction.Amount, transaction.Details);
                }
            }
            if (alreadyExistingCount > 0 && !forceImportSkipDuplicates)
            {
                _logger.LogError("Some transactions already exist. Please use the --skip-duplicates option to import the statement anyway and skip those transactions.");
                return;
            }

            if (newTransactions.Any())
            {
                await _transactionRepository.AddRangeAsync(newTransactions);
                await dbContextTransaction.CommitAsync();
                isCommited = true;
                _logger.LogInformation("Successfully imported {Count} new transactions.", newTransactions.Count);
                if (alreadyExistingCount > 0)
                {
                    _logger.LogInformation("Skipped {Count} transactions that already exist.", alreadyExistingCount);
                }
            }
            else
            {
                _logger.LogInformation("No new transactions to import.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing transactions: {Message}", ex.Message);
            throw;
        }
        finally 
        {
            if (!isCommited) 
            {
                _logger.LogInformation("No transactions are imported");                
            }
        }
    }
} 