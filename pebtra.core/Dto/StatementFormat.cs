using System.Collections.Generic;

namespace pebtra.core.Dto;

public class StatementFormat
{
    public required ICollection<StatementLineFormat> LineFormats { get; set; }
    public bool IsReverseOrder { get; set; }
    public bool DoFilterDates { get; set; }
    public string DateFormat { get; set; } = "dd.MM.yyyy";
    public string NumberFormat { get; set; } = "N2";
    public string NumberCulture { get; set; } = "ru-RU";
    public DuplicateTransactionBehavior DuplicateTransactionBehavior { get; set; } =
        DuplicateTransactionBehavior.AbortOnDuplicate;
} 