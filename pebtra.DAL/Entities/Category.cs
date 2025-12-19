using System;
using System.Collections.Generic;

namespace Pebtra.DAL
{
    public partial class Category
    {
        public Category()
        {
            Children = new HashSet<Category>();
            Transactions = new HashSet<Transaction>();
        }

        public required string Id { get; set; }
        public string? Name { get; set; }
        public int? TransactionTypeId { get; set; }
        public string? ParentId { get; set; }

        public virtual Category? Parent { get; set; }
        public virtual TransactionType? TransactionType { get; set; }
        public virtual ICollection<Category> Children { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
} 