using System;
using System.Collections.Generic;

namespace pebtra.core;

public interface IStatementFileReader
{
    IEnumerable<string> Read(string filePath);
} 