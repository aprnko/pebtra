using System;
using System.Collections.Generic;

namespace pebtra.DAL
{
    public partial class Currency
    {
        public Currency()
        {
            Accounts = new HashSet<Account>();
            CurrencyRateBaseCurrencies = new HashSet<CurrencyRate>();
            CurrencyRateQuoteCurrencies = new HashSet<CurrencyRate>();
        }

        public required string Id { get; set; }
        public bool IsBase { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<CurrencyRate> CurrencyRateBaseCurrencies { get; set; }
        public virtual ICollection<CurrencyRate> CurrencyRateQuoteCurrencies { get; set; }
    }
} 