using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace pebtra.core;

public class XlsFileReader : IStatementFileReader
{
    private readonly StatementFormatProvider _formatProvider;

    public XlsFileReader()
    {
        _formatProvider = new StatementFormatProvider();
    }

    public IEnumerable<string> Read(string filePath)
    {
        var lines = new List<string>();
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        IWorkbook workbook;
        
        // Create the appropriate workbook based on file extension
        if (extension == ".xls")
        {
            workbook = new HSSFWorkbook(fs); // For .xls files
        }
        else if (extension == ".xlsx")
        {
            workbook = new XSSFWorkbook(fs); // For .xlsx files
        }
        else
        {
            throw new NotSupportedException($"The file format {extension} is not supported.");
        }
        
        var sheet = workbook.GetSheetAt(0);

        foreach (IRow row in sheet)
        {
            if (row != null && row.Cells != null && row.Cells.Count > 0)
            {
                lines.Add(String.Join('|', row.Cells.Select(x => x?.ToString()?.Replace("\r\n", " ") ?? "").ToList()));
            }
        }
        
        return lines;
    }
} 