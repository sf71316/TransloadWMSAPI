using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.Interfaces;
using YAEP.Data.ORM.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager
    {
        public ManifestManager(IManifestRepository repository,
            IManifestItemListRepository itemrepository,
            IBolRepository bolrepository,
            IVesselManifestRepository vesselmanifestrepository,
            IVesselRepository vesselrepository,
            IWorkOrderRepository workorderrepository,
            IWorkOrderPodRepository workorderrpodepository,
            IWorkOrderPayloadRepository workorderpayloadrepository,
            IAuthenticationProvider authenticationInfoProvider,
            IItemManager itemManager,
            IPackageUomManager packageUomManager,
            IPackageManager packageManager,
            IPartyManager partyManager,
            IPackageVersionManager packageVersionManager,
            IWarehouseManger warehouseManager,
            ILabelManager labelManager,
            ILabelRepository labelRepository,
            IInventoryManager inventoryManager,
            IWarehouseRepository warehouseRepository,
            IWarehouseAgent warehouseAgent,
            ITicketRepository ticketRepository,
            ITicketInfoRepository ticketInfoRepository,
            ITicketRelationRepository ticketRelationRepository,
            ITicketInfoAssigneeRelationRepository ticketInfoAssigneeRelationRepository,
            ITicketAttachmentFolderRelationRepository ticketAttachmentFolderRelationRepository,
            IShipMethodRepository shipMethodRepository,
            INotificationReceiverRepository receiverRepository,
            IShipviaPaymentInfoRepository shipviaPaymentInfoRepository,
            INotificationSenderTaskRepository notificationSenderTaskRepository,
            IBulkPickRepository bulkPickRepository,
            IBulkPickTicketInfoRelationRepository bulkPickInfoTickInfoRelationRepository,
            IBulkPickWorkOrdrPayloadRelationRepository bulkPickWorkOrdrPayloadRelationRepository,
            IPayloadRepository payloadRepository,
            ISlotRepository slotRepository,
            IInventoryRepository inventoryRepository,
            IReplicationlogRepository replicationlogRepository,
            ISequenceAgent sequenceAgent, IAppSettings appSettings, IGroupManager groupManager
            , IObjectRelationalMappingLayer dbentities
            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IRefreshDrKnowAll refreshDKA, IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository)
            : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager, packageManager,
                  packageUomManager, itemManager, partyManager, itemmgmterfunc, dbentities, refreshDKA, itemRepository,
                  packageVersionRepository)
        {
            this.WarehouseAgent = warehouseAgent;
            this.ItemManager = itemManager;
            this.PackageManager = packageManager;
            this.WarehouseManager = warehouseManager;
            this.LabelManager = labelManager;
            this.InventoryManager = inventoryManager;
            this.PackageUomManager = packageUomManager;
            this.TicketManager = this;
            this.WorkOrderManager = this;
            this.PackageVersionManager = packageVersionManager;
            this.Repository = repository;
            this.WorkOrderRepository = workorderrepository;
            this.BolRepository = bolrepository;
            this.ManifestItemListRepository = itemrepository;
            this.LabelRepository = labelRepository;
            this.VesselManifestRepository = vesselmanifestrepository;
            this.WarehouseRepository = warehouseRepository;
            this.VesselRepository = vesselrepository;
            this.ShipMethodRepository = shipMethodRepository;
            this.WorkOrderPodRepository = workorderrpodepository;
            this.WorkOrderPayloadRepository = workorderpayloadrepository;
            this.ShipviaPaymentInfoRepository = shipviaPaymentInfoRepository;
            this.TicketRepository = ticketRepository;
            this.TicketInfoRepository = ticketInfoRepository;
            this.TicketRelationRepository = ticketRelationRepository;
            this.TicketInfoAssigneeRelationRepository = ticketInfoAssigneeRelationRepository;
            this.TicketAttachmentFolderRelationRepository = TicketAttachmentFolderRelationRepository;
            this.ReceiverRepository = receiverRepository;
            this.NotificationSenderTaskRepository = notificationSenderTaskRepository;
            this.PayloadRepository = payloadRepository;
            this.SlotRepository = slotRepository;
            this.InventoryRepository = inventoryRepository;
            this.ReplicationlogRepository = replicationlogRepository;
            this.BulkPickWorkOrdrPayloadRelationRepository = bulkPickWorkOrdrPayloadRelationRepository;
            StatusManageAgentParamters param = new StatusManageAgentParamters();
            param.LabelManager = this.LabelManager;
            param.BolRepository = this.BolRepository;
            param.ManifestItemRepository = this.ManifestItemListRepository;
            param.ManifestRepository = this.Repository;
            param.VesselRepository = this.VesselRepository;
            param.VesselManifestRepository = this.VesselManifestRepository;
            param.WorkOrderPayloadRepository = this.WorkOrderPayloadRepository;
            param.WorkOrderPodRepository = this.WorkOrderPodRepository;
            param.WorkOrderRepository = this.WorkOrderRepository;
            param.TicketRepository = TicketRepository;
            param.TicketInfoRepository = TicketInfoRepository;
            param.TicketManager = TicketManager;
            param.BolManager = this;
            param.InventoryManager = inventoryManager;
            param.TracingAgent = this.TracingAgent;
            param.AuthProvider = this.AuthProvider;
            StatusAgent = new StatusManageAgent(param);

            this.StatusCenter = new StatusCenter(StatusAgent, this.TracingAgent, this.AuthProvider);

            // Bulk Pick
            this._BulkPickRepository = bulkPickRepository;
            this._BulkPickTicketInfoRelationRepository = bulkPickInfoTickInfoRelationRepository;

            var replicationManagerInitParameters = new ReplicationManagerInitParameters();
            replicationManagerInitParameters.AuthenticationInfo = this.AuthProvider.GetAuthenticationInfo();
            replicationManagerInitParameters.InventoryManager = this.InventoryManager;
            replicationManagerInitParameters.PackageCacheManager = this.PackageCacheManager;
            replicationManagerInitParameters.ProductCacheManager = this.ProductCacheManager;
            replicationManagerInitParameters.ReplicationlogRepository = this.ReplicationlogRepository;
            replicationManagerInitParameters.TicketInfoRepository = this.TicketInfoRepository;
            replicationManagerInitParameters.TracingAgent = this.TracingAgent;
            this.ReplicationManager = new ReplicationManager(replicationManagerInitParameters);
        }
        internal bool CompareOnhand(Guid onhandPkg, Guid comparePkg, int onhandQty, int CompareQty)
        {
            var param = new AllocateExecutorParameters
            {
                InventoryManager = this.InventoryManager,
                ProductUtility = this.ProductUtility,
                WorkOrderPayloadRepository = this.WorkOrderPayloadRepository,
                LabelManager = this.LabelManager,
                PackageMappingCache = this.PackageCacheManager,
                SequenceAgent = this.SequenceAgent,
                TracingAgent = this.TracingAgent
            };
            var executor = new AllocateExecutor(param);
            return executor.CompareOnhand(onhandPkg, comparePkg, onhandQty, CompareQty) >= 0;
        }
        internal StatusManageAgentParamters GetStatusManageAgentParamters()
        {
            var parm = new StatusManageAgentParamters();
            parm.BolRepository = this.BolRepository;
            parm.ManifestItemRepository = this.ManifestItemListRepository;
            parm.ManifestRepository = this.Repository;
            parm.VesselRepository = this.VesselRepository;
            parm.VesselManifestRepository = this.VesselManifestRepository;
            parm.WorkOrderPayloadRepository = this.WorkOrderPayloadRepository;
            parm.WorkOrderPodRepository = this.WorkOrderPodRepository;
            parm.WorkOrderRepository = this.WorkOrderRepository;
            parm.TicketRepository = TicketRepository;
            parm.TicketInfoRepository = TicketInfoRepository;
            parm.TicketManager = this;
            parm.BolManager = this;
            parm.TracingAgent = this.TracingAgent;
            parm.AuthProvider = this.AuthProvider;
            return parm;
        }
        internal TicketProcessAgentParameter GetTicketProcessAgentParameter(IStatusManageAgentParamters statusManageAgent = null)
        {

            if (statusManageAgent == null)
            {
                statusManageAgent = GetStatusManageAgentParamters();
            }
            var _parameter = new TicketProcessAgentParameter();
            _parameter.AuthenticationProvider = this.AuthProvider;
            _parameter.ManifestManager = this;
            _parameter.WorkOrderManager = this.WorkOrderManager;
            _parameter.InventoryManager = this.InventoryManager;
            _parameter.SequenceAgent = this.SequenceAgent;
            _parameter.StatusAgent = new StatusManageAgent(statusManageAgent);
            _parameter.WarehouseManger = this.WarehouseAgent.WarehouseManager;
            _parameter.PackageManager = this.PackageManager;
            _parameter.TicketManager = this;
            _parameter.LabelRepository = this.LabelRepository;
            _parameter.PackageUomManager = this.PackageUomManager;
            _parameter.NotificationSenderTaskRepository = this.NotificationSenderTaskRepository;
            _parameter.LogInfiltrator = this;
            _parameter.AppConfigure = this.AppConfigure;
            _parameter.BulkPickManager = this;
            _parameter.ReplicationManager = this.ReplicationManager;
            _parameter.TicketInfoRepository = this.TicketInfoRepository;
            _parameter.TicketInfoAssigneeRelationRepository = this.TicketInfoAssigneeRelationRepository;
            _parameter.TicketRepository = this.TicketRepository;
            _parameter.WorkOrderPayloadRepository = this.WorkOrderPayloadRepository;
            _parameter.WorkOrderPodRepository = this.WorkOrderPodRepository;
            _parameter.WorkOrderRepository = this.WorkOrderRepository;
            _parameter.TracingAgent = this.TracingAgent;
            _parameter.PackageCacheManager = this.PackageCacheManager;
            _parameter.TransactionAction = this;
            return _parameter;
        }
        private StatusCenter StatusCenter { get; set; }
        private IBulkPickWorkOrdrPayloadRelationRepository BulkPickWorkOrdrPayloadRelationRepository { get; set; }
        private IReplicationlogRepository ReplicationlogRepository { get; set; }
        private INotificationSenderTaskRepository NotificationSenderTaskRepository { get; set; }
        private INotificationReceiverRepository ReceiverRepository { get; set; }
        private IShipMethodRepository ShipMethodRepository { get; set; }
        private IShipviaPaymentInfoRepository ShipviaPaymentInfoRepository { get; set; }
        //private IPartyManager PartyManager { get; set; }
        private IWarehouseAgent WarehouseAgent { get; set; }
        private ITicketAttachmentFolderRelationRepository TicketAttachmentFolderRelationRepository { get; set; }
        private StatusManageAgent StatusAgent { get; set; }
        private ITicketManager TicketManager { get; set; }
        private ITicketRepository TicketRepository { get; set; }
        private ITicketInfoRepository TicketInfoRepository { get; set; }
        private IPackageUomManager PackageUomManager { get; set; }
        private IWarehouseRepository WarehouseRepository { get; set; }
        private ILabelManager LabelManager { get; set; }
        private IWarehouseManger WarehouseManager { get; set; }
        private IManifestRepository Repository { get; set; }
        private IManifestItemListRepository ManifestItemListRepository { get; set; }
        private IBolRepository BolRepository { get; set; }
        private IVesselRepository VesselRepository { get; set; }
        private IVesselManifestRepository VesselManifestRepository { get; set; }
        private IVesselManager VesselManager { get; set; }
        private IWorkOrderRepository WorkOrderRepository { get; set; }
        private IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        private IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        private IItemManager ItemManager { get; set; }
        private IPackageManager PackageManager { get; set; }
        private IInventoryManager InventoryManager { get; set; }
        private IPackageVersionManager PackageVersionManager { get; set; }
        private IPayloadRepository PayloadRepository { get; set; }
        private ISlotRepository SlotRepository { get; set; }
        private ReplicationManager ReplicationManager { get; set; }
        #region Bulk Pick

        private readonly IBulkPickRepository _BulkPickRepository;
        private readonly IBulkPickTicketInfoRelationRepository _BulkPickTicketInfoRelationRepository;

        #endregion
    }
}
