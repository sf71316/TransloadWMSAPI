using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractServiceItemProcessModule
    {
        public AbstractServiceItemProcessModule(ServiceItemProcessModuleParameters parameters)
        {
            this.SequenceAgent = parameters.SequenceAgent;
            this.PackageManager = parameters.PackageManager;
            this.PackageUomManager = parameters.PackageUomManager;
            this.InstructionBuilder = parameters.InstructionBuilder;
            this.AuthenticationProvider = parameters.AuthenticationProvider;
        }
        public abstract Guid ServiceItemID { get; }
        protected ISequenceAgent SequenceAgent { get; set; }
        protected PackageCacheManager PackageManager { get; set; }
        protected IPackageUomManager PackageUomManager { get; set; }
        protected IInstructionBuilder InstructionBuilder { get; set; }
        protected IAuthenticationProvider AuthenticationProvider { get; set; }

        public abstract Tuple<IEnumerable<ITicketModel>, IEnumerable<ITicketInfoModel>, IEnumerable<ITicketRelationModel>>
            Execute(IEnumerable<ITicketGeneratoreDataModel> data, List<ParentTicketGenerateItem> parentTicket,
            ManifestType Type,Guid warehouseUID,bool ForceOpen=false);
        public static AbstractServiceItemProcessModule GetSubModule(ServiceProcessItem serviceItem, ServiceItemProcessModuleParameters parameters)
        {
            parameters.InstructionBuilder = GetIntructionBuilder(serviceItem);

            switch (serviceItem)
            {
                //case ServiceProcessItem.OutboundMoveManifest:
                //    return new OutboundMoveManifestServiceItemProcessModule(parameters);
                case ServiceProcessItem.Receiving:
                    return new InboundServiceItemProcessModule(parameters);
                case ServiceProcessItem.Outbound:
                    return new OutboundServiceItemProcessModule(parameters);
                case ServiceProcessItem.InboundMove:
                    return new InboundMoveServiceItemProcessModule(parameters);
                case ServiceProcessItem.OutboundMove:
                    return new OutboundMoveServiceItemProcessModule(parameters);
                case ServiceProcessItem.WarehouseMove:
                    return new WarehouseMoveServiceItemProcessModule(parameters);
                case ServiceProcessItem.BulkPick:
                    return new BulkPickServiceItemProcessModule(parameters);
                case ServiceProcessItem.InventoryCounting:
                    return new InventoryCountingServiceItemProcessModule(parameters);
            }
            return null;
        }

        public static IInstructionBuilder GetIntructionBuilder(ServiceProcessItem serviceItem)
        {
            switch (serviceItem)
            {
                case ServiceProcessItem.Receiving:
                    return new InboundInstructionModule();
                case ServiceProcessItem.Outbound:
                    return new OutboundInstructionModule();
                case ServiceProcessItem.InboundMove:
                    return new InboundMoveInstructionModule();
                case ServiceProcessItem.OutboundMove:
                    return new OutboundMoveInstructionModule();
                case ServiceProcessItem.WarehouseMove:
                    return new WarehouseMoveInstructionModule();
                case ServiceProcessItem.InventoryCounting:
                    return new InventoryCountingInstructionModule();
                default: break;
            }
            return null;
        }
    }
}
