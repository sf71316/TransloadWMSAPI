using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAllocatedReplicateModel
    {
        Guid WorkOrderPayloadUID { get; set; }
        Guid PartyUID { get; set; }
        Guid WorkOrderPodUID { get; set; }
        int LocationID { get; set; }
        int TicketInfoStatus { get; set; }
        int TicketInfoType { get; set; }
        string ExternalOrderNo { get; set; }
        int? WarehouseID { get; set; }
        Guid UID { get; set; }
        Guid BOLUID { get; set; }
        int Quantity { get; set; }
        int PickQuantity { get; set; }
        Guid ItemUID { get; set; }
        string ItemName { get; set; }
        Guid PackageUID { get; set; }
        string OriginalSlotName { get; set; }
        string LandingZoneSlotName { get; set; }
        string CurrentSlotName { get; set; }
        int? OriginalPayloadType { get; set; }
        int PayloadType { get; set; }

    }
}
