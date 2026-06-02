using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class DeallocatedInfoDataInnerModel : IDeallocatedInfoDataModel
    {
        public Guid WorkOrderUID { get; set; }
        public Guid VesselUID { get; set; }
        public Guid? WorkOrderPayloadUID { get; set; }
        public Guid? WorkOrderPodUID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public Guid TicketUID { get; set; }
    }
}
