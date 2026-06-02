using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class StatusCheckInnerModel : IStatusCheckModel
    {
        public Guid TicketInfoUID { get; set; }
        public int TicketInfoStatus { get; set; }
        public Guid TicketUID { get; set; }
        public int TicketStatus { get; set; }
        public Guid WorkOrderUID { get; set; }
        public int WorkOrderStatus { get; set; }
        public Guid VesselUID { get; set; }
        public int VesselStatus { get; set; }
        public Guid BolUID { get; set; }
        public int BolStatus { get; set; }
        public Guid ManifestUID { get; set; }
        public int ManifestStatus { get; set; }
        public Guid WorkOrderPayloadUID { get; set; }
        public int WorkOrderPayloadStatus { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public int WorkOrderPodStatus { get; set; }
    }
}
