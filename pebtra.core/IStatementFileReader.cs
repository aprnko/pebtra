using System;
using System.Collections.Generic;

namespace Pebtra.Core;

public interface IStatementFileReader
{
    IEnumerable<string> Read(string filePath);
} 