using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pebtra.Core;

public class StatementFormatProvider
{
    private readonly Dictionary<string, StatementFormat> _formats;

    public StatementFormatProvider() => _formats = new Dictionary<string, StatementFormat>
        {
            {
                "Kaspi", new StatementFormat
                {
                    DateFormat = "dd.MM.yy",
                    NumberFormat = "N2",
                    NumberCulture = "ru-RU",
                    IsReverseOrder = true,
                    DoFilterDates = false,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            Pattern = @"^(\d{2}\.\d{2}\.\d{2})\s+([+-]\s?\d[\d\s]*,\d{2})\s*\u20B8\s+(\w+)\s+(.+)",
                            FieldMappers =
                            [
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },

                                (transaction, value, format) => {
                                    transaction.Amount = decimal.Parse(value.RemoveSpaces(), NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                },

                                (transaction, value, format) => {
                                    transaction.ExtraDetails = value;
                                },

                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                },

                                (transaction, value, format) => { }
                            ]
                        },
                        new StatementLineFormat
                        {
                            Pattern = @"^\s*\(([+-]\s?\d[\d\s]*\,\d{2}\s*[A-Z]{3})\)\s*$",
                            FieldMappers =
                            [
                                (transaction, value, format) => {
                                    transaction.CurrencyDetails = value.Trim();
                                }
                            ]
                        }
                    ]
                }
            },
            {
                "KaspiDeposit", new StatementFormat
                {
                    DateFormat = "dd.MM.yy",
                    NumberFormat = "N2",
                    NumberCulture = "ru-RU",
                    IsReverseOrder = false,
                    DoFilterDates = false,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            // First line pattern: captures date, amount, type, details, and ignores balance at the end
                            // 1. Date (dd.MM.yy)
                            // 2. Amount with sign and currency symbol
                            // 3. Transaction type
                            // 4. Details (before the balance)
                            // 5. Balance (ignored)
                            Pattern = @"^(\d{2}\.\d{2}\.\d{2})\s+([+-]\s?\d[\d\s]*,\d{2})\s*\u20B8\s+(\w+)\s+(.+?)\s+(\d[\d\s]*,\d{2}\s*\u20B8)$",
                            FieldMappers =
                            [ 
                                // Date mapper
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },
                                
                                // Amount mapper
                                (transaction, value, format) => {
                                    transaction.Amount = decimal.Parse(value.RemoveSpaces(), NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                },
                                
                                // ExtraDetails mapper (transaction type)
                                (transaction, value, format) => {
                                    transaction.ExtraDetails = value;
                                },
                                
                                // Details mapper
                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                },
                                
                                // Ignore balance
                                (transaction, value, format) => { }
                            ]
                        },
                        // Second line pattern for additional details
                        new StatementLineFormat
                        {
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [ 
                                // Append to Details
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        },
                        // Third line pattern for any remaining content
                        new StatementLineFormat
                        {
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [ 
                                // Append to Details (same as second line)
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        }
                    ]
                }
            },
            {
                "Bereke", new StatementFormat
                {
                    DateFormat = "dd.MM.yyyy",
                    NumberFormat = "N2",
                    NumberCulture = "ru-RU",
                    IsReverseOrder = false,
                    DoFilterDates = true,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            Pattern = @"^(\d{2}\.\d{2}\.\d{4})\s+(.+?)\s+([-+]?\s?\d[\d\s]*,\d{2}\s+[A-Z]{3})(?:\s+\*+\s+\d+)?$",
                            FieldMappers =
                            [
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },

                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                },

                                (transaction, value, format) => {
                                    transaction.CurrencyDetails = value.Trim();
                                }
                            ]
                        },
                        new StatementLineFormat
                        {
                            Pattern = @"^([-+]?\s?\d[\d\s]*,\d{2})$",
                            FieldMappers =
                            [
                                (transaction, value, format) => {
                                    transaction.Amount = decimal.Parse(value.RemoveSpaces(), NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                }
                            ]
                        }
                    ]
                }
            },
            {
                "HomeCredit", new StatementFormat
                {
                    DateFormat = "dd.MM.yyyy",
                    NumberFormat = "N2",
                    NumberCulture = "ru-RU",
                    IsReverseOrder = true,
                    DoFilterDates = false,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            // First line pattern: captures date, details, and amount
                            // 1. Date (dd.mm.yyyy)
                            // 2. Details (merchant name)
                            // 3. Amount (with sign and currency symbol)
                            Pattern = @"^(\d{2}\.\d{2}\.\d{4})\s+(.+?)\s+([-+]\s?\d[\d\s]*,\d{2}\s+\u20B8)(?:\s+\+\s?\s?\d[\d\s]*,\d{2}\s+\u0411)?$",
                            FieldMappers =
                            [ 
                                // Date mapper
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },
                                
                                // Details mapper (merchant name)
                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                },
                                
                                // Amount mapper
                                (transaction, value, format) => {
                                    // Parse the amount: remove currency symbol, remove spaces, parse with culture
                                    transaction.Amount = decimal.Parse(
                                        value.RemoveLastGroup().RemoveSpaces(),
                                        NumberStyles.Any,
                                        new CultureInfo(format.NumberCulture)
                                    );
                                }
                            ]
                        },
                        // Second line (merchant category code)
                        new StatementLineFormat
                        {
                            Pattern = @"^(\d+)$",
                            FieldMappers = [
                                (transaction, value, format) => {
                                    transaction.ExtraDetails = value;
                                }
                            ]
                        },
                        // Third line (time): skip completely
                        new StatementLineFormat
                        {
                            Pattern = @"^\d{2}:\d{2}$",
                            FieldMappers = []
                        },
                        // Fourth line: ExtraDetails (category information)
                        new StatementLineFormat
                        {
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [
                                (transaction, value, format) => {
                                    transaction.ExtraDetails = $"{value} (MCC: {transaction.ExtraDetails})";
                                }
                            ]
                        }
                    ]
                }
            },
            {
                "Sber", new StatementFormat
                {
                    DateFormat = "dd.MM.yyyy",
                    NumberFormat = "N2",
                    NumberCulture = "ru-RU",
                    IsReverseOrder = true,
                    DoFilterDates = false,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            // First line of transaction in Sber format
                            // Example: 24.02.2025 07:27 138292 Перевод с карты 20 000,00 21 575,98
                            Pattern = @"^(\d{2}\.\d{2}\.\d{4})\s+(\d{2}:\d{2})\s+\d+\s+([^0-9]+)\s+(\+?\d[\d\s]*,\d{2})\s+(\d[\d\s]*,\d{2})$",
                            FieldMappers =
                            [
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },
                                
                                // Ignore Time mapper
                                (transaction, value, format) => { },
                                
                                // Ignore Operation Type
                                (transaction, value, format) => { },
                                
                                // Amount mapper
                                (transaction, value, format) => {
                                    // Parse amount and apply sign in one expression (+ sign means positive, no sign means negative)
                                    transaction.Amount = (value.Contains('+') ? 1 : -1) * decimal.Parse(
                                        value.RemoveSymbolOccurrences("+").Trim().RemoveSpaces(),
                                        NumberStyles.Any,
                                        new CultureInfo(format.NumberCulture)
                                    );
                                },
                                
                                // Ignore Balance
                                (transaction, value, format) => { }
                            ]
                        },
                        new StatementLineFormat
                        {
                            // Second line of a Sber transaction (date followed by details)
                            // Example: 24.02.2025 SBOL перевод на платежный счет **3527 К.
                            Pattern = @"^\d{2}\.\d{2}\.\d{4}\s+(.+)$",
                            FieldMappers =
                            [ 
                                // Start accumulating Details
                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                }
                            ]
                        },
                        new StatementLineFormat
                        {
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [ 
                                // Append to Details
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        },
                        new StatementLineFormat
                        {
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [ 
                                // Append to Details
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        }
                    ]
                }
            },
            {
                "Tinkoff", new StatementFormat
                {
                    DateFormat = "dd.MM.yyyy",
                    NumberFormat = "N2",
                    NumberCulture = "en-US", // Use en-US for dot decimal separator
                    IsReverseOrder = true,
                    DoFilterDates = false,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            // First line pattern: Contains date, amount, and details
                            // Match both dates at the beginning, both amounts, text, and 4-digit card number at the end
                            Pattern = @"^(\d{2}\.\d{2}\.\d{4})\s+\d{2}\.\d{2}\.\d{4}\s+([-+][\d\s]+\.\d{2}\s+.)\s+([-+][\d\s]+\.\d{2}\s+.)\s+(.+?)\s+(\d{4})$",
                            FieldMappers =
                            [ 
                                // Date mapper
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },
                                
                                // Currency Details mapper
                                (transaction, value, format) => {
                                    transaction.CurrencyDetails = value.Trim();
                                },
                                
                                // Amount mapper
                                (transaction, value, format) => {
                                    // Parse the amount: remove currency symbol, remove spaces, parse with culture
                                    transaction.Amount = decimal.Parse(value.RemoveLastGroup().RemoveSpaces(), NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                },
                                
                                // Details mapper
                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                },
                                
                                // Ignore card number
                                (transaction, value, format) => { }
                            ]
                        },
                        new StatementLineFormat
                        {
                            // Second line pattern for time and details
                            // Extract everything after the two time values
                            Pattern = @"^(\d{2}:\d{2})\s+(\d{2}:\d{2})\s+(.+)$",
                            FieldMappers =
                            [ 
                                // Ignore time1
                                (transaction, value, format) => { },
                                
                                // Ignore time2
                                (transaction, value, format) => { },
                                
                                // Append to Details
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        },
                        new StatementLineFormat
                        {
                            // Third+ line pattern - any text
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [ 
                                // Append to Details
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        },
                        new StatementLineFormat
                        {
                            // Fourth line pattern - any text
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [ 
                                // Append to Details
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        },
                        new StatementLineFormat
                        {
                            // Fifth line pattern - any text
                            Pattern = @"^(.+)$",
                            FieldMappers =
                            [ 
                                // Append to Details
                                (transaction, value, format) => {
                                    transaction.Details = transaction.Details.AppendString(value);
                                }
                            ]
                        }
                    ]
                }
            },
            {
                "HomeCreditDeposit", new StatementFormat
                {
                    DateFormat = "dd.MM.yyyy",
                    NumberFormat = "N2",
                    NumberCulture = "en-US", // Use en-US for dot decimal separator
                    IsReverseOrder = false,
                    DoFilterDates = false,
                    LineFormats = new List<StatementLineFormat>
                    {
                        new StatementLineFormat
                        {
                            // Pattern matches: Date, Details, Expense, Income, MonthlyInterest, Balance
                            // Example: 01.03.2025 Капитализация по вкладу 0 1,562.58 43508.57 4055875.14
                            //Pattern = @"^(\d{2}\.\d{2}\.\d{4})\s+(.+?)\s+(\d+(?:,\d+)*(?:\.\d+)?)\s+(\d+(?:,\d+)*(?:\.\d+)?)\s+\d+(?:\.\d+)?\s+\d+(?:\.\d+)?$",
                            Pattern = @"^(\d{2}\.\d{2}\.\d{4}) (?:(.+?) )?(\d+(?:,\d+)*(?:\.\d+)?) (\d+(?:,\d+)*(?:\.\d+)?) \d+(?:\.\d+)? \d+(?:\.\d+)?$",

                            FieldMappers = new FieldMapper[]
                            { 
                                // Date mapper
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },
                                
                                // Details mapper
                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                },
                                
                                // Expense amount mapper (always negative)
                                (transaction, value, format) => {
                                    decimal expense = decimal.Parse(value, NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                    if (expense != 0)
                                    {
                                        transaction.Amount = -Math.Abs(expense);
                                    }
                                },
                                
                                // Income amount mapper (always positive)
                                (transaction, value, format) => {
                                    decimal income = decimal.Parse(value, NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                    if (income != 0)
                                    {
                                        transaction.Amount = income;
                                    }
                                }
                            }
                        }
                    }
                }
            },
            {
                "BerekeDeposit", new StatementFormat
                {
                    DateFormat = "dd.MM.yyyy",
                    NumberFormat = "N2",
                    NumberCulture = "ru-RU",
                    IsReverseOrder = false,
                    DoFilterDates = false,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            // First line pattern: Date, Details, Operation type, Amount
                            // Example: 12.04.2024 Перенос начисленных процентов на счет до Зачисление 406,72
                            Pattern = @"^(\d{2}\.\d{2}\.\d{4})\s+(.+?)\s+(\w+)\s+([-]?\d[\d\s]*,\d{2})$",
                            FieldMappers =
                            [ 
                                // Date mapper
                                (transaction, value, format) => {
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(value, format.DateFormat, CultureInfo.InvariantCulture));
                                },
                                
                                // Details mapper
                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                },
                                
                                // Operation type mapper - store in ExtraDetails
                                (transaction, value, format) => {
                                    transaction.ExtraDetails = value;
                                },
                                
                                // Amount mapper
                                (transaction, value, format) => {
                                    transaction.Amount = decimal.Parse(value.RemoveSpaces(), NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                }
                            ]
                        },
                        // Lines 2-8: Just append the whole line to details
                        // Line 2
                        new() {
                            Pattern = @"^(.+)$",
                            FieldMappers = [ (transaction, value, format) => { transaction.Details = transaction.Details.AppendString(value);} ]
                        },
                        // Line 3
                        new() {
                            Pattern = @"^(.+)$",
                            FieldMappers = [ (transaction, value, format) => { transaction.Details = transaction.Details.AppendString(value);} ]
                        },
                        // Line 4
                        new() {
                            Pattern = @"^(.+)$",
                            FieldMappers = [ (transaction, value, format) => { transaction.Details = transaction.Details.AppendString(value);} ]
                        },
                        // Line 5
                        new() {
                            Pattern = @"^(.+)$",
                            FieldMappers = [ (transaction, value, format) => { transaction.Details = transaction.Details.AppendString(value);} ]
                        },
                        // Line 6
                        new() {
                            Pattern = @"^(.+)$",
                            FieldMappers = [ (transaction, value, format) => { transaction.Details = transaction.Details.AppendString(value);} ]
                        },
                        // Line 7
                        new() {
                            Pattern = @"^(.+)$",
                            FieldMappers = [ (transaction, value, format) => { transaction.Details = transaction.Details.AppendString(value);} ]
                        },
                        // Line 8
                        new() {
                            Pattern = @"^(.+)$",
                            FieldMappers = [ (transaction, value, format) => { transaction.Details = transaction.Details.AppendString(value);} ]
                        }
                    ]
                }
            },
            {
                "BCC", new StatementFormat
                {
                    DateFormat = "dd.MM.yyyy",
                    NumberFormat = "F2",
                    NumberCulture = "ru-RU", // comma separator
                    IsReverseOrder = false,
                    DoFilterDates = false,
                    LineFormats =
                    [
                        new StatementLineFormat
                        {
                            // Pattern to match pipe-separated cells
                            // Format: cell1|cell2(date)|cell3|cell4|cell5|cell6|cell7|cell8(expense)|cell9(income)|cell10|cell11|cell12(details)|cell13|cell14
                            Pattern = @"^[^|]*\|([^|]+)\|[^|]*\|[^|]*\|[^|]*\|[^|]*\|[^|]*\|([^|]*)\|([^|]*)\|[^|]*\|[^|]*\|(.*)\|[^|]*\|[^|]*$",
                            FieldMappers =
                            [ 
                                // Date mapper (cell 2) - extract only date part from "dd.mm.yyyy hh:mm:ss"
                                (transaction, value, format) => {
                                    string datePart = value.Split(' ')[0]; // Take only the date part
                                    transaction.Date = DateOnly.FromDateTime(DateTime.ParseExact(datePart, format.DateFormat, CultureInfo.InvariantCulture));
                                },
                                
                                // Expense amount mapper (cell 8)
                                (transaction, value, format) => {
                                    if (!string.IsNullOrWhiteSpace(value))
                                    {
                                        decimal expense = decimal.Parse(value, NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                        if (expense != 0)
                                        {
                                            transaction.Amount = -expense; // Negative amount for expenses
                                        }
                                    }
                                },
                                
                                // Income amount mapper (cell 9)
                                (transaction, value, format) => {
                                    if (!string.IsNullOrWhiteSpace(value) && transaction.Amount == 0)
                                    {
                                        decimal income = decimal.Parse(value, NumberStyles.Any, new CultureInfo(format.NumberCulture));
                                        if (income != 0)
                                        {
                                            transaction.Amount = income; // Positive amount for income
                                        }
                                    }
                                },
                                
                                // Details mapper (cell 12)
                                (transaction, value, format) => {
                                    transaction.Details = value.Trim();
                                }
                            ]
                        }
                    ]
                }
            }
        };

    public StatementFormat Get(string formatName)
    {
        if (string.IsNullOrEmpty(formatName) || !_formats.ContainsKey(formatName))
        {
            // Default to Kaspi if format not found
            formatName = "Kaspi";
        }

        return _formats[formatName];
    }

    public IEnumerable<string> GetAvailableFormats()
    {
        return _formats.Keys;
    }
} 