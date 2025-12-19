using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Extensions.Logging;
using pebtra.core.Dto;

namespace pebtra.core;

public class StatementParser
{
    public required StatementFormat Format { get; set; }
    public ILogger? Logger { get; set; }

    public bool IsReverseOrder
    {
        get => Format.IsReverseOrder;
        set => Format.IsReverseOrder = value;
    }

    public IEnumerable<ImportedTransaction> Parse(IEnumerable<string> lines)
    {
        var transactions = new List<ImportedTransaction>();
        ImportedTransaction? currentTransaction = null;
        var currentLineFormatIndex = 0;

        foreach (var line in lines)
        {
            // First check if this is a first line of a new transaction
            var format = Format.LineFormats.First();
            var match = Regex.Match(line, format.Pattern);
            
            if (match.Success)
            {
                Logger?.LogInformation(">>>  {Line}", line);
                currentLineFormatIndex = 0;
                
                // Save the previous transaction if it exists
                if (currentTransaction != null)
                {
                    transactions.Add(currentTransaction);
                }
                
                // Create a new transaction
                currentTransaction = new ImportedTransaction();
                ApplyFieldMappings(currentTransaction, format, match);
            }
            // then check if this is an n-th line of an active transaction
            else if (currentTransaction != null && ++currentLineFormatIndex < Format.LineFormats.Count)
            {
                format = Format.LineFormats.ElementAt(currentLineFormatIndex);
                match = Regex.Match(line, format.Pattern);
                if (match.Success)
                {
                    Logger?.LogInformation("-->  {Line}", line);
                    ApplyFieldMappings(currentTransaction, format, match);
                }
                else
                {
                    // No more transaction lines, save the current transaction
                    Logger?.LogInformation("x    {Line}", line);
                    transactions.Add(currentTransaction);
                    currentTransaction = null;
                    currentLineFormatIndex = 0;
                }
            }
            else 
            {
                Logger?.LogInformation(".    {Line}", line);
            }
            // If no match, and there is no active transaction, just skip this line.
        }

        // Save the last transaction if it exists
        if (currentTransaction != null)
        {
            transactions.Add(currentTransaction);
        }

        // If the statement is in reverse order (oldest first), reverse the list
        // to make the transactions appear in chronological order (newest first)
        if (Format.IsReverseOrder)
        {
            transactions.Reverse();
        }

        return transactions;
    }
    
    private void ApplyFieldMappings(ImportedTransaction transaction, StatementLineFormat format, Match match)
    {
        // Set fields based on field mappers
        for (int j = 0; j < format.FieldMappers.Length; j++)
        {
            var mapper = format.FieldMappers[j];
            var groupValue = match.Groups[j + 1].Value; // Groups start at 1
            
            // Apply the mapping function
            mapper(transaction, groupValue, Format);
        }
    }
} 