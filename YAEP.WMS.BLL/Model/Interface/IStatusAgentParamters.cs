using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal interface IStatusManageAgentParamters
    {
        ILabelManager LabelManager { get; set; }
        IManifestRepository ManifestRepository { get; set; }
        IManifestItemListRepository ManifestItemRepository { get; set; }
        IBolRepository BolRepository { get; set; }
        IVesselRepository VesselRepository { get; set; }
        IVesselManifestRepository VesselManifestRepository { get; set; }
        IWorkOrderRepository WorkOrderRepository { get; set; }
        IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        ITicketRepository TicketRepository { get; set; }
        ITicketInfoRepository TicketInfoRepository { get; set; }
        ITicketManager TicketManager { get; set; }
        IBolManager BolManager { get; set; }
        IInventoryManager InventoryManager { get; set; }
        ITracingAgent TracingAgent { get; set; }
        IAuthenticationProvider AuthProvider { get; set; }

    }
}
