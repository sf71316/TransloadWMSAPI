using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities.Attributes;

namespace YAEP.WMS.Constant.Enums
{
    public enum AllocateType
    {
        [EnumFieldInfo(Sort = 1)]
        GeneralAllocate = 100,
        [EnumFieldInfo(Sort = 2)]
        FutureAllocate = 200,    
    }
}
