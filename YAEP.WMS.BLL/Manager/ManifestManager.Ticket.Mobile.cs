using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.Utilities.Model;
using YAEP.Attachment.ClientAPI;
using YAEP.Constants;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Package.Interfaces.Models;
using YAEP.Core.Party.Interfaces.Models;
using System.Transactions;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, ITicketMobileManager
    {
        public IActionResult<IEnumerable<dynamic>> GetTickeInfotListDetail(Guid[] TicketInfoUID, Guid WorkOrderPodUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<dynamic>>();
            try
            {
                GetTicketInfoParameters parameters = new GetTicketInfoParameters();
                parameters.TicketInfoUIDs = TicketInfoUID;
                var collection = GetInfoData(parameters);
                var _serviceItem = (TicketCategory)collection.Content.First().TicketType;
                var _parser = AbstractTicketViewParser.GetParser(_serviceItem,
                    this.LabelRepository, this.ProductCacheManager, this.PackageCacheManager, this.PackageUomManager, this.WarehouseAgent);
                rs.Success = true;
                rs.Content = _parser.CustomParser(collection.Content)
                    .Where(p => p.WorkOrderPodUID == WorkOrderPodUID).SelectMany(x => x.Items);
                return rs;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        private IActionResult<IEnumerable<ITicketInfoListViewModel>> GetInfoData(IGetTicketInfoParameters parameters)
        {

            //特定Label類別陣列
            var boxLableType = new LabelType[] { LabelType.Box_SCC14, LabelType.Box_Self, LabelType.Item_PUOM };

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoListViewModel>>();
            try
            {
                var source = this.TicketInfoRepository.GetTicketInfo(parameters);
                if (source.Success)
                {
                    IActionResult<IEnumerable<IPodBarcodeInfo>> PodBarcodeInfoCollection = null;
                    var allowUsePalletQtyManifestType = new int[] { (int)ManifestType.BlukPick, (int)ManifestType.Outbound };
                    var allowUsePalletQtyTicketType = new int[] { (int)TicketType.BulkPick, (int)TicketType.Move };
                    var tgom = new TicketGeneralOperationalModule(this.PackageCacheManager, this.PackageUomManager);
                    var UOMList = this.PackageUomManager.GetPackageUomList().Content;
                    var collection = source.Content.PayloadData.ToList();
                    var podpy = this.WorkOrderPayloadRepository.GetList(new
                    {
                        WorkOrderPodUID =
                       source.Content.PodData
                       .GroupBy(g => g.WorkOrderPodUID).Select(y => y.Key)
                        //原本有判斷pod是否有只有一種item判斷，目前因虛擬item會將產品展開，故將此判斷移除
                        //WorkOrderPodUID =
                        //source.Content.PodData
                        //.GroupBy(g => g.WorkOrderPodUID).Where(x => x.Count() == 1).Select(y => y.Key)
                    });
                    ItemInnerParameterize param = new ItemInnerParameterize();
                    //source.Content.PodData.Select(p => p.ItemUID)
                    var itemUIDs = source.Content.PodData.Select(p => p.ItemUID)
                        .Union(source.Content.PayloadData.Select(x => x.ItemUID))
                        .GroupBy(x => x).Select(g => g.Key);
                    param.ListOfItemUID.AddRange(itemUIDs);
                    var itemids = this.ItemManager.GetItems(param);
                    var pldata = source.Content.PodData
                        .GroupBy(g => g.WorkOrderPodUID)
                       .Select(p => ConvertViewModel(p, itemids.Content)).ToList();

                    collection.AddRange(pldata);
                    //get label
                    var _allLabel = collection.Select(p => p.PodUID).ToList();
                    if (podpy.Success && podpy.Content.Count() > 0)
                    {
                        pldata.ForEach(p =>
                        {
                            var original = podpy.Content.FirstOrDefault(x => x.WorkOrderPodUID == p.WorkOrderPodUID);
                            p.PayloadUID = original.PayloadUID;
                        });
                        _allLabel.AddRange(podpy.Content.Select(p => p.PayloadUID));
                    }
                    _allLabel.AddRange(collection.Select(p => p.PayloadUID));
                    var _r_allLabel = this.LabelRepository.GetLabels(_allLabel.ToArray());
                    var lg = collection.GroupBy(p => p.SourceLoadingZoneSlotUID).Select(p => p.Key).ToList();
                    lg.AddRange(collection.GroupBy(p => p.SourceSlotUID).Select(p => p.Key));
                    var locationGroup = this.WarehouseAgent.SlotManager.GetLocations(lg.Select(p => p).ToArray());
                    var _podLabelTypes = new int[] { (int)StorageMethod.NewPallet };
                    var checkparam = new CheckPodBarcodeInfoParameters();
                    if (collection.All(p => allowUsePalletQtyManifestType.Contains(p.ManifestType)) &&
                        collection.All(p => allowUsePalletQtyTicketType.Contains(p.TicketType)))
                    {
                        //抓Ticket所屬warehouse指定item 的Receiving barcode
                        checkparam.ItemUID = collection.GroupBy(g => g.ItemUID).Select(p => p.Key);
                        checkparam.WarehouseUID = collection.GroupBy(g => g.WarehouseUID).Select(p => p.Key);
                        checkparam.LabelType = (int)LabelType.Pallet_OrginalTracking;
                        PodBarcodeInfoCollection = this.GetReceivingQtyBarcodeInfo(checkparam);
                    }
                    //get package name
                    foreach (var item in collection)
                    {

                        LabelType[] uomForLabelType = new LabelType[] { };
                        var converter = AbstractTicketConverter.GetInstance(item.ManifestType, item.TicketType);
                        var UOMInfo = UOMList.FirstOrDefault(p => p.UID == item.ContainerType);
                        item.ContainerTypeName = UOMInfo?.Name;
                        converter.Convert(item);
                        if (item.TargetPackage.HasValue)
                        {
                            uomForLabelType = tgom.PackageforUOM(item.TargetPackage.Value);
                        }
                        else if (item.OriginalPackage.HasValue)
                        {
                            uomForLabelType = tgom.PackageforUOM(item.OriginalPackage.Value);
                        }
                        var _podlabel = _r_allLabel.Content.FirstOrDefault(p => p.BelongToUID == item.PodUID);
                        if (_podlabel != null)
                        {
                            item.PodBarcode = _podlabel.Content;
                        }
                        item.Labels = _r_allLabel.Content
                           .Where(p =>
                           {
                               //在收貨時不管是用Pod或是Payload關聯都得用payload 去取得barcode
                               if (item.TicketType == (int)TicketType.Receiving)
                               {
                                   return (p.BelongToUID == item.PayloadUID)
                                        && uomForLabelType.Any(x => x == p.Type);
                               }
                               else
                               {
                                   return (p.BelongToUID == item.PodUID && item.MappingType == 1
                                      || (p.BelongToUID == item.PayloadUID && item.MappingType == 2
                                  && uomForLabelType.Any(x => x == p.Type))) && (
                                 (p.Type == LabelType.Pallet_OrginalTracking &&
                                  allowUsePalletQtyManifestType.Contains(item.ManifestType) &&
                                    allowUsePalletQtyTicketType.Contains(item.TicketType)) ||
                                    p.Type != LabelType.Pallet_OrginalTracking
                                  );
                               }
                           })
                           .Select(
                            p =>
                            {
                                #region 判斷要取得什麼標籤
                                IPackageTree pkgTree = null;
                                if (item.TargetPackage.HasValue)
                                {
                                    pkgTree = this.PackageCacheManager.GetPackageTree(item.TargetPackage.Value);
                                }
                                else
                                {
                                    pkgTree = this.PackageCacheManager.GetPackageTree(item.OriginalPackage.Value);
                                }
                                var pkgUnitQty = 1;
                                if (boxLableType.Any(x => x == p.Type))
                                {
                                    var barcodePkg = this.PackageCacheManager.FindPkgSCC14barcode(pkgTree.Root, p.Content);
                                    if (barcodePkg != null)
                                    {
                                        var minPkg = pkgTree.MinPackage();
                                        var rsPkg = this.PackageCacheManager.GetReceivePackageUomQuantity(barcodePkg.UID,
                                            minPkg.UID, 1);
                                        if (rsPkg.Success && rsPkg.Content > 1)
                                        {
                                            pkgUnitQty = rsPkg.Content;
                                        }
                                    }
                                    var puomPkgs = this.PackageCacheManager.GetPUOMbarcde(pkgTree.Root, p.Content);
                                    if (puomPkgs != null)
                                    {
                                        var minPkg = pkgTree.MinPackage();
                                        var rsPkg = this.PackageCacheManager.GetReceivePackageUomQuantity(puomPkgs.UID,
                                            minPkg.UID, 1);
                                        if (rsPkg.Success && rsPkg.Content > 1)
                                        {
                                            pkgUnitQty = rsPkg.Content;
                                        }
                                    }
                                }
                                else
                                {
                                    //非特定Label類別其最小數量為1
                                    pkgUnitQty = 1;
                                }
                                var label = new TicketLabelInnerModel
                                {
                                    Barcode = p.Content,
                                    AttachmentUID = p.FileUID,
                                    BarcodeType = (int)p.Type,
                                    BarcodeTypeName = p.Type.ToString(),
                                    BelongToType = (int)p.BelongToType,
                                    BelongToUID = p.BelongToUID,
                                    Status = p.Status,
                                    StatusName = ((LabelStatus)p.Status).ToString(),
                                    AddQty = pkgUnitQty
                                } as ITicketLabelViewModel;
                                return label;


                                #endregion
                            }
                           ).ToList();
                        //是否取得Pallet Tracking Qty Label
                        if (collection.All(p => allowUsePalletQtyManifestType.Contains(p.ManifestType)) &&
                          collection.All(p => allowUsePalletQtyTicketType.Contains(p.TicketType)))
                        {
                            if (PodBarcodeInfoCollection.Content.Count() > 0)
                            {
                                var barcodeInfoCollection = PodBarcodeInfoCollection.Content.Where(p =>
                                p.ItemUID == item.ItemUID && item.EstQty >= p.Qty);
                                item.Labels.AddRange(barcodeInfoCollection.Select(p => new TicketLabelInnerModel
                                {
                                    Barcode = p.Barcode,
                                    AttachmentUID = Guid.Empty,
                                    BarcodeType = (int)p.Type,
                                    BarcodeTypeName = p.Type.ToString(),
                                    BelongToType = (int)p.BelongToType,
                                    BelongToUID = p.BelongToUID,
                                    Status = p.Status,
                                    StatusName = ((LabelStatus)p.Status).ToString(),
                                    AddQty = p.Qty
                                } as ITicketLabelViewModel));
                            }
                        }
                        //Label sort
                        item.Labels = item.Labels.OrderBy(p => p.Barcode).ToList();
                        if (item.OriginalSlotUID.HasValue && item.OriginalSlotUID != Guid.Empty && locationGroup.Success)
                        {
                            var l = locationGroup.Content.FirstOrDefault(g => g.SlotUID == item.OriginalSlotUID);
                            item.OriginalLocation = l;
                        }
                        if (item.TargetSlotUID.HasValue && item.TargetSlotUID != Guid.Empty && locationGroup.Success)
                        {
                            var l = locationGroup.Content.FirstOrDefault(g => g.SlotUID == item.TargetSlotUID);
                            item.TargetLocation = l;
                        }
                    }
                    rs.Content = collection;
                    rs.Success = true;
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }


        private TicketInfoListInnerViewModel ConvertViewModel(IGrouping<Guid, ITicketInfoListViewModel> group,
            IEnumerable<IItemModel> items)
        {
            //var itemid = items.Where(p => group.GroupBy(g => g.ItemUID).Select(x => x.Key).All(x => x == p.UID));
            //因虛擬item 的關係，group 並不會只有一種item 使用All判斷無法滿足item 集合故改用Any
            var itemid = items.Where(p => group.GroupBy(g => g.ItemUID).Select(x => x.Key).Any(x => x == p.UID));
            //var intersectitemid = items.Select(p => p.UID).Intersect(group.GroupBy(g => g.ItemUID).Select(x => x.Key));
            //var itemid = items.Where(p => intersectitemid.Any(x => x == p.UID));
            TicketInfoListInnerViewModel e = new TicketInfoListInnerViewModel();
            e.ActQty = group.First().ActQty;
            e.Description = group.First().Description;
            e.EstQty = group.First().EstQty;
            e.IsPodExist = group.First().IsPodExist;
            if (itemid != null)
                e.ItemID = string.Join(",", itemid.Select(p => p.ID));
            e.ManifestType = group.First().ManifestType;
            e.MappingType = group.First().MappingType;
            e.OriginalPackageName = WMSAPIParameters.PALLET_UOM_KEYNAME;
            e.OriginalSlotUID = group.First().OriginalSlotUID;
            e.PodName = group.First().PodName;
            e.PodUID = group.First().PodUID;
            e.SavQty = group.First().SavQty;
            e.ShtQty = group.First().ShtQty;
            e.SourceLoadingZoneSlotUID = group.First().SourceLoadingZoneSlotUID;
            e.SourcePackageUID = group.First().SourcePackageUID;
            e.SourceSlotUID = group.First().SourceSlotUID;
            e.StorageType = group.First().StorageType;
            e.TargetPackageName = WMSAPIParameters.PALLET_UOM_KEYNAME;
            e.TicketID = group.First().TicketID;
            e.TicketInfoStatus = group.First().TicketInfoStatus;
            e.TicketInfoStatusName = group.First().TicketInfoStatusName;
            e.TicketType = group.First().TicketType;
            e.TicketTypeName = group.First().TicketTypeName;
            e.TargetSlotUID = group.First().TargetSlotUID;
            e.UID = group.First().UID;
            e.WorkOrderPodUID = group.Key;
            e.OperationInstruction = group.First().OperationInstruction;
            e.OperationSuggestion = group.First().OperationSuggestion;
            return e;
        }

        public IActionResult<IEnumerable<dynamic>> GetTicketInfo(IEnumerable<Guid> ticketUIDs,
            IEnumerable<Guid> filterticketinfoguids = null)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<dynamic>>();
            try
            {
                GetTicketInfoParameters parameters = new GetTicketInfoParameters();
                parameters.TicketUIDs = ticketUIDs;
                var collection = GetInfoData(parameters).Content;
                if (filterticketinfoguids != null)
                {
                    collection = collection.Where(p => filterticketinfoguids.Any(x => x == p.UID));
                }
                var _serviceItem = (TicketCategory)collection.First().TicketType;
                var _parser = AbstractTicketViewParser.GetParser(_serviceItem,
                    this.LabelRepository, this.ProductCacheManager, this.PackageCacheManager, this.PackageUomManager, this.WarehouseAgent);
                rs.Success = true;
                rs.Content = _parser.Parser(collection);
                return rs;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;

        }
        public IActionResult<IEnumerable<ITicketListViewModel>> GetTicketList(IGetTicketListParameters parameters)
        {
            if ((parameters.mtype != null || parameters.type != null) && parameters.groupIds.Length > 0)
            {
                var collection = this.TicketRepository.GetTicketList(parameters);
                if (collection.Success)
                {
                    var UOMList = this.PackageUomManager.GetPackageUomList().Content;
                    var grpParty = collection.Content.GroupBy(g => g.PartyUID);
                    var bulkPickData = collection.Content.Where(p => p.TicketType == (int)TicketType.BulkPick);
                    var param = new PartyParameterize();
                    param.ListOfPartyUID = grpParty.Select(p => p.Key).ToList();
                    var partyModels = this.PartyManager.GetParties(param).Content;
                    if (bulkPickData.Count() > 0)
                    {
                        var bulkPickModels = this._BulkPickRepository
                            .GetBulkPickByTicketCollection(bulkPickData.Select(x => x.TicketUID));
                        foreach (var item in bulkPickData)
                        {
                            var bt = bulkPickModels.Content.FirstOrDefault(p => p.TicketUID == item.TicketUID);
                            if (bt != null)
                            {
                                item.BulkPickNo = bt.ID;
                            }
                        }
                    }
                    foreach (var item in collection.Content)
                    {
                        var customer = partyModels.FirstOrDefault(p => p.UID == item.PartyUID);
                        if (customer != null)
                        {
                            item.PartyName = customer.Name;
                        }
                        var UOMInfo = UOMList.FirstOrDefault(p => p.UID == item.ContainerType);
                        item.ContainerTypeName = UOMInfo?.Name;
                    }
                }
                return collection;
            }
            else
            {
                var rs = ActionResultTemplates.Result<IEnumerable<ITicketListViewModel>>();
                rs.Success = false;
                if (parameters.mtype == null && parameters.type == null)
                {
                    rs.Message += "Manifest Type or service type not empty.";
                }
                if (parameters.groupIds.Length == 0)
                {
                    rs.Message += "Group id not empty.";
                }
                return rs;
            }
        }
        public IActionResult<IEnumerable<ITicketProcessModel>> GetTicketProcessModel(Guid[] TicketInfoUIDs)
        {
            return this.TicketInfoRepository.GetTicketProcessModel(TicketInfoUIDs);
        }
        public IActionResult<dynamic> BatchUploadTicketData(IEnumerable<IUploadTicketDataParameter> parameters)
        {
            var rs = ActionResultTemplates.Result<dynamic>();
            //get
            List<IActionResult<dynamic>> resultList = new List<IActionResult<dynamic>>();
            try
            {
                foreach (var parameter in parameters)
                {
                    resultList.Add(this.UploadTicketData(parameter));
                }
                rs.Success = true;
                rs.Content = resultList;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<dynamic> UploadTicketData(IUploadTicketDataParameter parameter)
        {
            //get
            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {
               // this.TracingAgent.Trace("begin process ticket item", parameter);
                var process_rs = this.ProcessUploadTicketData(parameter);
              //  this.TracingAgent.Trace("finish process ticket item", parameter);
                rs.Success = process_rs.Success;
                rs.Message = process_rs.Message;
            //    this.TracingAgent.Trace("begin process get ticket item view", parameter.Item.TicketInfoUID);
                rs.Content = this.GetTickeInfotListDetail(new Guid[] { parameter.Item.TicketInfoUID }
                       , parameter.Item.WorkOrderPodUID).Content.FirstOrDefault();
             //   this.TracingAgent.Trace("finish process get ticket item view", parameter.Item.TicketInfoUID);
                return rs;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        /// <summary>
        /// 目前僅提供給Modified inventory 使用
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IActionResult<bool> UploadTicketByPodBarcode(IUploadTicketDataParameter parameter)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var ticketInfos = this.TicketManager.GetPodBelongTicket(parameter.Item.Barcode.Select(x => x.Barcode));
                if (ticketInfos.Success && ticketInfos.Content.Count() > 0)
                {
                    parameter.Item.TicketInfoUID = ticketInfos.Content.FirstOrDefault().UID;
                    var rs1 = this.ProcessUploadTicketData(parameter);
                    rs.Content = rs1.Content;
                    rs.Success = rs1.Success;
                    rs.Message = rs1.Message;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.DATA_TICKET_KEY + " " + Resource.COMMON_DATA_NOT_FOUND;
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<dynamic> UploadTicketDataByPodBarcode(IUploadTicketDataParameter parameter)
        {
            var _RequestKey = this.RequestManager.GetObjectKey(parameter);
            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {
                if (!this.RequestManager.IsRequestProcessing(RequestAction.UPLOAD_OUTBOUND_TICKET_BY_POD, _RequestKey))
                {
                    this.RequestManager.AddRequest(RequestAction.UPLOAD_OUTBOUND_TICKET_BY_POD, _RequestKey);
                    var ticketInfos = this.TicketManager.GetPodBelongTicket(parameter.Item.Barcode.Select(x => x.Barcode));
                    if (ticketInfos.Success && ticketInfos.Content.Count() > 0)
                    {
                        this.TracingAgent.Trace($"UploadTicketDataByPodBarcode get data Ticket Info[{string.Join(",", ticketInfos.Content.Select(p => p.UID))}] ", ticketInfos.Content);
                        parameter.Item.TicketInfoUID = ticketInfos.Content.FirstOrDefault().UID;
                        var rs1 = this.ProcessUploadTicketData(parameter);
                        var ticketInfo = ticketInfos.Content.FirstOrDefault();
                        var _giContent = this.GetTicketInfo(new Guid[] { ticketInfo.TicketUID }).Content;
                        var collection = _giContent == null ? Enumerable.Empty<ITicketViewGroupItem>() : _giContent.Cast<ITicketViewGroupItem>();
                        var coll = collection.Where(p => parameter.Item.Barcode.Any(x => x.Barcode == p.PodBarcode));
                        rs.Content = coll;
                        rs.Success = rs1.Success;
                        rs.Message = rs1.Message;
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = Resource.DATA_TICKET_KEY + " " + Resource.COMMON_DATA_NOT_FOUND;
                    }
                }
                else
                {

                    rs.Success = false;
                    rs.Message = Resource.COMMON_REQUEST_ISPROCESSING;

                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            finally
            {
                this.RequestManager.RemoveRequest(RequestAction.UPLOAD_OUTBOUND_TICKET_BY_POD, _RequestKey);
            }
            return rs;

        }


        private IActionResult<bool> ProcessUploadTicketData(IUploadTicketDataParameter parameter)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var parm = this.GetStatusManageAgentParamters();
                var _parameter = this.GetTicketProcessAgentParameter(parm);
                var agent = AbstractProcessAgent.GetAgent(Constant.ProcessKind.TicketProcess,
                    _parameter);
                NotifySenderConfig config = null;

                var manifestModel = this.TicketInfoRepository.GetManifest(parameter.Item.TicketInfoUID);
                if (manifestModel.Content != null)
                {
                    if (manifestModel.Content.Type == 1 && manifestModel.Content.Description == "TransferOrder")
                    {
                        this.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.Transfer;
                    }
                    var receiverModel = this.ReceiverRepository.GetNotifyConfig(new { BelongToUID = manifestModel.Content.UID });
                    if (receiverModel.Content != null)
                    {
                        config = new NotifySenderConfig
                        {
                            ReceiverSecret = receiverModel.Content.ReceiverSecret,
                            ReceiverUrl = receiverModel.Content.ReceiverUrl
                        };
                    }
                }

                this.TracingAgent.Trace($"ProcessUploadTicketData get data  ", parameter);
                var process_rs = agent.Process(new IUploadTicketDataParameter[] { parameter },
                    config);//parameter.Item
                rs.Success = process_rs.Success;
                rs.Content = process_rs.Content;
                rs.Message = process_rs.Message;
                if (rs.AllComplete())
                {
                    // scope.Complete();
                }

                //}
                if (rs.AllComplete())
                {
                    this.DbEntities.InitConnection();
                    agent.CompleteUnexecutedMethod().ToList().ForEach(p =>
                    {
                        this.TracingAgent.Trace($"invoke method {p.Method.Name}  ");
                        var crs = p.Invoke();
                        this.TracingAgent.Trace($"invoke method result:{crs.Success} message:{crs.Message}  ");
                    });
                }
                return rs;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<IEnumerable<ITicketSummaryViewModel>> GetTicketSummaryData(Guid TicketUID, IEnumerable<Guid> _groups)
        {
            var rs = this.TicketRepository.GetTicketSummaryData(TicketUID, _groups.ToArray());
            var versionManager = this.PackageVersionManager;
            if (rs.Success)
            {
                foreach (var ticketinfo in rs.Content)
                {
                    var _item = this.ItemManager.GetItem(ticketinfo.ItemUID).Content;
                    var _minipkg = this.PackageCacheManager.GetMinPackage(ticketinfo.PackageUID);
                    //var _ver = versionManager.GetHashCode (_minipkg.VersionUID);
                    ticketinfo.ItemID = _item.ID;

                    ticketinfo.PackageName = $"{_minipkg.Name} ";
                    ticketinfo.ActQty = this.PackageManager.GetReceivePackageUomQuantity(
                        ticketinfo.PackageUID, _minipkg.UID, ticketinfo.ActQty).Content;
                    ticketinfo.EstQty = this.PackageManager.GetReceivePackageUomQuantity(
                        ticketinfo.PackageUID, _minipkg.UID, ticketinfo.EstQty).Content;
                    ticketinfo.SavQty = this.PackageManager.GetReceivePackageUomQuantity(
                        ticketinfo.PackageUID, _minipkg.UID, ticketinfo.SavQty).Content;
                    ticketinfo.ShtQty = this.PackageManager.GetReceivePackageUomQuantity(
                        ticketinfo.PackageUID, _minipkg.UID, ticketinfo.ShtQty).Content;
                    ticketinfo.PackageUID = _minipkg.UID;
                }
                var group = rs.Content.GroupBy(g => g.ItemUID).Select(p => new TicketSummaryViewInnerModel
                {
                    ActQty = p.Sum(x => x.ActQty),
                    EstQty = p.Sum(x => x.EstQty),
                    ShtQty = p.Sum(x => x.ShtQty),
                    SavQty = p.Sum(x => x.SavQty),
                    ItemID = p.First().ItemID,
                    ItemUID = p.Key,
                    PackageName = p.First().PackageName,
                    PackageUID = p.First().PackageUID,
                    TicketType = p.First().TicketType
                });
                rs.Content = group;
            }
            return rs;
        }

        public IEnumerable<dynamic> GetAttachmentTypeList(int belongToType)
        {
            var api = new HttpClientAgent(
                       this.AuthProvider.GetAuthenticationInfo().Token);
            string msg = "";
            return api.GetAttachmentType(belongToType, out msg);
        }
        public IActionResult<bool> UploadAttachment(ITicketUploadAttachmentParameters param)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var api = new HttpClientAgent(
                        this.AuthProvider.GetAuthenticationInfo().Token);
                var fileName = $"{param.File.FileName}";
                var uploadRs = api.UploadFile(param.File.InputStream, fileName, param.BelongToGuid,
                    param.BelongToType, null, param.AttachmentTypeUID, ((BelongToTypes)param.BelongToType).ToString(), "");
                try
                {
                    var uploadRsObj = JsonConvert.DeserializeObject<AttachmentResultModel>(uploadRs);
                    if (uploadRsObj != null)
                    {
                        if (uploadRsObj.success == "true")
                        {
                            rs.Content = true;
                            rs.Success = true;
                        }
                        else
                        {
                            rs.Success = rs.Content = false;
                            rs.Message = uploadRsObj.errormessage + " " + uploadRsObj.ExceptionMessage;
                        }
                    }
                }
                catch
                {
                    var uploadRsObj = JsonConvert.DeserializeObject<AttachmentFailureResultModel>(uploadRs);
                    if (uploadRsObj != null)
                    {
                        rs.Content =
                        rs.Success = false;
                        rs.Message = uploadRsObj.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<IEnumerable<dynamic>> GetAttachmentList(Guid belongTouid, int belongToType, Guid? attachmentTypeUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<dynamic>>();
            try
            {

                rs.Success = true;


                var api = new HttpClientAgent(
                           this.AuthProvider.GetAuthenticationInfo().Token);
                //var folderMap = this.TicketAttachmentFolderRelationRepository.GetAttachmentFolderUID(belongTouid, belongToType);
                //if (folderMap.Success)
                //{
                string message = "";
                var param = api.GenerateParamerts();
                param.Belong_To_Type = belongToType;
                param.Belong_To_UID = belongTouid;
                var atype = api.GetAttachmentType(belongToType, out message);
                var folders = api.GetFolders(param, out message);
                var afolders = folders.Where(p => p.Name == ((BelongToTypes)belongToType).ToString());
                if (afolders != null)
                {
                    List<AttachmentFileViewModel> fileList = new List<AttachmentFileViewModel>();
                    foreach (var item in afolders)
                    {
                        string msg = "";

                        fileList.AddRange(api.GetAttachments(item.UID, out msg)
                            .Where(p => p.TypeUID == attachmentTypeUID || !attachmentTypeUID.HasValue)
                            .Select(p => new AttachmentFileViewModel(p)));
                    }
                    foreach (var item in fileList)
                    {
                        var t = atype.FirstOrDefault(p => p.UID == item.TypeUID);
                        if (t != null)
                        {
                            item.TypeName = t.Name;
                        }
                    }
                    rs.Content = fileList.OrderByDescending(s => s.CreatedOn);
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.TICKET_GETATTACHMENTLIST_ERROR;
                }
                //}
                //else
                //{
                //    rs.Success = false;
                //    rs.Message = "not find folders.";
                //}
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<IAttachmentFilesInfoModel> DownloadAttachment(Guid AttachmentUID)
        {

            var rs = ActionResultTemplates.Result<IAttachmentFilesInfoModel>();
            try
            {
                var api = new HttpClientAgent(
                       this.AuthProvider.GetAuthenticationInfo().Token);
                string message = "";
                var file = api.GetFileInfo(AttachmentUID, out message);
                if (message.Contains("OK"))
                {
                    AttachmentFilesInfoInnerModel e = new AttachmentFilesInfoInnerModel(file);
                    rs.Content = e;
                    rs.Success = true;
                }
                else
                {
                    rs.Message = message;
                    rs.Success = false;
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;

        }


        /// <summary>
        /// 修改slot  【限制： 一個批次只允許1種Ticket Type, 一個批次只能一種 ManifestType】
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IActionResult<dynamic> BatchChangeToSlotAPI(IBatchChangeToSlotParameter parameters)
        {
            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {
                var ticketInfos = this.TicketInfoRepository.GetList(parameters.TicketInfoUIDs.ToArray());
                if (ticketInfos.Success && ticketInfos.Content != null)
                {
                    var tickets = this.TicketRepository.GetList(new { UID = ticketInfos.Content.GroupBy(g => g.TicketUID).Select(p => p.Key) });
                    //檢查不同的tciket list 內是否有跨warehouse，目前只允許同批次同warehouse的操作
                    var checkTicekstDiffWarehouse = tickets.Content.GroupBy(g => g.WarehouseUID).Count() > 1;
                    if (checkTicekstDiffWarehouse)
                    {
                        rs.Success = false;
                        rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_TICKETWAREHOUSEINCORRECT;
                        return rs;
                    }
                    //檢查所有ticket type 是否全部都為move type
                    var checkAllMoveType = tickets.Content.All(p => p.Type == (int)TicketType.Move);
                    if (checkAllMoveType == false)
                    {
                        rs.Success = false;
                        rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_TICKETSTATUSINCORRECT;
                        return rs;
                    }
                    //檢查所有ticket status 是否全部都為 OffPosition之前
                    var checkAllMoveStatus = tickets.Content.All(p => p.Status <= (int)TicketInfoStatus.OffPosition);
                    if (checkAllMoveType == false)
                    {
                        // ticket type not move
                        rs.Success = false;
                        rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_TICKETTYPEINCORRECT;
                        return rs;
                    }
                    //取得 Slot [目前同一批的不能跨warehouse ]
                    var slot = this.TicketRepository.CheckSlotExistByTicketInfo(parameters.TicketInfoUIDs.FirstOrDefault(), parameters.SlotName);
                    if (slot.Success && slot.Content != null)
                    {
                        IActionResult<IEnumerable<IWorkOrderPayloadModel>> workOrderPayloads = null;
                        var belongtouids =
                            ticketInfos.Content.Select(t => t.WorkOrderPayloadUID.HasValue ? t.WorkOrderPayloadUID : t.WorkOrderPodUID);
                        if (tickets.Content.FirstOrDefault().ManifestType == (int)ManifestType.Inbound)
                        {
                            var workorderpods = this.WorkOrderPodRepository.GetWorkOrderPodList(new { UID = belongtouids });
                            workOrderPayloads = this.WorkOrderPayloadRepository.GetList(
                                new { WorkorderPodUID = workorderpods.Content.Select(p => p.UID) });
                        }
                        else
                        {
                            workOrderPayloads = this.WorkOrderPayloadRepository.GetList(new { UID = belongtouids });
                        }
                        if (workOrderPayloads.Success && workOrderPayloads.Content.Count() > 0)
                        {
                            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                            using (var db = this.DbEntities.DbAdapter)
                            {
                                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                                foreach (var item in workOrderPayloads.Content)
                                {
                                    if (tickets.Content.FirstOrDefault().ManifestType == (int)ManifestType.Inbound)
                                    {
                                        item.SlotUID = slot.Content.UID;
                                    }
                                    else
                                    {
                                        item.LoadingZoneSlotUID = slot.Content.UID;
                                    }
                                    Result.Add(this.WorkOrderPayloadRepository.EditPayload(new { UID = item.UID }, item));
                                }
                                if (Result.All(x => x.Success))
                                {

                                    var ticketinfoview =
                                        this.GetTicketInfo(
                                         tickets.Content.Select(p => p.UID),
                                        ticketInfos.Content.Select(p => p.UID)
                                    );
                                    rs.Content = ticketinfoview.Content;
                                    rs.Success = true;
                                    db.Commit();
                                    //scope.Complete();
                                }
                                else
                                {
                                    db.Rollback();
                                    rs.Success = false;
                                    rs.Message = string.Join(",", Result.Where(p => !p.Success).Select(x => x.Message));
                                }
                            }
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER_PAYLOAD;
                        }
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = string.Format(Resource.TICKET_MOBILE_CHANGESLOT_SLOT_INVALID, parameters.SlotName);
                        // $"Slot#{parameters.SlotName} invalid.";
                    }
                }
                else
                {
                    //not find ticket info
                    rs.Success = false;
                    rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_NOTFINDTICKETINFO;
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;

        }


        public IActionResult<dynamic> ChangeToSlot(IChangeToSlotParameter parameters)
        {

            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {
                var ticketInfo = this.TicketInfoRepository.GetData(parameters.TicketInfoUID);
                if (ticketInfo.Success && ticketInfo.Content != null)
                {
                    var ticket = this.TicketRepository.GetList(new { UID = ticketInfo.Content.TicketUID });
                    //if (ticket.Content != null && ticket.Content.FirstOrDefault().ManifestType == (int)ManifestType.Outbound)
                    //{
                    if (ticketInfo.Content.Type == (int)TicketType.Move)
                    {
                        if (ticketInfo.Content.Status <= (int)TicketInfoStatus.OffPosition)
                        {
                            var slot = this.TicketRepository.CheckSlotExistByTicketInfo(parameters.TicketInfoUID, parameters.SlotName);
                            if (slot.Success && slot.Content != null)
                            {
                                IActionResult<IEnumerable<IWorkOrderPayloadModel>> workOrderPayloads = null;
                                var belongtouid = ticketInfo.Content.WorkOrderPayloadUID.HasValue ?
                                ticketInfo.Content.WorkOrderPayloadUID : ticketInfo.Content.WorkOrderPodUID;
                                if (ticket.Content.FirstOrDefault().ManifestType == (int)ManifestType.Inbound)
                                {
                                    var workorderpod = this.WorkOrderPodRepository.GetWorkOrderPod(new { UID = belongtouid });
                                    workOrderPayloads = this.WorkOrderPayloadRepository.GetList(
                                        new { WorkorderPodUID = workorderpod.Content.UID });
                                }
                                else
                                {
                                    workOrderPayloads = this.WorkOrderPayloadRepository.GetList(new { UID = belongtouid });
                                }

                                if (workOrderPayloads.Success && workOrderPayloads.Content.Count() > 0)
                                {
                                    List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                                    using (var db = this.DbEntities.DbAdapter)
                                    {
                                        this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                                        foreach (var item in workOrderPayloads.Content)
                                        {
                                            if (ticket.Content.FirstOrDefault().ManifestType == (int)ManifestType.Inbound)
                                            {
                                                item.SlotUID = slot.Content.UID;
                                            }
                                            else
                                            {
                                                item.LoadingZoneSlotUID = slot.Content.UID;
                                            }
                                            Result.Add(this.WorkOrderPayloadRepository.EditPayload(new { UID = item.UID }, item));
                                        }


                                        if (Result.All(x => x.Success))
                                        {

                                            var ticketinfoview =
                                                this.GetTicketInfo(new Guid[] { ticket.Content.FirstOrDefault().UID },
                                                new Guid[] { ticketInfo.Content.UID });
                                            rs.Content = ticketinfoview.Content?.FirstOrDefault();
                                            rs.Success = true;
                                            // scope.Complete();
                                            db.Commit();
                                        }
                                        else
                                        {
                                            db.Rollback();
                                            rs.Success = false;
                                            rs.Message = string.Join(",", Result.Where(p => !p.Success).Select(x => x.Message));
                                        }
                                    }
                                }
                                else
                                {
                                    rs.Success = false;
                                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER_PAYLOAD;
                                }
                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = string.Format(Resource.TICKET_MOBILE_CHANGESLOT_SLOT_INVALID, parameters.SlotName);
                                // $"Slot#{parameters.SlotName} invalid.";
                            }
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_TICKETSTATUSINCORRECT;
                        }
                    }
                    else
                    {
                        // ticket type not move
                        rs.Success = false;
                        rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_TICKETTYPEINCORRECT;
                    }
                    //}
                    //else
                    //{
                    //    rs.Success = false;
                    //    rs.Message = Resource.TICKET_MOBILE_CHANGETOSLOT_TICKETMUSTOUTBOUND;
                    //}
                }
                else
                {
                    //not find ticket info
                    rs.Success = false;
                    rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_NOTFINDTICKETINFO;
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;

        }


        public IActionResult<dynamic> ChangeFromSlotAPI(IChangeFromSlotParameter parameters)
        {
            #region logical flow
            /*
            1 SlotName 是否存在
            IF true
                    1.2.是否啟用修改FromSlot
                    IF true
                        1.2.1 指定slot 是否有庫存
                            IF true
                                1.2.1.1.建立新的allocated payload
                                1.2.1.2 刪除舊的allocated payload
                        1.2.2.w.payload 是否有allocated
                        IF true(allocated 過的w.payload(有config控制是否能啟用))
                                1.2.2.1 是否允許負數配貨
                                IF true
                                    1.2.2.1.1 建立新的allocated payload
                                ELSE
                                    1.2.2.1.2 PRINT not onhand insufficient allow change from slot
                       ELSE (未allocated 過的w.payload(有config控制是否能啟用) )
                                1.2.3.1 是否允許負數配貨
                                IF true
                                    1.2.3.1.1 建立新的allocated payload
                                ELSE
                                    1.2.3.1.2 PRINT not onhand insufficient allow change from slot
                    ELSE
                        1.2.1 PRINT not allow change from slot
            ELSE
                1.1 PRINT not find slot
            */
            #endregion
            var aparam = new AllocateExecutorParameters
            {
                InventoryManager = this.InventoryManager,
                ProductUtility = this.ProductUtility,
                WorkOrderPayloadRepository = this.WorkOrderPayloadRepository,
                LabelManager = this.LabelManager,
                PackageMappingCache = this.PackageCacheManager,
                SequenceAgent = this.SequenceAgent,
                TracingAgent = this.TracingAgent
            };
            var executor = new AllocateExecutor(aparam);
            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {
                var ticketInfo = this.TicketInfoRepository.GetData(parameters.TicketInfoUID);
                if (ticketInfo.Success && ticketInfo.Content != null)
                {
                    if (ticketInfo.Content.Type == (int)TicketType.Move)
                    {
                        if (ticketInfo.Content.Status < (int)TicketInfoStatus.OffPosition)
                        {
                            var slot = this.TicketRepository.CheckSlotExistByTicketInfo(parameters.TicketInfoUID, parameters.SlotName);
                            if (slot.Success && slot.Content != null)//1
                            {

                                if (this.AppConfigure.IsChangeFromPayload)//1.2
                                {
                                    var ticket = this.TicketRepository.GetList(new { UID = ticketInfo.Content.TicketUID });
                                    IActionResult<IEnumerable<IWorkOrderPayloadModel>> workOrderPayloads = null;
                                    var belongtouid = ticketInfo.Content.WorkOrderPayloadUID.HasValue ?
                                    ticketInfo.Content.WorkOrderPayloadUID : ticketInfo.Content.WorkOrderPodUID;
                                    //入倉/倉內移動不能使用Change From
                                    if (new int[] { (int)ManifestType.Inbound, (int)ManifestType.Move }
                                    .Any(p => p == (int)ticket.Content.FirstOrDefault().ManifestType))
                                    {
                                        rs.Success = false;
                                        rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_UNALLOWED_USE;
                                    }
                                    else
                                    {
                                        workOrderPayloads = this.WorkOrderPayloadRepository.GetList(new { UID = belongtouid });
                                        if (workOrderPayloads.Success && workOrderPayloads.Content.Count() > 0)
                                        {
                                            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                                            using (var db = this.DbEntities.DbAdapter)
                                            {
                                                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                                                //change from 的slot 不能放目的地相同
                                                if (workOrderPayloads.Content.All(p => p.LoadingZoneSlotUID != slot.Content.UID))
                                                {
                                                    foreach (var item in workOrderPayloads.Content)
                                                    {
                                                        var minPkg = this.PackageCacheManager.GetMinPackage(item.PackageUID);
                                                        var param = new GetAvailableInventoryInnerListParameters();
                                                        //一律補Stock
                                                        param.Items.Add((int)PayloadType.Stock, new Guid[] { item.ItemUID });
                                                        param.SlotStatuses = new SlotStatus[] { SlotStatus.InAndOut, SlotStatus.Out };
                                                        var onhandResult = this.WarehouseManager.GetAvailableInventoryData(param);
                                                        //尋找指定slot 是否有onhand,onhand 到後面再比對
                                                        var allocatedResult = onhandResult.Content.Where(p => p.SlotUID == slot.Content.UID);
                                                        //1.2.3
                                                        //1.2.3.1
                                                        //進行Allocated
                                                        var unallocated = processAllocated(item, ticketInfo.Content, allocatedResult, executor);
                                                        if (unallocated.Success)
                                                        {
                                                            Result.Add(unallocated);
                                                        }
                                                        if (item.PayloadUID != Guid.Empty && (this.AppConfigure.IsAllowNegativeOnhandByFixFailure) ||
                                                            (item.PayloadUID == Guid.Empty && this.AppConfigure.IsAllowNegativeOnhandByChangeFromPayload))
                                                        {
                                                            if (!unallocated.Success)//因選擇的slot 並沒有庫存故使用負庫存方式處理
                                                            {
                                                                //Result.Add(this.PayloadRepository.ChangePayloadType(item.PayloadUID,
                                                                //    (int)PayloadType.Onhand));
                                                                item.SlotUID = slot.Content.UID;
                                                                item.PayloadPackageUID = item.PackageUID;
                                                                var newPayloadUID = executor.ForceAllocated(
                                                                    item.PackageUID, item.ItemUID, item.Qty, item.SlotUID.Value,
                                                                    allocatedResult.Select(p => p.UID), Guid.Empty,
                                                                    item.UID);
                                                                if (newPayloadUID.Success)
                                                                {
                                                                    item.PayloadUID = newPayloadUID.Content;
                                                                    Result.Add(this.WorkOrderPayloadRepository
                                                                                    .EditPayload(new { UID = item.UID }, item));
                                                                }
                                                                else
                                                                {
                                                                    var r = ActionResultTemplates.OK();
                                                                    r.Success = false;
                                                                    r.Message = newPayloadUID.Message;
                                                                    Result.Add(r);
                                                                }

                                                            }
                                                            else
                                                            {
                                                                Result.Add(unallocated);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            var r = ActionResultTemplates.OK();
                                                            r.Success = false;
                                                            r.Message = Resource.TICKET_MOBILE_CHANGESLOT_UNALLOWED_NEGATIVE;
                                                            Result.Add(r);
                                                        }

                                                        #region 舊的寫法
                                                        //if (item.PayloadUID != Guid.Empty)//1.2.1 是否使用Change from slot 
                                                        //{
                                                        //    //1.2.3.1
                                                        //    if (!unallocated.Success)//因選擇的slot 並沒有庫存故使用負庫存方式處理
                                                        //    {

                                                        //        // 1.2.3.1.2
                                                        //        if (this.AppConfigure.IsAllowNegativeOnhandByChangeFromPayload)
                                                        //        {
                                                        //            //Result.Add(this.PayloadRepository.ChangePayloadType(item.PayloadUID,
                                                        //            //    (int)PayloadType.Onhand));
                                                        //            item.SlotUID = slot.Content.UID;
                                                        //            item.PayloadPackageUID = item.PackageUID;
                                                        //            var newPayloadUID = executor.ForceAllocated(
                                                        //                item.PackageUID, item.ItemUID, item.Qty, item.SlotUID.Value,
                                                        //                allocatedResult.Select(p => p.UID), Guid.Empty);
                                                        //            if (newPayloadUID.Success)
                                                        //            {
                                                        //                item.PayloadUID = newPayloadUID.Content;
                                                        //                Result.Add(this.WorkOrderPayloadRepository
                                                        //                                .EditPayload(new { UID = item.UID }, item));
                                                        //            }
                                                        //            else
                                                        //            {
                                                        //                var r = ActionResultTemplates.OK();
                                                        //                r.Success = false;
                                                        //                r.Message = newPayloadUID.Message;
                                                        //                Result.Add(r);
                                                        //            }
                                                        //        }
                                                        //        else
                                                        //        {
                                                        //            var r = ActionResultTemplates.OK();
                                                        //            r.Success = false;
                                                        //            r.Message = Resource.TICKET_MOBILE_CHANGESLOT_UNALLOWED_NEGATIVE;
                                                        //            Result.Add(r);
                                                        //        }
                                                        //    }
                                                        //    else
                                                        //    {
                                                        //        Result.Add(unallocated);
                                                        //    }
                                                        //}
                                                        //else
                                                        //{
                                                        //    if (!unallocated.Success)//因選擇的slot 並沒有庫存故使用負庫存方式處理
                                                        //    {
                                                        //        //1.2.3.2.2
                                                        //        if (this.AppConfigure.IsAllowNegativeOnhandByFixFailure)
                                                        //        {
                                                        //            var originalwpayload = this.GetWorkOrderPayload(new { UID = item.SeparateByUID.Value })
                                                        //                                    .Content.FirstOrDefault();
                                                        //            item.SlotUID = slot.Content.UID;
                                                        //            item.PayloadPackageUID = item.PackageUID;
                                                        //            var newPayloadUID = executor.ForceAllocated(
                                                        //                item.PackageUID, item.ItemUID, item.Qty, item.SlotUID.Value,
                                                        //                allocatedResult.Select(p => p.UID), originalwpayload.PayloadUID);
                                                        //            if (newPayloadUID.Success)
                                                        //            {
                                                        //                item.PayloadUID = newPayloadUID.Content;
                                                        //                Result.Add(this.WorkOrderPayloadRepository
                                                        //                                .EditPayload(new { UID = item.UID }, item));
                                                        //            }
                                                        //            else
                                                        //            {
                                                        //                var r = ActionResultTemplates.OK();
                                                        //                r.Success = false;
                                                        //                r.Message = newPayloadUID.Message;
                                                        //                Result.Add(r);
                                                        //            }
                                                        //        }
                                                        //        else
                                                        //        {
                                                        //            var r = ActionResultTemplates.OK();
                                                        //            r.Success = false;
                                                        //            r.Message = Resource.TICKET_MOBILE_CHANGESLOT_UNALLOWED_NEGATIVE;
                                                        //            Result.Add(r);
                                                        //        }
                                                        //    }
                                                        //    else
                                                        //    {
                                                        //        Result.Add(unallocated);
                                                        //    }
                                                        //}
                                                        #endregion

                                                    }
                                                    if (Result.All(x => x.Success))
                                                    {

                                                        var ticketinfoview =
                                                            this.GetTicketInfo(new Guid[] { ticket.Content.FirstOrDefault().UID },
                                                            new Guid[] { ticketInfo.Content.UID });
                                                        rs.Content = ticketinfoview.Content.FirstOrDefault();
                                                        rs.Success = true;
                                                        //scope.Complete();
                                                        db.Commit();
                                                    }
                                                    else
                                                    {
                                                        db.Rollback();
                                                        rs.Success = false;
                                                        rs.Message = string.Join(",", Result.Where(p => !p.Success).Select(x => x.Message));
                                                    }
                                                }
                                                else
                                                {
                                                    db.Rollback();
                                                    rs.Success = false;
                                                    rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_FROMSLOT_CANNOT_SAME;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            rs.Success = false;
                                            rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER_PAYLOAD;
                                        }
                                    }
                                }
                                else//1.1
                                {
                                    rs.Success = false;
                                    rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_UNALLOWED_USE_CHANGEFROM;
                                }
                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = string.Format(Resource.TICKET_MOBILE_CHANGESLOT_SLOT_INVALID, parameters.SlotName);
                            }
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_TICKETSTATUSINCORRECT;
                        }
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_TICKETSTATUSINCORRECT;
                    }

                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.TICKET_MOBILE_CHANGESLOT_NOTFINDTICKETINFO;
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        private IActionResult<bool> processAllocated(IWorkOrderPayloadModel witem, ITicketInfoModel ticketInfoModel,
            IEnumerable<ILocationItemViewModel> allocatedResult, AllocateExecutor executor)
        {
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            //先將目前allocated payload deallocated
            if (witem.PayloadUID != Guid.Empty)
            {
                //1.2.3.1.2 把原本的payload 轉成onhand
                var ticketInfo = this.TicketRepository.GetList(new { UID = ticketInfoModel.TicketUID }).Content.FirstOrDefault();
                Result.Add(
                   this.DeallocatedByWorkOrderPayload(
                   ticketInfo.WarehouseUID, new IWorkOrderPayloadModel[] { witem },
                    new ITicketModel[] { ticketInfo }, new ITicketInfoModel[] { ticketInfoModel }));
            }
            if (allocatedResult.Count() > 0)
            {
                //1.2.3.1.1/1.2.3.2.1
                List<ILocationItemViewModel> ixpayload = new List<ILocationItemViewModel>();

                GetAllocatedList(allocatedResult, ixpayload, witem.PackageUID, witem.Qty);
                IActionResult<IPayloadModel> newPayload = null;
                if (ixpayload.Count > 0)
                {

                    if (ixpayload.Count == 1)//比對onhand 是否充足，如果取得的結果為一筆，則allocated該payload 
                    {
                        //只會先將相關資料先接起來，之後再去allocated
                        newPayload = this.PayloadRepository.GetPayload(ixpayload.FirstOrDefault().UID);
                    }
                    else
                    {
                        // 若回傳的onhand payload 多筆則將原本 onhand payload 刪除
                        // 合併成一筆新的payload(這將會破壞payload的原始記錄)
                        foreach (var item in ixpayload)
                        {
                            this.InventoryManager.ChangePayloadStauts(item.UID, PayloadStatus.Inactive);
                        }
                        var pkg = this.PackageCacheManager.GetPackage(witem.PackageUID);
                        newPayload.Content = new PayloadInnerModel();
                        newPayload.Content.UID = Guid.NewGuid();
                        newPayload.Content.ItemUID = witem.ItemUID;
                        newPayload.Content.PackageUID = witem.PackageUID;
                        newPayload.Content.Quantity = witem.Qty;
                        newPayload.Content.SlotUID = allocatedResult.FirstOrDefault().SlotUID;
                        newPayload.Content.Status = (int)PayloadStatus.Active;
                        newPayload.Content.Type = (int)PayloadType.Stock;
                        newPayload.Content.VolumeLimit = this.ProductUtility.CalculateCBM(pkg, witem.Qty);
                        newPayload.Content.WeightLimit = this.ProductUtility.CaculateTTLWeight(pkg, witem.Qty);
                        newPayload.Content.Description = $"Merge payload from {string.Join(",", ixpayload.Select(p => p.UID))}";
                        Result.Add(this.InventoryManager.AddPayload(newPayload.Content));
                    }
                    witem.PayloadUID = newPayload.Content.UID;
                    witem.PayloadPackageUID = newPayload.Content.PackageUID;
                    witem.SlotUID = newPayload.Content.SlotUID;
                    //先修改資料才能allocated
                    Result.Add(this.WorkOrderPayloadRepository.EditPayload(new { UID = witem.UID }, witem));
                    Result.Add(executor.BatchExecuteAllocated(new IWorkOrderPayloadModel[] { witem }));
                    if (Result.All(x => x.Success))
                    {
                        rs.Content = true;
                        rs.Success = true;
                    }
                    else
                    {
                        rs.Content = false;
                        rs.Success = false;
                        rs.Message = string.Join(",", Result.Where(p => !p.Success).Select(x => x.Message));
                    }
                }
                else
                {
                    //未配置onhand
                    rs.Success = false;
                    rs.Content = true;
                }
            }
            else
            {
                //未配置onhand
                rs.Success = false;
                rs.Content = true;
            }
            return rs;
        }

        private void GetAllocatedList(IEnumerable<ILocationItemViewModel> onHandResult,
            List<ILocationItemViewModel> allocatedResult, Guid packageUID, int qty)
        {

            foreach (var item in onHandResult)
            {
                if (qty != 0)
                {
                    if (this.CompareOnhand(item.OriginalPackageUID, packageUID, item.Quantity, qty))
                    {
                        allocatedResult.Add(item);
                        qty = 0;
                        break;
                    }
                    else
                    {
                        var minpkg = this.PackageCacheManager.GetMinPackage(item.OriginalPackageUID);
                        var onhand = this.PackageCacheManager
                                        .GetReceivePackageUomQuantity(item.OriginalPackageUID, minpkg.UID, item.Quantity);
                        qty -= onhand.Content;
                        allocatedResult.Add(item);
                    }
                }
            }
            if (qty != 0)
                allocatedResult.Clear();
        }


        public IActionResult<IEnumerable<IPodBarcodeInfo>> GetReceivingQtyBarcodeInfo(ICheckPodBarcodeInfoParameters parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPodBarcodeInfo>>();
            try
            {
                parameters.LabelType = (int)LabelType.Pallet_OrginalTracking;
                //rs = this.WorkOrderPayloadRepository.GetPodBarcodeInfo(parameters);
                rs = this.WorkOrderPayloadRepository.GetReceivingQtyBarcodeInfo(parameters);
                foreach (var item in rs.Content)
                {
                    var minPkg = this.PackageCacheManager.GetMinPackage(item.PackageUID);
                    if (minPkg != null)
                    {
                        var minPkgQty = this.PackageCacheManager.GetReceivePackageUomQuantity(item.PackageUID, minPkg.UID, item.Qty);
                        if (minPkgQty.Success)
                        {
                            item.Qty = minPkgQty.Content;
                        }
                    }
                    else
                    {
                        item.Qty = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<dynamic> CompleteTicketData(String[] TicketIDs)
        {
            var rs = ActionResultTemplates.Result<dynamic>();
            rs.Content = this.GeneratoreObject();

            if (TicketIDs != null && TicketIDs.Length > 0)
            {
                List<IActionResult<bool>> results = new List<IActionResult<bool>>();
                try
                {
                    var _parameter = this.GetTicketProcessAgentParameter();
                    var agent = AbstractProcessAgent.GetAgent(Constant.ProcessKind.TicketProcess, _parameter);
                    //根據TicketID找出Ticket & TicketInfo
                    GetTicketInfoParameters parameters = new GetTicketInfoParameters();
                    parameters.TicketIDs = TicketIDs;
                    var collection = GetInfoData(parameters).Content;

                    if (collection != null && collection.Count() > 0)
                    {
                        TicketType ticket_type;
                        List<UploadTicketDataInnerParameter> update_parameters_list = null;
                        UploadTicketDataInnerParameter update_parameters = null;



                        List<int> excute_order = new List<int>() { (int)TicketType.Receiving, (int)TicketType.Move, (int)TicketType.Outbound };
                        var excute_order_list = collection
                            .GroupBy(p => new { p.TicketID, p.TicketType })
                            .Select(y => new
                            {
                                TicketID = y.Key.TicketID,
                                TicketType = y.Key.TicketType
                            })
                            .OrderBy(g => excute_order.IndexOf(g.TicketType))
                            .ThenBy(h => h.TicketID)
                            .ToList();
                        //根據TicketType & TicketID 的　GroupList
                        foreach (var excute_order_item in excute_order_list)
                        {
                            //依Ticket Type 設定參數並執行
                            ticket_type = (TicketType)excute_order_item.TicketType;

                            var Info_list = collection.Where(p => p.TicketID.Equals(excute_order_item.TicketID)).ToList();
                            if (Info_list != null && Info_list.Count > 0)
                            {
                                switch (ticket_type)
                                {
                                    case TicketType.Receiving:
                                    case TicketType.Outbound:
                                        update_parameters_list = new List<UploadTicketDataInnerParameter>();
                                        foreach (var item in Info_list)
                                        {
                                            update_parameters = new UploadTicketDataInnerParameter();
                                            update_parameters.ServiceItem = ticket_type;
                                            update_parameters.Item.ActQty = item.EstQty;
                                            update_parameters.Item.IsAllPass = true;
                                            update_parameters.Item.TicketInfoUID = item.UID;
                                            update_parameters_list.Add(update_parameters);
                                        }
                                        results.Add(agent.Process(update_parameters_list));
                                        break;
                                    default:
                                        foreach (var item in Info_list)
                                        {
                                            update_parameters = new UploadTicketDataInnerParameter();
                                            update_parameters.ServiceItem = ticket_type;
                                            update_parameters.Item.ActQty = item.EstQty;
                                            update_parameters.Item.IsAllPass = true;
                                            update_parameters.Item.TicketInfoUID = item.UID;
                                            results.Add(agent.Process(new UploadTicketDataInnerParameter[] { update_parameters }));
                                        }
                                        break;
                                }
                            }
                        }

                        rs.Success = results.All(p => p.Success);
                        if (!rs.Success)
                        {
                            rs.Message = string.Join(",", results.Select(x => x.Message));
                            rs.Content.IsComplete = false;
                            rs.Content.Message = rs.Message;
                        }
                        else
                        {
                            rs.Content.IsComplete = true;
                            rs.Content.Message = rs.Message;
                            rs.Success = true;
                        }

                    }
                }
                catch (Exception ex)
                {
                    this.TracingAgent.Trace(ex.Message, ex);
                    rs.Message = Resource.COMMON_RETRY;
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    rs.InnerException = ex;
                }
            }

            return rs;
        }
    }
}
