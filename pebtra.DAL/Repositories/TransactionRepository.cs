using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ardalis.GuardClauses;

namespace Pebtra.DAL.Repositories
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

        public async Task<(IEnumerable<Transaction> New, IEnumerable<Transaction> Existing)> ClassifyByExistenceAsync(
            IEnumerable<Transaction> incomingTransactions)
        {
            // Optimization: performing the in-memory check to avoid
            // 1) separate queries for each incoming transaction or
            // 2) a huge query with an unlimited number of OR clauses           
            var incomingTransactionDates = incomingTransactions.Select(x => x.Date);
            var accountId = incomingTransactions.Select(x => x.AccountId).Distinct().Single();

            // Load the candidates into memory, then filter in-memory.
            var candidates = await _context.Transactions.Where(t =>
                t.Date >= incomingTransactionDates.Min()
                && t.Date <= incomingTransactionDates.Max()
                && t.AccountId == accountId
            ).AsNoTracking().ToListAsync();

            Predicate<Transaction> exists = (t) => candidates.Any(x =>
                t.Date == x.Date &&
                t.Amount == x.Amount &&
                t.Details == x.Details &&
                t.AccountId == x.AccountId);

            return (incomingTransactions.Where(t => !exists(t)), incomingTransactions.Where(t => exists(t)));
        }

        public async Task<Account?> GetAccountAsync(string accountId) =>
            await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);

        public async Task<IEnumerable<Account>> GetAccountsWithStatementFormatsAsync() =>
            await _context.Accounts.Where(a => a.UniqueStatementString != null).ToListAsync();        

        public async Task<IDbContextTransaction> BeginTransactionAsync() =>
            await _context.Database.BeginTransactionAsync();        
    }
} 