using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IStatusCheckModel
    {
        Guid TicketInfoUID { get; set; }
        int TicketInfoStatus { get; set; }
        Guid TicketUID { get; set; }
        int TicketStatus { get; set; }
        Guid WorkOrderUID { get; set; }
        int WorkOrderStatus { get; set; }
        Guid WorkOrderPayloadUID { get; set; }
        int WorkOrderPayloadStatus { get; set; }
        Guid WorkOrderPodUID { get; set; }
        int WorkOrderPodStatus { get; set; }
        Guid VesselUID { get; set; }
        int VesselStatus { get; set; }
        Guid BolUID { get; set; }
        int BolStatus { get; set; }
        Guid ManifestUID { get; set; }
        int ManifestStatus { get; set; }
    }
}
