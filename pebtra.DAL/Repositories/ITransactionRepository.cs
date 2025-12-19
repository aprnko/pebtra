using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pebtra.DAL;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pebtra.DAL.Repositories
{
    public interface ITransactionRepository
    {
        Task AddRangeAsync(IEnumerable<Transaction> transactions);
        Task<(IEnumerable<Transaction> New, IEnumerable<Transaction> Existing)> ClassifyByExistenceAsync(
            IEnumerable<Transaction> incomingTransactions);
        Task<Account?> GetAccountAsync(string accountId);
        Task<IEnumerable<Account>> GetAccountsWithStatementFormatsAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
} 