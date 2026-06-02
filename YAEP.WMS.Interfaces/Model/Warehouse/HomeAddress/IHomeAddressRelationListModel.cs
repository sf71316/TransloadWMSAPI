using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IHomeAddressRelationListModel
    {
        Guid UID { get; set; }
        string WarehouseName { get; set; }
        string AreaName { get; set; }
        Guid AreaUID { get; set; }
        string BinName { get; set; }
        Guid BinUID { get; set; }
        Guid SlotUID { get; set; }
        string SlotName { get; set; }
        int Type { get; set; }
        string TypeName { get; set; }
        int? OutboundType { get; set; }
        string OutboundTypeName { get; set; }
        int Priority { get; set; }
    }
}
