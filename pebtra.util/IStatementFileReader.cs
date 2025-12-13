using System;
using System.Collections.Generic;

namespace Pebtra.Util;

public interface IStatementFileReader
{
    IEnumerable<string> Read(string filePath);
} 