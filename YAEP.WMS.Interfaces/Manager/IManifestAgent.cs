using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IManifestAgent
    {
        IManifestManger ManifestManager { get; set; }
        IBolManager BolManager { get; set; }
        IVesselManager VesselManager { get; set; }
        IWorkOrderManager WorkOrderManager { get; set; }
        ITicketManager TicketManager { get; set; }
        IOrderManager OrderManager { get; set; }
        IBulkPickManager BulkPickManager { get; set; }
    }
}
