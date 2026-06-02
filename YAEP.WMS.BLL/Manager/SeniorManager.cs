using System; 
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public class SeniorManager : IManifestAgent
    {
        public SeniorManager(IManifestManger manifestManager,
            IBolManager bolManager,
            IVesselManager vesselManager,
            IWorkOrderManager workorderManager,
            ITicketManager ticketManager,
            IOrderManager orderManager,
            IBulkPickManager bulkPickManager)
        {
            this.BolManager = bolManager;
            this.ManifestManager = manifestManager;
            this.VesselManager = vesselManager;
            this.WorkOrderManager = workorderManager;
            this.OrderManager = orderManager;
            this.TicketManager = ticketManager;
            this.BulkPickManager = bulkPickManager;
        }
        public IManifestManger ManifestManager { get; set; }
        public IBolManager BolManager { get; set; }
        public IVesselManager VesselManager { get; set; }
        public IWorkOrderManager WorkOrderManager { get; set; }
        public ITicketManager TicketManager { get; set; }
        public IOrderManager OrderManager { get; set; }
        public IBulkPickManager BulkPickManager { get; set; }
    }
}
