using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pebtra.Core.Dto
{
    public enum DuplicateTransactionBehavior
    {
        AbortOnDuplicate,
        SkipDuplicates
    }
}
