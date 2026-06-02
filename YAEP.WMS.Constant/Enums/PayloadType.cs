using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum PayloadType
    {
        Stock = 1,
        Allocated = 2,
        FutureAllocated = 3,
        BulkPickPending = 4,
        Salvage = 5,
        Shrinkage = 6,
        AsIs = 7,
        Sample = 8,
        Sakana = 9,
        TemporaryOnhand = 10
    }
}