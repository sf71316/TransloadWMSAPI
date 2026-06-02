using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IAddOnhandParameters
    {
        Guid WarehouseUID { get; set; }
        Guid ItemUID { get; set; }
        Guid TargetPackageUID { get; set; }
        Guid SlotUID { get; set; }
        InventoryType Type { get; set; }
        PayloadType PayloadType { get; set; }
        int Onhand { get; set; }
        bool IsAddPod { get; set; }
        string PayloadDescription { get; set; }
        string PodBarcode { get; set; }
        bool isPauseSync { get; set; }
    }
}
