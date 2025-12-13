using System;

namespace pebtra.DAL
{
    public partial class CurrencyRate
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public required string BaseCurrencyId { get; set; }
        public required string QuoteCurrencyId { get; set; }
        public decimal Rate { get; set; }

        public virtual Currency? BaseCurrency { get; set; }
        public virtual Currency? QuoteCurrency { get; set; }
    }
} 