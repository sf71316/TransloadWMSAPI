using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class ReceivingPlannerParameters
    {
        public ITransacationScope TransactionScope { get; set; }
        public IReceivingRequest ReceivingRequest { get; set; }
        public Guid WarehouseUID { get; set; }
        public IEnumerable<IVesselManifestModel> VesselManifests { get; set; }
        public Dictionary<string, IEnumerable<Guid>> LabelMapping { get; set; }
    }
}
