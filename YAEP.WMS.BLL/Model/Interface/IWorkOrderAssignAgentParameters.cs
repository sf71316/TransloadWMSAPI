using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal interface IWorkOrderAssignAgentParameters
    {
        IItemManager ItemManager { get; set; }
        IPackageManager PackageManager { get; set; }
        //ITransacationScope TransacationScope { get; set; }
        ISequenceAgent SequenceAgent { get; set; }
        IAuthenticationInfo AuthenticationInfo { get; set; }
        IWarehouseManger warehouseManger { get; set; }
        IWorkOrderRepository WorkOrderRepository { get; set; }
        IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        IBulkPickWorkOrdrPayloadRelationRepository BulkPickWorkOrdrPayloadRelationRepository { get; set; }
        IPackageUomManager PackageUomManager { get; set; }
        IInventoryManager InventoryManager { get; set; }
        IWorkOrderManager WorkOrderManager { get; set; }
        IVesselManifestRepository VesselManifestRepository { get; set; }
        IVesselRepository VesselRepository { get; set; }
        ILabelManager LabelManager { get; set; }
        ITracingAgent TracingAgent { get; set; }
        PackageCacheManager PackageCacheManager { get; set; }
        ProductCacheManager ProductCacheManager { get; set; }
    }
}
