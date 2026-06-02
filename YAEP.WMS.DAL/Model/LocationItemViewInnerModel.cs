using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    public class LocationItemViewInnerModel : ILocationItemViewModel
    {
        public string WarehouseName { get; set; }
        public string AreaName { get; set; }
        public string BinName { get; set; }
        public string SlotName { get; set; }
        public string SlotId { get; set; }
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid PodUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid VesselUID { get; set; }
        public Guid ItemUID { get; set; }
        public int Quantity { get; set; }
        public Guid PackageUID { get; set; }
        public decimal VolumeLimit { get; set; }
        public decimal WeightLimit { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string VesselRefNo { get; set; }
        public string BolRefNo { get; set; }
        public string PodName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int PackageQty { get; set; }
        public string ItemID { get; set; }
        public string PackageName { get; set; }
        public Guid OriginalPackageUID { get; set; }
        public string OriginalPackageName { get; set; }
        public Guid PayloadUID { get; set; }
        public int AllocatedQty { get; set; }
        public Guid VesselManifestUID { get; set; }
        public int Sequence { get; set; }
        public IEnumerable<ITicketLabelViewModel> Labels { get; set; }
        public long PackageSerialNumber { get; set; }
        public Guid? OriginalPayloadUID { get; set; }
    }
}
