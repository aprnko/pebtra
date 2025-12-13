using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace pebtra.DAL.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly FinContext _context;

        public TransactionRepository(FinContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Transaction transaction)
        {
            return await _context.Transactions
                .AnyAsync(t => 
                    t.Date == transaction.Date &&
                    t.Amount == transaction.Amount &&
                    t.Details == transaction.Details &&
                    t.AccountId == transaction.AccountId);
        }

        public async Task<Account?> GetAccountAsync(string accountId)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<IEnumerable<Account>> GetAccountsWithUniqueStatementStringAsync()
        {
            return await _context.Accounts
                .Where(a => a.UniqueStatementString != null)
                .ToListAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
} 