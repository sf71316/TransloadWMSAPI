using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class TicketExtraProcessParameters
    {
        public ConcurrentQueue<Func<IActionResult<bool>>> CompleteUnexecutedMethod { get; set; }
        public ITicketManager TicketManager { get; set; }
        public IInventoryManager InventoryManager { get; set; }
        public ITicketRepository TicketRepository { get; set; }
        public ITicketInfoRepository TicketInfoRepository { get; set; }
        public ITicketInfoAssigneeRelationRepository TicketInfoAssigneeRelationRepository { get; set; }
        public IWorkOrderManager WorkOrderManager { get; set; }
        public IAppConfigure AppConfigure { get; set; }
        public IWorkOrderRepository WorkOrderRepository { get; set; }
        public IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        public IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        public IBulkPickManager BulkPickManager { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }
        public ReplicationManager ReplicationManager { get; set; }
    }
}
