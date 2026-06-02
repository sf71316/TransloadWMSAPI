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
    internal class WorkOrderAssignAgentParameters : IWorkOrderAssignAgentParameters
    {
        //TODO 尚未移除Item/Package Manager 相依性
        public ISequenceAgent SequenceAgent { get; set; }
        public IAuthenticationInfo AuthenticationInfo { get; set; }
        public IWarehouseManger warehouseManger { get; set; }
        public IWorkOrderRepository WorkOrderRepository { get; set; }
        public IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        public IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        public IItemManager ItemManager { get; set; }
        public IPackageManager PackageManager { get; set; }
        public IInventoryManager InventoryManager { get; set; }
        public IWorkOrderManager WorkOrderManager { get; set; }
        public IVesselManifestRepository VesselManifestRepository { get; set; }
        public IVesselRepository VesselRepository { get; set; }
        public ILabelManager LabelManager { get; set; }
        public IPackageUomManager PackageUomManager { get; set; }
        //public ITransacationScope TransacationScope { get; set; }
        public PackageCacheManager PackageCacheManager { get; set; }
        public ProductCacheManager ProductCacheManager { get; set; }
        public IBulkPickWorkOrdrPayloadRelationRepository BulkPickWorkOrdrPayloadRelationRepository { get; set; }
        public ITracingAgent TracingAgent { get; set; }
    }
}
