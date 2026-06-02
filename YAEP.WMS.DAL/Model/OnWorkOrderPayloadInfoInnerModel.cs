using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class OnWorkOrderPayloadInfoInnerModel : IWorkOrderPayloadInfoModel
    {
        public DateTime? BOL_ETA_D { get; set; }
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid VesselManifestUID { get; set; }
        public Guid WorkOrderUID { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public Guid PayloadUID { get; set; }
        public Guid PayloadPackageUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid? SlotUID { get; set; }
        public Guid? TargetSlotUID { get; set; }
        public Guid PackageUID { get; set; }
        public Guid LoadingZoneSlotUID { get; set; }
        public Guid? SeparateByUID { get; set; }
        public int Qty { get; set; }
        public int Status { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Weight { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
    }
}