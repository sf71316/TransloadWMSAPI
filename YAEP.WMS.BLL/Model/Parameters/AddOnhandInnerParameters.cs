using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    public class AddOnhandInnerParameters : IAddOnhandParameters
    {
        public Guid WarehouseUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid TargetPackageUID { get; set; }
        public Guid SlotUID { get; set; }
        public InventoryType Type { get; set; }
        public PayloadType PayloadType { get; set; }
        public int Onhand { get; set; }
        public bool IsAddPod { get; set; }
        public string PayloadDescription { get; set; }
        public bool isPauseSync { get; set; }
        public string PodBarcode { get; set; }
    }
}
