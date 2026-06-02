using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class OutboundAutoAssignedParameters : AbstractAutoAssignedParameters
    {
        public IAllocatedRequest OutboundRequest { get; set; }
        public bool ForceWorkOrderOpen { get; set; }
        public bool PassPackageVersion { get; set; }
        public bool IsChinaWarehouse { get; set; }
        public IActionResult<ISlotModel> LandingSlot { get; set; }
        public IEnumerable<Func<IActionResult<bool>>> ManifestGenerateFuncs { get; set; }
        public Func<List<IActionResult<bool>>> TicketGenerateFuncs { get; set; }
    }
    internal class InboundAutoAssignedParameters : AbstractAutoAssignedParameters
    {
        public PackageCacheManager PackageCacheManager { get; set; }
        public Guid WarehouseUID { get; set; }
        public IReceivingRequest ReceivingRequest { get; set; }
        public bool ForceWorkOrderOpen { get; set; }
        public Dictionary<string, IEnumerable<Guid>> LabelMapping { get; set; }
    }
    internal abstract class AbstractAutoAssignedParameters
    {
        public ILogInfiltrator LogInfiltrator { get; set; }
        public IManifestModel Manifest { get; set; }
        public IEnumerable<IManifestItemListModel> ManifestItems { get; set; }
        public IEnumerable<IBolModel> Bol { get; set; }
        public IEnumerable<IVesselModel> Vessel { get; set; }
        public IEnumerable<IVesselManifestModel> VesselItems { get; set; }
    }
}
