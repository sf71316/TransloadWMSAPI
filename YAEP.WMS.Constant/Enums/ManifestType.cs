using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities.Attributes;

namespace YAEP.WMS.Constant.Enums
{
    public enum ManifestType
    {
        [EnumFieldInfo(Sort = 1)]
        Inbound = 1,
        [EnumFieldInfo(Sort = 2)]
        Outbound,
        [EnumFieldInfo(Sort = 3)]
        Move,
        [EnumFieldInfo(Sort = 4, Text = "Inventory Counting")]
        InventoryCounting,
        BlukPick
    }
}
