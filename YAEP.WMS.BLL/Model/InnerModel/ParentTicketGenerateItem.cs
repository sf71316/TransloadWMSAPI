using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ParentTicketGenerateItem
    {
        public ParentTicketGenerateItem()
        {
            this.WorkOrderPodUID = new List<Guid>();
            this.WorkOrderPayloadUID = new List<Guid>();
        }
        internal ITicketModel Ticket { get; set; }
        internal List<Guid> WorkOrderPodUID { get; set; }
        internal List<Guid> WorkOrderPayloadUID { get; set; }
    }
}
