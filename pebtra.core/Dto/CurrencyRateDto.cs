namespace Pebtra.Core.Dto;

public class CurrencyRateDto
{
    public DateTime Date { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public string QuoteCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

