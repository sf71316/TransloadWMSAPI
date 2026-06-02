using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public class WorkOrderPayloadInnerModel : IWorkOrderPayloadModel
    {
        public WorkOrderPayloadInnerModel()
        {

        }
        public WorkOrderPayloadInnerModel(IWorkOrderPayloadModel oldModel)
        {
            this.CreatedBy = oldModel.CreatedBy;
            this.CreatedOn = oldModel.CreatedOn;
            this.ID = oldModel.ID;
            this.ItemUID = oldModel.ItemUID;
            this.LoadingZoneSlotUID = oldModel.LoadingZoneSlotUID;
            this.ModifiedBy = oldModel.ModifiedBy;
            this.ModifiedOn = oldModel.ModifiedOn;
            this.Name = oldModel.Name;
            this.PackageUID = oldModel.PackageUID;
            this.PayloadPackageUID = oldModel.PayloadPackageUID;
            this.PayloadUID = oldModel.PayloadUID;
            this.Qty = oldModel.Qty;
            this.SlotUID = oldModel.SlotUID;
            this.Status = oldModel.Status;
            this.TargetSlotUID = oldModel.TargetSlotUID;
            this.Type = oldModel.Type;
            this.UID = oldModel.UID;
            this.VesselManifestUID = oldModel.VesselManifestUID;
            this.Volume = oldModel.Volume;
            this.Weight = oldModel.Weight;
            this.WorkOrderPodUID = oldModel.WorkOrderPodUID;
            this.WorkOrderUID = oldModel.WorkOrderUID;
            this.SeparateByUID = oldModel.SeparateByUID;
            this.ItemGroupUID = this.ItemGroupUID;
           
        }
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid VesselManifestUID { get; set; }
        public Guid WorkOrderUID { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public Guid PayloadUID { get; set; }
        public Guid AllocatedPayloadUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid? SlotUID { get; set; }
        public Guid? TargetSlotUID { get; set; }
        public Guid PackageUID { get; set; }
        public Guid LoadingZoneSlotUID { get; set; }
        public int Qty { get; set; }
        public int Status { get; set; }
        public decimal? Volume { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public decimal? Weight { get; set; }
        public Guid PayloadPackageUID { get; set; }
        public Guid? SeparateByUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
       
    }
}
