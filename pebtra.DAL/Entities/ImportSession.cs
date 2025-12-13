using System;
using System.Collections.Generic;

namespace pebtra.DAL
{
    public partial class ImportSession
    {
        public ImportSession()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public DateTime ImportDateTime { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Filename { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;

        public virtual Account? Account { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
} 