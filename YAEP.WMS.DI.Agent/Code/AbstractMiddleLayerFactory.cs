using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;
using YAEP.Core.Item.Interfaces;
using YAEP.Data.ORM.Dapper.Contrib;
using YAEP.Data.ORM.Interfaces;
using YAEP.Data.ORM.Templates;
using YAEP.Package.Interfaces;
using YAEP.Templates.DI.Unity;
using YAEP.Utilities;
using YAEP.WMS.BLL.Manager;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.DAL.Repository;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;
using YAEP.WMS.UniversalModule;

namespace YAEP.WMS.DI.Agent
{
    public abstract class AbstractMiddleLayerFactory : AbstractFactory
    {
        public AbstractMiddleLayerFactory(IUnityContainer container) : base(container)
        {

        }
        protected void CallPublicDIPool()
        {
            // 注入 查詢參數類別
            //base.RegisterType<IObjectRelationalMappingLayer, DbEntities>();
            //base.RegisterInstance<IObjectRelationalMappingLayer>(new DbEntities());
            
           


            // 注入 Model 定義 
            base.RegisterType<IModelDescriptor<ManifestModel>, GenericModelDescriptor<ManifestModel>>();
            base.RegisterType<IModelDescriptor<ManifestItemListModel>, GenericModelDescriptor<ManifestItemListModel>>();
            base.RegisterType<IModelDescriptor<WarehouseModel>, GenericModelDescriptor<WarehouseModel>>();

            base.RegisterType<IModelDescriptor<ReplicationlogModel>, GenericModelDescriptor<ReplicationlogModel>>();

            base.RegisterType<IModelDescriptor<BolModel>, GenericModelDescriptor<BolModel>>();
            base.RegisterType<IModelDescriptor<VesselModel>, GenericModelDescriptor<VesselModel>>();
            base.RegisterType<IModelDescriptor<VesselManifestModel>, GenericModelDescriptor<VesselManifestModel>>();

            base.RegisterType<IModelDescriptor<WorkOrderModel>, GenericModelDescriptor<WorkOrderModel>>();
            base.RegisterType<IModelDescriptor<WorkOrderPayloadModel>, GenericModelDescriptor<WorkOrderPayloadModel>>();
            base.RegisterType<IModelDescriptor<WorkOrderPodModel>, GenericModelDescriptor<WorkOrderPodModel>>();
            //base.RegisterType<IModelDescriptor<LabelModel>, GenericModelDescriptor<LabelModel>>();

            base.RegisterType<IModelDescriptor<InventoryModel>, GenericModelDescriptor<InventoryModel>>();
            base.RegisterType<IModelDescriptor<PodModel>, GenericModelDescriptor<PodModel>>();
            base.RegisterType<IModelDescriptor<PayloadModel>, GenericModelDescriptor<PayloadModel>>();
            base.RegisterType<IModelDescriptor<PayloadTransactionLogModel>, GenericModelDescriptor<PayloadTransactionLogModel>>();

            base.RegisterType<IModelDescriptor<WarehouseModel>, GenericModelDescriptor<WarehouseModel>>();
            base.RegisterType<IModelDescriptor<BinModel>, GenericModelDescriptor<BinModel>>();
            base.RegisterType<IModelDescriptor<AreaModel>, GenericModelDescriptor<AreaModel>>();
            base.RegisterType<IModelDescriptor<SlotModel>, GenericModelDescriptor<SlotModel>>();
            base.RegisterType<IModelDescriptor<LabelModel>, GenericModelDescriptor<LabelModel>>();
            base.RegisterType<IModelDescriptor<HomeAddressRelationModel>, GenericModelDescriptor<HomeAddressRelationModel>>();


            // 注入 Repository 處理器
            base.RegisterType<IRepositoryHandler<ManifestModel>, GenericRepositoryHandler<ManifestModel>>();
            base.RegisterType<IRepositoryHandler<ManifestItemListModel>, GenericRepositoryHandler<ManifestItemListModel>>();

            base.RegisterType<IRepositoryHandler<BolModel>, GenericRepositoryHandler<BolModel>>();
            base.RegisterType<IRepositoryHandler<VesselModel>, GenericRepositoryHandler<VesselModel>>();
            base.RegisterType<IRepositoryHandler<VesselManifestModel>, GenericRepositoryHandler<VesselManifestModel>>();

            base.RegisterType<IRepositoryHandler<WorkOrderModel>, GenericRepositoryHandler<WorkOrderModel>>();
            base.RegisterType<IRepositoryHandler<WorkOrderPayloadModel>, GenericRepositoryHandler<WorkOrderPayloadModel>>();
            base.RegisterType<IRepositoryHandler<WorkOrderPodModel>, GenericRepositoryHandler<WorkOrderPodModel>>();

            base.RegisterType<IRepositoryHandler<WarehouseModel>, GenericRepositoryHandler<WarehouseModel>>();
            //base.RegisterType<IRepositoryHandler<LabelModel>, GenericRepositoryHandler<LabelModel>>();

            base.RegisterType<IRepositoryHandler<InventoryModel>, GenericRepositoryHandler<InventoryModel>>();
            base.RegisterType<IRepositoryHandler<PodModel>, GenericRepositoryHandler<PodModel>>();
            base.RegisterType<IRepositoryHandler<PayloadModel>, GenericRepositoryHandler<PayloadModel>>();
            base.RegisterType<IRepositoryHandler<PayloadTransactionLogModel>, GenericRepositoryHandler<PayloadTransactionLogModel>>();

            base.RegisterType<IRepositoryHandler<WarehouseModel>, GenericRepositoryHandler<WarehouseModel>>();
            base.RegisterType<IRepositoryHandler<BinModel>, GenericRepositoryHandler<BinModel>>();
            base.RegisterType<IRepositoryHandler<AreaModel>, GenericRepositoryHandler<AreaModel>>();
            base.RegisterType<IRepositoryHandler<SlotModel>, GenericRepositoryHandler<SlotModel>>();
            base.RegisterType<IRepositoryHandler<LabelModel>, GenericRepositoryHandler<LabelModel>>();
            base.RegisterType<IRepositoryHandler<HomeAddressRelationModel>, GenericRepositoryHandler<HomeAddressRelationModel>>();

            base.RegisterType<IRepositoryHandler<ReplicationlogModel>, GenericRepositoryHandler<ReplicationlogModel>>();


            // 注入 Repository 實作
            base.RegisterType<IManifestRepository, ManifestRepository<ManifestModel>>();
            base.RegisterType<IManifestItemListRepository, ManifestItemListRepository<ManifestItemListModel>>();
            base.RegisterType<IPodRepository, PodRepository<PodModel>>();
            base.RegisterType<IPayloadRepository, PayloadRepository<PayloadModel>>();
            base.RegisterType<IVesselManifestRepository, VesselManifestRepository<VesselManifestModel>>();
            base.RegisterType<IVesselRepository, VesselRepository<VesselModel>>();
            base.RegisterType<IBolRepository, BolRepository<BolModel>>();
            base.RegisterType<IWorkOrderRepository, WorkOrderRepository<WorkOrderModel>>();
            base.RegisterType<IWorkOrderPodRepository, WorkOrderPodRepository<WorkOrderPodModel>>();
            base.RegisterType<IWorkOrderPayloadRepository, WorkOrderPayloadRepository<WorkOrderPayloadModel>>();
            base.RegisterType<ILabelRepository, LabelRepository<LabelModel>>();
            base.RegisterType<IWarehouseRepository, WarehouseRepository<WarehouseModel>>();
            base.RegisterType<IInventoryRepository, InventroyRepository<InventoryModel>>();
            base.RegisterType<IPayloadTransactionLogRepository, PayloadTransactionLogRepository<PayloadTransactionLogModel>>();
            base.RegisterType<IWarehouseRepository, WarehouseRepository<WarehouseModel>>();
            base.RegisterType<IAreaRepository, AreaRepository<AreaModel>>();
            base.RegisterType<IBinRepository, BinRepository<BinModel>>();
            base.RegisterType<ISlotRepository, SlotRepository<SlotModel>>();
            base.RegisterType<ILabelRepository, LabelRepository<LabelModel>>();
            base.RegisterType<IHomeAddressRelationRepository, HomeAddressRelationRepository<HomeAddressRelationModel>>();
            base.RegisterType<IReplicationlogRepository, ReplicationlogRepository<ReplicationlogModel>>();

            base.RegisterType<IItemRepository, BuiltinItemRepository<ItemModel>>();
            base.RegisterType<IPackageVersionRepository, BuiltinPackageRepository<PackageVersionViewModel>>();

            // 注入 Manager 實作
            base.RegisterType<IManifestManger, ManifestManager>();
            base.RegisterType<IHomeAddressRelationManager, HomeAddressRelationManager>();
            base.RegisterType<IBolManager, ManifestManager>();
            base.RegisterType<IVesselManager, ManifestManager>();
            base.RegisterType<IWorkOrderManager, ManifestManager>();
            base.RegisterType<IWarehouseManger, WarehouseManager>();
            base.RegisterType<ILabelManager, LabelManager>();
            base.RegisterType<IInventoryManager, InventoryManager>();
            base.RegisterType<IWarehouseManger, WarehouseManager>();
            base.RegisterType<IAreaManager, AreaManager>();
            base.RegisterType<IBinManager, BinManager>();
            base.RegisterType<ISlotManager, SlotManager>();
            base.RegisterType<ILabelManager, LabelManager>();
            base.RegisterType<IWarehouseAgent, WarehouseAgent>();

            //注入 Expcetion Trace Module
            base.RegisterType<IExceptionTraceHandler, ExceptionTraceHandler>();
            base.RegisterTypeInjectionProperties<IAreaRepository, AreaRepository<IAreaModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IManifestRepository, ManifestRepository<ManifestModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IManifestItemListRepository, ManifestItemListRepository<ManifestItemListModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IPodRepository, PodRepository<PodModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IPayloadRepository, PayloadRepository<PayloadModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IVesselManifestRepository, VesselManifestRepository<VesselManifestModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IVesselRepository, VesselRepository<VesselModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IBolRepository, BolRepository<BolModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IWorkOrderRepository, WorkOrderRepository<WorkOrderModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IWorkOrderPodRepository, WorkOrderPodRepository<WorkOrderPodModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IWorkOrderPayloadRepository, WorkOrderPayloadRepository<WorkOrderPayloadModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<ILabelRepository, LabelRepository<LabelModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IWarehouseRepository, WarehouseRepository<WarehouseModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IInventoryRepository, InventroyRepository<InventoryModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IPayloadTransactionLogRepository, PayloadTransactionLogRepository<PayloadTransactionLogModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IWarehouseRepository, WarehouseRepository<WarehouseModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IAreaRepository, AreaRepository<AreaModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IBinRepository, BinRepository<BinModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<ISlotRepository, SlotRepository<SlotModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<ILabelRepository, LabelRepository<LabelModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IHomeAddressRelationRepository, HomeAddressRelationRepository<HomeAddressRelationModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<IReplicationlogRepository, ReplicationlogRepository<ReplicationlogModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<ITicketRelationRepository, TicketRelationRepository<TicketRelationModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<ITicketRepository, TicketRepository<TicketModel>>("Tracehandler");
            base.RegisterTypeInjectionProperties<ITicketInfoRepository, TicketInfoRepository<TicketInfoModel>>("Tracehandler");

        }

    }
}

