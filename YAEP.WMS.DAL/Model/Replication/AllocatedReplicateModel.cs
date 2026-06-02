using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    public class AllocatedReplicateModel : IAllocatedReplicateModel
    {
        public int LocationID { get; set; }
        public int? WarehouseID { get; set; }
        public Guid UID { get; set; }
        public int Quantity { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemName { get; set; }
        public Guid PackageUID { get; set; }
        public string OriginalSlotName { get; set; }
        public string LandingZoneSlotName { get; set; }
        public string ExternalOrderNo { get; set; }
        public int TicketInfoStatus { get; set; }
        public int TicketInfoType { get; set; }
        public Guid WorkOrderPayloadUID { get; set; }
        public int PickQuantity { get; set; }
        public Guid BOLUID { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public Guid PartyUID { get; set; }
        public string CurrentSlotName { get; set; }
        public int? OriginalPayloadType { get; set; }
        public int PayloadType { get; set; }
    }
}
