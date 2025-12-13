namespace Pebtra.Util;

public class ImportedTransaction
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Details { get; set; } = String.Empty;
    public string? ExtraDetails { get; set; }
    public string? CurrencyDetails { get; set; } 
} 