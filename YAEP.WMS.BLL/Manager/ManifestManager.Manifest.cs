using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.BLL.Model.Parameters;
using System.Transactions;
using YAEP.Identities.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.BLL.Interfaces;
using System.Dynamic;
using YAEP.WMS.Interfaces.Model;
using YAEP.WMS.Cache.Redis;
using YAEP.Identities.Constants;
using General.Data.SQLConditionConverter.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IManifestManger
    {
        //public ManifestManager(IAuthenticationProvider authenticationInfoProvider,
        //    ISequenceAgent sequenceAgent,
        //    IAppSettings appSettings, IGroupManager groupManager)
        //   : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager)
        //{
        //}
        public IActionResult<bool> DeleteManifestAPI(IManifestDeleteParameters Parameters, bool forcedelete = false)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    rs = this.DeleteManifest(Parameters, forcedelete);
                    if (rs.Success)
                    {
                        db.Commit();
                    }
                    else
                    {
                        db.Rollback();
                    }
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
        public IActionResult<bool> DeleteManifest(IManifestDeleteParameters Parameters, bool forcedelete = false)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var manifestInfo = this.Repository.GetList(new { UID = Parameters.UID });
                if (manifestInfo.Success && manifestInfo.Content != null)
                {
                    var inStatus = new ManifestStatus[] { ManifestStatus.Review,
                        ManifestStatus.Reject,ManifestStatus.Draft,ManifestStatus.Open };
                    var canntDelete = manifestInfo.Content.Where(p => !inStatus.Contains(p.Status));
                    if (canntDelete.Count() > 0 && !forcedelete)
                    {
                        rs.Success = false;
                        rs.Message = Resource.MANIFEST_DELETE_FAILURE_NOT_IN_STATUS;
                    }
                    else
                    {
                        List<IActionResult<bool>> _result = new List<IActionResult<bool>>();
                        var bolinfo = this.BolRepository.GetList(new { ManifestUID = Parameters.UID });


                        if (bolinfo.Content != null && bolinfo.Content.Count() > 0)
                        {
                            BolDeleteInnerParameters param = new BolDeleteInnerParameters();
                            param.UID = bolinfo.Content.Select(p => p.UID).ToArray();
                            _result.Add(this.DeleteBol(param));
                        }

                        //delete manifest 
                        if (_result.All(p => p.Success))
                            _result.Add(this.Repository.Delete(Parameters));
                        //delete manifest item
                        if (_result.All(p => p.Success))
                            _result.Add(this.ManifestItemListRepository.Delete(new { ManifestUID = Parameters.UID }));
                        //delete receiver
                        if (_result.All(p => p.Success))
                            _result.Add(this.ReceiverRepository.Delete(new { BelongToUID = Parameters.UID }));
                        if (_result.All(c => c.Success))
                        {
                            rs.Success = true;

                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = string.Join(",", _result.Where(p => !p.Success).Select(x => x.Message));
                        }

                    }
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
        public IEnumerable<IEnumFieldInfo> GetManifestTypeList()
        {
            return EnumerableData.GetDataForGeneric(typeof(ManifestType));
        }

        public IActionResult<IManifestViewModel> GetManifestInfo(Guid uid)
        {

            var rs = ActionResultTemplates.Result<IManifestViewModel>();
            try
            {
                var manifestInfo = this.Repository.GetInfo(uid);
                var viewModel = new ManifestInnerViewModel(manifestInfo.Content);
                viewModel.StatusName = ((ManifestStatus)viewModel.Status).ToString();
                rs.Content = viewModel;
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
        public IActionResult<IManifestModel> GetManifest(object condition)
        {

            var rs = ActionResultTemplates.Result<IManifestModel>();
            try
            {
                var manifestInfo = this.Repository.GetList(condition);
                rs.Content = manifestInfo.Content.FirstOrDefault();
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
        public IActionResult<IManifestModel> GetManifestBySqlConverter(IQueryConditionExtractor conditionExtractor)
        {

            var rs = ActionResultTemplates.Result<IManifestModel>();
            try
            {
                var manifestInfo = this.Repository.GetListBySQLConverter(conditionExtractor);
                if (manifestInfo.Success)
                {
                    rs.Content = manifestInfo.Content.FirstOrDefault();
                    rs.Success = true;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = manifestInfo.Message;
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
        public IActionResult<IEnumerable<IManifestItemListModel>> GetManifestItemList(Guid uid)
        {
            var collection = this.ManifestItemListRepository.GetManifestItemList(uid);
            if (collection.Success)
            {
                //ItemInnerParameterize _parameters = new ItemInnerParameterize();
                //_parameters.ListOfItemUID =
                //    collection.Content.GroupBy(g => g.ItemUID).Select(p => p.Key).ToList();
                //var _items = this.ItemManager.GetItems(_parameters);
                var _items = this.ProductCacheManager.GetItems(collection.Content.Select(p => p.ItemUID));
                if (_items != null && _items.Count() > 0)
                {
                    //TODO 目前不能轉換
                    // var _vpkgmgr = PackageManager as IPackageVersionManager; 
                    foreach (var item in collection.Content)
                    {

                        var _item = _items.FirstOrDefault(p => p.UID == item.ItemUID);
                        var _pkg = this.PackageCacheManager.GetPackage(item.PackageUID);

                        item.StatusName = item.Status.ToString();
                        if (_pkg != null)
                        {
                            item.PackageName = _pkg.Name;
                            //  item.VersionName = _vpkgmgr.GetPackageVersion(_pkg.Content.VersionUID).Content.VersionId;
                        }
                        if (_item != null)
                        {
                            item.ItemID = _item.ID;
                            item.ItemName = _item.Name;
                            item.ItemDescription = _item.Description;
                        }

                    }
                }
                else
                {
                    collection.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;
                }
            }
            return collection;
        }

        public IActionResult<bool> DeleteManifestItem(IManifestItemListDeleteParameters parameters, bool isIgnoreCheck = false)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                if (!isIgnoreCheck)
                {
                    VesselManifestSearchInnerParameters parm = new VesselManifestSearchInnerParameters();
                    parm.ManifestItemUID = parameters.UID;

                    var vesselManifestInfo = this.VesselManifestRepository.GetList(parm);
                    if (vesselManifestInfo.Content.Count() > 0)
                    {
                        var vesselRefNo = this.VesselRepository.GetList(new { UID = vesselManifestInfo.Content.Select(x => x.VesselUID) });
                        rs.Message = string.Format(Resource.MANIFEST_REJECT_FAILURE_HAS_ASSIGNED_ITEM, string.Join(","
                            , vesselRefNo.Content.GroupBy(g => g.RefNo).Select(s => s.Key)));
                    }
                    else
                    {
                        return this.ManifestItemListRepository.Delete(parameters);
                    }
                }
                else
                {
                    return this.ManifestItemListRepository.Delete(parameters);
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

        public IActionResult<IManifestModel> AddManifest(IManifestModel Model)
        {

            var rs = ActionResultTemplates.Result<IManifestModel>();

            var _seq = this.SequenceAgent.GetManinfestSequence(SequenceAgent.GetManifestRootUID(),
                (ManifestType)Model.Type);
            Model.UID = Guid.NewGuid();
            Model.ID = _seq;
            var _Insertrs = this.Repository.Add(Model);
            if (_Insertrs.Success)
            {
                return this.Repository.GetInfo(Model.UID);
            }
            else
            {
                rs.Success = _Insertrs.Success;
                rs.Message = _Insertrs.Message;
                return rs;
            }
        }

        public IActionResult<bool> EditManifest(dynamic Model)
        {
            return this.Repository.Update(Model);
        }

        public IActionResult<bool> AddManifestItems(IEnumerable<IManifestItemListModel> Model)
        {

            foreach (var item in Model)
            {
                var _seq = this.SequenceAgent.GetMainfestItemListSequence(item.ManifestUID);
                item.ID = _seq;
                if (item.UID == Guid.Empty)
                    item.UID = Guid.NewGuid();
                //todo 計算 volumn,weight
            }
            return this.ManifestItemListRepository.Add(Model);
        }

        public IActionResult<bool> EditManifestItem(IManifestItemListModel Model)
        {
            return this.ManifestItemListRepository.Update(Model);
        }

        IActionResult<IEnumerable<R>> IManifestManger.GetManifestList<R>(IManifestSearchParameters Parameters)
        {
            return this.Repository.GetManifestList<R>(Parameters);
        }

        public IActionResult<IEnumerable<ICalVesselAddItemInnerModel>> GetManifestItemListByGroupItem(IGetAddItemListparameters parameters)
        {
            return this.ManifestItemListRepository.GetManifestItemListByGroupItem(parameters);
        }

        public IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestStatus status, ManifestItemListStatus ManifestStatus)
        {
            var rs1 = this.Repository.ChangeManifestStatus(manifestUID, status);
            var rs2 = this.ManifestItemListRepository.ChangeManifestStatus(manifestUID, ManifestStatus);
            rs1.Content &= rs2.Content;
            rs1.Success &= rs2.Success;
            return rs1;
        }

        public IActionResult<bool> CheckOnhand(Guid warhouseUID, Guid itemUID, Guid packageUID, int qty)
        {

            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            try
            {
                if (new Guid[] { warhouseUID, itemUID, packageUID }.Contains(Guid.Empty))
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_CHECKONHAND_LOST_PARAMETERS;
                }
                else if (qty <= 0)
                {
                    rs.Success = false;
                    rs.Message = string.Format(Resource.MANIFEST_QTY_MORE_THAN_ZERO, "");
                }
                if (rs.Success)
                {

                    var packageTree = this.PackageCacheManager.GetPackageTree(packageUID);
                    if (packageTree != null)
                    {
                        IActionResult<IEnumerable<ICheckOnhandModel>> collection = this.InventoryManager.GetOnhandData(warhouseUID, itemUID);
                        if (collection.Success && collection.Content.Count() > 0)
                        {
                            var requestMiniPackage = this.PackageCacheManager.GetMinPackage(packageUID);
                            //換算要求最小單位數量
                            var requestQty = this.PackageCacheManager.GetReceivePackageUomQuantity(packageUID,
                                requestMiniPackage.UID, qty).Content;
                            //找出與要求包裝單位相同版本的包裝&對應onhand
                            var allPackage = packageTree.Root.GetAllPackageUID();
                            var onhandModels = collection.Content.Where(p => allPackage.Contains(p.PackageUID));
                            //計算最小單位數量
                            var ttlOnhand = onhandModels.Sum(p => this.PackageCacheManager.GetReceivePackageUomQuantity(p.PackageUID,
                                 requestMiniPackage.UID, p.Qty).Content);
                            rs.Success = (ttlOnhand - requestQty) >= 0;
                            if (!rs.Success)
                            {
                                rs.Success = rs.Content = false;
                                rs.Message = Resource.MANIFEST_INSUFFICIENTONHAND;
                            }
                        }
                        else
                        {
                            rs.Success = rs.Content = false;
                            rs.Message = Resource.MANIFEST_INSUFFICIENTONHAND;
                        }
                    }
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

        public IActionResult<string> GetDefaultFolderName(Guid btu, int btp)
        {

            var rs = ActionResultTemplates.Result<string>();
            rs.Message = Resource.LABEL_UNDEFINED_BELONGTO_TYPE;
            var belongTotype = (YAEP.Constants.BelongToTypes)btp;
            //this.Repository.GetInfo
            if (belongTotype == Constants.BelongToTypes.Manifest)
            {
                var manifestInfo = this.Repository.GetInfo(btu);
                if (manifestInfo.Success)
                {
                    rs.Success = true;
                    rs.Content = manifestInfo.Content.ID;
                }
                else
                {
                    rs.Message = Resource.MANIFEST_NOT_FIND_DATA;
                }
            }
            return rs;
        }

        public IActionResult<IManifestViewModel> SubmitManifest(Guid manifestUID)
        {
            return this.StatusCenter.ProcessManifest(manifestUID);
        }

        public IActionResult<IEnumerable<IShipviaPaymentInfoModel>> GetShipviaPaymentInfo(Guid partyUID)
        {
            return this.ShipviaPaymentInfoRepository.GetList(new { partyUID = partyUID });
        }

        public IActionResult<IEnumerable<IShipMethodModel>> GetShipMethodList(Guid? partyUID)
        {
            if (partyUID.HasValue)
            {
                return this.ShipMethodRepository.GetList(new { partyUID = partyUID.Value, Status = 1 });
            }
            else
            {
                return this.ShipMethodRepository.GetList(new { Status = 1 });
            }

        }
        public IActionResult<IManifestViewModel> RejectManifest(Guid manifestuid)
        {
            return this.StatusCenter.RejectManifest(manifestuid);
        }
        public IActionResult<IEnumerable<ICheckManifestItemStatusResultModel>> GetCheckManifestItemStatusResult(Guid manifestUID)
        {
            return this.ManifestItemListRepository.GetCheckManifestItemStatusResult(manifestUID);
        }

        public IActionResult<IEnumerable<IGetModifyPayloadListModel>> GetModifyPayloadList(IGetModifyPayloadListParameters parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IGetModifyPayloadListModel>>();
            try
            {
              // this.TracingAgent.Trace("Start to prepare for items from cache.");
                List<Guid> customerUIDs = new List<Guid>();
                if (parameters.CustomerUID.HasValue)
                {
                    customerUIDs.Add(parameters.CustomerUID.Value);
                }
                var group = this.GetGroupUserViewByUser().Content;
                IEnumerable<IProductExtendModel> items = null;
                if (parameters.ItemNoList != null)
                {
                    items = parameters.ItemNoList.Select(
                       p =>
                       {
                           var item = this.ProductCacheManager.GetItem(p, customerUIDs, group);
                           if (item != null)
                               return item.FirstOrDefault();
                           else
                               return null;
                       }
                        ).ToList();
                }
                else
                {
                    if (parameters.CustomerUID.HasValue)
                    {
                        items = this.ProductCacheManager.GetItems(new List<string>(), parameters.CustomerUID.Value, group);
                    }
                    else
                    {
                        items = new List<ProductExtendModel>();
                    }
                }
               // this.TracingAgent.Trace("Done prepare for items from cache.");
                if (!items.Any(p => p == null))
                {
                  //  this.TracingAgent.Trace("Start to payload data from database.");
                    parameters.ItemUID = items.Select(p => p.UID).ToArray();
                    var result = this.InventoryRepository.GetModifyPayloadListData(parameters);
                    if (result.Content != null && result.Content.Count() > 0)
                    {
                      //  this.TracingAgent.Trace("Got payload data from database.");
                        if (parameters.CustomerUID.HasValue)
                        {
                            var filteritem = items.Where(y => y.CustomerUID == parameters.CustomerUID.Value.ToString("D"));
                            if (filteritem.Count() > 0)
                            {
                                result.Content = result.Content.Where(p => filteritem.Any(t => t.UID == p.ItemUID));
                            }
                            //else 沒給item 不回傳資料?
                            //{
                            //    result.Content = new List<IGetModifyPayloadListModel>();
                            //}
                        }
                        if (parameters.ItemNoList != null && parameters.ItemNoList.Count() > 0)
                        {
                            //var fitems = this.ProductCacheManager.GetItemWithoutCache(parameters.ItemNoList);
                            if (result.Content.Count() > 0)
                                result.Content = result.Content.Where(p => items.Any(a => a.UID == p.ItemUID));
                        }
                        if (items == null || (items != null && items.Count() == 0))
                        {

                            items = this.ProductCacheManager.GetItems(result.Content.Select(p => p.ItemUID)).ToList();

                        }
                       // this.TracingAgent.Trace("Start to fill payload data.");
                        foreach (var item in result.Content)
                        {
                            var pkgs = this.PackageCacheManager.GetPackagesByItem(item.ItemUID).OrderByDescending(o => o.VersionId).ThenByDescending(o => o.GrossWeight);
                            var pkg = this.PackageCacheManager.GetPackage(item.PackageUID);
                            var product = items.FirstOrDefault(x => x.UID == item.ItemUID);
                            item.ItemName = product?.Name;
                            item.ItemDescription = product?.Description;
                            item.IsVirtualItem = (product != null ? product.IsVirtualItem : false);
                            item.PackageName = pkg?.Name;
                            foreach (var vpkg in pkgs)
                            {
                                item.Package.Add(new ModifyPayloadPackageItem
                                {
                                    ItemName = item.ItemName,
                                    ItemUID = item.ItemUID,
                                    PackageName = vpkg.Name,
                                    PackageUID = vpkg.UID,
                                    VersionID = vpkg.VersionId
                                });
                            }
                            if (pkg != null)
                            {
                                var min_pacakge = PackageCacheManager.GetMinPackage(pkg.UID);
                                var packages = PackageCacheManager.GetPackagesByVersion(pkg.VersionUID);
                                item.EachQty = PackageCacheManager.GetReceivePackageUomQuantity(
                                        pkg.UID,
                                        min_pacakge.UID,
                                        item.Qty, packages
                                    ).Content;
                                //this.TracingAgent.Trace(String.Format("Get each qty for item {0}, package {1}, qty {2}.", item.ItemName, min_pacakge.Name, item.EachQty));

                            }
                        }
                       // this.TracingAgent.Trace("Filled payload data.");
                    }
                    rs.Content = result.Content;
                    rs.Success = result.Success;
                    rs.Message = result.Message;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.INVENTORY_ITEM_NOT_BELONG_TO_GROUP;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message + " " + ex.StackTrace;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        public IEnumerable<IEnumFieldInfo> GetPayloadStatusList()
        {
            return EnumerableData.GetDataForGeneric(typeof(PayloadStatus));
        }
        public IEnumerable<IEnumFieldInfo> GetPayloadTypeList()
        {
            return EnumerableData.GetDataForGeneric(typeof(PayloadType));
        }
        public IActionResult<bool> CreateSakanaAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> requests)
        {
            var allowAction = new int[] {
                (int)ModifyInventoryActionType.Modified
            };
            requests.ForEach(p => p.ActionType = (int)ModifyInventoryActionType.Modified);

            IActionResult<IWarehouseModel> warehouseInfo = null;
            var itemInfos = this.ProductCacheManager.GetItems(requests.Select(x => x.ItemUID));
            List<Func<IActionResult<bool>>> syncMethod = new List<Func<IActionResult<bool>>>();
            List<string> vaildResult = new List<string>();
            var rs = ActionResultTemplates.Result<bool>();
            var result = new List<IActionResult<bool>>();
            try
            {
                #region Check data
                foreach (var request in requests)
                {

                    warehouseInfo = this.WarehouseManager.GetWarehouse(request.WarehouseUID);
                    if (requests.GroupBy(g => g.WarehouseUID).Count() > 1)
                    {
                        vaildResult.Add(Resource.WAREHOSUE_MOVE_PAYLAOD_MULTIWAREHOUSE_REQUEST);
                    }
                    if (warehouseInfo.Content == null)
                    {
                        vaildResult.Add(Resource.WAREHOUSE_NOT_FIND);
                    }
                    var itemInfo = itemInfos.FirstOrDefault(p => p.UID == request.ItemUID);
                    var pkgInfo = this.PackageCacheManager.GetPackage(request.ModifyPackageUID);

                    if (itemInfo == null)
                    {
                        vaildResult.Add(Resource.COMMON_NOT_FIND_ITEM);
                    }
                    if (pkgInfo == null)
                    {
                        vaildResult.Add(string.Format(Resource.COMMON_NOT_FIND_PACKAGE, itemInfo.ID));
                    }
                    else
                    {
                        if (pkgInfo.ItemUID != request.ItemUID)
                        {
                            vaildResult.Add(string.Format(Resource.COMMON_PKG_NOT_MATCH, itemInfo.ID));
                        }
                    }
                    if (vaildResult.Count() > 0) //確認上面檢查通過才能繼續檢查其它資料
                        continue;
                    //slot 是否存在
                    var slotInfo = this.SlotRepository.GetList(new { UID = request.ModifySlotUID });
                    if (slotInfo.Content.Count() == 0)
                    {
                        vaildResult.Add(Resource.INVENTORY_NOT_FIND_SLOT);
                    }
                    if (checkslotStatusInboundOnly(slotInfo))
                    {
                        vaildResult.Add(Resource.COMMON_ILLEGAL_SLOT_STATUS);
                    }
                    //取得payload data (是否存在)
                    var payloadInfo = this.PayloadRepository.GetPayload(request.PayloadUID);

                    if (payloadInfo.Content != null)
                    {
                        if (payloadInfo.Content.PackageUID != request.ModifyPackageUID)
                        {
                            vaildResult.Add(Resource.WAREHOUSE_MODIFIED_ONHAND_CANNOTCHANGEPKG);


                            if (allowAction.Any(p => p == request.ActionType))
                            {

                            }
                            else
                            {
                                vaildResult.Add(string.Format(Resource.INVENTORY_MODIFY_INCORRENT_ACTION_TYPE,
                                       payloadInfo.Content.ID));
                            }
                        }
                        else
                        {
                            if (request.isNew)//是否為新增資料
                            {

                            }
                            else
                            {
                                rs.Success = false;
                            }
                        }

                    }
                }

                #endregion
                if (vaildResult.Count() == 0)
                {
                    var modifiedRequest = requests.Where(p => p.ActionType == (int)ModifyInventoryActionType.Modified);
                    using (var db = this.DbEntities.DbAdapter)
                    {
                        this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                        foreach (var request in modifiedRequest)
                        {
                            //是否為新增資料
                            if (!request.isNew)
                            {
                                var payloadInfo = this.PayloadRepository.GetPayload(request.PayloadUID);
                                //是否為修改
                                if (request.ActionType == (int)ModifyInventoryActionType.Modified)
                                {
                                    //判斷Transaction Action
                                    judeTransactionAction(request, payloadInfo.Content);

                                    var moditiedInfo = payloadInfo.Content.Clone<IPayloadModel>();


                                    //修改
                                    moditiedInfo.Quantity = request.ModifyQty;
                                    //暫不修改包裝
                                    // moditiedInfo.PackageUID = request.ModifyPackageUID;
                                    moditiedInfo.SlotUID = request.ModifySlotUID;
                                    if (moditiedInfo.Quantity == 0)
                                    {
                                        moditiedInfo.Status = (int)PayloadStatus.Inactive;
                                    }
                                    var mrs = this.PayloadRepository.UpatePayload(moditiedInfo);
                                    result.Add(mrs);

                                    if (result.All(x => x.Success))
                                    {
                                        //ActionType 為修改寫TransactionLog
                                        var logModel = new PayloadTransactionLogInnerModel();
                                        logModel.UID = Guid.NewGuid();
                                        logModel.ItemUID = payloadInfo.Content.ItemUID;
                                        logModel.OriginalPackage = payloadInfo.Content.PackageUID;
                                        logModel.TargetPackage = request.ModifyPackageUID;
                                        logModel.QtyBeforeTX = payloadInfo.Content.Quantity;
                                        logModel.QtyAfterTX = request.ModifyQty;
                                        logModel.OriginalSlotUID = payloadInfo.Content.SlotUID;
                                        logModel.TargetSlotUID = request.ModifySlotUID;
                                        logModel.PayloadUID = payloadInfo.Content.UID;
                                        logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                        //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                                        logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                        logModel.WarehouseUID = request.WarehouseUID;
                                        var rs3 = this.InventoryManager.AddLog(logModel);
                                    }
                                }

                            }
                            else
                            {
                                this.TracingAgent.TransactionInfo.Action = TransactionlogAction.AddInventory;
                                var itemInfo = itemInfos.FirstOrDefault(p => p.UID == request.ItemUID);
                                var pkginfo = this.PackageCacheManager.GetPackage(request.ModifyPackageUID);
                                //新增payload 
                                PayloadInnerModel payload = new PayloadInnerModel();
                                payload.UID = Guid.NewGuid();
                                payload.ID = this.SequenceAgent.GetPayloadSeqenceByTimeSerial(PayloadType.Sakana);
                                payload.ItemUID = request.ItemUID;
                                payload.PackageUID = request.ModifyPackageUID;
                                payload.PODUID = Guid.Empty;
                                payload.Quantity = request.ModifyQty;
                                payload.SlotUID = request.ModifySlotUID;
                                payload.Status = (int)PayloadStatus.Active;
                                payload.Type = (int)PayloadType.Sakana;

                                payload.VolumeLimit = this.ProductUtility.CalculateCUFT(pkginfo, payload.Quantity);
                                payload.WeightLimit = this.ProductUtility.CaculateTTLWeight(pkginfo, payload.Quantity);
                                payload.VesselUID = Guid.Empty;
                                var rs2 = this.PayloadRepository.AddPayload(payload);
                                result.Add(rs2);
                                if (result.All(p => p.Success))
                                {

                                    //ActionType 為修改寫TransactionLog
                                    var logModel = new PayloadTransactionLogInnerModel();
                                    logModel.UID = Guid.NewGuid();
                                    logModel.ItemUID = request.ItemUID;
                                    logModel.OriginalPackage = request.ModifyPackageUID;
                                    logModel.TargetPackage = request.ModifyPackageUID;
                                    logModel.QtyBeforeTX = 0;
                                    logModel.QtyAfterTX = request.ModifyQty;
                                    logModel.OriginalSlotUID = null;
                                    logModel.PayloadUID = payload.UID;
                                    logModel.TargetSlotUID = request.ModifySlotUID;
                                    logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                    //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                                    logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                    logModel.WarehouseUID = request.WarehouseUID;
                                    var rs3 = this.InventoryManager.AddLog(logModel);
                                }

                            }
                        }

                        if (result.All(x => x.Success))
                        {
                            rs.Success = true;
                            rs.Content = true;
                            //scope.Complete();
                            this.DbEntities.Commit();

                        }
                        else
                        {
                            rs.Message = string.Join("\r\n", result.Where(x => !x.Success).Select(p => p.Message));
                        }
                    }
                }
                else
                {
                    rs.Message = string.Join("<BR>", vaildResult.ToArray());
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace($"Occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> CreateAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> requests)
        {
            List<Func<IActionResult<bool>>> syncMethod = new List<Func<IActionResult<bool>>>();
            var allowAction = new int[] {
                (int)ModifyInventoryActionType.Modified,
                (int)ModifyInventoryActionType.Move
            };
            var allowPayloadType = new int[] {
            (int)PayloadType.Stock
            };
            var allowPayloadStatus = new int[] {
            (int)PayloadStatus.Active
            };
            var allocatedPayloadType = new int[] {
            (int)PayloadType.Allocated,
            (int)PayloadType.FutureAllocated,
            (int)PayloadType.BulkPickPending,
            };
            var flawedPayloadType = new int[] {
            (int)PayloadType.Salvage,
            (int)PayloadType.Sample,
            (int)PayloadType.Shrinkage,
            (int)PayloadType.AsIs,

            };
            List<string> vaildResult = new List<string>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<WMSReplicateMoveModel> moveReplicateCollection = new List<WMSReplicateMoveModel>();
                List<WMSReplicateOnhandModel> modifiedReplicateCollection = new List<WMSReplicateOnhandModel>();
                var itemInfos = this.ProductCacheManager.GetItems(requests.Select(x => x.ItemUID));
                IActionResult<IWarehouseModel> warehouseInfo = null;
                #region Check data
                foreach (var request in requests)
                {

                    warehouseInfo = this.WarehouseManager.GetWarehouse(request.WarehouseUID);
                    if (requests.GroupBy(g => g.WarehouseUID).Count() > 1)
                    {
                        vaildResult.Add(Resource.WAREHOSUE_MOVE_PAYLAOD_MULTIWAREHOUSE_REQUEST);
                    }
                    if (warehouseInfo.Content == null)
                    {
                        vaildResult.Add(Resource.WAREHOUSE_NOT_FIND);
                    }
                    var itemInfo = itemInfos.FirstOrDefault(p => p.UID == request.ItemUID);
                    var pkgInfo = this.PackageCacheManager.GetPackage(request.ModifyPackageUID);

                    if (itemInfo == null)
                    {
                        vaildResult.Add(Resource.COMMON_NOT_FIND_ITEM);
                    }
                    if (pkgInfo == null)
                    {
                        vaildResult.Add(string.Format(Resource.COMMON_NOT_FIND_PACKAGE, itemInfo.ID));
                    }
                    else
                    {
                        if (pkgInfo.ItemUID != request.ItemUID)
                        {
                            vaildResult.Add(string.Format(Resource.COMMON_PKG_NOT_MATCH, itemInfo.ID));
                        }
                    }
                    if (vaildResult.Count() > 0) //確認上面檢查通過才能繼續檢查其它資料
                        continue;
                    //slot 是否存在
                    var slotInfo = this.SlotRepository.GetList(new { UID = request.ModifySlotUID });
                    if (slotInfo.Content.Count() == 0)
                    {
                        vaildResult.Add(Resource.INVENTORY_NOT_FIND_SLOT);
                    }
                    else
                    {
                        if (slotInfo.Content.All(x => x.WarehouseUID != request.WarehouseUID))
                        {
                            vaildResult.Add(Resource.COMMON_WAREHOSUE_SLOT_DIFFERENCE);
                        }
                    }
                    if (checkslotStatusInboundOnly(slotInfo))
                    {
                        vaildResult.Add(Resource.COMMON_ILLEGAL_SLOT_STATUS);
                    }
                    //取得payload data (是否存在)
                    var payloadInfo = this.PayloadRepository.GetPayload(request.PayloadUID);

                    if (payloadInfo.Content != null)
                    {

                        // 狀態不是Acitve 會不能調整
                        if (payloadInfo.Content.Status != (int)PayloadStatus.Active)
                        {
                            vaildResult.Add(Resource.COMMON_ILLEGAL_PAYLOAD_STATUS);
                        }

                        if (payloadInfo.Content.PackageUID != request.ModifyPackageUID)
                        {
                            vaildResult.Add(Resource.WAREHOUSE_MODIFIED_ONHAND_CANNOTCHANGEPKG);


                            if (allowAction.Any(p => p == request.ActionType))
                            {
                                if (request.ActionType == (int)ModifyInventoryActionType.Move)
                                {
                                    var payloadPkgDepth = this.PackageCacheManager.FindPackageDepthIndex(payloadInfo.Content.PackageUID);
                                    var modifyPkgDepth = this.PackageCacheManager.FindPackageDepthIndex(request.ModifyPackageUID);
                                    var calIndex = this.PackageCacheManager.GetReceivePackageUomQuantity
                                               (payloadInfo.Content.PackageUID, request.ModifyPackageUID, 1);
                                    if (request.ModifyPackageUID != payloadInfo.Content.PackageUID)
                                    {
                                        //check 包裝是否有效 (小->大)
                                        if (modifyPkgDepth < payloadPkgDepth) //包裝(小->大)檢查是否整除
                                        {

                                            if (request.ModifyQty % calIndex.Content != 0)
                                            {
                                                vaildResult.Add(
                                                    string.Format(Resource.INVENTORY_MODIFY_QTY_UNABLE_CALCULATE, payloadInfo.Content.ID));
                                            }
                                        }
                                    }
                                    //移動Qty 是否為0
                                    if (request.ModifyQty == 0)
                                    {
                                        vaildResult.Add(Resource.INVENTORY_MODIFY_MOVE_CANNOT_ZERO);
                                    }
                                    //payload qty 是否足夠
                                    if (request.ModifyPackageUID != payloadInfo.Content.PackageUID)
                                    {
                                        var payloadqty = this.PackageCacheManager
                                          .GetReceivePackageUomQuantity(payloadInfo.Content.PackageUID,
                                             this.PackageCacheManager.GetMinPackage(payloadInfo.Content.PackageUID).UID,
                                              payloadInfo.Content.Quantity);
                                        var modifiedQty = this.PackageCacheManager
                                           .GetReceivePackageUomQuantity(payloadInfo.Content.PackageUID,
                                              this.PackageCacheManager.GetMinPackage(payloadInfo.Content.PackageUID).UID,
                                               payloadInfo.Content.Quantity);
                                        if (payloadqty.Content < modifiedQty.Content)
                                        {
                                            vaildResult.Add(string.Format(Resource.INVENTORY_MODIFY_INVENTORY_QTY_INSUFFICIENT,
                                            payloadInfo.Content.ID));

                                        }

                                    }
                                    else
                                    {
                                        if (payloadInfo.Content.Quantity < request.ModifyQty)
                                        {
                                            vaildResult.Add(string.Format(Resource.INVENTORY_MODIFY_INVENTORY_QTY_INSUFFICIENT,
                                                payloadInfo.Content.ID));
                                        }
                                    }
                                    if (request.ModifySlotUID == payloadInfo.Content.SlotUID)
                                    {
                                        vaildResult.Add(string.Format(Resource.INVENTORY_SLOT_THE_SAME,
                                                payloadInfo.Content.ID));
                                    }
                                    if (request.ModifySlotUID == Guid.Empty)
                                    {
                                        vaildResult.Add(string.Format(Resource.INVENTORY_SLOT_CANNOT_EMPTY,
                                                payloadInfo.Content.ID));
                                    }

                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                rs.Success = false;
                                vaildResult.Add(string.Format(Resource.INVENTORY_MODIFY_INCORRENT_ACTION_TYPE,
                                       payloadInfo.Content.ID));
                            }
                        }
                        else
                        {
                            if (request.isNew)//是否為新增資料
                            {

                            }
                            else
                            {
                                rs.Success = false;
                            }
                        }

                    }
                }
                //check virtual item
                var ActualProductGrp = requests.GroupBy(g => new
                {
                    ActualProduct = itemInfos.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct,
                    Slotuid = g.ModifySlotUID,
                    isNew = g.isNew
                });
                foreach (var apgitem in ActualProductGrp)
                {
                    if (!string.IsNullOrEmpty(apgitem.Key.ActualProduct))
                    {
                        var _iteminfo = itemInfos.FirstOrDefault(x => x.UID == apgitem.FirstOrDefault().ItemUID);
                        var combineItems = apgitem.Select(p =>
                        {
                            var vi = new VirtualItemInfo();
                            var citeminfo = itemInfos.FirstOrDefault(x => x.UID == p.ItemUID);
                            vi.ActualProduct = citeminfo.ActualProduct;
                            vi.ProductId = citeminfo.ID;
                            vi.Quantity = p.ModifyQty;
                            vi.ProductUID = citeminfo.UID;
                            vi.CustomerUID = new Guid(citeminfo.CustomerUID);
                            vi.PUOM = citeminfo.PUOM;
                            return vi;
                        });
                        var combineItem = this.ProductCacheManager.NewCombineToActualItem(combineItems);
                        if (!combineItem.Success)
                        {
                            //var virtualitems = this.ProductCacheManager.GetVirtualItemsByCache(apgitem.Key.ActualProduct,
                            //    new Guid[] { new Guid(_iteminfo.CustomerUID) }, this.GetGroupUserViewByUser().Content);
                            //vaildResult.Add(string.Format(Resource.COMMON_VIRTUAL_ITEM_CHECK_INVALID,
                            //    apgitem.Key.ActualProduct, string.Join(",", virtualitems.Select(p => p.ID))));
                            vaildResult.Add(string.Format(Resource.COMMON_VIRTUAL_ITEM_CHECK_INVALID,
                               apgitem.Key.ActualProduct));
                        }

                    }
                }
                #endregion
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);


                    var leglPayloadStatus = new int[] { (int)PayloadStatus.Active, (int)PayloadStatus.OffPosition };
                    if (vaildResult.Count() == 0)
                    {
                        #region allocated model paramters init
                        var provider = new AutoAssignAgentProviders()
                        {
                            WorkOrderAssignAgentParameters = this.GetWorkOrderAgentParameters(),
                        };
                        #endregion
                        var result = new List<IActionResult<bool>>();
                        var modifiedRequest = requests.Where(p => p.ActionType == (int)ModifyInventoryActionType.Modified);
                        var moveRequest = requests.Where(p => p.ActionType == (int)ModifyInventoryActionType.Move);
                        List<ICreateAdjustmentTicketRequest> virtualItmes = new List<ICreateAdjustmentTicketRequest>();
                        foreach (var request in modifiedRequest)
                        {
                            //是否為新增資料
                            if (!request.isNew)
                            {
                                var payloadInfo = this.PayloadRepository.GetPayload(request.PayloadUID);
                                //是否為修改
                                if (request.ActionType == (int)ModifyInventoryActionType.Modified)
                                {
                                    //判斷Transaction Action
                                    judeTransactionAction(request, payloadInfo.Content);

                                    var moditiedInfo = payloadInfo.Content.Clone<IPayloadModel>();

                                    //取得修改前  wms inventory 資料
                                    var sourceinvInfo = this.InventoryRepository.GetList(new
                                    {
                                        ItemUID = payloadInfo.Content.ItemUID,
                                        SlotUID = payloadInfo.Content.SlotUID,
                                        Type = payloadInfo.Content.Type
                                    });


                                    if (sourceinvInfo.Content.Count() == 0)
                                    {
                                        rs.Content = false;
                                        rs.Message = $"not find item#{payloadInfo.Content.ItemUID} in Slot#{payloadInfo.Content.SlotUID} onhand data.";
                                        return rs;
                                    }
                                    //修改
                                    moditiedInfo.Quantity = request.ModifyQty;
                                    //暫不修改包裝
                                    // moditiedInfo.PackageUID = request.ModifyPackageUID;
                                    moditiedInfo.SlotUID = request.ModifySlotUID;
                                    if (moditiedInfo.Quantity == 0)
                                    {
                                        moditiedInfo.Status = (int)PayloadStatus.Inactive;
                                    }
                                    var mrs = this.PayloadRepository.UpatePayload(moditiedInfo);
                                    result.Add(mrs);
                                    if (mrs.Success)
                                    {
                                        //重新計算該item ,Slot 上來源onhand
                                        var aftermodifiedBelongToPayload = this.PayloadRepository.GetListWithOriginalPayloadType(
                                           payloadInfo.Content.ItemUID, payloadInfo.Content.SlotUID
                                        );
                                        //當修改onhand 為Stock 時重新計算onhand 時需包含Allocated 資料  2021/11/8
                                        if (payloadInfo.Content.Type == (int)PayloadType.Stock)
                                        {
                                            aftermodifiedBelongToPayload.Content = aftermodifiedBelongToPayload.Content.Where(p =>
                                                (allocatedPayloadType.Any(x => p.Type == x) || allowPayloadType.Any(o => o == p.Type))
                                                && (p.Type != (int)PayloadType.Allocated ||
                                                    (p.Type == (int)PayloadType.Allocated &&
                                                        p.OriginalPayloadType == payloadInfo.Content.Type)));
                                        }
                                        else
                                        {
                                            //
                                            aftermodifiedBelongToPayload.Content = aftermodifiedBelongToPayload.Content.Where(p => p.Type == payloadInfo.Content.Type);
                                        }
                                        var calPayloadOnhand = 0;
                                        if (aftermodifiedBelongToPayload.Content.Count() > 0)
                                        {
                                            foreach (var item in aftermodifiedBelongToPayload.Content)
                                            {
                                                if (leglPayloadStatus.Any(p => p == item.Status))
                                                {
                                                    bool isnegative = false;
                                                    if (item.Quantity < 0)
                                                    {
                                                        isnegative = true;
                                                        item.Quantity = Math.Abs(item.Quantity);
                                                    }
                                                    //遇到負onhand 的處理

                                                    var addqty = this.PackageCacheManager
                                                        .GetReceivePackageUomQuantity(item.PackageUID,
                                                       this.PackageCacheManager.GetMinPackage(item.PackageUID).UID,
                                                        item.Quantity).Content;
                                                    if (isnegative)
                                                    {
                                                        addqty *= -1;
                                                    }
                                                    calPayloadOnhand += addqty;

                                                }
                                            }
                                        }
                                        else //沒有任何 payload 
                                        {

                                        }
                                        //計算同步給pbsc的onhand 差異值
                                        //                  修改後onhand        修改前                                                        
                                        var syncorgconhand = calPayloadOnhand - sourceinvInfo.Content.Sum(p => p.Qty);
                                        //計算onhand (扣除與payload包裝不同版本的onhand)
                                        var orgconhand = Math.Abs(calPayloadOnhand - sourceinvInfo.Content
                                               .Where(p => p.PackageUID != this.PackageCacheManager
                                               .GetMinPackage(payloadInfo.Content.PackageUID).UID).Sum(p => p.Qty));
                                        //重新計算該item ,Slot 上來源onhand
                                        result.Add(this.InventoryRepository
                                               .DeleteInventory(sourceinvInfo.Content.Where(p =>
                                               p.PackageUID == this.PackageCacheManager.GetMinPackage(payloadInfo.Content.PackageUID).UID)
                                               .Select(x => x.UID)));
                                        var orgohandparam = new AddOnhandInnerParameters();
                                        orgohandparam.Onhand = orgconhand;
                                        orgohandparam.SlotUID = payloadInfo.Content.SlotUID;
                                        orgohandparam.TargetPackageUID = this.PackageCacheManager
                                                        .GetMinPackage(payloadInfo.Content.PackageUID).UID;
                                        orgohandparam.WarehouseUID = request.WarehouseUID;
                                        orgohandparam.ItemUID = payloadInfo.Content.ItemUID;
                                        orgohandparam.Type = (InventoryType)payloadInfo.Content.Type;

                                        result.Add(this.InventoryManager.ProcessAddInventory(orgohandparam));
                                        //Slot 是否相同
                                        if (payloadInfo.Content.SlotUID != request.ModifySlotUID)
                                        {
                                            //重新計算該item ,Slot 上目的地的onhand
                                            var targetinvInfo = this.InventoryRepository.GetList(new
                                            {
                                                ItemUID = payloadInfo.Content.ItemUID,
                                                SlotUID = request.ModifySlotUID,
                                                Type = payloadInfo.Content.Type
                                            });
                                            var targetBelongToPayload = this.PayloadRepository.GetListWithOriginalPayloadType(
                                          payloadInfo.Content.ItemUID, request.ModifySlotUID);
                                            if (payloadInfo.Content.Type == (int)PayloadType.Stock)
                                            {
                                                targetBelongToPayload.Content = targetBelongToPayload.Content.Where(p =>
                                                (allocatedPayloadType.Any(x => p.Type == x) || allowPayloadType.Any(o => o == p.Type))
                                                && (p.Type != (int)PayloadType.Allocated ||
                                                    (p.Type == (int)PayloadType.Allocated &&
                                                        p.OriginalPayloadType == payloadInfo.Content.Type)));
                                            }
                                            var caltargetPayloadOnhand = 0;
                                            if (targetBelongToPayload.Content.Count() > 0)
                                            {
                                                foreach (var item in targetBelongToPayload.Content)
                                                {
                                                    if (leglPayloadStatus.Any(p => p == item.Status))
                                                    {

                                                        bool isnegative = false;
                                                        if (item.Quantity < 0)
                                                        {
                                                            isnegative = true;
                                                            item.Quantity = Math.Abs(item.Quantity);
                                                        }

                                                        var addqty = this.PackageCacheManager
                                                            .GetReceivePackageUomQuantity(item.PackageUID,
                                                           this.PackageCacheManager.GetMinPackage(item.PackageUID).UID,
                                                            item.Quantity).Content;
                                                        if (isnegative)
                                                        {
                                                            addqty *= -1;
                                                        }
                                                        caltargetPayloadOnhand += addqty;


                                                    }
                                                }
                                            }
                                            if (targetinvInfo.Content.Count() > 0)
                                            {
                                                result.Add(this.InventoryRepository
                                                   .DeleteInventory(targetinvInfo.Content
                                                   .Where(p =>
                                                    p.PackageUID == this.PackageCacheManager.GetMinPackage(moditiedInfo.PackageUID).UID)
                                                   .Select(x => x.UID)));
                                            }
                                            //計算同步給pbsc的onhand 差異值
                                            var targetsynconhand = caltargetPayloadOnhand - targetinvInfo.Content.Sum(p => p.Qty);
                                            //計算onhand (扣除與payload包裝不同版本的onhand)
                                            var targetonhand = Math.Abs(caltargetPayloadOnhand - targetinvInfo.Content
                                                .Where(p => p.PackageUID != this.PackageCacheManager
                                                .GetMinPackage(payloadInfo.Content.PackageUID).UID).Sum(p => p.Qty));
                                            var targetohandparam = new AddOnhandInnerParameters();
                                            targetohandparam.Onhand = targetonhand;
                                            targetohandparam.SlotUID = request.ModifySlotUID;
                                            targetohandparam.TargetPackageUID = this.PackageCacheManager
                                                                                .GetMinPackage(payloadInfo.Content.PackageUID).UID;
                                            targetohandparam.WarehouseUID = request.WarehouseUID;
                                            targetohandparam.ItemUID = payloadInfo.Content.ItemUID;
                                            targetohandparam.Type = (InventoryType)payloadInfo.Content.Type.Value;
                                            result.Add(this.InventoryManager.ProcessAddInventory(targetohandparam));

                                            //同步原本payload onhand
                                            var orgrepliatedata = new WMSReplicateOnhandModel();
                                            orgrepliatedata.ItemUID = payloadInfo.Content.ItemUID;
                                            orgrepliatedata.PayloadUID = payloadInfo.Content.UID;
                                            orgrepliatedata.Quantity = syncorgconhand;
                                            orgrepliatedata.SlotUID = payloadInfo.Content.SlotUID;
                                            orgrepliatedata.PayloadType = (int)payloadInfo.Content.Type.Value;
                                            modifiedReplicateCollection.Add(orgrepliatedata);
                                            //同步目的payload onhand

                                            var targetrepliatedata = new WMSReplicateOnhandModel();
                                            targetrepliatedata.ItemUID = moditiedInfo.ItemUID;
                                            targetrepliatedata.PayloadUID = moditiedInfo.UID;
                                            targetrepliatedata.Quantity = targetsynconhand;
                                            targetrepliatedata.SlotUID = moditiedInfo.SlotUID;
                                            targetrepliatedata.PayloadType = payloadInfo.Content.Type.Value;
                                            modifiedReplicateCollection.Add(targetrepliatedata);
                                        }
                                        else
                                        {
                                            var repliateonhanddata = new WMSReplicateOnhandModel();
                                            repliateonhanddata.ItemUID = payloadInfo.Content.ItemUID;
                                            repliateonhanddata.PayloadUID = payloadInfo.Content.UID;
                                            repliateonhanddata.Quantity = syncorgconhand;
                                            repliateonhanddata.SlotUID = request.ModifySlotUID;
                                            repliateonhanddata.PayloadType = (int)payloadInfo.Content.Type.Value;
                                            modifiedReplicateCollection.Add(repliateonhanddata);
                                        }

                                    }
                                    if (result.All(x => x.Success))
                                    {
                                        //ActionType 為修改寫TransactionLog
                                        var logModel = new PayloadTransactionLogInnerModel();
                                        logModel.UID = Guid.NewGuid();
                                        logModel.ItemUID = payloadInfo.Content.ItemUID;
                                        logModel.OriginalPackage = payloadInfo.Content.PackageUID;
                                        logModel.TargetPackage = request.ModifyPackageUID;
                                        logModel.QtyBeforeTX = payloadInfo.Content.Quantity;
                                        logModel.QtyAfterTX = request.ModifyQty;
                                        logModel.OriginalSlotUID = payloadInfo.Content.SlotUID;
                                        logModel.TargetSlotUID = request.ModifySlotUID;
                                        logModel.PayloadUID = payloadInfo.Content.UID;
                                        logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                        //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                                        logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                        logModel.WarehouseUID = request.WarehouseUID;
                                        var rs3 = this.InventoryManager.AddLog(logModel);
                                    }
                                }

                            }
                            else
                            {
                                this.TracingAgent.TransactionInfo.Action = TransactionlogAction.AddInventory;
                                var itemInfo = itemInfos.FirstOrDefault(p => p.UID == request.ItemUID);
                                //處理非虛擬item 
                                if (string.IsNullOrEmpty(itemInfo.ActualProduct))
                                {
                                    var vitems = this.ProductCacheManager.GetVirtualItems(itemInfo.Name, new Guid[] { new Guid(itemInfo.CustomerUID) });
                                    if (vitems.Content?.Count() > 0)
                                    {
                                        foreach (var item in vitems.Content)
                                        {
                                            var pkgs = this.PackageCacheManager.GetPackagesByItem(item.UID).OrderByDescending(o => o.CreatedOn);
                                            var pkg = pkgs.First();
                                            var parm = new AddOnhandInnerParameters();
                                            parm.ItemUID = item.UID;
                                            parm.Onhand = request.ModifyQty;
                                            parm.SlotUID = request.ModifySlotUID;
                                            parm.TargetPackageUID = pkg.UID;
                                            parm.WarehouseUID = request.WarehouseUID;
                                            parm.isPauseSync = true;
                                            parm.Type = InventoryType.Stock;
                                            var addRs = this.InventoryManager.ProcessAddInventory(parm, isAddPayload: true) as IExtensionActionResult<bool>;
                                            result.Add(addRs);

                                            var logModel = new PayloadTransactionLogInnerModel();
                                            logModel.UID = Guid.NewGuid();
                                            logModel.ItemUID = item.UID;
                                            logModel.OriginalPackage = request.ModifyPackageUID;
                                            logModel.TargetPackage = request.ModifyPackageUID;
                                            logModel.QtyBeforeTX = 0;
                                            logModel.QtyAfterTX = request.ModifyQty;
                                            logModel.OriginalSlotUID = null;
                                            logModel.TargetSlotUID = request.ModifySlotUID;
                                            logModel.PayloadUID = addRs.GetReturnValue<Guid>("NewPayloadUID");
                                            logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                            logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                            logModel.WarehouseUID = request.WarehouseUID;
                                            var rs3 = this.InventoryManager.AddLog(logModel);
                                        }
                                        //同步onhand 資料
                                        if (result.All(p => p.Success))
                                        {
                                            var repliateonhanddata = new WMSReplicateOnhandModel();
                                            repliateonhanddata.ItemUID = request.ItemUID;
                                            repliateonhanddata.Quantity = request.ModifyQty;
                                            repliateonhanddata.SlotUID = request.ModifySlotUID;
                                            repliateonhanddata.PayloadType = (int)InventoryType.Stock;
                                            modifiedReplicateCollection.Add(repliateonhanddata);
                                        }
                                    }
                                    else //request 不是虛擬item則直接新增
                                    {

                                        //新增payload
                                        var parm = new AddOnhandInnerParameters();
                                        parm.ItemUID = request.ItemUID;
                                        parm.Onhand = request.ModifyQty;
                                        parm.SlotUID = request.ModifySlotUID;
                                        parm.TargetPackageUID = request.ModifyPackageUID;
                                        parm.WarehouseUID = request.WarehouseUID;
                                        parm.Type = InventoryType.Stock;
                                        parm.isPauseSync = true;
                                        var addRs = this.InventoryManager.ProcessAddInventory(parm, isAddPayload: true);
                                        result.Add(addRs);
                                        if (result.All(p => p.Success))
                                        {
                                            var minpkg = this.PackageCacheManager.GetMinPackage(request.ModifyPackageUID);
                                            var synconhand = this.PackageCacheManager.GetReceivePackageUomQuantity(request.ModifyPackageUID,
                                                minpkg.UID, request.ModifyQty);
                                            var extaddRs = addRs as ExtensionActionResultContainer<bool>;
                                            var newpayloaduid = extaddRs.GetReturnValue<Guid>("NewPayloadUID");
                                            //同步onhand 資料
                                            var repliateonhanddata = new WMSReplicateOnhandModel();
                                            repliateonhanddata.ItemUID = request.ItemUID;
                                            repliateonhanddata.Quantity = synconhand.Content;
                                            repliateonhanddata.SlotUID = request.ModifySlotUID;
                                            repliateonhanddata.PayloadUID = newpayloaduid;
                                            repliateonhanddata.PayloadType = (int)InventoryType.Stock;
                                            modifiedReplicateCollection.Add(repliateonhanddata);
                                            //ActionType 為修改寫TransactionLog
                                            var logModel = new PayloadTransactionLogInnerModel();
                                            logModel.UID = Guid.NewGuid();
                                            logModel.ItemUID = request.ItemUID;
                                            logModel.OriginalPackage = request.ModifyPackageUID;
                                            logModel.TargetPackage = request.ModifyPackageUID;
                                            logModel.QtyBeforeTX = 0;
                                            logModel.QtyAfterTX = request.ModifyQty;
                                            logModel.OriginalSlotUID = null;
                                            logModel.PayloadUID = newpayloaduid;
                                            logModel.TargetSlotUID = request.ModifySlotUID;
                                            logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                            //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                                            logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                            logModel.WarehouseUID = request.WarehouseUID;
                                            var rs3 = this.InventoryManager.AddLog(logModel);
                                        }
                                    }
                                }
                                else
                                {
                                    virtualItmes.Add(request);
                                }
                            }

                        }
                        //處理虛擬item 
                        if (virtualItmes.Count > 0)
                        {
                            this.TracingAgent.TransactionInfo.Action = TransactionlogAction.AddInventory;
                            var combineItem = this.ProductCacheManager.NewCombineToActualItem(virtualItmes.Select(p =>
                            {
                                var vi = new VirtualItemInfo();
                                var subiteminfo = itemInfos.FirstOrDefault(x => x.UID == p.ItemUID);
                                vi.ActualProduct = subiteminfo.ActualProduct;
                                vi.ProductId = subiteminfo.ID;
                                vi.Quantity = p.ModifyQty;
                                vi.ProductUID = subiteminfo.UID;
                                vi.PUOM = subiteminfo.PUOM;
                                vi.CustomerUID = new Guid(subiteminfo.CustomerUID);
                                return vi;
                            }));
                            if (combineItem.Success)
                            {
                                //新增payload
                                var lastestpkg = this.PackageCacheManager.GetPackagesByItem(combineItem.Content.UID)
                                    .OrderByDescending(o => o.CreatedOn).FirstOrDefault();
                                var minpkg = this.PackageCacheManager.GetMinPackage(lastestpkg.UID);
                                var parm = new AddOnhandInnerParameters();
                                parm.ItemUID = combineItem.Content.UID;
                                parm.Onhand = virtualItmes.FirstOrDefault().ModifyQty;
                                parm.SlotUID = virtualItmes.FirstOrDefault().ModifySlotUID;
                                parm.TargetPackageUID = minpkg.UID;
                                parm.WarehouseUID = virtualItmes.FirstOrDefault().WarehouseUID;
                                parm.isPauseSync = true;
                                parm.Type = InventoryType.Stock;
                                //(this.InventoryManager as AbstractManager).SetTransactionObject(this.TransacationScopeObject);

                                result.Add(this.InventoryManager.ProcessAddInventory(parm, isAddPayload: true));
                                if (result.All(p => p.Success))
                                {
                                    //同步onhand 資料
                                    var repliateonhanddata = new WMSReplicateOnhandModel();
                                    repliateonhanddata.ItemUID = combineItem.Content.UID;
                                    repliateonhanddata.Quantity = virtualItmes.FirstOrDefault().ModifyQty;
                                    repliateonhanddata.SlotUID = virtualItmes.FirstOrDefault().ModifySlotUID;
                                    repliateonhanddata.PayloadType = (int)InventoryType.Stock;
                                    modifiedReplicateCollection.Add(repliateonhanddata);
                                    var logModel = new PayloadTransactionLogInnerModel();
                                    logModel.UID = Guid.NewGuid();
                                    logModel.ItemUID = combineItem.Content.UID;
                                    logModel.OriginalPackage = minpkg.UID;
                                    logModel.TargetPackage = minpkg.UID;
                                    logModel.QtyBeforeTX = 0;
                                    logModel.QtyAfterTX = virtualItmes.FirstOrDefault().ModifyQty;
                                    logModel.OriginalSlotUID = null;
                                    logModel.TargetSlotUID = virtualItmes.FirstOrDefault().ModifySlotUID;
                                    logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                    //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                                    logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                    logModel.WarehouseUID = virtualItmes.FirstOrDefault().WarehouseUID;
                                    var rs3 = this.InventoryManager.AddLog(logModel);
                                }
                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = Resource.COMMON_VIRTUALITEM_COMBINE_FAILURE;
                            }
                        }
                        if (moveRequest.Count() > 0)
                        {
                            var sparam = GetStatusManageAgentParamters();

                            var workOrderStatusManageAgent = new WorkOrderStatusManageAgent(sparam);
                            var ticketStatusManageAgent = new TicketStatusManageAgent(sparam);
                            var agent = AbstractWorkOrderAssignAgent.GetAgent(ManifestType.Move,
                                                                            provider.WorkOrderAssignAgentParameters);
                            var converter = new AssignedParameterConverter();
                            //var completeTicket = moveRequest.Where(p => p.CompleteAction);
                            //var noncompleteTicket = moveRequest.Where(p => !p.CompleteAction);
                            #region Ticket not complete
                            if (moveRequest.Count() > 0)
                            {
                                var nonTicketWO = new AssignedMoveWorkOrderCollection();
                                nonTicketWO.Items = new List<IAssignedOutboundWorkOrderPayload>();
                                nonTicketWO.ServiceType = ManifestType.Move;
                                nonTicketWO.VesselUID = Guid.Empty;
                                var moveRequestGrp = moveRequest.GroupBy(g => new
                                {
                                    ActualProductID =
                                   itemInfos.FirstOrDefault(x => x.UID == g.ItemUID).ActualProduct
                                });
                                foreach (var req in moveRequestGrp)
                                {
                                    if (string.IsNullOrEmpty(req.Key.ActualProductID))
                                    {
                                        foreach (var request in req)
                                        {
                                            var payloadModel = new AssignedMoveWorkOrderPayload()
                                            {
                                                PayloadUID = request.PayloadUID,
                                                ItemUID = request.ItemUID,
                                                VesselMainifestUID = Guid.Empty,
                                                AllocatedQty = request.ModifyQty,
                                                SlotUID = request.ModifySlotUID,
                                                PickPackageUID = request.ModifyPackageUID
                                            };
                                            nonTicketWO.Items.Add(payloadModel);
                                        }

                                    }
                                    else
                                    {
                                        var _itemGrp = Guid.NewGuid();
                                        foreach (var request in req)
                                        {
                                            var payloadModel = new AssignedMoveWorkOrderPayload()
                                            {
                                                PayloadUID = request.PayloadUID,
                                                ItemUID = request.ItemUID,
                                                ItemGroupUID = _itemGrp,
                                                VesselMainifestUID = Guid.Empty,
                                                AllocatedQty = request.ModifyQty,
                                                SlotUID = request.ModifySlotUID,
                                                PickPackageUID = request.ModifyPackageUID
                                            };
                                            nonTicketWO.Items.Add(payloadModel);
                                        }
                                    }
                                }
                                var workOrder = converter.MoveParameterConvert(nonTicketWO);
                                var wresult = agent.Execute(workOrder);
                                if (wresult.Success)
                                {
                                    if (wresult.Content.WorkOrderUID != Guid.Empty)
                                    {
                                        var workOrderpayload = this.WorkOrderPayloadRepository
                                                        .GetList(new { workOrderUID = wresult.Content });
                                        if (workOrderpayload.Content.Count() > 0)
                                        {
                                            var workorderPod = new WorkOrderPodInnerModel();
                                            workorderPod.PodUID = Guid.NewGuid();
                                            workorderPod.UID = Guid.NewGuid();
                                            workorderPod.ID = this.SequenceAgent.GetWorkOrderPodSeqenceByTimeSerial(ManifestType.Move);
                                            workorderPod.Name = "";
                                            workorderPod.Type = workOrder.StorageMethod;
                                            workorderPod.WorkOrderUID = wresult.Content.WorkOrderUID;
                                            workorderPod.Status = (int)WorkOrderPodStatus.Open;
                                            workorderPod.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                                            workorderPod.Weight = workOrderpayload.Content.Sum(p => p.Weight);
                                            workorderPod.Volume = workOrderpayload.Content.Sum(p => p.Volume);
                                            var addPodResult = this.WorkOrderPodRepository.AddWorkOrderPod(workorderPod);
                                            if (addPodResult.Success)
                                            {
                                                var assignpodResult = this.WorkOrderPayloadRepository
                                                    .AssignedPayloadtoPod(workorderPod.UID, workOrderpayload.Content.Select(p => p.UID));
                                                if (assignpodResult.Success)
                                                {

                                                }
                                                else
                                                {
                                                    var failure = ActionResultTemplates.Result<bool>();
                                                    failure.Success = false;
                                                    failure.Message = "mapping workorder pod failure.";
                                                    result.Add(failure);
                                                }
                                            }
                                            else
                                            {
                                                var failure = ActionResultTemplates.Result<bool>();
                                                failure.Success = false;
                                                failure.Message = "Not find workorder pod.";
                                                result.Add(failure);
                                            }
                                        }
                                        //generate ticket
                                        var param = new TicketGenerateInnerParameter();
                                        param.WorkOrderUID = wresult.Content.WorkOrderUID;
                                        param.WarehouseUID = warehouseInfo.Content.UID;
                                        param.ForceOpen = true;
                                        var rsGenerateTicket = this.TicketManager.GeneratreTicket(param);
                                        result.Add(rsGenerateTicket);
                                        //assigned ticket
                                        if (result.All(x => x.Success))
                                        {
                                            var ticketInfos = this.TicketRepository.GetTicketInfoListByWorkOrderUID(wresult.Content.WorkOrderUID);
                                            if (ticketInfos.Content != null && ticketInfos.Content.Count() > 0)
                                            {
                                                //assigned ticket
                                                var groups = this.GetGroupUserViewByUser();
                                                var groupsInfo = DrKnowAll.GetGroup(groups.Content.Select(p => p.GroupUID));
                                                groupsInfo = groupsInfo.Where(x => x.Type == (int)GroupTypes.Team);
                                                var mparam = new MaintainWorkderInnerParameters();
                                                mparam.GroupUID = groupsInfo.Select(p => p.UID).ToArray();
                                                mparam.TicketInfoUID = ticketInfos.Content.Select(p => p.UID).ToArray();
                                                result.Add(this.TicketManager.AddWorkder(mparam));
                                            }
                                            else
                                            {
                                                var failure = ActionResultTemplates.Result<bool>();
                                                failure.Success = false;
                                                failure.Message = Resource.TICKET_NOT_FIND_TICKETINFO;
                                                result.Add(failure);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var failure = ActionResultTemplates.Result<bool>();
                                        failure.Success = false;
                                        failure.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER;
                                        result.Add(failure);
                                    }
                                }
                            }
                            #endregion
                        }


                        if (result.All(x => x.Success))
                        {
                            rs.Success = true;
                            rs.Content = true;
                            //scope.Complete();
                            this.DbEntities.Commit();
                            if (result.All(x => x.Success) && moveReplicateCollection.Count > 0)
                                syncMethod.Add(() => this.ReplicationManager.Move(moveReplicateCollection));
                            if (result.All(x => x.Success) && modifiedReplicateCollection.Count > 0)
                                syncMethod.Add(() => this.ReplicationManager.ModifiedOnhand(modifiedReplicateCollection));
                        }
                        else
                        {
                            rs.Message = string.Join("\r\n", result.Where(x => !x.Success).Select(p => p.Message));
                        }
                    }
                    else
                    {
                        rs.Message = string.Join("<BR>", vaildResult.ToArray());
                    }
                    //}
                }
                this.DbEntities.InitConnection();
                if (syncMethod.Count > 0)
                {
                    foreach (var item in syncMethod)
                    {
                        this.TracingAgent.Trace($"CreateAdjustmentTicket sync method {item.Method.Name}");
                        var syncRs = item.Invoke();
                        this.TracingAgent.Trace($"CreateAdjustmentTicket sync method Result:{syncRs.Success} Message:{syncRs.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace($"Occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        private bool checkslotStatusInboundOnly(IActionResult<IEnumerable<ISlotModel>> slotInfo)
        {
            return slotInfo.Content.Any(x => x.Status == (int)SlotStatus.In);
            //return false;
        }

        public IActionResult<bool> SetGroupMoveAdjustment(IEnumerable<ISetGroupMoveAdjustmentRequest> requests)
        {
            List<Func<IActionResult<bool>>> syncMethod = new List<Func<IActionResult<bool>>>();
            var allowAction = new int[] {
                (int)ModifyInventoryActionType.Modified,
                (int)ModifyInventoryActionType.Move
            };
            var allowPayloadType = new int[] {
            (int)PayloadType.Stock
            };
            var allocatedPayloadType = new int[] {
            (int)PayloadType.Allocated,
            (int)PayloadType.FutureAllocated,
            (int)PayloadType.BulkPickPending,
            };
            var flawedPayloadType = new int[] {
            (int)PayloadType.Salvage,
            (int)PayloadType.Sample,
            (int)PayloadType.Shrinkage,
            (int)PayloadType.AsIs,

            };
            List<string> vaildResult = new List<string>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<WMSReplicateMoveModel> moveReplicateCollection = new List<WMSReplicateMoveModel>();
                List<WMSReplicateOnhandModel> modifiedReplicateCollection = new List<WMSReplicateOnhandModel>();
                var itemInfos = this.ProductCacheManager.GetItems(requests.Select(x => x.TargetItemUID));
                IActionResult<IWarehouseModel> warehouseInfo = null;
                #region Check data
                foreach (var request in requests)
                {
                    var SourcePayloadList = this.PayloadRepository.GetList(new { Status = (int)PayloadStatus.Active, UID = request.SourcePayloadUIDList }).Content;
                    warehouseInfo = this.WarehouseManager.GetWarehouse(request.TargetWarehouseUID);
                    if (requests.GroupBy(g => g.TargetWarehouseUID).Count() > 1)
                    {
                        vaildResult.Add(Resource.WAREHOSUE_MOVE_PAYLAOD_MULTIWAREHOUSE_REQUEST);
                    }
                    if (warehouseInfo.Content == null)
                    {
                        vaildResult.Add(Resource.WAREHOUSE_NOT_FIND);
                    }
                    var itemInfo = itemInfos.FirstOrDefault(p => p.UID == request.TargetItemUID);
                    var pkgInfo = this.PackageCacheManager.GetPackage(request.TargetPackageUID);
                    if (pkgInfo.ItemUID != request.TargetItemUID)
                    {
                        vaildResult.Add(string.Format(Resource.COMMON_PKG_NOT_MATCH, itemInfo.ID));
                    }
                    if (itemInfo == null)
                    {
                        vaildResult.Add(Resource.COMMON_NOT_FIND_ITEM);
                    }
                    if (pkgInfo == null)
                    {
                        vaildResult.Add(string.Format(Resource.COMMON_NOT_FIND_PACKAGE, itemInfo.ID));
                    }
                    if (vaildResult.Count() > 0) //確認上面檢查通過才能繼續檢查其它資料
                        continue;
                    //slot 是否存在
                    var slotInfo = this.SlotRepository.GetList(new { UID = request.TargetSlotUID });
                    if (slotInfo.Content.Count() == 0)
                    {
                        vaildResult.Add(Resource.INVENTORY_NOT_FIND_SLOT);
                    }
                    if (checkslotStatusInboundOnly(slotInfo))
                    {
                        vaildResult.Add(Resource.COMMON_ILLEGAL_SLOT_STATUS);
                    }
                    //check payload 是否有效
                    if (SourcePayloadList.Count() != request.SourcePayloadUIDList.Count)
                    {
                        vaildResult.Add(Resource.COMMON_ILLEGAL_PAYLOAD);
                    }
                    //check payload type 是否一致
                    if (SourcePayloadList != null && SourcePayloadList.GroupBy(g => g.Type).Count() > 1)
                    {
                        vaildResult.Add(Resource.COMMON_ILLEGAL_PAYLOAD_TYPE);
                    }
                }
                #endregion
                var leglPayloadStatus = new int[] { (int)PayloadStatus.Active, (int)PayloadStatus.OffPosition };
                //using (var scope = this.GetNewTransactionScope(30 * 60))
                //{
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    this.ExistTransactionScope = true;
                    if (vaildResult.Count() == 0)
                    {
                        var result = new List<IActionResult<bool>>();

                        foreach (var request in requests)
                        {
                            #region 檢查來源是否充足
                            //需求之單件(Each)數量
                            int TargetEachQty = PackageCacheManager.GetReceivePackageUomQuantity(
                                        request.TargetPackageUID,
                                        PackageCacheManager.GetMinPackage(request.TargetPackageUID).UID,
                                        request.TargetQty
                                    ).Content;
                            var SourcePayloadList = this.PayloadRepository.GetList(new { UID = request.SourcePayloadUIDList }).Content;
                            var ModifiedSourcePayloadList = new List<IPayloadModel>();
                            foreach (var pitem in SourcePayloadList)
                            {
                                ModifiedSourcePayloadList.Add(pitem.Clone<IPayloadModel>());
                            }

                            var CurrentPayloadList = SourcePayloadList.ToList();
                            var CTargetList = CurrentPayloadList
                                .Where(x => x.Quantity > 0)
                                .Select(c =>
                                {
                                    dynamic e = new ExpandoObject();
                                    e.UID = c.UID;
                                    e.ItemUID = c.ItemUID;
                                    e.EachQuantity = PackageCacheManager.GetReceivePackageUomQuantity(
                                        c.PackageUID,
                                        PackageCacheManager.GetMinPackage(c.PackageUID).UID,
                                        c.Quantity
                                    ).Content;
                                    e.MinPackageUID = PackageCacheManager.GetMinPackage(c.PackageUID).UID;
                                    e.MoveQty = 0;
                                    e.Type = c.Type;
                                    e.Payload = c;
                                    return e;
                                }).OrderBy(o => o.EachQuantity).ToList();

                            foreach (var target_item in CTargetList)
                            {
                                if (TargetEachQty > 0)
                                {
                                    target_item.Payload.PackageUID = target_item.MinPackageUID;
                                    int left = TargetEachQty - target_item.EachQuantity;
                                    if (left >= 0)
                                    {
                                        target_item.Payload.Quantity = 0;
                                        target_item.MoveQty = target_item.EachQuantity;
                                        TargetEachQty -= target_item.EachQuantity;
                                    }
                                    else
                                    {
                                        target_item.Payload.Quantity = -left;
                                        target_item.MoveQty = TargetEachQty;
                                        TargetEachQty = 0;
                                    }
                                }
                                else
                                { break; }
                            }

                            #endregion
                            #region 調整來源庫存
                            var ModifyTargetPayloadList = CTargetList.FindAll(x => x.MoveQty > 0);
                            this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ModifiedInventory;
                            if (Enum.IsDefined(typeof(PayloadType), request.PayloadType))
                            {
                                PayloadType pt = (PayloadType)request.PayloadType;
                                if (pt == PayloadType.Stock)
                                {
                                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ModifiedInventory;
                                }
                                else
                                {
                                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ModifiedInventorySetType;
                                }
                            }
                            foreach (var modify_item in ModifyTargetPayloadList)
                            {
                                //是否為修改
                                var moditiedInfo = modify_item.Payload;
                                var payloadInfo = ModifiedSourcePayloadList.Find(p => p.UID.Equals(moditiedInfo.UID));
                                //取得修改前 inventory 資料
                                var sourceinvInfo = this.InventoryRepository.GetList(new
                                {
                                    ItemUID = moditiedInfo.ItemUID,
                                    SlotUID = moditiedInfo.SlotUID,
                                    Type = moditiedInfo.Type
                                });

                                if (sourceinvInfo.Content.Count() == 0)
                                {
                                    rs.Content = false;
                                    rs.Message = $"not find item#{moditiedInfo.ItemUID} in Slot#{moditiedInfo.SlotUID} onhand data.";
                                    return rs;
                                }
                                //修改狀態
                                if (moditiedInfo.Quantity == 0)
                                {
                                    moditiedInfo.Status = (int)PayloadStatus.Inactive;
                                }
                                var mrs = this.PayloadRepository.UpatePayload(moditiedInfo);
                                result.Add(mrs);
                                if (mrs.Success)
                                {
                                    //重新計算該item ,Slot 上目的onhand
                                    var aftermodifiedBelongToPayload = this.PayloadRepository.GetList(new
                                    {
                                        ItemUID = moditiedInfo.ItemUID,
                                        SlotUID = moditiedInfo.SlotUID
                                    });
                                    //當修改onhand 為Stock 時重新計算onhand 時需包含Allocated 資料  2021/11/8
                                    if (moditiedInfo.Type == (int)PayloadType.Stock)
                                    {
                                        aftermodifiedBelongToPayload.Content = aftermodifiedBelongToPayload.Content.Where(p => allocatedPayloadType.Any(x => p.Type == x) ||
                                        allowPayloadType.Any(o => o == p.Type));
                                    }
                                    else
                                    {
                                        //
                                        aftermodifiedBelongToPayload.Content = aftermodifiedBelongToPayload.Content.Where(p => p.Type == moditiedInfo.Type);
                                    }
                                    var calPayloadOnhand = 0;
                                    if (aftermodifiedBelongToPayload.Content.Count() > 0)
                                    {
                                        foreach (var item in aftermodifiedBelongToPayload.Content)
                                        {
                                            if (leglPayloadStatus.Any(p => p == item.Status))
                                            {
                                                bool isnegative = false;
                                                if (item.Quantity < 0)
                                                {
                                                    isnegative = true;
                                                    item.Quantity = Math.Abs(item.Quantity);
                                                }
                                                //遇到負onhand 的處理

                                                var addqty = this.PackageCacheManager
                                                    .GetReceivePackageUomQuantity(item.PackageUID,
                                                   this.PackageCacheManager.GetMinPackage(item.PackageUID).UID,
                                                    item.Quantity).Content;
                                                if (isnegative)
                                                {
                                                    addqty *= -1;
                                                }
                                                calPayloadOnhand += addqty;
                                            }
                                        }
                                    }
                                    else //沒有任何 payload 
                                    {

                                    }
                                    //計算同步給pbsc的onhand 差異值
                                    //                  修改後onhand        修改前                                                        
                                    var syncorgconhand = calPayloadOnhand - sourceinvInfo.Content.Sum(p => p.Qty);
                                    //計算onhand (扣除非payload包裝的onhand)
                                    var orgconhand = Math.Abs(calPayloadOnhand - sourceinvInfo.Content
                                           .Where(p => p.PackageUID != this.PackageCacheManager
                                           .GetMinPackage(moditiedInfo.PackageUID).UID).Sum(p => p.Qty));
                                    //重新計算該item ,Slot 上來源onhand
                                    result.Add(this.InventoryRepository
                                           .DeleteInventory(sourceinvInfo.Content.Where(p =>
                                           p.PackageUID == this.PackageCacheManager.GetMinPackage(moditiedInfo.PackageUID).UID)
                                           .Select(x => x.UID)));
                                    var orgohandparam = new AddOnhandInnerParameters();
                                    orgohandparam.Onhand = orgconhand;
                                    orgohandparam.SlotUID = moditiedInfo.SlotUID;
                                    orgohandparam.TargetPackageUID = this.PackageCacheManager
                                                    .GetMinPackage(moditiedInfo.PackageUID).UID;
                                    orgohandparam.WarehouseUID = request.TargetWarehouseUID;
                                    orgohandparam.ItemUID = moditiedInfo.ItemUID;
                                    orgohandparam.Type = (InventoryType)modify_item.Type;

                                    result.Add(this.InventoryManager.ProcessAddInventory(orgohandparam));


                                    //Slot相同所以不用另外計算
                                    var repliateonhanddata = new WMSReplicateOnhandModel();
                                    repliateonhanddata.ItemUID = moditiedInfo.ItemUID;
                                    repliateonhanddata.PayloadUID = moditiedInfo.UID;
                                    repliateonhanddata.Quantity = syncorgconhand;
                                    repliateonhanddata.SlotUID = moditiedInfo.SlotUID;
                                    repliateonhanddata.PayloadType = modify_item.Type; ;
                                    modifiedReplicateCollection.Add(repliateonhanddata);
                                }
                                if (result.All(x => x.Success))
                                {
                                    //ActionType 為修改寫TransactionLog
                                    var logModel = new PayloadTransactionLogInnerModel();
                                    logModel.UID = Guid.NewGuid();
                                    logModel.ItemUID = moditiedInfo.ItemUID;
                                    logModel.OriginalPackage = moditiedInfo.PackageUID;
                                    logModel.TargetPackage = moditiedInfo.PackageUID;
                                    logModel.QtyBeforeTX = payloadInfo.Quantity;
                                    logModel.QtyAfterTX = moditiedInfo.Quantity;
                                    logModel.OriginalSlotUID = moditiedInfo.SlotUID;
                                    logModel.TargetSlotUID = moditiedInfo.SlotUID;
                                    logModel.PayloadUID = moditiedInfo.UID;
                                    logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                    //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                                    logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                    logModel.WarehouseUID = request.TargetWarehouseUID;
                                    var rs3 = this.InventoryManager.AddLog(logModel);
                                }
                            }
                            #endregion
                            #region 新增目標Palyload
                            //List<ISetGroupMoveAdjustmentRequest> virtualItmes = new List<ISetGroupMoveAdjustmentRequest>();
                            if (Enum.IsDefined(typeof(PayloadType), request.PayloadType))
                            {
                                PayloadType pt = (PayloadType)request.PayloadType;
                                if (pt == PayloadType.Stock)
                                {
                                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.AddInventory;
                                }
                                else
                                {
                                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.AddInventorySetType;
                                }
                            }

                            var itemInfo = itemInfos.FirstOrDefault(p => p.UID == request.TargetItemUID);
                            //處理非虛擬item 
                            if (string.IsNullOrEmpty(itemInfo.ActualProduct))
                            {
                                var vitems = this.ProductCacheManager.GetVirtualItems(itemInfo.Name, new Guid[] { new Guid(itemInfo.CustomerUID) });
                                if (vitems.Content?.Count() > 0)
                                {
                                    foreach (var item in vitems.Content)
                                    {
                                        var pkgs = this.PackageCacheManager.GetPackagesByItem(item.UID).OrderByDescending(o => o.CreatedOn);
                                        var pkg = pkgs.First();
                                        var parm = new AddOnhandInnerParameters();
                                        parm.ItemUID = item.UID;
                                        parm.Onhand = request.TargetQty;
                                        parm.SlotUID = request.TargetSlotUID;
                                        parm.TargetPackageUID = pkg.UID;
                                        parm.WarehouseUID = request.TargetWarehouseUID;
                                        parm.isPauseSync = true;
                                        parm.Type = InventoryType.Stock;
                                        if (Enum.IsDefined(typeof(PayloadType), request.PayloadType))
                                        {
                                            PayloadType pt = (PayloadType)request.PayloadType;
                                            this.TracingAgent.Trace($"SetGroupMoveAdjustment setup payload type {pt}");
                                            parm.PayloadType = pt;
                                            parm.Type = (InventoryType)request.PayloadType;
                                        }
                                        else
                                        {
                                            parm.PayloadType = PayloadType.Stock;
                                        }
                                        var addRs = this.InventoryManager.ProcessAddInventory(parm, isAddPayload: true) as IExtensionActionResult<bool>;
                                        result.Add(addRs);

                                        var logModel = new PayloadTransactionLogInnerModel();
                                        logModel.UID = Guid.NewGuid();
                                        logModel.ItemUID = item.UID;
                                        logModel.OriginalPackage = request.TargetPackageUID;
                                        logModel.TargetPackage = request.TargetPackageUID;
                                        logModel.QtyBeforeTX = 0;
                                        logModel.QtyAfterTX = request.TargetQty;
                                        logModel.OriginalSlotUID = null;
                                        logModel.TargetSlotUID = request.TargetSlotUID;
                                        logModel.PayloadUID = addRs.GetReturnValue<Guid>("NewPayloadUID");
                                        logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                        logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                        logModel.WarehouseUID = request.TargetWarehouseUID;
                                        var rs3 = this.InventoryManager.AddLog(logModel);
                                    }
                                    //同步onhand 資料
                                    if (result.All(p => p.Success))
                                    {
                                        var repliateonhanddata = new WMSReplicateOnhandModel();
                                        repliateonhanddata.ItemUID = request.TargetItemUID;
                                        repliateonhanddata.Quantity = request.TargetQty;
                                        repliateonhanddata.SlotUID = request.TargetSlotUID;
                                        repliateonhanddata.PayloadType = request.PayloadType;
                                        modifiedReplicateCollection.Add(repliateonhanddata);
                                    }
                                }
                                else
                                {
                                    //request 不是虛擬item則直接新增 payload
                                    var parm = new AddOnhandInnerParameters();
                                    parm.ItemUID = request.TargetItemUID;
                                    parm.Onhand = request.TargetQty;
                                    parm.SlotUID = request.TargetSlotUID;
                                    parm.TargetPackageUID = request.TargetPackageUID;
                                    parm.WarehouseUID = request.TargetWarehouseUID;
                                    parm.Type = InventoryType.Stock;
                                    parm.isPauseSync = true;
                                    if (Enum.IsDefined(typeof(PayloadType), request.PayloadType))
                                    {
                                        PayloadType pt = (PayloadType)request.PayloadType;
                                        this.TracingAgent.Trace($"SetGroupMoveAdjustment setup payload type {pt}");
                                        parm.PayloadType = pt;
                                        parm.Type = (InventoryType)request.PayloadType;
                                    }
                                    else
                                    {
                                        parm.PayloadType = PayloadType.Stock;
                                    }
                                    var addRs = this.InventoryManager.ProcessAddInventory(parm, isAddPayload: true);
                                    result.Add(addRs);
                                    if (result.All(p => p.Success))
                                    {
                                        var minpkg = this.PackageCacheManager.GetMinPackage(request.TargetPackageUID);
                                        var synconhand = this.PackageCacheManager.GetReceivePackageUomQuantity(request.TargetPackageUID,
                                            minpkg.UID, request.TargetQty);
                                        var extaddRs = addRs as ExtensionActionResultContainer<bool>;
                                        var newpayloaduid = extaddRs.GetReturnValue<Guid>("NewPayloadUID");
                                        //同步onhand 資料
                                        var repliateonhanddata = new WMSReplicateOnhandModel();
                                        repliateonhanddata.ItemUID = request.TargetItemUID;
                                        repliateonhanddata.Quantity = synconhand.Content;
                                        repliateonhanddata.SlotUID = request.TargetSlotUID;
                                        repliateonhanddata.PayloadType = request.PayloadType;
                                        repliateonhanddata.PayloadUID = newpayloaduid;
                                        modifiedReplicateCollection.Add(repliateonhanddata);
                                        //ActionType 為修改寫TransactionLog
                                        var logModel = new PayloadTransactionLogInnerModel();
                                        logModel.UID = Guid.NewGuid();
                                        logModel.ItemUID = request.TargetItemUID;
                                        logModel.OriginalPackage = request.TargetPackageUID;
                                        logModel.TargetPackage = request.TargetPackageUID;
                                        logModel.QtyBeforeTX = 0;
                                        logModel.QtyAfterTX = request.TargetQty;
                                        logModel.OriginalSlotUID = null;
                                        logModel.PayloadUID = newpayloaduid;
                                        logModel.TargetSlotUID = request.TargetSlotUID;
                                        logModel.Status = (int)PayloadTransactionLogStatus.Active;
                                        //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                                        logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                                        logModel.WarehouseUID = request.TargetWarehouseUID;
                                        var rs3 = this.InventoryManager.AddLog(logModel);
                                    }
                                }
                            }
                            //else
                            //{
                            //    virtualItmes.Add(request);
                            //}

                            //處理虛擬item 
                            //if (virtualItmes.Count > 0)
                            //{
                            //    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.AddInventory;
                            //    var combineItem = this.ProductCacheManager.NewCombineToActualItem(virtualItmes.Select(p =>
                            //    {
                            //        var vi = new VirtualItemInfo();
                            //        var subiteminfo = itemInfos.FirstOrDefault(x => x.UID == p.TargetItemUID);
                            //        vi.ActualProduct = subiteminfo.ActualProduct;
                            //        vi.ProductId = subiteminfo.ID;
                            //        vi.Quantity = p.TargetQty;
                            //        vi.ProductUID = subiteminfo.UID;
                            //        vi.PUOM = subiteminfo.PUOM;
                            //        vi.CustomerUID = new Guid(subiteminfo.CustomerUID);
                            //        return vi;
                            //    }));
                            //    if (combineItem.Success)
                            //    {
                            //        //新增payload
                            //        var lastestpkg = this.PackageCacheManager.GetPackagesByItem(combineItem.Content.UID)
                            //            .OrderByDescending(o => o.CreatedOn).FirstOrDefault();
                            //        var minpkg = this.PackageCacheManager.GetMinPackage(lastestpkg.UID);
                            //        var parm = new AddOnhandInnerParameters();
                            //        parm.ItemUID = combineItem.Content.UID;
                            //        parm.Onhand = virtualItmes.FirstOrDefault().TargetQty;
                            //        parm.SlotUID = virtualItmes.FirstOrDefault().TargetSlotUID;
                            //        parm.TargetPackageUID = minpkg.UID;
                            //        parm.WarehouseUID = virtualItmes.FirstOrDefault().TargetWarehouseUID;
                            //        parm.isPauseSync = true;
                            //        parm.Type = InventoryTypes.Stock;
                            //        //(this.InventoryManager as AbstractManager).SetTransactionObject(this.TransacationScopeObject);

                            //        result.Add(this.InventoryManager.ProcessAddInventory(parm, isAddPayload: true));
                            //        if (result.All(p => p.Success))
                            //        {
                            //            //同步onhand 資料
                            //            var repliateonhanddata = new WMSReplicateOnhandModel();
                            //            repliateonhanddata.ItemUID = combineItem.Content.UID;
                            //            repliateonhanddata.Quantity = virtualItmes.FirstOrDefault().TargetQty;
                            //            repliateonhanddata.SlotUID = virtualItmes.FirstOrDefault().TargetSlotUID;
                            //            modifiedReplicateCollection.Add(repliateonhanddata);
                            //            var logModel = new PayloadTransactionLogInnerModel();
                            //            logModel.UID = Guid.NewGuid();
                            //            logModel.ItemUID = combineItem.Content.UID;
                            //            logModel.OriginalPackage = minpkg.UID;
                            //            logModel.TargetPackage = minpkg.UID;
                            //            logModel.QtyBeforeTX = 0;
                            //            logModel.QtyAfterTX = virtualItmes.FirstOrDefault().TargetQty;
                            //            logModel.OriginalSlotUID = null;
                            //            logModel.TargetSlotUID = virtualItmes.FirstOrDefault().TargetSlotUID;
                            //            logModel.Status = (int)PayloadTransactionLogStatus.Active;
                            //            //logModel.Type = (int)PayloadTransactionLogTypes.MODIFIED_ONHAND;
                            //            logModel.Type = (int)this.TracingAgent.GetTransactionLogType();
                            //            logModel.WarehouseUID = virtualItmes.FirstOrDefault().TargetWarehouseUID;
                            //            var rs3 = this.InventoryManager.AddLog(logModel);
                            //        }
                            //    }
                            //    else
                            //    {
                            //        rs.Success = false;
                            //        rs.Message = Resource.COMMON_VIRTUALITEM_COMBINE_FAILURE;
                            //    }
                            //}
                            #endregion
                        }

                        if (result.All(x => x.Success))
                        {
                            rs.Success = true;
                            rs.Content = true;
                            //scope.Complete();
                            this.DbEntities.Commit();
                            if (result.All(x => x.Success) && moveReplicateCollection.Count > 0)
                                syncMethod.Add(() => this.ReplicationManager.Move(moveReplicateCollection));
                            if (result.All(x => x.Success) && modifiedReplicateCollection.Count > 0)
                                syncMethod.Add(() => this.ReplicationManager.ModifiedOnhand(modifiedReplicateCollection));
                        }
                        else
                        {
                            rs.Message = string.Join("\r\n", result.Where(x => !x.Success).Select(p => p.Message));
                        }
                    }
                    else
                    {
                        rs.Message = string.Join("\r\n", vaildResult.ToArray());
                    }
                    //}
                }
                this.DbEntities.InitConnection();
                if (syncMethod.Count > 0)
                {
                    foreach (var item in syncMethod)
                    {
                        this.TracingAgent.Trace($"SetGroupMoveAdjustment sync method {item.Method.Name}");
                        var syncRs = item.Invoke();
                        this.TracingAgent.Trace($"SetGroupMoveAdjustment sync method Result:{syncRs.Success} Message:{syncRs.Message}");
                    }
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

        private void judeTransactionAction(ICreateAdjustmentTicketRequest request, IPayloadModel payloadInfo)
        {
            if (request.ModifyQty == 0)
            {
                this.TracingAgent.TransactionInfo.Action = TransactionlogAction.DeleteInventory;
            }
            else
            {
                //1
                if (request.ModifyQty != payloadInfo.Quantity
                    &&
                    (payloadInfo.SlotUID == request.ModifySlotUID && payloadInfo.PackageUID == request.ModifyPackageUID))
                {
                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ModifiedInventory;
                }
                if (payloadInfo.SlotUID != request.ModifySlotUID
                    &&
                    (payloadInfo.Quantity == request.ModifyQty && payloadInfo.PackageUID == request.ModifyPackageUID))
                {
                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ChangeSlot;
                }
                if (request.ModifyPackageUID != payloadInfo.PackageUID &&
                    (payloadInfo.Quantity == request.ModifyQty && payloadInfo.SlotUID == request.ModifySlotUID))
                {
                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ChangePackage;
                }

                //2
                if (request.ModifyQty != payloadInfo.Quantity && payloadInfo.SlotUID != request.ModifySlotUID
                   &&
                   (payloadInfo.PackageUID == request.ModifyPackageUID))
                {
                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ModifiedInventoryChangeSlot;
                }
                if (request.ModifyQty != payloadInfo.Quantity && payloadInfo.PackageUID != request.ModifyPackageUID
                     &&
                     (payloadInfo.SlotUID == request.ModifySlotUID))
                {
                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ModifiedInventoryChangePackage;
                }
                if (payloadInfo.SlotUID != request.ModifySlotUID && payloadInfo.PackageUID != request.ModifyPackageUID
                    &&
                    (payloadInfo.Quantity == request.ModifyQty))
                {
                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ChangePackageSlot;
                }

                //3
                if (request.ModifyPackageUID != payloadInfo.PackageUID &&
                    payloadInfo.Quantity != request.ModifyQty &&
                    payloadInfo.SlotUID != request.ModifySlotUID)
                {
                    this.TracingAgent.TransactionInfo.Action = TransactionlogAction.ModifiedInventoryChangePackageSlot;
                }
            }
        }

        public void ReplicationTest()
        {
            #region 測試Ticket 狀態同步
            //var parm = this.GetStatusManageAgentParamters();
            //var _parameter = this.GetTicketProcessAgentParameter(parm);
            //AbstractProcessAgent agent = AbstractProcessAgent.GetAgent(Constant.ProcessKind.TicketProcess,
            //      _parameter);
            //var _action = agent.CheckStatus(new Guid("59240BCC-E147-4DB9-912C-D2B537F0114B"));
            //_action.ToList().ForEach(m =>
            //{
            //    var rs2 = m.Invoke();
            //});
            #endregion
            #region Test
            var ss = this.PackageCacheManager.GetPackagesByItem(new Guid("40F91224-C579-423B-A769-58C1A6FB0155"));
            #endregion
            //var parameter = new ReplicateDataParameter();
            //parameter.TicketUID = new Guid[] { 
            //    new Guid("0E3D97E3-B173-44BE-A0A0-EE018F62B85D"), 
            //    new Guid("CC4A8605-1121-4E8E-8F8A-0396F3A3C75D"),
            //    new Guid("BEBD5B62-2A98-4842-9008-131C7E180AA2"),
            //    new Guid("78ADA63B-0CFD-486C-94C7-6AA1663050DA")};

            //not finish
            //var inboundTicketInfoUIDs = new Guid[] {
            //    new Guid("5D327920-2E84-412F-956F-F22364CA18FD"),
            //    new Guid("19F76325-0E13-4DD1-A6CD-CA393179143B"),
            //    new Guid("35E72F85-47AB-4ADC-B74B-9874B9EB7E4B")
            //};
            //this.ReplicationManager.Receiving(inboundTicketInfoUIDs);

            //var inboundTicketInfoUIDs = new Guid[] {
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("A2D10CE8-14A2-422E-834E-16AA2DF6A2F0"),
            //    new Guid("52E64F83-4A27-4E0C-A7A2-A4EC1CE86E89"),
            //    new Guid("1DCAEBB9-39E9-44C4-B5E9-FFDDD5A09293"),
            //    new Guid("1DED2AB2-1CFF-4D0B-86A1-60E1F87D77D8"),
            //    new Guid("75CC97DA-7E0E-4573-890C-9059081F83FF")
            //};
            //this.ReplicationManager.Receivied(Guid.Empty, inboundTicketInfoUIDs);

            //inbound move
            //var parameter = new List<WMSReplicateMoveModel>();
            //parameter.AddRange(new WMSReplicateMoveModel[] {
            //    new WMSReplicateMoveModel{
            //    ItemUID=new Guid("DE2E67EA-2CF5-42C7-A05B-DF47CC290D65"),
            //    OriginalSlotUID=new Guid("F76B3DC6-1DE3-439C-BBA9-79EBA659C7A8"),
            //    Quantity=10,
            //    ManifestType=(int)ManifestType.Inbound,
            //    TargetSlotUID=new Guid("7554F54A-B7E6-4936-946F-000AA9E58655"),
            //    TicketUID=new Guid("9EF802F7-F649-4C33-BA44-3D647658D396"),
            //    ItemGroup=new Guid("B22905FA-7742-4202-B4F7-D578C09260E4")
            //    },
            //    new WMSReplicateMoveModel{
            //      ItemUID=new Guid("00EDCAC5-BE5F-48DC-A8DE-CEA72A1DB636"),
            //    OriginalSlotUID=new Guid("F76B3DC6-1DE3-439C-BBA9-79EBA659C7A8"),
            //    Quantity=10,
            //    ManifestType=(int)ManifestType.Inbound,
            //    TargetSlotUID=new Guid("7554F54A-B7E6-4936-946F-000AA9E58655"),
            //    TicketUID=new Guid("9EF802F7-F649-4C33-BA44-3D647658D396"),
            //    ItemGroup=new Guid("B22905FA-7742-4202-B4F7-D578C09260E4")
            //    }
            //}
            //);
            //var parameter2 = new List<WMSReplicateMoveModel>();
            //parameter2.AddRange(new WMSReplicateMoveModel[] {
            //    new WMSReplicateMoveModel{
            //    ItemUID=new Guid("DE2E67EA-2CF5-42C7-A05B-DF47CC290D65"),
            //    OriginalSlotUID=new Guid("F76B3DC6-1DE3-439C-BBA9-79EBA659C7A8"),
            //    Quantity=10,
            //    ManifestType=(int)ManifestType.Inbound,
            //    TargetSlotUID=new Guid("7554F54A-B7E6-4936-946F-000AA9E58655"),
            //    TicketUID=new Guid("9EF802F7-F649-4C33-BA44-3D647658D396"),
            //    ItemGroup=new Guid("385060BE-3F15-4D9F-AC7C-5C7539F25B99")
            //    },
            //    new WMSReplicateMoveModel{
            //      ItemUID=new Guid("00EDCAC5-BE5F-48DC-A8DE-CEA72A1DB636"),
            //    OriginalSlotUID=new Guid("F76B3DC6-1DE3-439C-BBA9-79EBA659C7A8"),
            //    Quantity=10,
            //    ManifestType=(int)ManifestType.Inbound,
            //    TargetSlotUID=new Guid("7554F54A-B7E6-4936-946F-000AA9E58655"),
            //    TicketUID=new Guid("9EF802F7-F649-4C33-BA44-3D647658D396"),
            //    ItemGroup=new Guid("385060BE-3F15-4D9F-AC7C-5C7539F25B99")
            //    }
            //}
            //);
            //this.ReplicationManager.Move(parameter);
            //this.ReplicationManager.Move(parameter2);

            //outbound
            //var parameter = new ReplicateDataParameter();
            //parameter.TicketInfoUID = new Guid[] {
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //    new Guid("B5E3ECDB-9984-4BD9-84F8-118BFC91F049"),
            //};
            //this.ReplicationManager.Outbound(parameter);
        }

        public IActionResult<bool> CheckAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> parameters)
        {
            var result = ActionResultTemplates.OK();
            var collection = parameters.Where(p => !p.isNew);
            var payloaduids = collection.Select(x => x.PayloadUID);
            var payloads = this.PayloadRepository.GetList(new { UID = payloaduids });
            result.Success = result.Content = true;
            if (payloads.Content != null && payloads.Content.Count() > 0)
            {

                if (payloads.Content.Any(x => x.Type != (int)PayloadType.Stock))
                {
                    result.Success = result.Content = false;
                    result.Message = Resource.WAREHOUSE_MODIFIED_ONHAND_PAYLOAD_TYPE_ILLEGAL;
                }
                else if (payloads.Content.Any(x => x.Status != (int)PayloadStatus.Active))
                {
                    result.Success = result.Content = false;
                    result.Message = Resource.WAREHOUSE_MODIFIED_ONHAND_PAYLOAD_STATUS_ILLEGAL;
                }
            }
            return result;


        }
        public IActionResult<bool> CheckSakanaAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> parameters)
        {
            var result = ActionResultTemplates.OK();
            var collection = parameters.Where(p => !p.isNew);
            var payloaduids = collection.Select(x => x.PayloadUID);
            var payloads = this.PayloadRepository.GetList(new { UID = payloaduids });
            if (payloads.Content != null && payloads.Content.Count() > 0)
            {
                result.Success = result.Content = payloads.Content.All(x => x.Status == (int)PayloadStatus.Active &&
                   x.Type == (int)PayloadType.Sakana);
            }
            return result;


        }



        //private StatusManageAgentParamters GetStatusManageAgentParamters()
        //{
        //    return new StatusManageAgentParamters
        //    {
        //        WorkOrderPayloadRepository = this.WorkOrderPayloadRepository,
        //        WorkOrderPodRepository = this.WorkOrderPodRepository,
        //        WorkOrderRepository = this.WorkOrderRepository,
        //        TicketRepository = this.TicketRepository,
        //        TicketInfoRepository = this.TicketInfoRepository,
        //        TicketManager = this.TicketManager
        //    };
        //}

    }

}
