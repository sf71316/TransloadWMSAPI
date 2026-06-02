using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IDeallocatedInfoDataModel
    {
        Guid WorkOrderUID { get; set; }
        Guid VesselUID { get; set; }
        Guid? WorkOrderPayloadUID { get; set; }
        Guid? WorkOrderPodUID { get; set; }
        Guid TicketInfoUID { get; set; }
        Guid TicketUID { get; set; }
    }
}
