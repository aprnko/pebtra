using System;
using System.Collections.Generic;

namespace pebtra.DAL
{
    public partial class Transaction
    {
        public Transaction()
        {
            InverseParent = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public bool IsActive { get; set; }
        public required string AccountId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? CategoryId { get; set; }
        public string? Details { get; set; }
        public string? Comment { get; set; }
        public Guid? GroupId { get; set; }
        public int? ParentId { get; set; }
        public string? ExtraDetails { get; set; }
        public string? CurrencyDetails { get; set; }
        public int? ImportSessionId { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Category? Category { get; set; }
        public virtual Transaction? Parent { get; set; }
        public virtual ImportSession? ImportSession { get; set; }
        public virtual ICollection<Transaction> InverseParent { get; set; }
    }
} 