using System;

namespace pebtra.core.Dto;

// Define a delegate type for the field mapping function
public delegate void FieldMapper(ImportedTransaction transaction, string value, StatementFormat format);

public class StatementLineFormat
{
    public required string Pattern { get; set; }
    public required FieldMapper[] FieldMappers { get; set; }
} 