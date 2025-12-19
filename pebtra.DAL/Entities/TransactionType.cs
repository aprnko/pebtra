using System;
using System.Collections.Generic;

namespace Pebtra.DAL
{
    public partial class TransactionType
    {
        public TransactionType()
        {
            Categories = new HashSet<Category>();
        }

        public int Id { get; set; }
        public required string Name { get; set; }

        public virtual ICollection<Category> Categories { get; set; }
    }
} 