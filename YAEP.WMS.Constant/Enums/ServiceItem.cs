using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities.Attributes;

namespace YAEP.WMS.Constant.Enums
{
    public enum TicketType
    {
        [EnumFieldInfo(Sort = 1)]
        Receiving = 100,
        [EnumFieldInfo(Sort = 2)]
        Outbound = 200,
        [EnumFieldInfo(Sort = 3)]
        Staging = 210,
        [EnumFieldInfo(Sort = 4)]
        BulkPick = 301,
        [EnumFieldInfo(Sort = 5)]
        Move = 300,
        [EnumFieldInfo(Sort = 6, Text = "Inventory Counting")]
        InventoryCounting = 400,
    }
    public enum ServiceProcessItem
    {
        Receiving = 100,
        Outbound = 200,
        InboundMove = 300,
        OutboundMove = 301,
        WarehouseMove = 302,
        BulkPick = 303,
        InventoryCounting = 400,
    }
    public enum TicketInfoType
    {
        Receiving = 100,
        Outbound = 200,
        Move = 300,
        BulkPick = 301,
        MoveSummary = 310,
        InventoryCounting = 400,
    }
}
