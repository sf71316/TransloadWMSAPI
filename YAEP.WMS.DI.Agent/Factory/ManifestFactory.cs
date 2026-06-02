using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using YAEP.Core.Item.Interfaces;
using YAEP.Data.ORM.Dapper.Contrib;
using YAEP.Data.ORM.Interfaces;
using YAEP.Data.ORM.Templates;
using YAEP.Package.Interfaces;
using YAEP.Templates.DI.Unity;
using YAEP.WMS.BLL.Manager;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.DAL.Repository;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;

namespace YAEP.WMS.DI.Agent.Factory
{
    public class ManifestFactory : AbstractMiddleLayerFactory
    {
        public ManifestFactory(IUnityContainer container) : base(container)
        {
            this.CallPublicDIPool();
            // 注入 Model 定義 &  Repository 處理器 
            this.registerModelDefinition<NotificationReceiverModel>();
            this.registerModelDefinition<NotificationSenderTaskModel>();
            this.registerModelDefinition<ShipviaPaymentInfoModel>();
            this.registerModelDefinition<ShipMethodModel>();
            this.registerModelDefinition<TicketModel>();
            this.registerModelDefinition<TicketInfoModel>();
            this.registerModelDefinition<TicketInfoAssigneeRelationModel>();
            this.registerModelDefinition<TicketRelationModel>();
            this.registerModelDefinition<TicketAttachmentFolderRelationModel>();
            this.registerModelDefinition<ItemModel>();
            this.registerModelDefinition<PackageVersionViewModel>();



            // 注入 Repository 實作
            base.RegisterType<IShipMethodRepository, ShipMethodRepository<ShipMethodModel>>();
            base.RegisterType<INotificationReceiverRepository, NotificationReceiverRepository<NotificationReceiverModel>>();
            base.RegisterType<INotificationSenderTaskRepository, NotificationSenderTaskRepository<NotificationSenderTaskModel>>();
            base.RegisterType<IShipviaPaymentInfoRepository, ShipviaPaymentInfoRepository<ShipviaPaymentInfoModel>>();
            base.RegisterType<ITicketRepository, TicketRepository<TicketModel>>();
            base.RegisterType<ITicketInfoRepository, TicketInfoRepository<TicketInfoModel>>();
            base.RegisterType<ITicketInfoAssigneeRelationRepository, TicketInfoAssigneeRelationRepository<TicketInfoAssigneeRelationModel>>();
            base.RegisterType<ITicketRelationRepository, TicketRelationRepository<TicketRelationModel>>();
            base.RegisterType<ITicketAttachmentFolderRelationRepository, TicketAttachmentFolderRelationRepository<TicketAttachmentFolderRelationModel>>();



            // 注入 Manager 實作
            //base.RegisterType<IManifestAgent, SeniorManager>();
            base.RegisterType<IInventoryManager, InventoryManager>();
            base.RegisterType<IWorkOrderManager, ManifestManager>();
            base.RegisterType<ITicketManager, ManifestManager>();
            base.RegisterType<IOrderManager, ManifestManager>();
            base.RegisterType<IManifestAgent, SeniorManager>();

            //base.RegisterType<IWarehouseAgent, WarehouseAgent>();
            // 注入 物件
            base.RegisterType<IManifestDeleteParameters, ManifestDeleteParameters>();
            base.RegisterType<IManifestItemListDeleteParameters, ManifestItemListDeleteParameters>();
            base.RegisterType<IBolSearchParameters, BolSearchParameters>();
            base.RegisterType<IVesselSearchParameters, VesselSearchParameters>();
            base.RegisterType<IVesselDeleteParamters, VesselDeleteParamters>();
            base.RegisterType<IVesselManifestDeleteParameters, VesselManifestDeleteParameters>();
            base.RegisterType<IVesselManifestSearchParameters, VesselManifestSearchParameters>();

            base.RegisterType<IManifestItemListModel, ManifestItemListModel>();
            base.RegisterType<IManifestModel, ManifestModel>();
            base.RegisterType<IBolModel, BolModel>();
            base.RegisterType<IVesselModel, VesselModel>();
            base.RegisterType<IVesselManifestModel, VesselManifestModel>();
            base.RegisterType<IWorkOrderModel, WorkOrderModel>();
            base.RegisterType<IWorkOrderPodModel, WorkOrderPodModel>();
            base.RegisterType<IWorkOrderPayloadModel, WorkOrderPayloadModel>();
            base.RegisterType<IVoidTicketParameters, VoidTicketParameters>();

            #region Bulk Pick

            // 注入 Model 定義 &  Repository 處理器 
            this.registerModelDefinition<BulkPickModel>();
            this.registerModelDefinition<BulkPickTicketInfoRelationModel>();
            this.registerModelDefinition<BulkPickWorkOrderPayloadRelationModel>();
            // 注入 Repository 實作
            base.RegisterType<IBulkPickRepository, BulkPickRepository<BulkPickModel>>();
            base.RegisterType<IBulkPickWorkOrdrPayloadRelationRepository, BulkPickWorkOrdrPayloadRelationRepository<BulkPickWorkOrderPayloadRelationModel>>();
            base.RegisterType<IBulkPickTicketInfoRelationRepository, BulkPickTicketInfoRelationRepository<BulkPickTicketInfoRelationModel>>();
            // 注入 Manager 實作 
            base.RegisterType<IBulkPickManager, ManifestManager>();
            // 注入 參數
            base.RegisterType<IBulkPickSearchParameters, BulkPickSearchParameters>();
            base.RegisterType<IBulkPickManifestSearchParameters, BulkPickManifestSearchParameters>();

            #endregion

        }

        private void registerModelDefinition<TModel>() where TModel : class
        {
            // 注入 Model 定義  
            base.RegisterType<IModelDescriptor<TModel>, GenericModelDescriptor<TModel>>();
            // 注入 Repository 處理器
            base.RegisterType<IRepositoryHandler<TModel>, GenericRepositoryHandler<TModel>>();
        }

        public IManifestAgent CreateManger()
        {
            var instance = base.Resolve<IManifestAgent>();
            return instance;
        }
        public IVoidTicketParameters GenerateVoidTicketParameters()
        {
            var instance = base.Resolve<IVoidTicketParameters>();
            return instance;
        }

    }
}
