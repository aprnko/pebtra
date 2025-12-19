using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pebtra.core.Dto
{
    public enum DuplicateTransactionBehavior
    {
        AbortOnDuplicate,
        SkipDuplicates
    }
}
