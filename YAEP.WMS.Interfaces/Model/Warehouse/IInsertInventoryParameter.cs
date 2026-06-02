using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IInsertInventoryParameter
    {
        Guid WarehouseUID { get; set; }
        Guid ItemUID { get; set; }
        Guid TargetPackageUID { get; set; }
        Guid SlotUID { get; set; }
        InventoryType Type { get; set; }
        int Qty { get; set; }
        bool UseMiniPackage { get; set; }
    }
}
