using Microsoft.Extensions.Logging;
using Pebtra.DAL.Repositories;
using Pebtra.DAL;
using Pebtra.Core.Dto;

namespace Pebtra.Core;

public class StatementImportService(ITransactionRepository transactionRepository, StatementFileReaderFactory statementFileReaderFactory, 
    ILogger logger, ILoggerFactory loggerFactory)
{
    private readonly StatementFormatProvider _formatProvider = new StatementFormatProvider();    

    public void ExtractText(string filePath)
    {
        var reader = statementFileReaderFactory.GetInstance(filePath);
        var lines = reader.Read(filePath);
        logger.LogInformation(string.Join("\n", lines));
    }

    public async Task ImportAsync(string filename)
    {
        logger.LogInformation("Importing file: {FilePath}", filename);

        bool isCommited = false;
        using var dbContextTransaction = await transactionRepository.BeginTransactionAsync();
        try
        {
            var reader = statementFileReaderFactory.GetInstance(filename);
            var lines = reader.Read(filename);

            var accounts = await transactionRepository.GetAccountsWithStatementFormatsAsync();

            var matchingAccounts = accounts.Where(a => lines.Any(l => l.Contains(a.UniqueStatementString ?? string.Empty))).ToList();

            if (matchingAccounts.Count == 0)
            {
                logger.LogError($"No matching account found for the statement file {filename}");
                return;
            }

            if (matchingAccounts.Count > 1)
            {
                var matchingAccountIdsString = String.Join(", ", matchingAccounts.Select(a => a.Id));
                logger.LogError($"Multiple accounts found matching the statement file: {matchingAccountIdsString}");
                throw new InvalidOperationException("Statement file matches multiple accounts. Please ensure UniqueStatementString values are unique.");
            }

            var account = matchingAccounts.First();

            logger.LogInformation("Detected account: {AccountId}", account.Id);

            if (!_formatProvider.GetAvailableFormats().Contains(account.FormatName))
            {
                logger.LogError("Format '{FormatName}' not found. Available formats: {AvailableFormats}",
                    account.FormatName, string.Join(", ", _formatProvider.GetAvailableFormats()));
                return;
            }

            var format = _formatProvider.Get(account.FormatName!);
            if (format == null)
            {
                logger.LogError("Format '{FormatName}' not found. Available formats: {AvailableFormats}",
                    account.FormatName, string.Join(", ", _formatProvider.GetAvailableFormats()));
                return;
            }
            var parser = new StatementParser { Format = format, Logger = loggerFactory.CreateLogger(String.Empty) };

            logger.LogInformation("-------------------------------- Parsing the statement file --------------------------------");

            var importedTransactions = parser.Parse(lines);

            logger.LogInformation("-------------------------------- Importing transactions --------------------------------");

            var transactionsToAdd = importedTransactions.Select(t => new Transaction()
            {
                Date = t.Date.ToDateTime(TimeOnly.MinValue),
                Amount = t.Amount,
                Details = t.Details,
                ExtraDetails = t.ExtraDetails,
                CurrencyDetails = t.CurrencyDetails,
                AccountId = account.Id,
                IsActive = true
            });

            var classifiedTransactions = await transactionRepository.ClassifyByExistenceAsync(transactionsToAdd);

            if (classifiedTransactions.Existing.Any())
            {
                logger.LogWarning("The following transactions already exist:\n{Transactions}",
                    String.Join("\n", classifiedTransactions.Existing.Select(t => $"\t\t{t.Date} -- {t.Amount} -- {t.Details}")));

                if (format.DuplicateTransactionBehavior == DuplicateTransactionBehavior.AbortOnDuplicate)
                {
                    logger.LogError("Some transactions already exist. The {Format} format does not support skipping existing transactions.", account.FormatName);
                    return;
                }
            }

            if (classifiedTransactions.New.Any())
            {
                await transactionRepository.AddRangeAsync(classifiedTransactions.New);
                await dbContextTransaction.CommitAsync();
                isCommited = true;
                logger.LogInformation("Successfully imported {Count} new transactions.", classifiedTransactions.New.ToList().Count);
                if (classifiedTransactions.Existing.Any())
                {
                    logger.LogInformation("Skipped {Count} transactions that already exist.", classifiedTransactions.Existing.ToList().Count);
                }
            }
            else
            {
                logger.LogInformation("No new transactions to import.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error importing transactions: {Message}", ex.Message);
            throw;
        }
        finally 
        {
            if (!isCommited) 
            {
                logger.LogInformation("No transactions are imported");                
            }
        }
    }
} 