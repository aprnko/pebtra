using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pebtra.DAL;
using Microsoft.EntityFrameworkCore.Storage;

namespace pebtra.DAL.Repositories
{
    public interface ITransactionRepository
    {
        Task AddRangeAsync(IEnumerable<Transaction> transactions);
        Task<bool> ExistsAsync(Transaction transaction);
        Task<Account?> GetAccountAsync(string accountId);
        Task<IEnumerable<Account>> GetAccountsWithUniqueStatementStringAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
} 