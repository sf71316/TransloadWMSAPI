using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class InventoryDetailViewModel : IInventoryDetailViewModel
    {
        public string WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string ManifestID { get; set; }
        public string BolRef { get; set; }
        public string VesselRef { get; set; }
        public string ItemID { get; set; }
        public string Package { get; set; }

        public Guid UID { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid PackageUID { get; set; }
        public string PackageName { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemName { get; set; }
        public int Type { get; set; }
        public int Qty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int Status { get; set; }
    }
}
