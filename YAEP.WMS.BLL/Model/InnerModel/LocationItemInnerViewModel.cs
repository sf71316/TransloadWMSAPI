using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class LocationItemInnerViewModel : ILocationItemViewModel
    {
        public LocationItemInnerViewModel()
        {

        }
        public LocationItemInnerViewModel(ILocationItemViewModel model)
        {
            this.AllocatedQty = model.AllocatedQty;
            this.AreaName = model.AreaName;
            this.BinName = model.BinName;
            this.BolRefNo = model.BolRefNo;
            this.CreatedBy = model.CreatedBy;
            this.CreatedOn = model.CreatedOn;
            this.Description = model.Description;
            this.ID = model.ID;
            this.ItemID = model.ItemID;
            this.ItemUID = model.ItemUID;
            this.ModifiedBy = model.ModifiedBy;
            this.ModifiedOn = model.ModifiedOn;
            this.Name = model.Name;
            this.OriginalPackageName = model.OriginalPackageName;
            this.OriginalPackageUID = model.OriginalPackageUID;
            this.PackageName = model.PackageName;
            this.PackageQty = model.PackageQty;
            this.PackageUID = model.PackageUID;
            this.PayloadUID = model.PayloadUID;
            this.PodName = model.PodName;
            this.PodUID = model.PodUID;
            this.Quantity = model.Quantity;
            this.SlotName = model.SlotName;
            this.SlotId = model.SlotId;
            this.SlotUID = model.SlotUID;
            this.Status = model.Status;
            this.Type = model.Type;
            this.UID = model.UID;
            this.VesselRefNo = model.VesselRefNo;
            this.VesselUID = model.VesselUID;
            this.VolumeLimit = model.VolumeLimit;
            this.WarehouseName = model.WarehouseName;
            this.WeightLimit = model.WeightLimit;
            this.Sequence = model.Sequence;
            this.Labels = model.Labels;
            this.PackageSerialNumber = model.PackageSerialNumber;
            this.OriginalPayloadUID = model.OriginalPayloadUID;
        }
        public string WarehouseName { get; set; }
        public string AreaName { get; set; }
        public string BinName { get; set; }
        public string SlotName { get; set; }
        public Guid UID { get; set; }
        public Guid PayloadUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid PodUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid VesselUID { get; set; }
        public Guid ItemUID { get; set; }
        public int Quantity { get; set; }
        public Guid OriginalPackageUID { get; set; }
        public string OriginalPackageName { get; set; }
        public int PackageQty { get; set; }
        public int AllocatedQty { get; set; }
        public string PackageName { get; set; }
        public Guid PackageUID { get; set; }
        public string ItemID { get; set; }
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
        public Guid VesselManifestUID { get; set; }
        public int Sequence { get; set; }
        public IEnumerable<ITicketLabelViewModel> Labels { get; set; }
        public long PackageSerialNumber { get; set; }
        public string SlotId { get; set; }
        public Guid? OriginalPayloadUID { get; set; }
    }
}
