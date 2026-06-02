using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class StatusManageAgentParamters : IStatusManageAgentParamters
    {
        public IManifestRepository ManifestRepository { get; set; }
        public IManifestItemListRepository ManifestItemRepository { get; set; }
        public IBolRepository BolRepository { get; set; }
        public IVesselRepository VesselRepository { get; set; }
        public IVesselManifestRepository VesselManifestRepository { get; set; }
        public IWorkOrderRepository WorkOrderRepository { get; set; }
        public IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        public IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        public ITicketRepository TicketRepository { get; set; }
        public ITicketInfoRepository TicketInfoRepository { get; set; }
        public ITicketManager TicketManager { get; set; }
        public IBolManager BolManager { get; set; }
        public ILabelManager LabelManager { get; set; }
        public IInventoryManager InventoryManager { get; set; }
        public ITracingAgent TracingAgent { get; set; }
        public IAuthenticationProvider AuthProvider { get; set; }
    }
}
