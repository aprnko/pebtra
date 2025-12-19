using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pebtra.core
{
    public class StatementFileReaderFactory
    {
        public IStatementFileReader GetInstance(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => new PdfFileReader(),
                ".xls" or ".xlsx" => new XlsFileReader(),
                _ => throw new NotSupportedException($"File format {extension} is not supported.")
            };
        }

    }
}
