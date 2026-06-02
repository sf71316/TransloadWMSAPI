using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderPayloadModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int Type { get; set; }
        Guid VesselManifestUID { get; set; }
        Guid WorkOrderUID { get; set; }
        Guid WorkOrderPodUID { get; set; }
        Guid PayloadUID { get; set; }
        Guid PayloadPackageUID { get; set; }
        Guid? ItemGroupUID { get; set; }
        Guid ItemUID { get; set; }
        Guid? SlotUID { get; set; }
        Guid? TargetSlotUID { get; set; }
        Guid PackageUID { get; set; }
        Guid LoadingZoneSlotUID { get; set; }
        Guid? SeparateByUID { get; set; }
        int Qty { get; set; }
        int Status { get; set; }
        decimal? Volume { get; set; }
        decimal? Weight { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }

    }
}
