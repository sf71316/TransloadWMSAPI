using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class InventoryViewModel : IInventoryViewModel
    {
        public string WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string TypeName { get; set; }
        public string UOM { get; set; }
        public int InboundQty { get; set; }
        public int OutboundQty { get; set; }
        public int Onhand { get; set; }
        public string PackageTree { get; set; }
        public Guid UID { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid PackageUID { get; set; }
        public Guid ItemUID { get; set; }
        public int Type { get; set; }
        public int Qty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int Status { get; set; }
    }
}
