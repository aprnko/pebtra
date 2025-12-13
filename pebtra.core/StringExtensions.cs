namespace Pebtra.Core;

public static class StringExtensions
{
    /// <summary>
    /// Appends text to an existing string with a separator if the base string is not empty.
    /// If the base string is null or empty, the text becomes the new value.
    /// </summary>
    /// <param name="baseString">The original string to append to</param>
    /// <param name="textToAppend">The text to append</param>
    /// <param name="separator">The separator to use between the strings (default is space)</param>
    /// <returns>The combined string</returns>
    public static string AppendString(this string baseString, string textToAppend, string separator = " ")
    {
        if (string.IsNullOrEmpty(baseString))
            return textToAppend.Trim();
        else
            return baseString + separator + textToAppend.Trim();
    }
    
    /// <summary>
    /// Removes all occurrences of a specific symbol from a string.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="symbol">The symbol to remove</param>
    /// <returns>The string with all occurrences of the symbol removed</returns>
    public static string RemoveSymbolOccurrences(this string input, string symbol)
    {
        return input.Replace(symbol, "");
    }
    
    /// <summary>
    /// Removes all spaces from a string.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <returns>The string with all spaces removed</returns>
    public static string RemoveSpaces(this string input)
    {
        return input.RemoveSymbolOccurrences(" ");
    }
    
    /// <summary>
    /// Removes the last group from a string, defined as everything after the last occurrence of a separator.
    /// If the separator is not found, returns the original string.
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="separator">The separator character (default is space)</param>
    /// <returns>The string with the last group removed</returns>
    public static string RemoveLastGroup(this string input, char separator = ' ')
    {
        int lastSeparatorIndex = input.LastIndexOf(separator);
        if (lastSeparatorIndex > 0)
        {
            return input.Substring(0, lastSeparatorIndex);
        }
        return input;
    }
} 