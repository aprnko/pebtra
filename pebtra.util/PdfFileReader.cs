using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Pebtra.Util;

public class PdfFileReader : IStatementFileReader
{
    private readonly StatementFormatProvider _formatProvider;

    public PdfFileReader()
    {
        _formatProvider = new StatementFormatProvider();
    }

    public IEnumerable<string> Read(string filePath)
    {
        var lines = new List<string>();
        using var pdfReader = new iText.Kernel.Pdf.PdfReader(filePath);
        using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);

        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var page = pdfDocument.GetPage(i);
            var strategy = new LocationTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(page, strategy);
            lines.AddRange(text.Split('\n'));
        }

        return lines;
    }
} 