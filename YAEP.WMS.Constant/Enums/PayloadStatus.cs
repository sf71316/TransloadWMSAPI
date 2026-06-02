using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities.Attributes;

namespace YAEP.WMS.Constant.Enums
{
    public enum PayloadStatus
    {
        [EnumFieldInfo(Sort = 1)]
        WaitingForProcessing = 100,
        [EnumFieldInfo(Sort = 2)]
        Processing = 200,
        [EnumFieldInfo(Sort = 3)]
        InPosition = 300,
        [EnumFieldInfo(Sort = 4)]
        OffPosition = 400,
        [EnumFieldInfo(Sort = 5)]
        Active = 500,
        Inactive = 0
    }
}
