using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.BLL.Model;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Identities.Interfaces;
using System.Transactions;
using YAEP.Core.Party.Interfaces;
using YAEP.Package.Interfaces.Models;
using System.Collections;
using YAEP.Data.ORM.Interfaces;
using YAEP.WMS.Interfaces.Model;
using System.Collections.Concurrent;

namespace YAEP.WMS.BLL.Manager
{
    public class InventoryManager : AbstractManager, IInventoryManager
    {
        private readonly IInventoryRepository _InventoryRepository;
        private readonly ISlotRepository _SlotRepository;
        //private readonly IItemManager _ItemManager;
        //private readonly IPackageManager _PackageManager;
        private readonly IPackageUomManager _PackageUomManager;
        private readonly IPodRepository _PodRepository;
        private readonly IPayloadRepository _PayloadRepository;
        private readonly IPayloadTransactionLogRepository _PayloadTransactionLogRepository;
        private readonly ILabelManager _LabelManager;
        private readonly IWarehouseManger _WarehouseManger;
        private readonly ILabelRepository _LabelRepository;
        private readonly IWorkOrderPayloadRepository _workOrderPayloadRepository;

        public InventoryManager(IAppSettings appSettings, IAuthenticationProvider authenticationInfoProvider,
                                            IInventoryRepository inventoryRepository,
                                            ISlotRepository slotRepository,
                                            IPodRepository podRepository,
                                            IPayloadRepository payloadRepository,
                                            IPayloadTransactionLogRepository payloadTransactionLogRepository,
                                            IItemManager itemManager,
                                            IPackageManager packageManager,
                                            IPackageUomManager packageUomManager,
                                            ILabelManager labelManager,
                                            IWarehouseManger warehouseManger,
                                            ISequenceAgent sequenceAgent,
                                            IReplicationlogRepository replicationlogRepository,
                                            ITicketInfoRepository ticketInfoRepository,
                                            IGroupManager groupManager,
                                            ILabelRepository labelRepository,
                                            IWorkOrderPayloadRepository workOrderPayloadRepository,
                                            IPartyManager partyManager,
                                            IObjectRelationalMappingLayer dbentities
                                            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IRefreshDrKnowAll refreshDKA,
             IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository
                                           )
            : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager, packageManager,
                  packageUomManager, itemManager, partyManager, itemmgmterfunc, dbentities, refreshDKA, itemRepository,
                  packageVersionRepository)
        {
            this._SlotRepository = slotRepository;
            this._InventoryRepository = inventoryRepository;
            this._PayloadTransactionLogRepository = payloadTransactionLogRepository;
            this._PayloadRepository = payloadRepository;
            this._PodRepository = podRepository;
            this._LabelRepository = labelRepository;
            this._LabelManager = labelManager;
            _PackageUomManager = packageUomManager;
            this._workOrderPayloadRepository = workOrderPayloadRepository;
            this._WarehouseManger = warehouseManger;
            var replicationManagerInitParameters = new ReplicationManagerInitParameters();
            replicationManagerInitParameters.AuthenticationInfo = this.AuthProvider.GetAuthenticationInfo();
            replicationManagerInitParameters.InventoryManager = this;
            replicationManagerInitParameters.PackageCacheManager = this.PackageCacheManager;
            replicationManagerInitParameters.ProductCacheManager = this.ProductCacheManager;
            replicationManagerInitParameters.ReplicationlogRepository = replicationlogRepository;
            replicationManagerInitParameters.TicketInfoRepository = ticketInfoRepository;
            replicationManagerInitParameters.TracingAgent = TracingAgent;
            this.ReplicationManager = new ReplicationManager(replicationManagerInitParameters);
            var labelAgentInitParameter = new LabelAgentInitParameter();
            //labelAgentInitParameter.ItemManager = this.ItemManager;
            labelAgentInitParameter.LabelManager = this._LabelManager;
            labelAgentInitParameter.PackageCacheManager = this.PackageCacheManager;
            labelAgentInitParameter.PackageUomManager = this.PackageUomManager;
            labelAgentInitParameter.ProductCacheManager = this.ProductCacheManager;
            this.LabelAgent = new LabelAgent(labelAgentInitParameter);
        }
        private ReplicationManager ReplicationManager { get; set; }
        private LabelAgent LabelAgent { get; set; }
        public IActionResult<bool> AddLog(IPayloadTransactionLogModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }

            if (model.UID == Guid.Empty)
            {
                model.UID = Guid.NewGuid();
            }

            return this._PayloadTransactionLogRepository.AddLog(model);
        }
        public IActionResult<bool> BatchAddLog(IEnumerable<IPayloadTransactionLogModel> model)
        {
            return this._PayloadTransactionLogRepository.BatchAddLog(model);
        }
        public IActionResult<Guid> AddPayloadWithModifedSpecialLogical(Guid packageUID, Guid itemUID, int onhandQty,
            Guid slotUID, Guid allocatedPayloadUID)
        {
            List<IActionResult<bool>> result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<Guid>();
            var slotInfo = this._SlotRepository.GetList(new { UID = slotUID });
            var warehouseUID = slotInfo.Content.FirstOrDefault().WarehouseUID;
            var _currentPkg = this.PackageCacheManager.GetPackage(packageUID);
            //新增payload 
            PayloadInnerModel payload = new PayloadInnerModel();
            payload.UID = Guid.NewGuid();
            payload.ID = this.SequenceAgent.GetPayloadSeqenceByTimeSerial(PayloadType.Allocated);
            payload.ItemUID = itemUID;
            payload.PackageUID = packageUID;
            payload.PODUID = Guid.NewGuid();
            payload.Quantity = onhandQty;
            payload.SlotUID = slotUID;
            payload.Status = (int)PayloadStatus.Active;
            payload.Type = (int)PayloadType.Allocated;
            payload.VolumeLimit = this.ProductUtility.CalculateCUFT(_currentPkg, payload.Quantity);
            payload.WeightLimit = this.ProductUtility.CaculateTTLWeight(_currentPkg, payload.Quantity);
            payload.VesselUID = Guid.Empty;
            payload.OriginalPayloadUID = allocatedPayloadUID;
            result.Add(this.AddPayload(payload));
            //TODO 新增Trans log
            var logModel = new PayloadTransactionLogInnerModel();
            logModel.UID = Guid.NewGuid();
            logModel.ItemUID = itemUID;
            logModel.TargetPackage = packageUID;
            logModel.QtyAfterTX = onhandQty;
            logModel.TargetSlotUID = slotUID;
            logModel.Status = (int)PayloadTransactionLogStatus.Active;
            logModel.Type = (int)PayloadTransactionLogTypes.Adjust_Web_Modified_Inventory;
            logModel.WarehouseUID = warehouseUID;
            logModel.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
            logModel.CreatedOn = DateTime.UtcNow;
            result.Add(this.AddLog(logModel));

            //新增 Label
            result.Add(this.LabelAgent.GenerateItemLabel(itemUID, packageUID, payload.UID));

            if (result.All(p => p.Success))
            {
                rs.Success = true;
                rs.Content = payload.UID;
            }
            else
            {
                rs.Success = false;
                rs.Message = string.Join(",", result.Select(p => p.Message));
            }
            return rs;
        }
        public IActionResult<bool> AddInventory(IAddOnhandParameters addOnhandParameters, bool isAddPayload = false)
        {
            if (addOnhandParameters == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(addOnhandParameters));
            }
            if (addOnhandParameters.WarehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(addOnhandParameters.WarehouseUID));
            }
            if (addOnhandParameters.TargetPackageUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(addOnhandParameters.TargetPackageUID));
            }
            if (addOnhandParameters.SlotUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(addOnhandParameters.SlotUID));
            }
            if (addOnhandParameters.ItemUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(addOnhandParameters.ItemUID));
            }
            if (addOnhandParameters.Type == 0)
            {
                addOnhandParameters.Type = YAEP.WMS.Constant.Enums.InventoryType.Stock;
            }
            TransactionScope scope = null;

            var rs = this.GetExtensionActionResultContainer<bool>();

            using (var db = this.DbEntities.DbAdapter)
            {
                try
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    Func<IActionResult<bool>> syncMethod = null;
                    var rs1 = ProcessAddInventory(addOnhandParameters, syncMethod, isAddPayload);
                    if (rs1.Success)
                    {
                        db.Commit();
                    }
                    else
                    {
                        db.Rollback();
                    }
                    return rs1;
                }
                catch (Exception ex)
                {
                    rs.Content = false;
                    rs.Message = ex.Message + " " + ex.StackTrace;
                    this.Log(ex.Message + " " + ex.StackTrace, "Inventory", this.AuthProvider.GetAuthenticationInfo().Account,
                        Logger.WARN, (int)YAEP.Constants.BelongToTypes.Inventory, application: WMSAPIParameters.APPLICATION_NAME);
                }
            }

            return rs;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addOnhandParameters"></param>
        /// <param name="syncMethod">isPauseSync =false 需傳method參數等待呼叫</param>
        /// <param name="isAddPayload"></param>
        /// <returns></returns>
        public IActionResult<bool> ProcessAddInventory(IAddOnhandParameters addOnhandParameters, Func<IActionResult<bool>> syncMethod = null, bool isAddPayload = false)
        {
            var rs = this.GetExtensionActionResultContainer<bool>();
            IActionResult<bool> rs1 = ActionResultTemplates.Result<bool>(success: false);
            List<IActionResult<bool>> result = new List<IActionResult<bool>>();
            //修改onhand
            var model = new InventoryInnerModel();
            //var _prodMiniPkg = this._PackageManager.GetPackageTree(addOnhandParameters.TargetPackageUID).Content.GetMinPackage();
            var _prodMiniPkg = this.PackageCacheManager.GetMinPackage(addOnhandParameters.TargetPackageUID);
            var _currentPkg = this.PackageCacheManager.GetPackage(addOnhandParameters.TargetPackageUID);
            //var currentInvInfo = this.GetInventory(addOnhandParameters.WarehouseUID, addOnhandParameters.ItemUID,
            //    _prodMiniPkg.UID, addOnhandParameters.SlotUID);
            if (_currentPkg.ItemUID == addOnhandParameters.ItemUID)
            {
                var usedPod = addOnhandParameters.IsAddPod;
                var Isaddpodcondition = (usedPod && !string.IsNullOrEmpty(addOnhandParameters.PodBarcode) &&
                                    !this._LabelRepository.ExistByBarcode(addOnhandParameters.PodBarcode,
                                    (int)LabelType.Pallet_OrginalTracking).Success);

                model.UID = Guid.NewGuid();
                model.ItemUID = addOnhandParameters.ItemUID;
                model.PackageUID = _prodMiniPkg.UID;
                var addonhand = this.PackageCacheManager.GetReceivePackageUomQuantity(
                                        addOnhandParameters.TargetPackageUID, _prodMiniPkg.UID, addOnhandParameters.Onhand).Content;
                if (addOnhandParameters.Onhand < 0)
                {
                    addonhand = addonhand * -1;
                }

                model.Qty = addonhand;
                model.SlotUID = addOnhandParameters.SlotUID;
                model.WarehouseUID = addOnhandParameters.WarehouseUID;
                model.Type = (int)addOnhandParameters.Type;

                if (model.Qty == 0)
                {
                    model.Status = (int)InventoryStatus.Void;
                }
                else
                {
                    model.Status = (int)InventoryStatus.Active;
                }
                if (!usedPod)
                {
                    rs1 = this._InventoryRepository.AddInventory(model);
                }
                else
                {
                    if (usedPod && Isaddpodcondition)
                        rs1 = this._InventoryRepository.AddInventory(model);
                }

                var addpayloadrs = true;
                var newPayloadUID = Guid.Empty;
                var addpayloadmessage = "";
                if (isAddPayload)
                {
                    if (addOnhandParameters.Onhand > 0)
                    {
                        var poduid = Guid.NewGuid();
                        //新增Pod
                        if (usedPod)
                        {
                            if (!string.IsNullOrEmpty(addOnhandParameters.PodBarcode))
                            {
                                if (Isaddpodcondition)
                                {
                                    PodInnerModel pod = new PodInnerModel();
                                    pod.UID = poduid;
                                    pod.ID = this.SequenceAgent.GetPodSeqenceByTimeSerial(PayloadType.Stock);
                                    pod.Name = pod.ID;
                                    pod.IsPack = true;
                                    pod.Status = (int)PodStatus.Open;
                                    result.Add(this.AddPod(pod));
                                    result.Add(this.LabelAgent.GenerateReceivingQtyBarcodeLabel(pod.UID, addOnhandParameters.PodBarcode));
                                }
                                else
                                {
                                    addpayloadrs = false;
                                    var addpodfail = ActionResultTemplates.OK();
                                    addpodfail.Success = false;
                                    addpodfail.Message = addOnhandParameters.PodBarcode + " barcode had exist ";
                                    result.Add(addpodfail);
                                }

                            }
                            else
                            {
                                addpayloadrs = false;
                                var addpodfail = ActionResultTemplates.OK();
                                addpodfail.Success = false;
                                addpodfail.Message = "must set pod barcode";
                                result.Add(addpodfail);
                            }
                        }
                        if (Isaddpodcondition || !usedPod)
                        {
                            //新增payload 
                            PayloadInnerModel payload = new PayloadInnerModel();
                            payload.UID = Guid.NewGuid();
                            payload.Type = (addOnhandParameters.PayloadType != 0 ? (int)addOnhandParameters.PayloadType : (int)PayloadType.Stock);
                            newPayloadUID = payload.UID;
                            payload.ID = this.SequenceAgent.GetPayloadSeqenceByTimeSerial((PayloadType)payload.Type);
                            payload.ItemUID = addOnhandParameters.ItemUID;
                            payload.PackageUID = addOnhandParameters.TargetPackageUID;
                            payload.PODUID = poduid;
                            payload.Quantity = addOnhandParameters.Onhand;
                            payload.SlotUID = addOnhandParameters.SlotUID;
                            payload.Status = (int)PayloadStatus.Active;

                            //payload.Type = (int)PayloadType.Onhand;


                            payload.VolumeLimit = this.ProductUtility.CalculateCUFT(_currentPkg, payload.Quantity);
                            payload.WeightLimit = this.ProductUtility.CaculateTTLWeight(_currentPkg, payload.Quantity);
                            payload.VesselUID = Guid.Empty;
                            payload.Description = addOnhandParameters.PayloadDescription;
                            var rs2 = this.AddPayload(payload);
                            if (rs2.Success)
                            {
                                rs.AddReturnValue("NewPayloadUID", payload.UID);
                            }
                            //由外部呼叫端寫入transaction log
                            //var logModel = new PayloadTransactionLogInnerModel();
                            //logModel.UID = Guid.NewGuid();
                            //logModel.ItemUID = addOnhandParameters.ItemUID;
                            //logModel.TargetPackage = addOnhandParameters.TargetPackageUID;
                            //logModel.QtyAfterTX = addOnhandParameters.Onhand;
                            //logModel.TargetSlotUID = addOnhandParameters.SlotUID;
                            //logModel.Status = (int)PayloadTransactionLogStatus.Active;
                            //logModel.Type = (int)PayloadTransactionLogTypes.ADD_ONHAND;
                            //logModel.WarehouseUID = addOnhandParameters.WarehouseUID;
                            //logModel.PayloadUID = payload.UID;
                            //var rs3 = this.AddLog(logModel);
                            addpayloadrs = rs2.Success;
                            addpayloadmessage = " " + rs2.Message;
                            //新增UPC/EAN Label
                            var labelRs = this.LabelAgent.GenerateItemLabel(addOnhandParameters.ItemUID,
                                addOnhandParameters.TargetPackageUID, payload.UID);
                            result.Add(labelRs);

                            addpayloadrs &= labelRs.Success;
                        }
                        if (addpayloadrs)
                        {
                            if (!addOnhandParameters.isPauseSync)
                            {

                                var replicatedata = new WMSReplicateOnhandModel();
                                replicatedata.ItemUID = addOnhandParameters.ItemUID;
                                replicatedata.Quantity = addOnhandParameters.Onhand;
                                replicatedata.SlotUID = addOnhandParameters.SlotUID;
                                replicatedata.PayloadUID = newPayloadUID;
                                replicatedata.PayloadType = (int)addOnhandParameters.PayloadType;
                                syncMethod = () => this.ReplicationManager.ModifiedOnhand(new WMSReplicateOnhandModel[] { replicatedata });
                            }
                        }

                    }
                    else
                    {
                        rs1.Success = false;
                        rs1.Content = false;
                        rs1.Message = "must specified payload.";
                    }
                }

                if (rs1.Success && result.All(x => x.Success) && addpayloadrs)
                {


                    rs.Success = rs.Content = true;


                }
                else
                {
                    rs.Message = rs1.Message + string.Join(",", result.Where(p => !p.Success).Select(y => y.Message)) + addpayloadmessage;
                }
            }
            else
            {
                rs.Success = false;
                rs.Message = Resource.WAREHOUSE_MODIFIED_ONHAND_PKGNOTMATCH;
            }
            return rs;
        }
        public IActionResult<bool> DeleteInventory(Guid inventoryUID)
        {
            if (inventoryUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(inventoryUID));
            }

            return this._InventoryRepository.DeleteInventory(inventoryUID);
        }
        public IAddOnhandParameters CreateAddInventoryParameters()
        {
            return new AddOnhandInnerParameters();
        }
        public IEditOnhandParameters CreateEditInventoryParameters()
        {
            return new EditOnhandInnerParameters();
        }
        public IActionResult<bool> UpdateInventory(IEditOnhandParameters editOnhandParameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //var _prodMiniPkg = this._PackageManager
                //    .GetPackageTree(editOnhandParameters.TargetPackageUID).Content.GetMinPackage();
                var _prodMiniPkg = this.PackageCacheManager.GetMinPackage(editOnhandParameters.TargetPackageUID);
                var _model = this.GetInventory(editOnhandParameters.WarehouseUID,
               editOnhandParameters.ItemUID, _prodMiniPkg.UID, editOnhandParameters.SlotUID);

                if (_model.Success)
                {
                    var _qty = this.PackageCacheManager.GetReceivePackageUomQuantity(editOnhandParameters.TargetPackageUID,
                           _model.Content.PackageUID, Math.Abs(editOnhandParameters.Onhand));
                    if (editOnhandParameters.Onhand < 0)
                    {
                        _model.Content.Qty -= _qty.Content;
                    }
                    else
                    {
                        _model.Content.Qty += _qty.Content;
                    }
                    //if (_model.Content.Qty <= 0)
                    //{
                    //    _model.Content.Status = (int)InventoryStatus.Void;
                    //}
                    return this._InventoryRepository.EditInventory(_model.Content);
                }
                else
                {
                    rs.Content =
                    rs.Success = false;
                    rs.Message = Resource.TICKET_NOT_FIND_INVENTORY;
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;


        }
        public IActionResult<bool> InsertInventory(IEnumerable<IInsertInventoryParameter> parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            IPackageNode _prodMiniPkg = null;
            try
            {
                var userInfo = this.AuthProvider.GetAuthenticationInfo();
                List<InventoryInnerModel> collection = new List<InventoryInnerModel>();

                foreach (var parameter in parameters)
                {
                    if (parameter.UseMiniPackage)
                    {
                        _prodMiniPkg = this.PackageCacheManager.GetMinPackage(parameter.TargetPackageUID);


                        var _qty = this.PackageCacheManager.GetReceivePackageUomQuantity(parameter.TargetPackageUID,
                               _prodMiniPkg.UID, Math.Abs(parameter.Qty));
                        if (parameter.Qty < 0)
                        {
                            parameter.Qty = _qty.Content * -1;
                        }
                        else
                        {
                            parameter.Qty = _qty.Content;
                        }
                    }
                    InventoryInnerModel _model = new InventoryInnerModel();
                    _model.CreatedBy = userInfo.Account;
                    _model.CreatedOn = DateTime.UtcNow;
                    _model.ItemUID = parameter.ItemUID;
                    _model.Qty = parameter.Qty;
                    _model.SlotUID = parameter.SlotUID;
                    _model.Status = (int)InventoryStatus.Active;
                    _model.PackageUID = parameter.TargetPackageUID;
                    _model.WarehouseUID = parameter.WarehouseUID;
                    _model.Type = (int)parameter.Type;
                    collection.Add(_model);
                }

                return this._InventoryRepository.BatchAddInventory(collection);


            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;


        }

        public IActionResult<IEnumerable<IInventoryViewModel>> GetInventory(IInventorySearchParameters parameters)
        {
            var result = this._InventoryRepository.GetInventory(parameters);
            return result;
        }

        public IActionResult<IInventoryModel> GetInventory(Guid warehouseUID, Guid itemUID, Guid packageUID, Guid slotUID)
        {
            if (warehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IInventoryModel>(nameof(warehouseUID));
            }
            if (itemUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IInventoryModel>(nameof(itemUID));
            }
            if (packageUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IInventoryModel>(nameof(packageUID));
            }
            if (slotUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IInventoryModel>(nameof(slotUID));
            }

            return this._InventoryRepository.GetInventory(warehouseUID, itemUID, packageUID, slotUID);
        }

        public IActionResult<IEnumerable<IInventoryDetailViewModel>> GetInventoryDetail(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<IInventoryDetailViewModel>>(nameof(itemUID));
            }

            return this._InventoryRepository.GetInventoryDetail(itemUID);
        }

        public IActionResult<bool> IsItemInSlot(Guid warehouseUID, Guid itemUID, Guid packageUID, Guid slotUID)
        {
            if (warehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(warehouseUID));
            }
            if (itemUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(itemUID));
            }
            if (packageUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(packageUID));
            }
            if (slotUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(slotUID));
            }
            var minipkgRoot = this.PackageCacheManager.GetMinPackage(packageUID);
            if (minipkgRoot != null)
            {
                var minipkg = minipkgRoot;
                return this._InventoryRepository.IsItemInSlot(warehouseUID, itemUID, minipkg.UID, slotUID);
            }
            else
            {
                var rs = ActionResultTemplates.Result<bool>();
                rs.Message = Resource.LABEL_NOT_FIND_PKG_TREE;
                return rs;
            }
        }

        public IActionResult<IEnumerable<IPayloadTransactionLogViewModel>> GetTranascationList
            (IPayloadTransactionLogParameters parameters)
        {
            var rs = this._PayloadTransactionLogRepository.GetTranascationList(parameters);
            if (rs.Content != null)
            {
                foreach (var item in rs.Content)
                {
                    var pkg = this.PackageCacheManager.GetPackage(item.TargetPackage);
                    if (pkg != null)
                    {
                        item.UOMUID = pkg.UOM;
                        item.PackageName = pkg.Name;
                    }
                }
            }
            return rs;
        }
        public IActionResult<bool> AddPod(IPodModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }

            if (model.UID == Guid.Empty)
            {
                model.UID = Guid.NewGuid();
            }

            return this._PodRepository.AddPod(model);
        }

        public IActionResult<bool> AddPayload(IPayloadModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }

            if (model.UID == Guid.Empty)
            {
                model.UID = Guid.NewGuid();
            }

            return this._PayloadRepository.AddPayload(model);
        }
        public IActionResult<bool> BatchAddPayload(IEnumerable<IPayloadModel> model)
        {
            return this._PayloadRepository.BatchAddPayload(model);
        }
        public IActionResult<bool> UpdatePayload(IPayloadModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }
            if (model.UID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model.UID));
            }

            return this._PayloadRepository.UpatePayload(model);
        }
        public IActionResult<bool> BatchUnPack(IEnumerable<Guid> model)
        {
            return this._PodRepository.UnPack(model);
        }
        public IActionResult<bool> BatchUpdatePayload(IEnumerable<IPayloadModel> model)
        {
            return this._PayloadRepository.BatchUpatePayload(model);
        }
        public IActionResult<bool> DeletePayloadFromDb(object condition)
        {
            return this._PayloadRepository.DeletePayloadFromDb(condition);
        }
        public IActionResult<bool> DeletePodFromDb(object condition)
        {
            return this._PodRepository.DeletePodFromDb(condition);
        }
        public IActionResult<IEnumerable<IPayloadModel>> GetPayloadList(object condition)
        {
            if (condition == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<IPayloadModel>>(nameof(condition));
            }

            return this._PayloadRepository.GetList(condition);
        }
        /// <summary>
        /// 取得payload 資料(會找被刪除的資料)
        /// </summary>
        /// <param name="payloadUID"></param>
        /// <returns></returns>
        public IActionResult<IPayloadModel> GetPayload(Guid payloadUID)
        {
            if (payloadUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IPayloadModel>(nameof(payloadUID));
            }

            return this._PayloadRepository.GetPayload(payloadUID);
        }
        public IActionResult<IEnumerable<IPayloadModel>> GetPayload(IEnumerable<Guid> PayloadUID)
        {
            return this._PayloadRepository.FindList(PayloadUID);
        }
        public IActionResult<IEnumerable<IPayloadModel>> GetPayload(IEnumerable<Guid> PayloadUID, PayloadType type)
        {
            return this._PayloadRepository.FindList(PayloadUID, type);
        }
        public IActionResult<IEnumerable<IPodModel>> GetPod(Guid[] PodUIDs)
        {
            return this._PodRepository.GetPod(PodUIDs);
        }
        public IActionResult<bool> UpdatePod(IPodModel model)
        {
            return this._PodRepository.UpdatePod(model);
        }
        public IActionResult<bool> UnPack(Guid PodUID)
        {
            return this._PodRepository.UnPack(PodUID);
        }

        public IActionResult<IEnumerable<IAvailableInventoryModel>> GeteAvailableInventoryList(IGetAvailableInventoryParameters parameters)
        {
            var _result = this._InventoryRepository.GeteAvailableInventoryList(parameters);
            if (_result.Success)
            {

                foreach (var item in _result.Content)
                {
                    item.ItemName = this.ProductCacheManager.GetItem(item.ItemUID).Name;
                    item.PackageName = this.PackageCacheManager.GetPackage(item.PackageUID).Name;
                    item.StatusName = ((PayloadStatus)item.Status).ToString();
                }
            }
            return _result;
        }

        public IActionResult<IEnumerable<IAllocatedModel>> GetAllocatedData(Guid[] warehouseUID, Guid[] itemUID)
        {
            return this._PayloadRepository.GetAllocatedData(warehouseUID, itemUID);
        }

        public IActionResult<bool> ChangePodStauts(Guid poduid, PodStatus status)
        {
            return this._PodRepository.ChangePodStauts(poduid, status);
        }

        public IActionResult<bool> ChangePayloadStauts(Guid payloaduid, PayloadStatus status)
        {
            return this._PayloadRepository.ChangePayloadStauts(payloaduid, status);
        }
        public IActionResult<bool> ChangePayloadStauts(IEnumerable<Guid> payloaduid, PayloadStatus status)
        {
            return this._PayloadRepository.ChangePayloadStauts(payloaduid, status);
        }
        public IActionResult<bool> ChangePayloadType(Guid payloaduid, int type)
        {
            return this._PayloadRepository.ChangePayloadType(payloaduid, type);
        }
        public IActionResult<bool> DeallocatedPayload(IEnumerable<IDeallocatedParameters> deallocatedParameters)
        {

            return this._workOrderPayloadRepository.DeallcatedByWorkOrderPayload(deallocatedParameters);
        }
        public IActionResult<bool> ChangePayloadType(IEnumerable<Guid> payloaduid, int type)
        {

            return this._PayloadRepository.ChangePayloadType(payloaduid, type);
        }
        public IActionResult<IEnumerable<ICheckOnhandModel>> GetOnhandData(Guid warhouseUID, Guid itemUID)
        {
            return this._InventoryRepository.GetOnhandData(warhouseUID, itemUID);
        }

        public IActionResult<IEnumerable<IPayloadModel>> GetListByTicket(Guid ticketUID)
        {
            return this._PayloadRepository.GetListByTicket(ticketUID);
        }

        public IActionResult<IEnumerable<IEnumFieldInfo>> GetTranascationTypeList()
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IEnumFieldInfo>>();
            try
            {
                rs.Content = EnumerableData.GetDataForGeneric(typeof(PayloadTransactionLogTypes));
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;

        }

        public IActionResult<IEnumerable<ISlotMappingLocation>> GetSlotMappingList(IEnumerable<Guid> slotlist)
        {
            return this._SlotRepository.GetSlotMappingList(slotlist);

        }

        public IActionResult<IEnumerable<IImportTSReceivingDataResponseModel>> ImportTotalSolutionReceivingData(
            IEnumerable<IImportTSReceivingDataRequestModel> data)
        {
            List<WMSReplicateOnhandModel> replicateOnhandModels = new List<WMSReplicateOnhandModel>();
            List<IImportTSReceivingDataResponseModel> response = new List<IImportTSReceivingDataResponseModel>();
            data = data.OrderBy(p => p.ItemUID);
            var rs = ActionResultTemplates.Result<IEnumerable<IImportTSReceivingDataResponseModel>>();


            try
            {
                foreach (var importdata in data)
                {
                    var rep = new ImportTSReceivingDataResponseModel();
                    var a = this.PackageCacheManager.GetPackagesByItem(importdata.ItemUID);
                    var b = a.OrderByDescending(p => p.CreatedOn).FirstOrDefault();
                    var pkgUID = b.UID;
                    var minipkg = this.PackageCacheManager.GetMinPackage(pkgUID);
                    var param = this.CreateAddInventoryParameters();
                    rep.ItemUID = importdata.ItemUID;
                    rep.ReceivingBarcode = importdata.ReceivingBarcode;
                    rep.SlotUID = importdata.SlotUID;
                    rep.WarehouseUID = importdata.WarehouseUID;
                    param.ItemUID = importdata.ItemUID;
                    param.Onhand = importdata.Qty;
                    param.SlotUID = importdata.SlotUID;
                    param.TargetPackageUID = minipkg.UID;
                    param.Type = InventoryType.Stock;
                    param.WarehouseUID = importdata.WarehouseUID;
                    param.IsAddPod = true;
                    param.PodBarcode = importdata.ReceivingBarcode;
                    //param.isPauseSync = true;
                    param.PayloadDescription = importdata.Description;
                    var importRs = this.ProcessAddInventory(param, isAddPayload: true);
                    if (importRs.Success)
                    {
                        var extaddRs = importRs as ExtensionActionResultContainer<bool>;
                        var newpayloaduid = extaddRs.GetReturnValue<Guid>("NewPayloadUID");
                        var replicatedata = new WMSReplicateOnhandModel();
                        replicatedata.ItemUID = param.ItemUID;
                        replicatedata.Quantity = param.Onhand;
                        replicatedata.SlotUID = param.SlotUID;
                        replicatedata.PayloadUID = newpayloaduid;
                        replicateOnhandModels.Add(replicatedata);

                        rep.Key = importdata.Key;
                        rep.Success = true;
                    }
                    else
                    {
                        rep.Key = importdata.Key;
                        if (!importRs.Message.Contains("barcode"))
                            rep.Success = false;
                        else
                            rep.Success = true;
                        rep.Message = importRs.Message;
                    }
                    response.Add(rep);
                }
                if (response.All(p => p.Success))
                {

                    if (replicateOnhandModels.Count > 0)
                    {
                        var syncRS = this.ReplicationManager.ModifiedOnhand(replicateOnhandModels);
                        if (syncRS.Success)
                        {

                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = "Sync onhand failure";
                        }
                    }
                    else
                    {

                    }

                }
                rs.Content = response;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }

            return rs;
        }

        public IActionResult<int> GetItemUsageStatus(Guid ItemUID)
        {
            return this._InventoryRepository.GetItemUsageStatus(ItemUID);
        }

        public IActionResult<IEnumerable<IProductPackageExtendModel>> GetItemListFromCache(Guid? CustomerUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IProductPackageExtendModel>>();
            // this.TracingAgent.Trace("Start getting items from cache.");
            var item_list = this.ProductPackageManager.GetItems(null);
            //  this.TracingAgent.Trace("Got packages from cache.");

            rs.Content = item_list;
            rs.Success = true;
            //this.TracingAgent.Trace("Start getting items from cache.");
            //var items = this.ProductCacheManager.GetItems(null);
            //this.TracingAgent.Trace("Got items from cache.");
            //if (items != null && items.Count() > 0)
            //{
            //if (CustomerUID.HasValue)
            //{
            //    items = items.Where(x => x.CustomerUID.Equals(CustomerUID.Value)).Select(x => x);
            //}

            //this.TracingAgent.Trace("Start getting packages from cache.");
            //ConcurrentBag<ProductPackageExtendModel> item_list = new ConcurrentBag<ProductPackageExtendModel>();
            //Parallel.ForEach(items, x =>
            // {
            //     ProductPackageExtendModel target = new ProductPackageExtendModel(x);
            //     var target_packages = this.PackageCacheManager.GetPackagesByItem(x.UID).OrderBy(o => o.VersionId);
            //     if (target_packages.Count() > 0)
            //     {
            //         List<PackageExtendModel> target_package = new List<PackageExtendModel>();
            //         Parallel.ForEach(target_packages, p =>
            //         {
            //             target_package.Add(new PackageExtendModel()
            //             {
            //                 UID = p.UID,
            //                 //ID = p.ID,
            //                 Name = p.Name,
            //                 VersionId = p.VersionId
            //             });
            //         });
            //         target.Packages = target_package;
            //     }
            //     item_list.Add(target);
            // });

            //var target_packages = this.PackageCacheManager.GetPackagesByItems(items.Select(p => p.UID));
            //IEnumerable<ProductPackageExtendModel> item_list = items.Select(c =>
            //{
            //    var target = new ProductPackageExtendModel(c);
            //    target.Packages = target_packages.Where(p => p.ItemUID == c.UID).Select(p =>
            //    {
            //        return new PackageExtendModel()
            //        {
            //            UID = p.UID,
            //            //ID = p.ID,
            //            Name = p.Name,
            //            VersionId = p.VersionId
            //        };
            //    });
            //    return target;
            //});

            //this.TracingAgent.Trace("Got packages from cache.");

            //rs.Content = item_list;
            //rs.Success = true;
            //}

            return rs;
        }
    }
}
