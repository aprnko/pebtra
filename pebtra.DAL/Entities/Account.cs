using System;
using System.Collections.Generic;

namespace pebtra.DAL
{
    public partial class Account
    {
        public Account()
        {
            Transactions = new HashSet<Transaction>();
        }

        public required string Id { get; set; }
        public string? Name { get; set; }
        public required string CurrencyId { get; set; }
        public bool IsTransit { get; set; }
        public bool IsSaving { get; set; }
        public string? FormatName { get; set; }
        public string? UniqueStatementString { get; set; }

        public virtual Currency? Currency { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
} 