using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.BLL.Module;
using System.Transactions;
using System.Diagnostics;
using YAEP.Core.Party.Interfaces;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.NotificationReceiver.Common;
using Newtonsoft.Json;
using System.Reflection.Emit;
using YAEP.WMS.BLL.ShippingService;
using YAEP.WMS.Cache.Redis;
using System.Data.SqlClient;
using YAEP.Identities.Constants;
using General.Data.SQLConditionConverter;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IOrderManager
    {
        #region 配貨(Allocated)
        public IActionResult<IAllocatedResponse> Allocated(IAllocatedRequest request)
        {
            var rs = ActionResultTemplates.Result<IAllocatedResponse>();
            if (!this.RequestManager.IsRequestProcessing(RequestAction.ALLOCATED, request.RefNo))
            {
                this.RequestManager.AddRequest(RequestAction.ALLOCATED, request.RefNo);
                IActionResult<IEnumerable<IAssignedTicketInfoModel>> tIcollection = null;
                List<ReplicateDataParameter> rparams = new List<ReplicateDataParameter>();
                var param = new MaintainWorkderInnerParameters();
                List<Func<IActionResult<bool>>> syncMethod = new List<Func<IActionResult<bool>>>();
                var newrequest = new AllocatedRequest(request);
                Stopwatch sw = new Stopwatch();
                var groups = this.GetGroupUserViewByUser();
                ProductCacheManager productManagerCache = this.ProductCacheManager;
                ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
                List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                OutboundAutoAssignedResult responseAllocated = new OutboundAutoAssignedResult();
                var customerUIDs = new List<Guid>();

                //先不使用Customer id 判斷
                //if (!string.IsNullOrEmpty(request.CustomerPartyName))
                //    customerUIDs.AddRange(this.GetCustomer(groups.Content.Select(x => x.GroupUID),
                //        request.CustomerPartyName).Select(p => p.UID));
                //else
                customerUIDs.Add(request.CustomerUID);

                if (customerUIDs.Count() > 0)
                {
                    try
                    {


                        IManifestModel manifestModel = null;
                        rs.Content = responseAllocated.Response;
                        rs.Success = true;
                        List<string> notFinditem = new List<string>();
                        List<string> duplicateitem = new List<string>();
                        List<string> nothavepkg = new List<string>();
                        ConcurrentBag<ItemInfo> items = new ConcurrentBag<ItemInfo>();
                        List<IManifestItemListModel> manifestItems = new List<IManifestItemListModel>();
                        List<IVesselManifestModel> vesselManifestModels = new List<IVesselManifestModel>();
                        List<IActionResult<bool>> allocatedResult = new List<IActionResult<bool>>();
                        List<IAllocatedItemRequest> requestItemByVirtualItem = new List<IAllocatedItemRequest>();
                        //itemNo convert item Model

                        using (var a1 = this.TracingAgent.StartActivity($"collect item info"))
                        {
                            var pgrp = request.Items.GroupBy(p => p.ItemNo);
                            Parallel.ForEach(pgrp, item =>
                          {
                              ItemInfo i = new ItemInfo();
                              var itemOriginals = productManagerCache.GetItem(item.Key, customerUIDs, groups.Content);
                              if (itemOriginals != null && itemOriginals.Count() > 1)//是否有重覆的Item#
                              {
                                  duplicateitem.Add(item.Key);
                              }
                              if (itemOriginals != null && itemOriginals.Count() == 1)
                              {
                                  //TODO 改讀取VersionUID 取得最小包裝
                                  var itemOrg = itemOriginals.FirstOrDefault();
                                  var pkgcollection = this.PackageCacheManager.GetPackagesByItem(itemOrg.UID);
                                  if (pkgcollection != null && pkgcollection.Count() > 0) //是否設定包裝
                                  {
                                      //RETEST 取得最新版本全部包裝並取得該版本最小包裝
                                      var latestPkg = pkgcollection.GroupBy(g => g.VersionUID)
                                        .OrderByDescending(o => o.OrderByDescending(o1 => o1.CreatedOn).First().CreatedOn)
                                        .Select(p => p).FirstOrDefault();
                                      i.Item = itemOrg;
                                      i.Package = this.PackageCacheManager.GetMinPackage(latestPkg);

                                      //取得虛擬Item
                                      var virtualItems = this.ProductCacheManager.GetVirtualItems(itemOrg.Name, customerUIDs);
                                      if (virtualItems.Content?.Count() > 0)
                                      {
                                          //TODO 有發生回傳虛擬產品資料有缺，需加上驗証
                                          foreach (var subitem in item)
                                          {
                                              var _itemGrp = Guid.NewGuid();
                                              subitem.VirtualItems = virtualItems.Content.Select(p => p as IItemModel).ToList();
                                              subitem.VirtualItemParentItemNo = subitem.ItemNo;
                                              subitem.IsHasVirtualItem = true;
                                              subitem.ItemGroupUID = _itemGrp;
                                          }
                                          i.VirtualItems = virtualItems.Content;

                                          foreach (var subitem in item)
                                          {

                                              foreach (var viitem in virtualItems.Content)
                                              {
                                                  var allocatedvirtualItem = new AllocatedItemRequest();
                                                  allocatedvirtualItem.ItemGroupUID = subitem.ItemGroupUID;
                                                  allocatedvirtualItem.Carrier = subitem.Carrier;
                                                  allocatedvirtualItem.ManifestItemUID = subitem.ManifestItemUID;
                                                  allocatedvirtualItem.ComponentType = subitem.ComponentType;
                                                  allocatedvirtualItem.VesselManifestUID = subitem.VesselManifestUID;
                                                  allocatedvirtualItem.ItemNo = viitem.Name;
                                                  allocatedvirtualItem.Line_No = subitem.Line_No;
                                                  allocatedvirtualItem.PalletBarcode = subitem.PalletBarcode;
                                                  allocatedvirtualItem.PalletID = subitem.PalletID;
                                                  allocatedvirtualItem.ParentItemNo = subitem.ParentItemNo;
                                                  allocatedvirtualItem.Qty = subitem.Qty;
                                                  allocatedvirtualItem.ShipPackageUID = subitem.ShipPackageUID;
                                                  allocatedvirtualItem.ShipViaNo = subitem.ShipViaNo;
                                                  allocatedvirtualItem.UseMiniPackage = subitem.UseMiniPackage;
                                                  allocatedvirtualItem.VesselManifestUID = subitem.VesselManifestUID;
                                                  allocatedvirtualItem.VirtualItemParentItemNo = subitem.ItemNo;
                                                  allocatedvirtualItem.ShipViaUID = subitem.ShipViaUID;
                                                  allocatedvirtualItem.AllocatedOnhandType = subitem.AllocatedOnhandType;
                                                  requestItemByVirtualItem.Add(allocatedvirtualItem);

                                                  ItemInfo vii = new ItemInfo();
                                                  vii.Item = viitem;
                                                  var _pkgcollection = this.PackageCacheManager.GetPackagesByItem(viitem.UID);
                                                  var _latestPkg = _pkgcollection.GroupBy(g => g.VersionUID)
                                                  .OrderByDescending(o => o.OrderByDescending(o1 => o1.CreatedOn).First().CreatedOn)
                                                  .Select(p => p).FirstOrDefault();
                                                  vii.Package = this.PackageCacheManager.GetMinPackage(_latestPkg);
                                                  items.Add(vii);
                                              }
                                          }
                                      }
                                      else
                                      {
                                          virtualItems.Content = new List<ProductExtendModel>();
                                      }

                                      items.Add(i);
                                  }
                                  else
                                  {
                                      nothavepkg.Add(item.Key);
                                  }
                              }
                              if (itemOriginals == null || (itemOriginals != null && itemOriginals.Count() == 0)) //item 是否存在
                              {
                                  notFinditem.Add(item.Key);
                              }

                          });
                        }
                        //todo 檢查request 是否已經有配貨過的line#
                        if (notFinditem.Count == 0 && duplicateitem.Count == 0 && nothavepkg.Count == 0)
                        {
                            //this.TracingAgent.Trace("Ready to Allocated", request.RefNo);
                            this.LogConnectionSPID();
                            #region Manifest 
                            //if manifest exist
                            var manifestInfo = this.Repository.GetData(new
                            {
                                RefNo = request.RefNo.ToNvarchar(),
                                //PartyUID = request.CustomerUID, //SO# +WarehouseUID 應該可以判定這個manifest唯一性
                                WarehouseUID = request.WarehouseUID
                            });
                            if (manifestInfo.Content != null)//manifest exist
                            {
                                manifestModel = manifestInfo.Content;
                            }
                            else  //manifest unexist
                            {
                                using (var a2 = this.TracingAgent.StartActivity("prepare manifestinfo "))
                                {
                                    var _seq = this.SequenceAgent.GetManinfestSequence(
                                    SequenceAgent.GetManifestRootUID(), ManifestType.Outbound);
                                    // create manifest
                                    ManinfestInnerModel model = new ManinfestInnerModel();
                                    model.UID = Guid.NewGuid();
                                    model.ID = _seq;
                                    model.Name = request.CustomerPartyName;
                                    model.RefNo = request.RefNo;
                                    model.PartyUID = customerUIDs.FirstOrDefault();//TODO 暫定
                                    model.Status = ManifestStatus.Open;
                                    model.Type = (int)ManifestType.Outbound;
                                    model.WarehouseUID = request.WarehouseUID;
                                    model.Volume = 0;
                                    model.Weight = 0;
                                    model.CreatedBy = request.RequestBy;
                                    model.CreatedOn = DateTime.UtcNow;
                                    _action.Push(() => this.Repository.Add(model));
                                    manifestModel = model;
                                }
                            }
                            #endregion
                            #region  manifest item

                            //created mainifest item 
                            var itemCollection = request.Items
                             .Where(p => !p.IsHasVirtualItem)
                            .GroupBy(p => new
                            {
                                ItemNo = p.ItemNo,
                                LineNo = p.Line_No,
                                OnhandType = p.AllocatedOnhandType
                            }).ToList();
                            itemCollection.AddRange(requestItemByVirtualItem.GroupBy(p => new
                            {
                                ItemNo = p.ItemNo,
                                LineNo = p.Line_No,
                                OnhandType = p.AllocatedOnhandType
                            }));
                            using (var a3 = this.TracingAgent.StartActivity("prepare manifestinfo item"))
                            {

                                var _miseq = this.SequenceAgent.GetMainfestItemListSequence(
                                    SequenceAgent.GetManifestRootUID(), itemCollection.Count());
                                foreach (var item in itemCollection)
                                {
                                    var itemInfo = items.FirstOrDefault(p => p.Item.Name == item.Key.ItemNo);
                                    if (itemInfo != null)
                                    {
                                        //if (item.First().VirtualItems.Count == 0)
                                        //{
                                        ManifestItemInnerModel m = new ManifestItemInnerModel();
                                        m.UID = Guid.NewGuid();
                                        m.ID = _miseq.Dequeue();
                                        m.ItemUID = itemInfo.Item.UID;
                                        m.ManifestUID = manifestModel.UID;
                                        m.PackageQty = item.Sum(x => x.Qty);
                                        m.PackageUID = itemInfo.Package.UID;
                                        m.OnhandType = (item.Key.OnhandType == 0) ? (int)PayloadType.Stock : item.Key.OnhandType;
                                        m.Name = item.Key.LineNo.ToString();
                                        m.Volume = this.ProductUtility.CalculateCUFT(itemInfo.Package, item.Sum(x => x.Qty));
                                        m.Weight = this.ProductUtility.CaculateTTLWeight(itemInfo.Package, item.Sum(x => x.Qty));
                                        m.Status = ManifestItemListStatus.Open;
                                        m.CreatedBy = request.RequestBy;
                                        m.CreatedOn = DateTime.UtcNow;
                                        foreach (var mIitem in item)
                                        {
                                            mIitem.ManifestItemUID = m.UID;
                                        }
                                        manifestItems.Add(m);


                                    }
                                    else
                                    {
                                        //?
                                    }
                                }
                            }
                            _action.Push(() => this.ManifestItemListRepository.Add(manifestItems));
                            #endregion
                            List<IBolModel> bolModels = new List<IBolModel>();
                            List<IVesselModel> vesselModels = new List<IVesselModel>();
                            #region create bol
                            List<IVesselManifestModel> vesselManifestCollection = new List<IVesselManifestModel>();
                            var groupbyBol = request.Items.GroupBy(p => p.ShipViaUID);
                            var _bseq = this.SequenceAgent.GetBOLSeqence(SequenceAgent.GetManifestRootUID(), groupbyBol.Count());
                            //Check bol exist 偶爾會發生重覆allocated 尚未找到原因，先判斷阻擋

                            var bollist = groupbyBol.Select(g => "BOL-" + request.RefNo + "-" + g.First().ShipViaNo);
                            //var parm = new BolSearchInnerParameters();
                            //parm.RefNo = bollist;
                            //parm.ManifestUID = null;
                            //var existBols = this.BolRepository.GetList(parm);
                            var existBols = this.BolRepository.GetBolRefNo(bollist);

                            using (var a4 = this.TracingAgent.StartActivity("prepare bol/vessel "))
                            {
                                if (existBols.Content.Count() > 0)
                                {
                                    var responseItem = new AllocatedItemInnerResponse();
                                    responseItem.ComponentType = (int)AllocatedComponentType.DataExist;
                                    rs.Content.Results.Add(responseItem);
                                    rs.Success = false;
                                    rs.Content.IsComplete = false;
                                    rs.Content.Message =
                                    rs.Message = $"Bol:{string.Join(",", existBols.Content.Select(p => p))} have exist";
                                    return rs;
                                }
                                Queue<string> _vseqBatch, _viseq;
                                var vesselTTL = itemCollection.SelectMany(g => g)
                                      .GroupBy(p => p.PalletID).OrderBy(o => o.Key);
                                //using (var act3 = this.TracingAgent.StartActivity("get vessl/vessl manifest sequence "))
                                //{
                                _vseqBatch = this.SequenceAgent.GetVesselSeqence(SequenceAgent.GetManifestRootUID(),
                               vesselTTL.Select(p => p.Key).Count());
                                _viseq = this.SequenceAgent.GetVesselManifestSequence(SequenceAgent.GetManifestRootUID(),
                              itemCollection.SelectMany(g => g).Count());
                                //}
                                //using (var act4 = this.TracingAgent.StartActivity("mapping vessl/vessl"))
                                //{
                                foreach (var bol in groupbyBol)
                                {

                                    BolInnerModel bolModel = new BolInnerModel();
                                    bolModel.UID = Guid.NewGuid();
                                    bolModel.ID = _bseq.Dequeue();
                                    bolModel.Name = "BOL-" + request.RefNo + "-" + bol.First().ShipViaNo;
                                    bolModel.RefNo = "BOL-" + request.RefNo + "-" + bol.First().ShipViaNo;
                                    bolModel.ManifestUID = manifestModel.UID;
                                    bolModel.ShipViaUID = Guid.Empty;
                                    bolModel.ShipMethodUID = Guid.Empty;
                                    bolModel.Status = BolStatus.Open;
                                    bolModel.ETA = request.ETD;
                                    bolModel.ShipToAddress = request.ShipToAddress;
                                    bolModel.ShipToCity = request.ShipToCity;
                                    bolModel.ShipToCountry = request.ShipToCountry;
                                    bolModel.ShipToState = request.ShipToState;
                                    bolModel.ShipToZip = request.ShipToZip;
                                    bolModel.CreatedBy = request.RequestBy;
                                    bolModel.CreatedOn = DateTime.UtcNow;
                                    if (request.UsePackingStation && bol.First().Carrier.HasValue)
                                    {
                                        var partyUID = GetShipviaUIDByCarrier(bol.First().Carrier.Value);
                                        if (partyUID.HasValue)
                                            bolModel.ShipViaUID = partyUID.Value;
                                    }
                                    _action.Push(() => this.BolRepository.AddBol(bolModel));
                                    bolModels.Add(bolModel);
                                    var vesselItemSource = itemCollection.SelectMany(g => g).Where(p => p.ShipViaUID == bol.Key)
                                        .GroupBy(p => p.PalletID).OrderBy(o => o.Key);

                                    _vseqBatch = this.SequenceAgent.GetVesselSeqence(SequenceAgent.GetManifestRootUID(),
                                   vesselItemSource.Count());
                                    foreach (var pallet in vesselItemSource)
                                    {
                                        #region create vessel

                                        var _vseq = _vseqBatch.Dequeue();
                                        VesselInnerModel vesselModel = new VesselInnerModel();
                                        vesselModel.UID = Guid.NewGuid();
                                        vesselModel.ID = _vseq;
                                        vesselModel.Name = $"Vessel {request.RefNo}-{pallet.Key}";
                                        vesselModel.RefNo = pallet.FirstOrDefault().PalletBarcode;
                                        vesselModel.Type = 1;
                                        vesselModel.BolUID = bolModel.UID;
                                        vesselModel.Status = (int)VesselStatus.Open;
                                        vesselModel.CreatedBy = request.RequestBy;
                                        vesselModel.CreatedOn = DateTime.UtcNow;
                                        vesselModels.Add(vesselModel);

                                        #region create vessel manifest item
                                        // Queue<string> _viseq;

                                        // _viseq = this.SequenceAgent.GetVesselManifestSequence(SequenceAgent.GetManifestRootUID(),
                                        //pallet.Count());

                                        foreach (var item in pallet)
                                        {

                                            var mitemInfo = manifestItems.FirstOrDefault(p => p.UID == item.ManifestItemUID);
                                            var itemInfo = items.FirstOrDefault(p => p.Item.UID == mitemInfo.ItemUID);
                                            VesselManifestItemInnerModel vitem = new VesselManifestItemInnerModel();

                                            vitem.UID = Guid.NewGuid();
                                            vitem.ID = _viseq.Dequeue();
                                            vitem.ItemUID = mitemInfo.ItemUID;
                                            vitem.ItemGroupUID = item.ItemGroupUID;
                                            vitem.ManifestItemUID = mitemInfo.UID;
                                            vitem.OnhandType = mitemInfo.OnhandType;
                                            vitem.VesselUID = vesselModel.UID;
                                            vitem.BolUID = bolModel.UID;
                                            vitem.CreatedBy = request.RequestBy;
                                            vitem.Status = (int)VesselManifestStatus.Open;
                                            vitem.Qty = item.Qty; //mitemInfo.PackageQty.Value;
                                            vitem.Volume = this.ProductUtility.CalculateCUFT(itemInfo.Package, vitem.Qty);
                                            vitem.Weight = this.ProductUtility.CaculateTTLWeight(itemInfo.Package, vitem.Qty);
                                            vitem.PackageUID = mitemInfo.PackageUID;
                                            vitem.CreatedBy = request.RequestBy;
                                            vitem.CreatedOn = DateTime.UtcNow;
                                            vesselManifestModels.Add(vitem);
                                            vesselManifestCollection.Add(vitem);
                                            //_action.Push(() => this.VesselManifestRepository.AddVesselManifest(vitem));
                                            item.VesselManifestUID = vitem.UID;

                                        }

                                        #endregion
                                        #endregion
                                    }
                                }
                                //}
                            }
                            #endregion
                            _action.Push(() => this.VesselRepository.BatchAddVessel(vesselModels));
                            _action.Push(() => this.VesselManifestRepository.BatchAddVesselManifest(vesselManifestCollection));
                            //重新組合request 物件
                            var newrequestCollection = newrequest.Items.ToList();
                            newrequestCollection.AddRange(itemCollection.SelectMany(p => p));
                            newrequest.Items = newrequestCollection;
                            //allocate flow

                            #region allocated model paramters init
                            var provider = new AutoAssignAgentProviders()
                            {
                                PartyManager = this.PartyManager,
                                PackageManager = this.PackageManager,
                                PackageUomManager = this.PackageUomManager,
                                VesselManifestRepository = this.VesselManifestRepository,
                                WarehouseManager = this.WarehouseManager,
                                BolManager = this,
                                ManifestManager = this,
                                VesselManager = this,
                                WorkOrderAssignAgentParameters = this.GetWorkOrderAgentParameters(),
                                TicketRepository = this.TicketRepository,
                                TicketRelationRepository = this.TicketRelationRepository,
                                TicketInfoRepository = this.TicketInfoRepository,
                                LabelRepository = this.LabelRepository,
                                ItemManager = this.ItemManager,
                                WorkOrderManager = this.WorkOrderManager,
                                ProductCacheManager = this.ProductCacheManager,
                                PackageVersionManager = this.PackageVersionManager,
                                PackageCacheManager = this.PackageCacheManager,
                                TracingAgent = this.TracingAgent,
                                PackageVersionRepository = this.PackageVersionRepository,
                                DbEntities = this.DbEntities
                            };
                            #endregion
                            //var autoAssignAgent = new ExternalOutboundAutoAssignAgent(provider);
                            var autoAssignAgent = new ExternalOutboundFullAllocatedAutoAssignAgent(provider);
                            var parameters = new OutboundAutoAssignedParameters();
                            parameters.OutboundRequest = newrequest;
                            parameters.Manifest = manifestModel;
                            parameters.ManifestItems = manifestItems;
                            parameters.Vessel = vesselModels;
                            parameters.VesselItems = vesselManifestModels;
                            parameters.Bol = bolModels;
                            parameters.ForceWorkOrderOpen = true;
                            parameters.PassPackageVersion = request.PassPackageVersion;
                            parameters.ManifestGenerateFuncs = _action.Reverse();
                            parameters.TicketGenerateFuncs = () =>
                            {
                                List<IActionResult<bool>> actionResults = new List<IActionResult<bool>>();
                                actionResults.Add(
                                    this.BatchForceApproveBol(bolModels
                                        .Select(p => p.UID), manifestModel.WarehouseUID, manifestModel.Type));
                                return actionResults;
                            };
                            responseAllocated = autoAssignAgent.Execute(parameters);

                            if (responseAllocated.Response.IsComplete)//資料全部新建完成
                            {
                                var groupsInfo = DrKnowAll.GetGroup(groups.Content.Select(p => p.GroupUID));
                                groupsInfo = groupsInfo.Where(x => x.Type == (int)GroupTypes.Team);
                                //this.TracingAgent.Trace($"Save item  elapsed {sw.ElapsedMilliseconds}ms");
                                tIcollection = this.TicketRepository.GetTicketInfoList(bolModels.Select(p => p.UID));
                                var ticketinfos = tIcollection.Content;

                                param.GroupUID = groupsInfo.Select(p => p.UID).ToArray();
                                param.TicketInfoUID = ticketinfos.Select(p => p.UID).ToArray();

                                this.LogConnectionStatus("Allocated after get ticket  data for assigned worker");
                                if (responseAllocated.Response.Results.All(p => p.IsComplete))
                                {
                                    //replicate to subscriber
                                    var parameter = new ReplicateDataParameter();
                                    parameter.TicketInfoUID = tIcollection.Content.Select(p => p.UID);
                                    rparams.Add(parameter);
                                    foreach (var rparam in rparams)
                                    {
                                        syncMethod.Add(() => this.ReplicationManager.Allcoated(rparam));
                                    }
                                    rs.Success = true;
                                    rs.Content = responseAllocated.Response;
                                }
                                else
                                {

                                    rs.Success = false;
                                    rs.Message += string.Join(",", allocatedResult.Where(p => !p.Success).Select(p => p.Message).ToArray());
                                    // rs.Content = responseAllocated.Response;
                                    foreach (var eitem in allocatedResult.Where(p => !p.Success))
                                    {
                                        this.TracingAgent.Trace($"Result:{eitem.GetType()} execute failure", eitem);
                                    }
                                }

                            }
                            else //資料建立失敗
                            {
                                //this.DbEntities.Rollback();
                                this.RollbackTransaction();
                                SqlConnection.ClearPool(this.DbEntities.Connection as SqlConnection);
                                rs.Success = false;
                                responseAllocated.Response.IsComplete = false;
                                responseAllocated.Response.Message = (responseAllocated.Response.Message ?? "") + string.Join(",",
                                    allocatedResult.Where(x => !x.Success).Select(y => y.Message));
                                rs.Message = responseAllocated.Response.Message;
                                rs.Content = responseAllocated.Response;
                                //建立失敗將Deallocated 資料
                            }


                            #region Allocated舊流程
                            // using (var db = this.DbEntities.DbAdapter)
                            //  {
                            // this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                            //  this.LogConnectionStatus();
                            //responseAllocated = autoAssignAgent.Execute(parameters);

                            /*
                            if (responseAllocated.Response.IsComplete) //配貨成功
                            {
                                this.LogConnectionStatus("Allocated after save work order data");
                                var action = _action.Reverse();
                                using (var a5 = this.TracingAgent.StartActivity("execute insert manifest data"))
                                {
                                    foreach (var item in action)
                                    {
                                        if (allocatedResult.All(p => p.Success))
                                        {
                                            allocatedResult.Add(item.Invoke());
                                        }
                                    }
                                }
                                using (var a6 = this.TracingAgent.StartActivity("generate ticket data"))
                                {
                                    foreach (var bolModel in bolModels)
                                    {
                                        if (allocatedResult.All(p => p.Success))
                                        {
                                            allocatedResult.Add(this.ForceApproveBol(bolModel.UID, manifestModel.WarehouseUID
                                            , manifestModel.Type));
                                        }
                                    }
                                }
                                if (allocatedResult.All(p => p.Success))//資料全部新建完成
                                {
                                    var groupsInfo = DrKnowAll.GetGroup(groups.Content.Select(p => p.GroupUID));
                                    groupsInfo = groupsInfo.Where(x => x.Type == (int)GroupTypes.Team);
                                    this.TracingAgent.Trace($"Save item  elapsed {sw.ElapsedMilliseconds}ms");
                                    tIcollection = this.TicketRepository.GetTicketInfoList(bolModels.Select(p => p.UID));
                                    var ticketinfos = tIcollection.Content;

                                    param.GroupUID = groupsInfo.Select(p => p.UID).ToArray();
                                    param.TicketInfoUID = ticketinfos.Select(p => p.UID).ToArray();

                                    this.LogConnectionStatus("Allocated after get ticket  data for assigned worker");
                                    if (allocatedResult.All(p => p.Success))
                                    {
                                        this.LogConnectionSPID();
                                        //this.TracingAgent.Trace($"Commit order");
                                        this.CommitTransaction("Allocated");
                                        SqlConnection.ClearPool(this.DbEntities.Connection as SqlConnection);
                                        //replicate to subscriber
                                        var parameter = new ReplicateDataParameter();
                                        parameter.TicketInfoUID = tIcollection.Content.Select(p => p.UID);
                                        rparams.Add(parameter);
                                        foreach (var rparam in rparams)
                                        {
                                            syncMethod.Add(() => this.ReplicationManager.Allcoated(rparam));
                                        }
                                        rs.Success = true;
                                        rs.Content = responseAllocated.Response;
                                    }
                                    else
                                    {
                                        //this.DbEntities.Rollback();
                                        //this.TracingAgent.Trace($"Rollback order");
                                        this.RollbackTransaction();
                                        SqlConnection.ClearPool(this.DbEntities.Connection as SqlConnection);
                                        rs.Success = false;
                                        rs.Message += string.Join(",", allocatedResult.Where(p => !p.Success).Select(p => p.Message).ToArray());
                                        // rs.Content = responseAllocated.Response;
                                        foreach (var eitem in allocatedResult.Where(p => !p.Success))
                                        {
                                            this.TracingAgent.Trace($"Result:{eitem.GetType()} execute failure", eitem);
                                        }
                                    }

                                }
                                else //資料建立失敗
                                {
                                    //this.DbEntities.Rollback();
                                    this.RollbackTransaction();
                                    SqlConnection.ClearPool(this.DbEntities.Connection as SqlConnection);
                                    rs.Success = false;
                                    responseAllocated.Response.IsComplete = false;
                                    responseAllocated.Response.Message = string.Join(",",
                                        allocatedResult.Where(x => !x.Success).Select(y => y.Message));
                                    rs.Content = responseAllocated.Response;
                                    //建立失敗將Deallocated 資料
                                }
                            }
                            else //缺貨
                            {
                                //this.DbEntities.Rollback();
                                this.RollbackTransaction();
                                SqlConnection.ClearPool(this.DbEntities.Connection as SqlConnection);
                                rs.Success = true;
                                rs.Content = responseAllocated.Response;

                            }
                           */
                            //  }
                            #endregion

                            this.DbEntities.ReInitConnectionInstance();
                            if (syncMethod.Count > 0)
                            {
                                var syncRsArr = new List<IActionResult<bool>>();
                                foreach (var item in syncMethod)
                                {
                                    // this.TracingAgent.Trace($"invoke sync method {item.Method.Name} ");
                                    var syncRs = item.Invoke();
                                    // this.TracingAgent.Trace($"invoke sync method end Result: {syncRs.Success} Message:{syncRs.Message} ");
                                    syncRsArr.Add(syncRs);
                                }
                            }
                            if (responseAllocated.Response.Results.Count > 0 && responseAllocated.Response.Results.All(p => p.IsComplete))
                            {


                                allocatedResult.Clear();
                                var awrs = Policy.Create().Retry(2, 1000, (obj, args) =>
                                 {

                                 }).Execute(() =>
                                  {
                                      var ars = ActionResultTemplates.OK();
                                      ars.Success = true;
                                      #region Assigned worker & add receivied queue
                                      using (var a7 = this.TracingAgent.StartActivity("assigned worker to ticket"))
                                      {
                                          sw.Restart();
                                          // this.TracingAgent.Trace($"ready to assigned worker");
                                          allocatedResult.Add(this.TicketManager.AddWorkder(param, true));
                                          //  this.TracingAgent.Trace($"add workorder result:{allocatedResult.Last().Success} message:{allocatedResult.Last().Message}"
                                          //        , allocatedResult.Last());

                                          //register receiver 
                                          if (allocatedResult.All(p => p.Success) &&
                                                !string.IsNullOrEmpty(request.ReceiverUrl) &&
                                                !string.IsNullOrEmpty(request.ReceiverSecret))
                                          {
                                              var receiver = new NotificationReceiverModel();
                                              var existData = this.ReceiverRepository.GetNotifyConfig(new { BelongToUID = manifestModel.UID });
                                              if (existData.Content == null)
                                              {
                                                  receiver.BelongToUID = manifestModel.UID;
                                                  receiver.ReceiverSecret = request.ReceiverSecret;
                                                  receiver.ReceiverUrl = request.ReceiverUrl;
                                                  receiver.Status = (int)ReceiverStatus.Active;
                                                  receiver.UID = Guid.NewGuid();
                                                  allocatedResult.Add(this.ReceiverRepository.Add(receiver));
                                              }

                                          }
                                          sw.Stop();
                                          OutpubToDebugLine($"Assigned ticket  elapsed {sw.ElapsedMilliseconds}ms");
                                          #endregion
                                          if (!allocatedResult.All(p => p.Success))
                                          {
                                              ars.Success = false;
                                              ars.Message = string.Join(",",
                                                  allocatedResult.Where(x => !x.Success).Select(a => a.Message));
                                          }
                                      }
                                      return ars;
                                  });
                                if (awrs == null || (awrs != null && !awrs.Success)) //如果assigned worker 2次失敗
                                {
                                    // 修正 NRE：新建的出貨 manifest 時 manifestInfo.Content 為 null(查詢時尚不存在)，應用 manifestModel(本次建立/取得的那張)
                                    this.ChangeManifestStatus(manifestModel.UID, ManifestStatus.WaitingtoAssignWorker,
                                        ManifestItemListStatus.Open);
                                }
                            }
                            else
                            {
                                this.TracingAgent.Trace($"allocated failure ", allocatedResult.Where(p => !p.Success));
                            }
                        }
                        else
                        {
                            responseAllocated.Response.IsComplete = false;
                            StringBuilder msg = new StringBuilder();
                            if (notFinditem.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_FIND, string.Join(",", notFinditem)));
                                foreach (var item in notFinditem)
                                {
                                    var requestItems = request.Items.Where(p => p.ItemNo == item);
                                    foreach (var reqItem in requestItems)
                                    {
                                        var responseItem = new AllocatedItemInnerResponse(reqItem);
                                        responseItem.ComponentType = (int)AllocatedComponentType.NotFindProduct;
                                        responseAllocated.Response.Results.Add(responseItem);
                                    }

                                }
                            }
                            if (duplicateitem.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_DUPLICATE, string.Join(",", duplicateitem)));
                                foreach (var item in notFinditem)
                                {
                                    var requestItems = request.Items.Where(p => p.ItemNo == item);
                                    foreach (var reqItem in requestItems)
                                    {
                                        var responseItem = new AllocatedItemInnerResponse(reqItem);
                                        responseItem.ComponentType = (int)AllocatedComponentType.Duplicateitem;
                                        responseAllocated.Response.Results.Add(responseItem);
                                    }

                                }
                            }
                            if (nothavepkg.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_PACKAGE, string.Join(",", nothavepkg)));
                                foreach (var item in notFinditem)
                                {
                                    var requestItems = request.Items.Where(p => p.ItemNo == item);
                                    foreach (var reqItem in requestItems)
                                    {
                                        var responseItem = new AllocatedItemInnerResponse(reqItem);
                                        responseItem.ComponentType = (int)AllocatedComponentType.NotFindPackage;
                                        responseAllocated.Response.Results.Add(responseItem);
                                    }

                                }
                            }
                            responseAllocated.Response.Message = msg.ToString();
                        }

                    }
                    catch (Exception ex)
                    {
                        this.RollbackTransaction();
                        this.TracingAgent.Trace(ex.Message + " " + ex.StackTrace);
                        rs.Message = Resource.COMMON_RETRY;
                        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                        rs.Success = false;
                        rs.InnerException = ex;
                        rs.Content.Message = Resource.COMMON_RETRY;
                        rs.Content.IsComplete = false;
                    }
                    finally
                    {
                        this.RequestManager.RemoveRequest(RequestAction.ALLOCATED, request.RefNo);
                    }
                    return rs;
                }
                else
                {
                    this.RequestManager.RemoveRequest(RequestAction.ALLOCATED, request.RefNo);
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_ORDER_ALLOCATED_NOT_FIND_CUSTOMERUID;
                    return rs;
                }
            }
            else
            {
                rs.Content.IsComplete = false;
                rs.Content.Message = string.Format(Resource.COMMON_REQUEST_ISPROCESSING, $"SO#{request.RefNo}");
                return rs;
            }
        }
        public IActionResult<IAllocatedResponse> FutureAllocated(IAllocatedRequest request)
        {

            IActionResult<IEnumerable<IAssignedTicketInfoModel>> tIcollection = null;
            List<ReplicateDataParameter> rparams = new List<ReplicateDataParameter>();
            var param = new MaintainWorkderInnerParameters();
            List<Func<IActionResult<bool>>> syncMethod = new List<Func<IActionResult<bool>>>();
            var newrequest = new AllocatedRequest(request);
            Stopwatch sw = new Stopwatch();
            var groups = this.GetGroupUserViewByUser();
            ProductCacheManager productManagerCache = this.ProductCacheManager;
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            OutboundAutoAssignedResult responseAllocated = new OutboundAutoAssignedResult();
            var customerUIDs = new List<Guid>();
            //先不使用Customer id 判斷
            //if (!string.IsNullOrEmpty(request.CustomerPartyName))
            //    customerUIDs.AddRange(this.GetCustomer(groups.Content.Select(x => x.GroupUID),
            //        request.CustomerPartyName).Select(p => p.UID));
            //else
            customerUIDs.Add(request.CustomerUID);
            var rs = ActionResultTemplates.Result<IAllocatedResponse>();
            if (!this.RequestManager.IsRequestProcessing(RequestAction.FUTUREALLOCATED, request.RefNo))
            {
                this.RequestManager.AddRequest(RequestAction.FUTUREALLOCATED, request.RefNo);
                if (customerUIDs.Count() > 0)
                {
                    try
                    {
                        sw.Start();
                        IManifestModel manifestModel = null;
                        rs.Content = responseAllocated.Response;
                        rs.Success = true;
                        List<string> notFinditem = new List<string>();
                        List<string> duplicateitem = new List<string>();
                        List<string> nothavepkg = new List<string>();
                        List<ItemInfo> items = new List<ItemInfo>();
                        List<IManifestItemListModel> manifestItems = new List<IManifestItemListModel>();
                        List<IVesselManifestModel> vesselManifestModels = new List<IVesselManifestModel>();
                        List<IActionResult<bool>> allocatedResult = new List<IActionResult<bool>>();
                        List<IAllocatedItemRequest> requestItemByVirtualItem = new List<IAllocatedItemRequest>();
                        //itemNo convert item Model
                        var pgrp = request.Items.GroupBy(p => p.ItemNo);
                        foreach (var item in pgrp)
                        {
                            ItemInfo i = new ItemInfo();
                            var itemOriginals = productManagerCache.GetItem(item.Key, customerUIDs, groups.Content);
                            if (itemOriginals != null && itemOriginals.Count() > 1)//是否有重覆的Item#
                            {
                                duplicateitem.Add(item.Key);
                            }
                            if (itemOriginals != null && itemOriginals.Count() == 1)
                            {
                                //TODO 改讀取VersionUID 取得最小包裝
                                var itemOrg = itemOriginals.FirstOrDefault();
                                var pkgcollection = this.PackageCacheManager.GetPackagesByItem(itemOrg.UID);
                                if (pkgcollection != null && pkgcollection.Count() > 0) //是否設定包裝
                                {
                                    //RETEST 取得最新版本全部包裝並取得該版本最小包裝
                                    var latestPkg = pkgcollection.GroupBy(g => g.VersionUID)
                                        .OrderByDescending(o => o.OrderByDescending(o1 => o1.CreatedOn).First().CreatedOn)
                                        .Select(p => p).FirstOrDefault();
                                    i.Item = itemOrg;
                                    i.Package = this.PackageCacheManager.GetMinPackage(latestPkg);

                                    //取得虛擬Item
                                    var virtualItems = this.ProductCacheManager.GetVirtualItems(itemOrg.Name, customerUIDs);
                                    if (virtualItems.Content?.Count() > 0)
                                    {

                                        foreach (var subitem in item)
                                        {
                                            var _itemGrp = Guid.NewGuid();
                                            subitem.VirtualItems = virtualItems.Content.Select(p => p as IItemModel).ToList();
                                            subitem.VirtualItemParentItemNo = subitem.ItemNo;
                                            subitem.IsHasVirtualItem = true;
                                            subitem.ItemGroupUID = _itemGrp;
                                        }
                                        i.VirtualItems = virtualItems.Content;

                                        foreach (var subitem in item)
                                        {

                                            foreach (var viitem in virtualItems.Content)
                                            {
                                                var allocatedvirtualItem = new AllocatedItemRequest();
                                                allocatedvirtualItem.ItemGroupUID = subitem.ItemGroupUID;
                                                allocatedvirtualItem.Carrier = subitem.Carrier;
                                                allocatedvirtualItem.ManifestItemUID = subitem.ManifestItemUID;
                                                allocatedvirtualItem.ComponentType = subitem.ComponentType;
                                                allocatedvirtualItem.VesselManifestUID = subitem.VesselManifestUID;
                                                allocatedvirtualItem.ItemNo = viitem.Name;
                                                allocatedvirtualItem.Line_No = subitem.Line_No;
                                                allocatedvirtualItem.PalletBarcode = subitem.PalletBarcode;
                                                allocatedvirtualItem.PalletID = subitem.PalletID;
                                                allocatedvirtualItem.ParentItemNo = subitem.ParentItemNo;
                                                allocatedvirtualItem.Qty = subitem.Qty;
                                                allocatedvirtualItem.ShipPackageUID = subitem.ShipPackageUID;
                                                allocatedvirtualItem.ShipViaNo = subitem.ShipViaNo;
                                                allocatedvirtualItem.UseMiniPackage = subitem.UseMiniPackage;
                                                allocatedvirtualItem.VesselManifestUID = subitem.VesselManifestUID;
                                                allocatedvirtualItem.VirtualItemParentItemNo = subitem.ItemNo;
                                                allocatedvirtualItem.ShipViaUID = subitem.ShipViaUID;
                                                allocatedvirtualItem.AllocatedOnhandType = subitem.AllocatedOnhandType;
                                                requestItemByVirtualItem.Add(allocatedvirtualItem);

                                                ItemInfo vii = new ItemInfo();
                                                vii.Item = viitem;
                                                var _pkgcollection = this.PackageCacheManager.GetPackagesByItem(viitem.UID);
                                                var _latestPkg = _pkgcollection.GroupBy(g => g.VersionUID)
                                                .OrderByDescending(o => o.OrderByDescending(o1 => o1.CreatedOn).First().CreatedOn)
                                                .Select(p => p).FirstOrDefault();
                                                vii.Package = this.PackageCacheManager.GetMinPackage(_latestPkg);
                                                items.Add(vii);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        virtualItems.Content = new List<ProductExtendModel>();
                                    }

                                    items.Add(i);
                                }
                                else
                                {
                                    nothavepkg.Add(item.Key);
                                }
                            }
                            if (itemOriginals == null || (itemOriginals != null && itemOriginals.Count() == 0)) //item 是否存在
                            {
                                notFinditem.Add(item.Key);
                            }

                        }
                        sw.Stop();
                        sw.Restart();
                        //todo 檢查request 是否已經有配貨過的line#
                        if (notFinditem.Count == 0 && duplicateitem.Count == 0 && nothavepkg.Count == 0 &&
                            request.Items.All(x => x.AllocatedOnhandType == (int)PayloadType.Stock))
                        {
                            //this.TracingAgent.Trace("Ready to Allocated", request.RefNo);
                            #region Manifest 
                            //if manifest exist
                            var manifestInfo = this.Repository.GetData(new
                            {
                                RefNo = request.RefNo.ToNvarchar(),
                                //PartyUID = request.CustomerUID, //SO# +WarehouseUID 應該可以判定這個manifest唯一性
                                WarehouseUID = request.WarehouseUID
                            });
                            if (manifestInfo.Content != null)//manifest exist
                            {
                                manifestModel = manifestInfo.Content;
                            }
                            else  //manifest unexist
                            {
                                var _seq = this.SequenceAgent.GetManinfestSequence(
                                    SequenceAgent.GetManifestRootUID(), ManifestType.Outbound);
                                // create manifest
                                ManinfestInnerModel model = new ManinfestInnerModel();
                                model.UID = Guid.NewGuid();
                                model.ID = _seq;
                                model.Name = request.CustomerPartyName;
                                model.RefNo = request.RefNo;
                                model.PartyUID = customerUIDs.FirstOrDefault();//TODO 暫定
                                model.Status = ManifestStatus.Open;
                                model.Type = (int)ManifestType.Outbound;
                                model.WarehouseUID = request.WarehouseUID;
                                model.Volume = 0;
                                model.Weight = 0;
                                model.CreatedBy = request.RequestBy;
                                model.CreatedOn = DateTime.UtcNow;
                                _action.Push(() => this.Repository.Add(model));
                                manifestModel = model;
                            }
                            #endregion
                            #region  manifest item
                            //created mainifest item 
                            var itemCollection = request.Items
                                 .Where(p => !p.IsHasVirtualItem)
                                .GroupBy(p => new
                                {
                                    ItemNo = p.ItemNo,
                                    LineNo = p.Line_No,
                                    OnhandType = p.AllocatedOnhandType
                                }).ToList();
                            itemCollection.AddRange(requestItemByVirtualItem.GroupBy(p => new
                            {
                                ItemNo = p.ItemNo,
                                LineNo = p.Line_No,
                                OnhandType = p.AllocatedOnhandType
                            }));
                            //var _miseq = this.SequenceAgent.GetMainfestItemListSequence(
                            //    SequenceAgent.GetManifestRootUID(), request.Items.Count);
                            var _miseq = this.SequenceAgent.GetMainfestItemListSequence(
                                SequenceAgent.GetManifestRootUID(), itemCollection.Count());
                            foreach (var item in itemCollection)
                            {
                                var itemInfo = items.FirstOrDefault(p => p.Item.Name == item.Key.ItemNo);
                                if (itemInfo != null)
                                {
                                    ManifestItemInnerModel m = new ManifestItemInnerModel();
                                    m.OnhandType = (item.Key.OnhandType == 0) ? (int)PayloadType.Stock : item.Key.OnhandType;
                                    m.UID = Guid.NewGuid();
                                    m.ID = _miseq.Dequeue();
                                    m.ItemUID = itemInfo.Item.UID;
                                    m.ManifestUID = manifestModel.UID;
                                    m.PackageQty = item.Sum(x => x.Qty);
                                    m.PackageUID = itemInfo.Package.UID;
                                    m.Name = item.Key.LineNo.ToString();
                                    m.Volume = this.ProductUtility.CalculateCUFT(itemInfo.Package, item.Sum(x => x.Qty));
                                    m.Weight = this.ProductUtility.CaculateTTLWeight(itemInfo.Package, item.Sum(x => x.Qty));
                                    m.Status = ManifestItemListStatus.Open;
                                    m.CreatedBy = request.RequestBy;
                                    m.CreatedOn = DateTime.UtcNow;
                                    foreach (var mIitem in item)
                                    {
                                        mIitem.ManifestItemUID = m.UID;
                                    }
                                    manifestItems.Add(m);
                                }
                                else
                                {
                                    //?
                                }
                            }

                            _action.Push(() => this.ManifestItemListRepository.Add(manifestItems));
                            #endregion
                            List<IBolModel> bolModels = new List<IBolModel>();
                            List<IVesselModel> vesselModels = new List<IVesselModel>();
                            #region create bol
                            List<IVesselManifestModel> vesselManifestCollection = new List<IVesselManifestModel>();
                            var groupbyBol = request.Items.GroupBy(p => p.ShipViaUID);
                            var _bseq = this.SequenceAgent.GetBOLSeqence(SequenceAgent.GetManifestRootUID(), groupbyBol.Count());
                            //Check bol exist 偶爾會發生重覆allocated 尚未找到原因，先判斷阻擋
                            var bollist = groupbyBol.Select(g => "BOL-" + request.RefNo + "-" + g.First().ShipViaNo);
                            var existBols = this.BolRepository.GetList(new { RefNo = bollist });
                            if (existBols.Content.Count() > 0)
                            {
                                var responseItem = new AllocatedItemInnerResponse();
                                responseItem.ComponentType = (int)AllocatedComponentType.DataExist;
                                rs.Content.Results.Add(responseItem);
                                rs.Success = false;
                                rs.Content.IsComplete = false;
                                rs.Content.Message =
                                rs.Message = $"Bol:{string.Join(",", existBols.Content.Select(p => p.RefNo))} have exist";
                                return rs;
                            }
                            foreach (var bol in groupbyBol)
                            {

                                BolInnerModel bolModel = new BolInnerModel();
                                bolModel.UID = Guid.NewGuid();
                                bolModel.ID = _bseq.Dequeue();
                                bolModel.Name = "BOL-" + request.RefNo + "-" + bol.First().ShipViaNo;
                                bolModel.RefNo = "BOL-" + request.RefNo + "-" + bol.First().ShipViaNo;
                                bolModel.ManifestUID = manifestModel.UID;
                                bolModel.ShipViaUID = Guid.Empty;
                                bolModel.ShipMethodUID = Guid.Empty;
                                bolModel.Status = BolStatus.Open;
                                bolModel.ETA = request.ETD;
                                bolModel.ShipToAddress = request.ShipToAddress;
                                bolModel.ShipToCity = request.ShipToCity;
                                bolModel.ShipToCountry = request.ShipToCountry;
                                bolModel.ShipToState = request.ShipToState;
                                bolModel.ShipToZip = request.ShipToZip;
                                bolModel.CreatedBy = request.RequestBy;
                                bolModel.CreatedOn = DateTime.UtcNow;
                                if (request.UsePackingStation && bol.First().Carrier.HasValue)
                                {
                                    var partyUID = GetShipviaUIDByCarrier(bol.First().Carrier.Value);
                                    if (partyUID.HasValue)
                                        bolModel.ShipViaUID = partyUID.Value;
                                }
                                _action.Push(() => this.BolRepository.AddBol(bolModel));
                                bolModels.Add(bolModel);
                                var vesselItemSource = itemCollection.SelectMany(g => g).Where(p => p.ShipViaUID == bol.Key)
                                    .GroupBy(p => p.PalletID).OrderBy(o => o.Key);
                                var _vseqBatch = this.SequenceAgent.GetVesselSeqence(SequenceAgent.GetManifestRootUID(),
                                    vesselItemSource.Count());
                                foreach (var pallet in vesselItemSource)
                                {
                                    #region create vessel

                                    var _vseq = _vseqBatch.Dequeue();
                                    VesselInnerModel vesselModel = new VesselInnerModel();

                                    vesselModel.UID = Guid.NewGuid();
                                    vesselModel.ID = _vseq;
                                    vesselModel.Name = $"Vessel {request.RefNo}-{pallet.Key}";
                                    vesselModel.RefNo = pallet.FirstOrDefault().PalletBarcode;
                                    vesselModel.Type = 1;
                                    vesselModel.BolUID = bolModel.UID;
                                    vesselModel.Status = (int)VesselStatus.Open;
                                    vesselModel.CreatedBy = request.RequestBy;
                                    vesselModel.CreatedOn = DateTime.UtcNow;
                                    vesselModels.Add(vesselModel);

                                    #region create vessel manifest item
                                    var _viseq = this.SequenceAgent.GetVesselManifestSequence(SequenceAgent.GetManifestRootUID(),
                                        pallet.Count());
                                    foreach (var item in pallet)
                                    {

                                        var mitemInfo = manifestItems.FirstOrDefault(p => p.UID == item.ManifestItemUID);
                                        var itemInfo = items.FirstOrDefault(p => p.Item.UID == mitemInfo.ItemUID);
                                        VesselManifestItemInnerModel vitem = new VesselManifestItemInnerModel();
                                        vitem.UID = Guid.NewGuid();
                                        vitem.ID = _viseq.Dequeue();
                                        vitem.OnhandType = mitemInfo.OnhandType;
                                        vitem.ItemUID = mitemInfo.ItemUID;
                                        vitem.ManifestItemUID = mitemInfo.UID;
                                        vitem.VesselUID = vesselModel.UID;
                                        vitem.BolUID = bolModel.UID;
                                        vitem.CreatedBy = request.RequestBy;
                                        vitem.Status = (int)VesselManifestStatus.Open;
                                        vitem.Qty = item.Qty; //mitemInfo.PackageQty.Value;
                                        vitem.Volume = this.ProductUtility.CalculateCUFT(itemInfo.Package, vitem.Qty);
                                        vitem.Weight = this.ProductUtility.CaculateTTLWeight(itemInfo.Package, vitem.Qty);
                                        vitem.PackageUID = mitemInfo.PackageUID;
                                        vitem.CreatedOn = DateTime.UtcNow;
                                        vesselManifestModels.Add(vitem);
                                        vesselManifestCollection.Add(vitem);
                                        item.VesselManifestUID = vitem.UID;

                                    }
                                    #endregion
                                }



                                #endregion
                            }
                            #endregion
                            _action.Push(() => this.VesselRepository.BatchAddVessel(vesselModels));
                            _action.Push(() => this.VesselManifestRepository.BatchAddVesselManifest(vesselManifestCollection));
                            //重新組合request 物件
                            var newrequestCollection = newrequest.Items.ToList();
                            newrequestCollection.AddRange(itemCollection.SelectMany(p => p));
                            newrequest.Items = newrequestCollection;
                            //allocate flow
                            #region allocated model paramters init
                            var provider = new AutoAssignAgentProviders()
                            {
                                PartyManager = this.PartyManager,
                                PackageManager = this.PackageManager,
                                PackageUomManager = this.PackageUomManager,
                                VesselManifestRepository = this.VesselManifestRepository,
                                WarehouseManager = this.WarehouseManager,
                                BolManager = this,
                                ManifestManager = this,
                                VesselManager = this,
                                WorkOrderAssignAgentParameters = this.GetWorkOrderAgentParameters(),
                                TicketRepository = this.TicketRepository,
                                TicketRelationRepository = this.TicketRelationRepository,
                                TicketInfoRepository = this.TicketInfoRepository,
                                LabelRepository = this.LabelRepository,
                                ItemManager = this.ItemManager,
                                WorkOrderManager = this.WorkOrderManager,
                                ProductCacheManager = this.ProductCacheManager,
                                PackageVersionManager = this.PackageVersionManager,
                                PackageCacheManager = this.PackageCacheManager,
                                TracingAgent = this.TracingAgent,
                                PackageVersionRepository = this.PackageVersionRepository,
                                DbEntities = this.DbEntities
                            };
                            #endregion
                            var autoAssignAgent = new ExternalOutboundFullAllocatedAutoAssignAgent(provider);
                            var parameters = new OutboundAutoAssignedParameters();
                            parameters.OutboundRequest = newrequest;
                            parameters.Manifest = manifestModel;
                            parameters.ManifestItems = manifestItems;
                            parameters.Vessel = vesselModels;
                            parameters.VesselItems = vesselManifestModels;
                            parameters.Bol = bolModels;
                            parameters.ForceWorkOrderOpen = true;
                            parameters.PassPackageVersion = request.PassPackageVersion;
                            parameters.ManifestGenerateFuncs = _action.Reverse();
                            parameters.TicketGenerateFuncs = () =>
                            {
                                List<IActionResult<bool>> actionResults = new List<IActionResult<bool>>();
                                actionResults.Add(
                                    this.BatchForceApproveBol(bolModels
                                        .Select(p => p.UID), manifestModel.WarehouseUID, manifestModel.Type));
                                return actionResults;
                            };


                            responseAllocated = autoAssignAgent.Execute(parameters);

                            if (responseAllocated.Response.IsComplete) //配貨成功
                            {


                                var groupsInfo = DrKnowAll.GetGroup(groups.Content.Select(p => p.GroupUID));
                                groupsInfo = groupsInfo.Where(x => x.Type == (int)GroupTypes.Team);
                                tIcollection = this.TicketRepository.GetTicketInfoList(bolModels.Select(p => p.UID));
                                var ticketinfos = tIcollection.Content;
                                param.GroupUID = groupsInfo.Select(p => p.UID).ToArray();
                                param.TicketInfoUID = ticketinfos.Select(p => p.UID).ToArray();
                                this.LogConnectionStatus("Allocated after get ticket  data for assigned worker");
                                if (allocatedResult.All(p => p.Success))
                                {
                                    var parameter = new ReplicateDataParameter();
                                    parameter.TicketInfoUID = tIcollection.Content.Select(p => p.UID);
                                    rparams.Add(parameter);
                                    foreach (var rparam in rparams)
                                    {
                                        syncMethod.Add(() => this.ReplicationManager.Allcoated(rparam));
                                    }
                                    rs.Success = true;
                                    rs.Content = responseAllocated.Response;
                                }
                                else
                                {
                                    //this.DbEntities.Rollback();
                                    this.RollbackTransaction();
                                    SqlConnection.ClearPool(this.DbEntities.Connection as SqlConnection);
                                    rs.Success = false;
                                    rs.Message += string.Join(",", allocatedResult.Where(p => !p.Success).Select(p => p.Message).ToArray());
                                    // rs.Content = responseAllocated.Response;
                                }
                            }
                            else //缺貨
                            {
                                //this.DbEntities.Rollback();
                                this.RollbackTransaction();
                                SqlConnection.ClearPool(this.DbEntities.Connection as SqlConnection);
                                rs.Success = true;
                                rs.Content = responseAllocated.Response;

                            }


                            this.DbEntities.ReInitConnectionInstance();
                            if (syncMethod.Count > 0)
                            {

                                foreach (var item in syncMethod)
                                {
                                    //  this.TracingAgent.Trace($"invoke sync method {item.Method.Name} ");
                                    var syncRs = item.Invoke();
                                    // this.TracingAgent.Trace($"invoke sync method end Result: {syncRs.Success} Message:{syncRs.Message} ");

                                }
                            }
                            if (responseAllocated.Response.Results.Count > 0 && responseAllocated.Response.Results.All(p => p.IsComplete))
                            {
                                #region Assigned worker & add notify queue
                                allocatedResult.Clear();
                                sw.Restart();
                                var awrs = Policy.Create().Retry(2, 1000, (obj, args) =>
                                 {

                                 }).Execute(() =>
                                 {
                                     var ars = ActionResultTemplates.OK();
                                     ars.Success = true;
                                     #region Assigned worker & add receivied queue
                                     sw.Restart();
                                     //this.TracingAgent.Trace($"ready to assigned worker", param);
                                     allocatedResult.Add(this.TicketManager.AddWorkder(param, true));
                                     //  this.TracingAgent.Trace($"add workorder result:{allocatedResult.Last().Success} message:{allocatedResult.Last().Message}"
                                     //             , allocatedResult.Last());

                                     //register receiver 
                                     if (allocatedResult.All(p => p.Success) &&
                                           !string.IsNullOrEmpty(request.ReceiverUrl) &&
                                           !string.IsNullOrEmpty(request.ReceiverSecret))
                                     {
                                         var receiver = new NotificationReceiverModel();
                                         var existData = this.ReceiverRepository.GetNotifyConfig(new { BelongToUID = manifestModel.UID });
                                         if (existData.Content == null)
                                         {
                                             receiver.BelongToUID = manifestModel.UID;
                                             receiver.ReceiverSecret = request.ReceiverSecret;
                                             receiver.ReceiverUrl = request.ReceiverUrl;
                                             receiver.Status = (int)ReceiverStatus.Active;
                                             receiver.UID = Guid.NewGuid();
                                             allocatedResult.Add(this.ReceiverRepository.Add(receiver));
                                         }

                                     }
                                     sw.Stop();
                                     OutpubToDebugLine($"Assigned ticket  elapsed {sw.ElapsedMilliseconds}ms");
                                     #endregion
                                     if (!allocatedResult.All(p => p.Success))
                                     {
                                         ars.Success = false;
                                         ars.Message = string.Join(",",
                                             allocatedResult.Where(x => !x.Success).Select(a => a.Message));
                                     }

                                     return ars;
                                 });
                                if (!awrs.Success) //如果assigned worker 2次失敗
                                {
                                    this.ChangeManifestStatus(manifestInfo.Content.UID, ManifestStatus.WaitingtoAssignWorker,
                                        ManifestItemListStatus.Open);
                                }
                                sw.Stop();
                                OutpubToDebugLine($"Assigned ticket  elapsed {sw.ElapsedMilliseconds}ms");
                                #endregion

                            }
                        }
                        else
                        {
                            responseAllocated.Response.IsComplete = false;
                            StringBuilder msg = new StringBuilder();
                            if (notFinditem.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_FIND, string.Join(",", notFinditem)));
                                foreach (var item in notFinditem)
                                {
                                    var requestItems = request.Items.Where(p => p.ItemNo == item);
                                    foreach (var reqItem in requestItems)
                                    {
                                        var responseItem = new AllocatedItemInnerResponse(reqItem);
                                        responseItem.ComponentType = (int)AllocatedComponentType.NotFindProduct;
                                        responseAllocated.Response.Results.Add(responseItem);
                                    }

                                }
                            }
                            if (duplicateitem.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_DUPLICATE, string.Join(",", duplicateitem)));
                                foreach (var item in notFinditem)
                                {
                                    var requestItems = request.Items.Where(p => p.ItemNo == item);
                                    foreach (var reqItem in requestItems)
                                    {
                                        var responseItem = new AllocatedItemInnerResponse(reqItem);
                                        responseItem.ComponentType = (int)AllocatedComponentType.Duplicateitem;
                                        responseAllocated.Response.Results.Add(responseItem);
                                    }

                                }
                            }
                            if (nothavepkg.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_PACKAGE, string.Join(",", nothavepkg)));
                                foreach (var item in notFinditem)
                                {
                                    var requestItems = request.Items.Where(p => p.ItemNo == item);
                                    foreach (var reqItem in requestItems)
                                    {
                                        var responseItem = new AllocatedItemInnerResponse(reqItem);
                                        responseItem.ComponentType = (int)AllocatedComponentType.NotFindPackage;
                                        responseAllocated.Response.Results.Add(responseItem);
                                    }

                                }
                            }
                            if (request.Items.Any(x => x.AllocatedOnhandType != (int)PayloadType.Stock))
                            {
                                var nsitems = request.Items.Where(p => p.AllocatedOnhandType != (int)PayloadType.Stock)
                                                           .Select(o => o.ItemNo);
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_NOT_STOCK_REQUEST,
                                    string.Join(",", nsitems)));
                                //foreach (var item in nsitems)
                                //{
                                //    var requestItems = request.Items.Where(p => p.ItemNo == item);
                                //    foreach (var reqItem in requestItems)
                                //    {
                                //        var responseItem = new AllocatedItemInnerResponse(reqItem);
                                //        responseItem.ComponentType = (int)AllocatedComponentType.NotStock;
                                //        responseAllocated.Response.Results.Add(responseItem);
                                //    }

                                //}
                            }
                            responseAllocated.Response.Message = msg.ToString();
                        }

                    }
                    catch (Exception ex)
                    {
                        this.RollbackTransaction();
                        this.TracingAgent.Trace(ex.Message + " " + ex.StackTrace);
                        rs.Message = Resource.COMMON_RETRY;
                        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                        rs.Success = false;
                        rs.InnerException = ex;
                        rs.Content.Message = Resource.COMMON_RETRY;
                        rs.Content.IsComplete = false;
                    }
                    finally
                    {
                        this.RequestManager.RemoveRequest(RequestAction.FUTUREALLOCATED, request.RefNo);
                    }
                    return rs;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_ORDER_ALLOCATED_NOT_FIND_CUSTOMERUID;
                    return rs;
                }
            }
            else
            {
                rs.Content.IsComplete = false;
                rs.Content.Message = string.Format(Resource.COMMON_REQUEST_ISPROCESSING, $"SO#{request.RefNo}");
                return rs;
            }
        }
        #endregion

        #region 取消配貨(Deallocated)


        public IActionResult<IDeallocateResponse> Deallocated(IEnumerable<IDeallocatedRequest> requests)
        {
            DeallocateInnerResponse response = new DeallocateInnerResponse();
            List<IActionResult<IDeallocateResponse>> innerResponses = new List<IActionResult<IDeallocateResponse>>();
            var rs = ActionResultTemplates.Result<IDeallocateResponse>();
            var bols = this.BolRepository.GetList(new { Refno = requests.Select(x => x.BolNo) });
            try
            {
                foreach (var request in requests)
                {
                    var bolinfo = bols.Content.FirstOrDefault(x => x.RefNo == request.BolNo);
                    if (bolinfo != null)
                    {
                        request.BolUID = bolinfo.UID;
                        innerResponses.Add(this.Deallocated(request));
                    }
                }
                response.IsComplete = innerResponses.All(x => x.Success);
                response.Message = string.Join(",", innerResponses.Where(p => !p.Success).Select(p => p.Message));
                rs.Content = response;
                rs.Success = true;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace("Deallocated failure " + ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<IDeallocateResponse> Deallocated(IDeallocatedRequest request)
        {
            var rs = ActionResultTemplates.Result<IDeallocateResponse>();//DeallocateInnerResponse
            DeallocateInnerResponse response = new DeallocateInnerResponse();
            Func<IActionResult<bool>> syncMethod = null;

            this.ExistTransactionScope = true;
            using (var db = this.DbEntities.DbAdapter)
            {
                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                if (!this.RequestManager.IsRequestProcessing(RequestAction.DEALLOCATED, request.RefNo + request.BolUID.ToString()))
                {
                    try
                    {
                        this.RequestManager.AddRequest(RequestAction.DEALLOCATED, request.RefNo + request.BolUID.ToString());
                        if (request.BolUID.HasValue) //deallocated by bol
                        {
                            IActionResult<bool> rs1 = ActionResultTemplates.Result(success: true);
                            IActionResult<bool> rs2 = null;
                            //var parm = new VesselManifestSearchInnerParameters();
                            var vesselitems = this.GetVesselManifestByBol(new Guid[] { request.BolUID.Value });
                            if (vesselitems.Content.Count() > 0)
                            {
                                var parameter = new ReplicateDataParameter();
                                parameter.BOLUID = new Guid[] { request.BolUID.Value };
                                IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = null;
                                allocatedReplicateModels = this.TicketInfoRepository.GetAllocatedData(parameter).Content;
                                //change manifest to open
                                var bol = this.BolRepository.GetBol(new { UID = request.BolUID });
                                if (bol.Content != null)
                                {
                                    var manifestinfo = this.GetManifestInfo(bol.Content.ManifestUID);
                                    if (manifestinfo.Content != null)
                                    {
                                        this.ChangeManifestStatus(manifestinfo.Content.UID, ManifestStatus.Open, ManifestItemListStatus.Open);
                                    }
                                }

                                //delete bol
                                var parameters = new BolDeleteInnerParameters();
                                parameters.UID = new Guid[] { request.BolUID.Value };
                                rs2 = this.DeleteBol(parameters);
                                //delete manifest item
                                if (rs2.Success && vesselitems.Content != null && vesselitems.Content.Count() > 0)
                                {
                                    var param = new ManifestItemListDeleteInnerParameters();
                                    param.UID = vesselitems.Content.Select(p => p.ManifestItemUID).ToArray();
                                    rs1 = this.DeleteManifestItem(param, true);
                                }
                                //update manifest status
                                if (rs1.Success && rs2.Success)
                                {

                                    //replicate to subscriber
                                    //var rrs = this.ReplicationManager.Deallocated(parameter, allocatedReplicateModels);
                                    //if (rrs.Success)
                                    //{
                                    rs.Success = true;
                                    response.IsComplete = true;
                                    //this.DbEntities.Commit();
                                    this.CommitTransaction();
                                    //if (scope != null)
                                    //    scope.Complete();
                                    syncMethod = () => this.ReplicationManager.Deallocated(parameter, allocatedReplicateModels);
                                    //}
                                    //else
                                    //{
                                    //    rs.Success = false;
                                    //    rs.Message = rrs.Message;
                                    //    response.IsComplete = false;
                                    //    response.Message = rrs.Message;
                                    //}
                                }
                                else
                                {
                                    rs.Success = false;
                                    response.IsComplete = false;
                                    response.Message = rs1.Message + " " + rs2.Message;
                                    //this.DbEntities.Rollback();
                                    this.RollbackTransaction();
                                }

                            }
                            else
                            {
                                response.IsComplete = true;
                                rs.Success = true;
                                response.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_VESSELMANIFST;
                                //this.DbEntities.Rollback();
                                this.RollbackTransaction();
                            }
                        }
                        else //deallocated by syspon
                        {

                            var manifestInfo = this.GetManifest(new
                            {
                                RefNo = request.RefNo.ToNvarchar(),
                                PartyUID = request.CustomerUID,
                                WarehouseUID = request.WarehouseUID
                            });
                            if (manifestInfo.Content != null)
                            {
                                var parameter = new ReplicateDataParameter();
                                parameter.ManifestUID = new Guid[] { manifestInfo.Content.UID };
                                IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = null;
                                allocatedReplicateModels = this.TicketInfoRepository.GetAllocatedData(parameter).Content;
                                var parameters = new ManifestDeleteInnerParameters();
                                parameters.UID = new Guid[] { manifestInfo.Content.UID };
                                var result = this.DeleteManifest(parameters);
                                response.IsComplete = result.Success;
                                response.Message = result.Message;

                                if (result.Success)
                                {
                                    //replicate to subscriber
                                    syncMethod = () => this.ReplicationManager.Deallocated(parameter, allocatedReplicateModels);
                                    //if (rrs.Success)
                                    //{
                                    rs.Success = true;
                                    //this.DbEntities.Commit();
                                    this.CommitTransaction();
                                    //if (scope != null)
                                    //    scope.Complete();
                                    //}
                                    //else
                                    //{
                                    //    rs.Success = false;
                                    //    rs.Message = rrs.Message;
                                    //    response.IsComplete = false;
                                    //    response.Message = rrs.Message;
                                    //}
                                }

                            }
                            else
                            {
                                rs.Success = false;
                                response.IsComplete = false;
                                response.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;
                                //this.DbEntities.Rollback();
                                this.RollbackTransaction();
                            }
                        }
                        rs.Content = response;
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
                        this.RequestManager.RemoveRequest(RequestAction.DEALLOCATED, request.RefNo + request.BolUID.ToString());
                    }
                    //if (scope != null)
                    //{

                    //    scope.Dispose();
                    //}
                }
                else
                {

                }
            }
            if (rs.Success && syncMethod != null)
            {
                this.DbEntities.ReInitConnectionInstance();
                var syncRs = syncMethod.Invoke();
                //    this.TracingAgent.Trace($"Deallocated SyncRs:{syncRs.Success} Message:{syncRs.Message}");
            }
            return rs;
        }
        public IActionResult<IDeleteManifestResponse> DeleteManifestByOrder(IDeleteManifestRequest request)
        {
            Func<IActionResult<bool>> syncMethod = null;
            DeleteManifestInnerResponse response = new DeleteManifestInnerResponse();
            var rs = ActionResultTemplates.Result<IDeleteManifestResponse>();
            //TransactionScope scope = null;
            if (!this.RequestManager.IsRequestProcessing(RequestAction.DELETE_MANIFEST, request.RefNo))
            {
                try
                {
                    this.RequestManager.AddRequest(RequestAction.DELETE_MANIFEST, request.RefNo);
                    rs.Content = response;
                    var manifestInfo = this.GetManifest(new
                    {
                        RefNo = request.RefNo.ToNvarchar(),
                        PartyUID = request.CustomerUID,
                        WarehouseUID = request.WarehouseUID
                    });
                    if (manifestInfo.Content != null)
                    {
                        this.ExistTransactionScope = true;
                        var parameter = new ReplicateDataParameter();
                        parameter.ManifestUID = new Guid[] { manifestInfo.Content.UID };
                        var parameters = new ManifestDeleteInnerParameters();
                        parameters.UID = new Guid[] { manifestInfo.Content.UID };
                        IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = null;
                        IEnumerable<IReceiviedReplicateModel> receivingReplicateModels = null;
                        if (manifestInfo.Content.Type == (int)ManifestType.Outbound)
                        {

                            allocatedReplicateModels = this.TicketInfoRepository.GetAllocatedData(parameter).Content;
                        }
                        else
                        {
                            receivingReplicateModels = this.TicketInfoRepository.GetReceiviedData(parameter).Content;
                        }
                        using (var db = this.DbEntities.DbAdapter)
                        {
                            this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                            var result = this.DeleteManifest(parameters, request.ForceDelete);
                            if (result.Success)
                            {
                                //this.DbEntities.Commit();
                                this.CommitTransaction();
                                rs.Success = response.IsComplete = result.Success;
                                rs.Message = response.Message = result.Message;
                                //replicate to subscriber
                                if (manifestInfo.Content.Type == (int)ManifestType.Outbound)
                                {
                                    syncMethod = () => this.ReplicationManager.Deallocated(parameter, allocatedReplicateModels);
                                }
                                else
                                {
                                    syncMethod = () => this.ReplicationManager.CancelReceiving(manifestInfo.Content.UID, receivingReplicateModels);
                                }

                            }
                            else
                            {

                                this.RollbackTransaction();
                                rs.Success = false;
                                rs.Message = response.Message = result.Message;
                            }
                        }
                    }
                    else
                    {
                        if (request.IgnoreCheckManifest)
                        {
                            response.IsComplete =
                            rs.Success = true;
                        }
                        else
                        {
                            response.IsComplete = false;
                            rs.Success = false;
                            rs.Message = response.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;
                        }
                    }

                    if (syncMethod != null)
                    {
                        this.DbEntities.InitConnection();
                        var syncRs = syncMethod.Invoke();
                        this.TracingAgent.Trace($"DeleteManifestByOrder syncResult:{syncRs.Success} Message:{syncRs.Message}");
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
                    this.RequestManager.RemoveRequest(RequestAction.DELETE_MANIFEST, request.RefNo);
                }
            }
            else
            {
                rs.Content.IsComplete = false;
                rs.Content.Message = string.Format(Resource.COMMON_REQUEST_ISPROCESSING, $"SO#{request.RefNo}");
                return rs;
            }
            return rs;
        }

        /// <summary>
        /// Transload 出貨作廢（ManifestUID 入口，杜絕 RefNo 跨倉風險）。
        /// RemoveWorkOrder：void ticket + 還 onhand(DeallocatedByWorkOrderPayload) + 刪 pod/payload/workorder；
        /// DeleteManifest：刪 manifest+BOL+Vessel+Item+Receiver。
        /// 全程本地操作、不觸發 ReplicationManager；單一交易，任一步失敗整批 rollback。
        /// </summary>
        public IActionResult<bool> VoidOutboundByTransload(Guid manifestUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = false;
            try
            {
                if (manifestUID == Guid.Empty) { rs.Message = "ManifestUID is required."; return rs; }

                // 1. 以 UID 解析 manifest（避開 RefNo 跨倉）+ 驗出貨單、未作廢
                var manifest = this.GetManifest(new { UID = manifestUID }).Content;
                if (manifest == null || manifest.Status <= 0) { rs.Message = "找不到出貨單(或已作廢)"; return rs; }
                if (manifest.Type != (int)ManifestType.Outbound) { rs.Message = "此單非出貨單(Outbound)。"; return rs; }

                // 2. 單一交易：DeleteManifest 即可。其 cascade「DeleteBol→DeleteVessel→RemoveWorkOrder」
                //    已含 void ticket + 還 onhand(DeallocatedByWorkOrderPayload) + 刪 pod/payload/workorder/vessel，
                //    再刪 manifest+BOL+item+receiver。**不可自己先呼 RemoveWorkOrder**，否則 DeleteVessel 取不到 workorder 會 NRE。
                this.ExistTransactionScope = true;
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);

                    var delParam = new ManifestDeleteInnerParameters { UID = new Guid[] { manifestUID } };
                    var rsDel = this.DeleteManifest(delParam, true);    // forcedelete=true(完成單非 Open)

                    if (rsDel.Success)
                    {
                        this.CommitTransaction();
                        rs.Success = true; rs.Content = true;
                    }
                    else
                    {
                        this.RollbackTransaction();
                        rs.Message = rsDel.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = ex.Message; rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false; rs.InnerException = ex;
            }
            return rs;
        }
        #endregion

        #region 撿貨(Pick)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult<IPickAllResponse> PickAll(IPickAllRequest request)
        {
            List<IActionResult<bool>> results = new List<IActionResult<bool>>();
            List<Func<IActionResult<bool>>> actions = new List<Func<IActionResult<bool>>>();
            List<PayloadTransactionLogInnerModel> pxLogs = new List<PayloadTransactionLogInnerModel>();
            List<InsertInventoryParameter> insertInventoryParameters = new List<InsertInventoryParameter>();
            var rs = ActionResultTemplates.Result<IPickAllResponse>();
            var prepareUpdateDatas = new List<IPickallViewModel>();
            try
            {
                rs.Content = new PickAllInnerResponse();
                if (!this.RequestManager.IsRequestProcessing(RequestAction.PICK_ALL, request.RefNo))
                {
                    this.RequestManager.AddRequest(RequestAction.PICK_ALL, request.RefNo);
                    //1 抓Manifest
                    var conditon = new QueryConditionTranslator<ManinfestInnerModel>();
                    conditon.AddCondition(p => p.RefNo == request.RefNo && p.Status > 0);

                    var manifestInfo = this.GetManifestBySqlConverter(conditon);
                    if (manifestInfo.Content == null)
                    {
                        rs.Success = false;
                        rs.Message = $"RefNo:{request.RefNo} " + Resource.MANIFEST_NOT_FIND_DATA;
                    }
                    else
                    {
                        //this.TracingAgent.Trace($"PickAll prepare process data ", request.RefNo);
                        #region 2取得Pick All 所需資料 (ticketinfo, payload,workorderpayload)
                        NotifySenderConfig config = null;
                        var receiverModel = this.ReceiverRepository.GetNotifyConfig(new { BelongToUID = manifestInfo.Content.UID });
                        if (receiverModel.Content != null)
                        {
                            config = new NotifySenderConfig
                            {
                                ReceiverSecret = receiverModel.Content.ReceiverSecret,
                                ReceiverUrl = receiverModel.Content.ReceiverUrl
                            };
                        }
                        var notcompleteStatus = new int[] {
                                  (int)TicketInfoStatus.Open,
                                  (int)TicketInfoStatus.Processing,
                                  (int)TicketInfoStatus.OffPosition
                                };
                        var index = 0;
                        var wgrp = request.ItemRefUID.GroupBy(g => index++ / 2000);
                        foreach (var item in wgrp)
                        {
                            var para = new PickAllParameters()
                            {
                                WorkPayloadUID = item,
                                TicketInfoStatus = notcompleteStatus
                            };
                            var r = this.TicketInfoRepository.GetTicketInfoByPickAll(para);
                            if (r.Success)
                            {
                                prepareUpdateDatas.AddRange(r.Content);
                            }
                            else
                            {
                                this.TracingAgent.Trace($"PickAll prepare process data failure ", r.Message);
                            }
                        }
                        //this.TracingAgent.Trace($"prepare process data", prepareUpdateDatas);
                        if (prepareUpdateDatas.All(p => p.PayloadType != (int)PayloadType.FutureAllocated))
                        {


                            //讀取item 資料，確保後續動作在讀取資料不會失敗
                            var iteminfos = this.ProductCacheManager.GetItems(prepareUpdateDatas.Select(p => p.ItemUID));
                            #endregion
                            if (prepareUpdateDatas.Count() > 0)
                            {


                                #region 3取得修改Ticket Info狀態的動作集合
                                //  this.TracingAgent.Trace($"prepare process ticket data", prepareUpdateDatas.GroupBy(g => g.TicketUID).Select(p => p.Key));
                                actions.Add(() =>
                                this.TicketInfoRepository.CompleteTicketInfoByTicket(
                                    prepareUpdateDatas.GroupBy(g => g.TicketUID).Select(p => p.Key), request.RequestBy));
                                #endregion

                                #region 4 CheckStatus [Manifest,ManifestItem, BOL, Vessel, VesselManifest , WorkOrder, WorkOrderPod,WorkOrderPayload,Payload, Label, Ticket, TicketInfo]

                                var labeluid = prepareUpdateDatas.Select(p => p.PayloadUID).ToList();
                                labeluid.AddRange(
                                    prepareUpdateDatas.Select(p => p.PodUID));
                                labeluid = labeluid.GroupBy(g => g).Select(p => p.Key).ToList();
                                actions.Add(() => this.LabelRepository.ChangeLabelStatusByBelongToUID(labeluid.ToArray(),
                                                  LabelStatus.Inactive, request.RequestBy));
                                #endregion
                                #region 5改onhand &log
                                var gropitems = prepareUpdateDatas.Where(x => x.TicketInfoType == (int)TicketInfoType.Outbound).GroupBy(g => new
                                {
                                    SlotUID = g.SlotUID,
                                    ItemUID = g.ItemUID,
                                    PackageUID = g.PackageUID,
                                    OriginalPayloadType = g.OriginalPayloadType
                                });
                                foreach (var item in gropitems)
                                {
                                    InsertInventoryParameter iparam = new InsertInventoryParameter();
                                    iparam.ItemUID = item.Key.ItemUID;
                                    iparam.Qty = item.Sum(p => p.Quantity) * -1;
                                    iparam.SlotUID = item.Key.SlotUID;
                                    iparam.TargetPackageUID = item.Key.PackageUID;
                                    iparam.Type = (InventoryType)item.Key.OriginalPayloadType;
                                    iparam.WarehouseUID = manifestInfo.Content.WarehouseUID;
                                    iparam.UseMiniPackage = true;
                                    insertInventoryParameters.Add(iparam);
                                    foreach (var sub in item)
                                    {
                                        PayloadTransactionLogInnerModel _log = new PayloadTransactionLogInnerModel();
                                        _log.UID = Guid.NewGuid();
                                        _log.ItemUID = sub.ItemUID;
                                        _log.WarehouseUID = manifestInfo.Content.WarehouseUID;
                                        _log.OriginalPackage = sub.PackageUID;
                                        _log.OriginalSlotUID = sub.SlotUID;
                                        _log.TargetPackage = sub.PackageUID;
                                        _log.TargetSlotUID = sub.SlotUID;
                                        _log.PayloadUID = sub.PayloadUID;
                                        _log.TicketInfoUID = sub.TicketInfoUID;
                                        _log.WorkOrderPayloadUID = sub.WorkOrderPayloadUID;
                                        _log.QtyBeforeTX = 0;
                                        _log.QtyAfterTX = sub.Quantity * -1;
                                        _log.Type = (int)this.TracingAgent.GetTransactionLogType();
                                        _log.Status = (int)PayloadTransactionLogStatus.Active;
                                        _log.CreatedBy = request.RequestBy;
                                        _log.CreatedOn = DateTime.UtcNow;
                                        pxLogs.Add(_log);
                                    }
                                }
                                actions.Add(() => this.InventoryManager.InsertInventory(insertInventoryParameters));
                                actions.Add(() => this.PayloadRepository.ChangePayloadStauts(
                                    prepareUpdateDatas.Select(p => p.PayloadUID), PayloadStatus.Inactive, request.RequestBy));
                                actions.Add(() => this.InventoryManager.BatchAddLog(pxLogs));
                                #endregion
                                #region 6 執行指令
                                // this.TracingAgent.Trace($"PickAll ready to execute sql ", request.RefNo);
                                var isComplete = false;
                                using (var db = this.DbEntities.DbAdapter)
                                {
                                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);

                                    try
                                    {
                                        foreach (var action in actions)
                                        {
                                            results.Add(action.Invoke());
                                        }
                                        var statusActions = this.StatusCenter.CheckAllManifestStatus(() =>
                                        this.TicketManager.GetManifestStatusCollection(prepareUpdateDatas.Select(p => p.TicketUID)), request.RequestBy);
                                        foreach (var action in statusActions)
                                        {
                                            results.Add(action.Invoke());
                                        }
                                        if (results.All(p => p.Success))
                                        {
                                            // this.TracingAgent.Trace($"PickAll  execute sql complete", request.RefNo);
                                            isComplete = true;

                                            this.CommitTransaction();
                                        }
                                        else
                                        {

                                            this.RollbackTransaction();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //this.DbEntities.Rollback();
                                        this.RollbackTransaction();
                                        this.TracingAgent.Trace($"PickAll  execute sql occur exception", ex);
                                        var exceptionRS = ActionResultTemplates.OK();
                                        exceptionRS.Success = false;
                                        exceptionRS.Message = Resource.COMMON_RETRY;
                                        results.Add(exceptionRS);

                                    }
                                }

                                #endregion
                                if (isComplete)
                                {
                                    this.DbEntities.InitConnection();
                                    #region 7通知shipping
                                    //if (config != null)
                                    //{
                                    //    List<NotificationSendTaskModel> taskModels = new List<NotificationSendTaskModel>();
                                    //    foreach (var item in prepareUpdateDatas.Where(x => x.TicketInfoType == (int)TicketInfoType.Outbound))
                                    //    {
                                    //        var syncrequest = new OutboundTicketInfoCompleteRequest();
                                    //        var processItem = new NotificationProcessInfo();
                                    //        var task = new NotificationSendTaskModel();
                                    //        task.UID = Guid.NewGuid();
                                    //        task.EventName = EventHelper.OUTBOUND_TICKET_INFO_COMPLETED;
                                    //        task.TicketInfoUID = item.TicketInfoUID;
                                    //        task.ReceiverSecret = config.ReceiverSecret;
                                    //        task.ReceiverUrl = config.ReceiverUrl;
                                    //        task.RefNo = manifestInfo.Content.RefNo;
                                    //        task.RetryCount = 0;
                                    //        task.Status = (int)SenderTaskStatus.InQueue;
                                    //        processItem.PickQty = item.Quantity;

                                    //        processItem.ProcessItemUID = item.WorkOrderPayloadUID;
                                    //        syncrequest.ProcessItems.Add(processItem);
                                    //        syncrequest.RefNo = manifestInfo.Content.RefNo;
                                    //        syncrequest.Sender = this.AuthProvider.GetAuthenticationInfo().Account;
                                    //        task.Message = JsonConvert.SerializeObject(syncrequest);
                                    //        taskModels.Add(task);
                                    //    }
                                    //    results.Add(this.NotificationSenderTaskRepository.BatchAdd(taskModels));
                                    //}
                                    #endregion
                                    #region 8 通知PBSC
                                    // PBSC 外部同步為 commit 後的「事後通知其它平台」(deferred/非阻塞)。
                                    // 庫存已在 WMS 本機扣妥;外部通知失敗(例:本機無 PBSC 設定時回 FormatException)
                                    // 不應讓已 commit 的揀貨報失敗 → 不加入決定 rs.Success 的 results,失敗僅記 log。
                                    var parameter = new ReplicateDataParameter();
                                    parameter.TicketInfoUID = prepareUpdateDatas.Select(p => p.TicketInfoUID).ToArray();
                                    try
                                    {
                                        var _replRs = this.ReplicationManager.Outbound(parameter);
                                        if (_replRs == null || !_replRs.Success)
                                            this.TracingAgent.Trace("PickAll PBSC 外部通知未成功(不影響本機已完成的揀貨/扣庫存): " + (_replRs == null ? "(null)" : _replRs.Message));
                                    }
                                    catch (Exception replEx)
                                    {
                                        this.TracingAgent.Trace("PickAll PBSC 外部通知例外(不影響本機已完成的揀貨/扣庫存): " + replEx.Message);
                                    }
                                    #endregion
                                }
                                if (results.AllComplete())
                                {
                                    rs.Success = true;
                                    rs.Content.IsComplete = true;
                                }
                                else
                                {
                                    rs.Success = false;
                                    rs.Content.Message = rs.Message = string.Join(",", results.Where(p => !p.Success).Select(x => x.Message));
                                }
                            }
                            else
                            {
                                rs.Success = true;
                                rs.Content.IsComplete = true;
                                rs.Content.Message = Resource.MANIFEST_PICK_ALL_ALREADY_COMPLETE;
                            }
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Content.IsComplete = false;
                            rs.Content.Message = Resource.MANIFEST_PICK_ALL_HAS_FUTURE_ALLOCATED;
                        }
                    }

                    //this.RequestManager.RemoveRequest(RequestAction.PICK_ALL, request.RefNo);
                }
                else
                {
                    rs.Content.IsComplete = false;
                    rs.Content.Message = string.Format(Resource.COMMON_REQUEST_ISPROCESSING, $"SO#{request.RefNo}");
                    rs.Content.ErrorCode = 800;
                }
            }
            catch (Exception ex)
            {
                //this.RequestManager.RemoveRequest(RequestAction.PICK_ALL, request.RefNo);
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            finally
            {
                this.RequestManager.RemoveRequest(RequestAction.PICK_ALL, request.RefNo);
            }
            return rs;

        }

        public IActionResult<IPickItemResponse> PickItem(IPickItemRequest request)
        {
            //only outbound manifest
            var completeunexecuteCollection = new List<ConcurrentQueue<Func<IActionResult<bool>>>>();
            NotifySenderConfig config = null;
            List<IActionResult<bool>> results = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<IPickItemResponse>();
            rs.Content = new PickItemInnerResponse();
            var conditon = new QueryConditionTranslator<ManinfestInnerModel>();
            conditon.AddCondition(p => p.RefNo == request.RefNo && p.Status > 0);
            var manifestInfo = this.GetManifestBySqlConverter(conditon);
            try
            {
                if (!manifestInfo.Success || manifestInfo.Content == null)
                {
                    rs.Success = false;
                    rs.Message = $"RefNo:{request.RefNo} " + Resource.MANIFEST_NOT_FIND_DATA;
                }
                else if (manifestInfo.Content.Type != (int)ManifestType.Outbound)
                {
                    rs.Success = false;
                    rs.Message = Resource.ORDER_MANIFEST_TYPE_INCORRECT;
                    rs.Content.IsComplete = false;
                    rs.Content.Message = rs.Message;
                }
                else
                {
                    var receiverModel = this.ReceiverRepository.GetNotifyConfig(new { BelongToUID = manifestInfo.Content.UID });
                    if (receiverModel.Content != null)
                    {
                        config = new NotifySenderConfig
                        {
                            ReceiverSecret = receiverModel.Content.ReceiverSecret,
                            ReceiverUrl = receiverModel.Content.ReceiverUrl
                        };
                    }
                    var _parameter = this.GetTicketProcessAgentParameter();
                    var agent = AbstractProcessAgent.GetAgent(Constant.ProcessKind.TicketProcess, _parameter);
                    //get workorderpod
                    var ticketInfos = this.GetTicketInfoByPickAll(workorderPayloadUID: request.ItemRefUID);
                    if (ticketInfos.Content.Count() == 0)
                    {
                        rs.Success = false;
                        rs.Message = Resource.DATA_TICKET_KEY + " " + Resource.COMMON_DATA_NOT_FOUND;
                        rs.Content.IsComplete = false;
                        rs.Content.Message = rs.Message;
                    }
                    else
                    {
                        if (!Enum.GetValues(typeof(PickItemStatus)).Cast<int>().Contains(request.ChangeStatus))
                        {
                            rs.Success = false;
                            rs.Message = Resource.DATA_TICKET_KEY + " Status invaild.";
                            rs.Content.IsComplete = false;
                            rs.Content.Message = rs.Message;
                        }
                        else
                        {

                            var notcompleteStatus = new int[] {
                                  (int)TicketInfoStatus.Open,
                                  (int)TicketInfoStatus.Processing,
                                  (int)TicketInfoStatus.OffPosition,
                                };
                            var changeStatus = (PickItemStatus)request.ChangeStatus;
                            var collection = ticketInfos.Content.Where(x => notcompleteStatus.Contains(x.Status))
                                .GroupBy(p => p.Type).OrderByDescending(g => g.Key);
                            //this.TracingAgent.Trace($"PickItem get ticketinfo data  ", collection);
                            foreach (var Grpitem in collection)
                            {
                                if (Grpitem.Key == (int)TicketInfoType.Outbound)
                                {
                                    var param = new List<UploadTicketDataInnerParameter>();
                                    foreach (var item in Grpitem)
                                    {
                                        var parm = new UploadTicketDataInnerParameter();
                                        parm.ServiceItem = (TicketType)item.Type;
                                        if (changeStatus == PickItemStatus.Complete)
                                        {
                                            parm.Item.ActQty = item.ActQty;
                                            parm.Item.IsAllPass = true;
                                        }
                                        parm.Item.TicketInfoUID = item.UID;
                                        param.Add(parm);
                                    }
                                    //this.TracingAgent.Trace($"PickItem process outbound ticket ", param);
                                    if (results.All(p => p.Success))
                                    {
                                        results.Add(agent.Process(param, SendInfo: config));
                                        completeunexecuteCollection.Add(agent.CompleteUnexecutedMethod());
                                    }
                                }
                                else
                                {
                                    //一次處理一個Move Ticket item
                                    //foreach (var item in Grpitem)
                                    //{
                                    //    var parm = new UploadTicketDataInnerParameter();
                                    //    parm.ServiceItem = (TicketType)item.Type;
                                    //    if (changeStatus == PickItemStatus.Complete)
                                    //    {
                                    //        parm.Item.ActQty = item.ActQty;
                                    //        parm.Item.IsAllPass = true;
                                    //    }
                                    //    parm.Item.TicketInfoUID = item.UID;
                                    //    results.Add(agent.Process(new UploadTicketDataInnerParameter[] { parm }));
                                    //}
                                    //批次處理Ticket item
                                    var param = new List<UploadTicketDataInnerParameter>();
                                    foreach (var item in Grpitem)
                                    {
                                        var parm = new UploadTicketDataInnerParameter();
                                        parm.ServiceItem = (TicketType)item.Type;
                                        if (changeStatus == PickItemStatus.Complete)
                                        {
                                            parm.Item.ActQty = item.ActQty;
                                            parm.Item.IsAllPass = true;
                                        }
                                        parm.Item.TicketInfoUID = item.UID;
                                        param.Add(parm);
                                    }
                                    //if (results.All(p => p.Success) && results.Count > 0)
                                    //this.TracingAgent.Trace($"PickItem process move ticket ", param);
                                    results.Add(agent.Process(param));
                                    completeunexecuteCollection.Add(agent.CompleteUnexecutedMethod());
                                }
                            }
                            rs.Success = results.All(p => p.Success);
                            if (!rs.Success)
                            {
                                rs.Message = string.Join(",", results.Where(x => !x.Success).Select(x => x.Message));
                                rs.Content.IsComplete = false;
                                rs.Content.Message = rs.Message;
                            }
                            else
                            {
                                rs.Content.IsComplete = true;
                                rs.Success = true;
                                //scope.Complete();


                            }
                            //}
                            if (rs.Success)
                            {
                                this.DbEntities.InitConnection();
                                foreach (var invokemethod in completeunexecuteCollection)
                                {
                                    invokemethod.ToList().ForEach(p =>
                                    {
                                        // this.TracingAgent.Trace($"invoke method {p.Method.Name}  ");
                                        var crs = p.Invoke();
                                        //  this.TracingAgent.Trace($"invoke method result:{crs.Success} message:{crs.Message}  ");
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace($"Pick item occur exception", ex.Message, ex.StackTrace);
                rs.Message = Resource.COMMON_RETRY;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        #endregion

        #region 其他
        public IActionResult<dynamic> GetAllCurrentProeccingRequest()
        {
            var rs = ActionResultTemplates.Result<dynamic>();
            rs.Success = true;
            rs.Content = this.RequestManager.GetCurrentRequest();
            return rs;
        }
        public IActionResult<bool> RemoveProcessingRequestStatus(string actionkey, string requestKey)
        {
            var rs = ActionResultTemplates.Result<bool>();
            this.RequestManager.RemoveRequest(actionkey, requestKey);
            rs.Success = true;
            rs.Content = true;
            return rs;
        }
        public IActionResult<bool> AddProcessingRequestStatus(string actionkey, string requestKey)
        {
            var rs = ActionResultTemplates.Result<bool>();
            this.RequestManager.AddRequest(actionkey, requestKey);
            rs.Success = true;
            rs.Content = true;
            return rs;
        }
        public IActionResult<Guid?> GetLatestPackageByItem(Guid itemUID)
        {
            var rs = ActionResultTemplates.Result<Guid?>();
            var itemInfo = this.ProductCacheManager.GetItem(itemUID);
            if (itemInfo != null)
            {
                var pkgcollection = this.PackageCacheManager.GetPackagesByItem(itemInfo.UID);
                if (pkgcollection != null && pkgcollection.Count() > 0) //是否設定包裝
                {
                    //RETEST 取得最新版本全部包裝並取得該版本最小包裝
                    var latestPkg = pkgcollection.GroupBy(g => g.VersionUID)
                      .OrderByDescending(o => o.OrderByDescending(o1 => o1.CreatedOn).First().CreatedOn)
                      .Select(p => p).FirstOrDefault();
                    rs.Content = this.PackageCacheManager.GetMinPackage(latestPkg).UID;
                }
            }
            return rs;
        }

        /// <summary>
        /// 取消撿貨
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult<IRollbackTicketResponse> RollbackTicket(IRollbackTicketRequest request)
        {
            List<Func<IActionResult<bool>>> syncMethod = new List<Func<IActionResult<bool>>>();
            var authorInfo = this.AuthProvider.GetAuthenticationInfo();
            var response = new RollbackInnerTicketResponse();
            var rs = ActionResultTemplates.Result<IRollbackTicketResponse>();
            using (var db = this.DbEntities.DbAdapter)
            {
                try
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    this.ExistTransactionScope = true;
                    List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                    IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = null;
                    var parameter = new ReplicateDataParameter();
                    parameter.BOLUID = request.BolRefUID;
                    allocatedReplicateModels = this.TicketInfoRepository.GetAllocatedData(parameter).Content;


                    if (Result.All(x => x.Success))
                    {
                        foreach (var BolUID in request.BolRefUID)
                        {
                            var rsbol = this.StatusCenter.RollbackBOL(BolUID, request.RequestBy);
                            if (rsbol.AllComplete())
                            {
                                syncMethod.Add(() =>
                              this.ReplicationManager
                              .Rollback(parameter, allocatedReplicateModels.Where(p => p.BOLUID == BolUID)));
                            }
                            Result.Add(rsbol);
                        }
                        response.IsComplete = true;
                        rs.Content = response;
                        rs.Success = true;
                        db.Commit();

                    }
                    else
                    {
                        rs.Message = "Error:" + string.Join(",", Result.Where(p => !p.Success).Select(x => x.Message));
                        rs.Content = response;
                        rs.Success = false;
                    }

                }
                catch (Exception ex)
                {
                    db.Rollback();
                    this.TracingAgent.Trace(ex.Message, ex);
                    rs.Message = Resource.COMMON_RETRY;
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    rs.InnerException = ex;
                }
            }
            if (syncMethod.Count > 0)
            {
                this.DbEntities.InitConnection();
                foreach (var item in syncMethod)
                {
                    item.Invoke();
                }
            }
            return rs;
        }
        /// <summary>
        /// 同步Tracking# (Packaging)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult<ISyncTrackingResponse> SyncTrackingNoAPI(ISyncTrackingNoRequest request)
        {
            SyncTrackingResponse response = new SyncTrackingResponse();
            response.IsComplete = true;
            var rs = ActionResultTemplates.Result<ISyncTrackingResponse>();
            try
            {
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    if (request.Packages.All(p => !string.IsNullOrEmpty(p.TrackingNo)))
                    {
                        List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                        var wpods = this.WorkOrderPodRepository.GetWorkOrderPodList(
                            new { UID = request.Packages.Select(x => x.PalletRefUID) });
                        if (wpods.Content.Count() > 0)
                        {
                            //清除目前的Pod label (Tracking#)
                            Result.Add(this.LabelManager.ChangeLabelStatus(
                                wpods.Content.Select(p => p.BarcodeUID).ToArray(), LabelStatus.Inactive));
                            foreach (var wpod in wpods.Content)
                            {
                                var barcode = request.Packages.FirstOrDefault(p => p.PalletRefUID == wpod.UID);
                                if (barcode != null)
                                {
                                    //重新新增Pod Label(Tracking#)
                                    Result.Add(this.WorkOrderManager.SetWorkOrderPodBarcode(wpod, barcode.TrackingNo, false));
                                }
                                else
                                {
                                    var notfind = ActionResultTemplates.Result(success: false);
                                    notfind.Message = $"Not find palletRefUID {wpod.PodUID}";
                                    Result.Add(notfind);
                                }
                            }


                        }
                        else
                        {
                            var notfind = ActionResultTemplates.Result(success: false);
                            notfind.Message = $"Not find all palletRefUID";
                            Result.Add(notfind);
                        }
                        if (Result.All(p => p.Success))
                        {
                            rs.Success = true;
                            response.IsComplete = true;
                            response.Message = "";
                            db.Commit();
                        }
                        else
                        {
                            db.Rollback();
                            rs.Success = true;
                            response.IsComplete = false;
                            response.Message = string.Join(",", Result.Select(p => p.Message));
                        }
                        rs.Content = response;
                    }
                    else
                    {
                        db.Rollback();
                        var notTrackingNo = request.Packages.Where(p => string.IsNullOrEmpty(p.TrackingNo)).GroupBy(g => g.Syspon);
                        response.IsComplete = false;
                        List<string> msgs = new List<string>();
                        foreach (var item in notTrackingNo)
                        {
                            msgs.Add($"{item.Key} Pkg empty Tracking#");
                        }
                        response.IsComplete = false;
                        response.Message = string.Join(",", msgs);
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
            rs.Content = response;
            return rs;
        }

        public IActionResult<IGetOnhandResponse> GetOnhand(IGetOnhandRequest request)
        {
            GetOnhandResponse response = new GetOnhandResponse();
            var onhandItems = new List<GetOnhandItem>();
            var rs = ActionResultTemplates.Result<IGetOnhandResponse>();
            try
            {
                var itemCollection = this.ProductCacheManager.GetItems(request.Items.Select(x => x.ItemNo),
                    request.CustomerUID, this.GetGroupUserViewByUser().Content);
                //foreach (var item in request.Items)
                //{
                //    var itemInfo = itemCollection.FirstOrDefault(p => p.ID == item.ItemNo);
                //    if (itemInfo != null)
                //        item.ItemUID = itemInfo.UID;
                //}
                var collection = this.WarehouseManager.GetOnhandPayload(request.WarehouseUID,
                    itemCollection.Select(p => p.UID), new int[] {
                     (int)SlotStatus.InAndOut,(int)SlotStatus.Out
                    });
                var onhandGrp = collection.Content.GroupBy(p => p.ItemUID);
                foreach (var ig in request.Items)
                {
                    var itemInfo = itemCollection.FirstOrDefault(p => ig.ItemNo == p.ID);
                    if (itemInfo != null)
                    {
                        var onhand = onhandGrp.FirstOrDefault(p => p.Key == itemInfo.UID);
                        GetOnhandItem oi = new GetOnhandItem();
                        oi.ItemNo = itemInfo.ID;
                        oi.ItemUID = itemInfo.UID;
                        if (onhand != null)
                        {
                            var minpkg = this.PackageCacheManager.GetMinPackage(onhand.FirstOrDefault().PackageUID);
                            oi.Onhand = onhand.Sum(p => this.PackageCacheManager
                                               .GetReceivePackageUomQuantity(p.PackageUID, minpkg.UID, p.Quantity).Content);
                        }
                        onhandItems.Add(oi);
                    }
                }
                response.Items = onhandItems;
                rs.Content = response;
                rs.Success = true;
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
        /// 同步ProNo (Truck)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IActionResult<ISyncProNoResponse> SyncProNoAPI(ISyncProNoRequest request)
        {
            SyncProNoResponse response = new SyncProNoResponse();
            response.IsComplete = true;
            var rs = ActionResultTemplates.Result<ISyncProNoResponse>();
            try
            {
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                    var bolInfoCollection = this.BolRepository.GetList(
                        new { UID = request.Items.Select(x => x.ShipViaRefUID) });
                    if (bolInfoCollection.Content.Count() > 0)
                    {
                        foreach (var bol in bolInfoCollection.Content)
                        {
                            var requestItem = request.Items.FirstOrDefault(p => p.ShipViaRefUID == bol.UID);
                            if (requestItem != null)
                            {
                                if (!string.IsNullOrEmpty(requestItem.ProNo))
                                {
                                    var refNosplit = bol.RefNo.Split(' ');
                                    if (refNosplit.Count() >= 1)
                                    {
                                        bol.RefNo = refNosplit[0] + " " + requestItem.ProNo;
                                    }
                                    else
                                    {
                                        bol.RefNo = bol.RefNo + " " + requestItem.ProNo;
                                    }
                                    Result.Add(this.BolRepository.EditBol(bol));
                                }
                            }
                            else
                            {
                                var notfind = ActionResultTemplates.Result(success: false);
                                notfind.Message = $"Not find shipviaRefUID {bol.UID}";
                                Result.Add(notfind);
                            }
                        }
                    }
                    else
                    {
                        var notfind = ActionResultTemplates.Result(success: false);
                        notfind.Message = $"Not find all palletRefUID";
                        Result.Add(notfind);
                    }
                    if (Result.All(p => p.Success))
                    {
                        rs.Success = true;
                        response.IsComplete = true;
                        response.Message = "";
                        //scope.Complete();
                        db.Commit();
                    }
                    else
                    {
                        db.Rollback();
                        rs.Success = true;
                        response.IsComplete = false;
                        response.Message = string.Join(",", Result.Select(p => p.Message));
                    }
                    rs.Content = response;


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
            rs.Content = response;
            return rs;
        }

        public IActionResult<ICommonResponse> ClearProductCache()
        {
            CommonResponse response = new CommonResponse();
            var rs = ActionResultTemplates.Result<ICommonResponse>();
            try
            {
                base.ClearProductCache();
                response.IsComplete = true;
                rs.Content = response;
                rs.Success = true;
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

        public IActionResult<ICommonResponse> ClearPackageCache()
        {
            CommonResponse response = new CommonResponse();
            var rs = ActionResultTemplates.Result<ICommonResponse>();
            try
            {
                base.ClearPackageCache();
                response.IsComplete = true;
                rs.Content = response;
                rs.Success = true;
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

        public IActionResult<ICommonResponse> ReloadProductPackageCache()
        {

            CommonResponse response = new CommonResponse();
            var rs = ActionResultTemplates.Result<ICommonResponse>();
            try
            {
                this.RefreshProductCache();
                response.IsComplete = true;
                rs.Content = response;
                rs.Success = true;
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

        public IActionResult<dynamic> ResendSynctoPBSC()
        {

            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {
                var groups = this.GetGroupUserViewByUser();
                var ff = this.ProductCacheManager.GetItem(new Guid("FFC4B68D-1EBE-455D-8AE0-4FA0EBEEA1C4"));
                ItemInfo i = new ItemInfo();
                rs.Content = ff;
                //var viteminfos = this.ProductCacheManager.GetItems(vitems);
                //var vic = viteminfos.Select(p =>
                //{
                //    var vi = new VirtualItemInfo();
                //    var citeminfo = p;
                //    vi.ActualProduct = citeminfo.ActualProduct;
                //    vi.ProductId = citeminfo.ID;
                //    vi.Quantity = 1;
                //    vi.ProductUID = citeminfo.UID;
                //    vi.CustomerUID = new Guid(citeminfo.CustomerUID);
                //    vi.PUOM = citeminfo.PUOM;
                //    return vi;

                //});
                //var aa = this.ProductCacheManager.NewCombineToActualItem(vic);
                //var itemOriginals = this.ProductCacheManager.GetItem("HHWG-GG1BKM", new Guid[] {
                //new Guid("632689E6-C643-43EE-AE85-7384E20E587A")
                //}, groups.Content);


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
        public IActionResult<IEnumerable<IProductExtendModel>> GetAllItem()
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IProductExtendModel>>();
            try
            {

                var groups = this.GetGroupUserViewByUser();
                var ff = this.ProductCacheManager.GetItems(null);
                rs.Content = ff;


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
        public IActionResult<dynamic> GetItemNo(string itemNo)
        {

            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {

                var groups = this.GetGroupUserViewByUser();
                var ff = this.ProductCacheManager.GetItem(itemNo,
                    new Guid[] { new Guid("632689E6-C643-43EE-AE85-7384E20E587A") }, groups.Content);
                rs.Content = ff;


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
        public IActionResult<ICheckBolExistResponse> CheckBolExist(IEnumerable<string> request)
        {
            List<CheckBolItemResponse> items = new List<CheckBolItemResponse>();
            var rs = ActionResultTemplates.Result<ICheckBolExistResponse>();
            try
            {
                CheckBolResponse response = new CheckBolResponse();
                IActionResult<IEnumerable<string>> bols;
                bols = this.BolRepository.GetBolRefNo(request);

                foreach (var item in request)
                {
                    CheckBolItemResponse itemResponse = new CheckBolItemResponse();
                    itemResponse.BolRefNo = item;
                    var bol = bols.Content.FirstOrDefault(p => p == item);
                    if (bol != null)
                    {
                        itemResponse.IsExist = true;
                    }
                    items.Add(itemResponse);
                }
                response.IsComplete = true;
                response.Items = items;
                rs.Content = response;
                rs.Success = true;

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
        #endregion

        #region 補貨處理
        public IActionResult<int> SetFillFutureAllocated(IEnumerable<IReplenishmentModel> fillList, IEnumerable<Guid> target_future_allocated = null)
        {
            var rs = ActionResultTemplates.Result<int>();
            try
            {
                if (fillList != null && fillList.Count() > 0)
                {
                    Guid warehouse_uid = fillList.FirstOrDefault().WarehouseUID.Value;
                    //列出該列表中 Item Future Allocated 項目 (FA payload list) with ETD & CreatedOn
                    var FAPayloadList = this.WorkOrderPayloadRepository.GetWorkOrderPayloadInfo(
                        new List<Guid>() { warehouse_uid },
                        fillList.Select(x => x.ItemUID).ToList(),
                        new int[] { (int)PayloadType.FutureAllocated },
                        new int[] { (int)SlotType.Dummy,(int)SlotType.InboundTemp, (int)SlotType.LandingZone,
                    (int)SlotType.OpenStorageArea, (int)SlotType.OutboundTemp, (int)SlotType.PackingArea,
                    (int)SlotType.Rack_LTL, (int)SlotType.Rack_LTL_Parcel, (int)SlotType.Rack_Parcel,
                    (int)SlotType.Regular, (int)SlotType.StagingArea_LTL, (int)SlotType.StagingArea_Parcel,
                    (int)SlotType.FutureAllocated
                        });

                    if (FAPayloadList != null && FAPayloadList.Content != null)
                    {
                        var CurrentFAPayloadList = FAPayloadList.Content;

                        if (target_future_allocated != null && target_future_allocated.Count() > 0)
                        {
                            CurrentFAPayloadList = CurrentFAPayloadList.Where(x => target_future_allocated.Contains(x.UID));
                        }

                        //依照優先權排序 ETD較早 > 能夠補足的品項 > 較早產生的品項
                        var FATargetList = CurrentFAPayloadList
                            .Select(c => new
                            {
                                c.UID,
                                c.VesselManifestUID,
                                c.ItemGroupUID,
                                c.WorkOrderPodUID,
                                c.ItemUID,
                                c.Qty,
                                c.CreatedOn,
                                c.BOL_ETA_D,
                                EachQuantity = PackageCacheManager.GetReceivePackageUomQuantity(
                                    c.PackageUID,
                                    PackageCacheManager.GetMinPackage(c.PackageUID).UID,
                                    c.Qty
                                ).Content
                            })
                            .OrderBy(c => c.BOL_ETA_D)
                            .ThenBy(c => c.EachQuantity)
                            .ThenByDescending(c => c.CreatedOn)
                            .ToList();

                        //列出該Item onHand (總和)
                        var CurrentPayloadList = this.PayloadRepository.GetOnhandPayload(
                            new List<Guid>() { warehouse_uid },
                            fillList.Select(x => x.ItemUID).ToList(),
                            new int[] { (int)PayloadType.Stock },
                            new int[] { (int)SlotType.Dummy, (int)SlotType.LandingZone, (int)SlotType.OpenStorageArea,
                    (int)SlotType.OutboundTemp, (int)SlotType.PackingArea, (int)SlotType.Rack_LTL,
                    (int)SlotType.Rack_LTL_Parcel, (int)SlotType.Rack_Parcel, (int)SlotType.Regular,
                    (int)SlotType.StagingArea_LTL, (int)SlotType.StagingArea_Parcel
                            });

                        if (CurrentPayloadList != null && CurrentPayloadList.Content != null)
                        {
                            var CTargetList = CurrentPayloadList.Content
                                .Where(x => x.Quantity > 0)
                                .Select(c => new
                                {
                                    c.UID,
                                    c.ItemUID,
                                    c.Quantity,
                                    c.Status,
                                    c.CreatedOn,
                                    EachQuantity = PackageCacheManager.GetReceivePackageUomQuantity(
                                        c.PackageUID,
                                        PackageCacheManager.GetMinPackage(c.PackageUID).UID,
                                        c.Quantity
                                    ).Content
                                }).ToList();

                            if (CTargetList.Count > 0)
                            {
                                List<Func<IActionResult<bool>>> actions = new List<Func<IActionResult<bool>>>();
                                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);

                                List<WMSChangeAllocatedInfoModel> shipping_deallocate_list = new List<WMSChangeAllocatedInfoModel>();

                                List<IActionResult<bool>> results = new List<IActionResult<bool>>();
                                //取目前庫存數總量
                                var CTargetItemGroupList = CTargetList
                                .GroupBy(x => x.ItemUID)
                                .Select(c => new OnHandInnerModel()
                                {
                                    ItemUID = c.FirstOrDefault().ItemUID,
                                    //確認可補貨總數
                                    TotalEachQty = c.Select(x => x.EachQuantity).Sum()
                                })
                                .ToList();

                                //處理多箱產品
                                var MBFATargetList = FATargetList
                                    .Where(x => x.ItemGroupUID != null)
                                    .GroupBy(x => x.ItemGroupUID)
                                    .Select(x => new
                                    {
                                        ItemGroupUID = x.FirstOrDefault().ItemGroupUID,
                                        PayloadList = x,
                                        GroupInfo = x
                                        .GroupBy(c => c.ItemUID)
                                        .Select(c => new OnHandInnerModel()
                                        {
                                            ItemUID = c.FirstOrDefault().ItemUID,
                                            ItemGroupUID = c.FirstOrDefault().ItemGroupUID,
                                            //確認可補貨總數
                                            TotalEachQty = c.Select(v => v.EachQuantity).Sum()
                                        })
                                    })
                                    .ToList();
                                using (var act = this.TracingAgent.StartActivity($"process multi-box item "))
                                {
                                    if (MBFATargetList != null && MBFATargetList.Count > 0)
                                    {
                                        var MBVesselManifestList = this.VesselManifestRepository.GetList(new { UID = FATargetList.Where(x => x.ItemGroupUID != null).Select(x => x.VesselManifestUID) });
                                        var MBTIList = this.TicketInfoRepository.GetList(new { WorkOrderPayloadUID = FATargetList.Where(x => x.ItemGroupUID != null).Select(x => x.UID) });
                                        if (MBVesselManifestList != null && MBVesselManifestList.Content != null && MBTIList != null && MBTIList.Content != null)
                                        {
                                            var TIList = MBTIList.Content.ToList();
                                            var VesselManifestList = MBVesselManifestList.Content.ToList();
                                            var NWorkerList = this.TicketInfoAssigneeRelationRepository.GetAssignedList(TIList.Select(x => x.UID).ToArray());
                                            var WorkerList = NWorkerList.Content.ToList();

                                            //當多箱產品庫存均足夠則補貨
                                            foreach (var mbitem in MBFATargetList)
                                            {
                                                List<IAllocatedReplicateModel> prepare_for_deallocate = new List<IAllocatedReplicateModel>();
                                                List<Guid> prepare_for_allocate = new List<Guid>();

                                                //先確認是否均為庫存充足
                                                List<bool> pass_flag = new List<bool>();
                                                foreach (var toitem in mbitem.GroupInfo)
                                                {
                                                    var current_onhand = CTargetItemGroupList.Find(x => x.ItemUID.Equals(toitem.ItemUID));
                                                    if (current_onhand != null)
                                                    {
                                                        if (toitem.TotalEachQty <= current_onhand.TotalEachQty)
                                                        {
                                                            pass_flag.Add(true);
                                                        }
                                                        else
                                                        { pass_flag.Add(false); }
                                                    }
                                                }

                                                if (!pass_flag.Contains(false))
                                                {
                                                    var TNVesselManifestUIDList = mbitem.PayloadList.Select(c => c.VesselManifestUID).ToList();
                                                    var TNVesselManifestList = VesselManifestList.FindAll(x => TNVesselManifestUIDList.Contains(x.UID));
                                                    //如果VesselManifest qty 與 WorkOrderPayload qty 不一致，WorkOrderPayload qty 為主
                                                    if (TNVesselManifestList != null && TNVesselManifestList.Count > 0)
                                                    {
                                                        TNVesselManifestList.ForEach(x => x.Qty = mbitem.PayloadList.FirstOrDefault().Qty);//??
                                                    }
                                                    //處理配貨列表
                                                    var FillPayloadList = ProcessFillAllocatedAndGetWorkOrderPayload(warehouse_uid,
                                                        mbitem.PayloadList.FirstOrDefault().WorkOrderPodUID, TNVesselManifestList,
                                                        mbitem.ItemGroupUID, true, false);
                                                    if (FillPayloadList != null && FillPayloadList.Count > 0)
                                                    {
                                                        var current_na_item = new WMSChangeAllocatedInfoModel()
                                                        {
                                                            OldAllocatedItemRefUID = mbitem.PayloadList.Select(x => x.UID).ToArray(),
                                                            NewAllocatedItems = FillPayloadList.Select(x => new NewAllocatedItem()
                                                            {
                                                                BolUID = TNVesselManifestList.FirstOrDefault().BolUID,
                                                                WorkOrderPodUid = x.WorkOrderPodUID,
                                                                WorkOrderPayloadUID = x.UID,
                                                                ItemUID = x.ItemUID,
                                                                ItemNo = ProductCacheManager.GetItem(x.ItemUID).ID,
                                                                Qty = x.Qty,
                                                                SlotUID = x.SlotUID.Value,
                                                                SlotName = ""
                                                            }).ToArray()
                                                        };
                                                        this.TracingAgent.Trace("Prepare for shipping to clear future allocated items", beforeObject: current_na_item);
                                                        shipping_deallocate_list.Add(current_na_item);

                                                        //減去目前庫存
                                                        foreach (var toitem in mbitem.GroupInfo)
                                                        {
                                                            var current_onhand = CTargetItemGroupList.Find(x => x.ItemUID.Equals(toitem.ItemUID));
                                                            if (current_onhand != null)
                                                            {
                                                                current_onhand.TotalEachQty -= toitem.TotalEachQty;
                                                            }

                                                            var MBFATargetItemList = mbitem.PayloadList.Where(x => x.ItemUID.Equals(toitem.ItemUID)).ToList();
                                                            foreach (var fa_ti_item in MBFATargetItemList)
                                                            {
                                                                var fa_ti_list = TIList.FindAll(x => x.WorkOrderPayloadUID.Equals(fa_ti_item.UID));
                                                                if (fa_ti_list != null && fa_ti_list.Count > 0)
                                                                {
                                                                    var parameter = new ReplicateDataParameter();
                                                                    parameter.TicketInfoUID = fa_ti_list.Select(x => x.UID);
                                                                    IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = this.TicketInfoRepository.GetAllocatedData(parameter).Content;
                                                                    var rp_ti_list = SetPrepareCopyTicketInfo(fa_ti_list, WorkerList, FillPayloadList.Where(x => x.ItemUID.Equals(toitem.ItemUID)).ToList());
                                                                    //完成補貨後的任務項目調整
                                                                    if (rp_ti_list != null && rp_ti_list.Count > 0)
                                                                    {
                                                                        //Prepare for Deallocated
                                                                        prepare_for_deallocate.AddRange(allocatedReplicateModels);
                                                                        prepare_for_allocate.AddRange(rp_ti_list.Select(x => x.UID));

                                                                        this.WorkOrderPayloadRepository.ChangeStatus(fa_ti_item.UID, WorkOrderPayloadStatus.Inactive);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                //多箱產品同步需合併執行
                                                if (prepare_for_deallocate.Count > 0 && prepare_for_allocate.Count > 0)
                                                {
                                                    //Deallocated
                                                    this.TracingAgent.Trace("Prepare for multibox deallocate in replenishment", beforeObject: prepare_for_deallocate);
                                                    actions.Add(() => this.ReplicationManager.Deallocated(null, prepare_for_deallocate));

                                                    //Allocated
                                                    this.TracingAgent.Trace("Prepare for multibox allocate in replenishment", beforeObject: prepare_for_allocate);
                                                    actions.Add(() => this.ReplicationManager.Allcoated(new ReplicateDataParameter() { TicketInfoUID = prepare_for_allocate }));
                                                }
                                            }

                                        }
                                    }
                                }
                                //處理一般Item
                                var NFATargetList = FATargetList.Where(x => x.ItemGroupUID == null).ToList();
                                using (var act2 = this.TracingAgent.StartActivity($"process regular item "))
                                {
                                    if (NFATargetList != null && NFATargetList.Count > 0)
                                    {
                                        var NVesselManifestList = this.VesselManifestRepository.GetList(new { UID = NFATargetList.Select(x => x.VesselManifestUID) });
                                        var NTIList = this.TicketInfoRepository.GetList(new { WorkOrderPayloadUID = NFATargetList.Select(x => x.UID) });
                                        if (NVesselManifestList != null && NVesselManifestList.Content != null && NTIList != null && NTIList.Content != null)
                                        {
                                            var TIList = NTIList.Content.ToList();
                                            var VesselManifestList = NVesselManifestList.Content.ToList();
                                            var NWorkerList = this.TicketInfoAssigneeRelationRepository.GetAssignedList(TIList.Select(x => x.UID).ToArray());
                                            var WorkerList = NWorkerList.Content.ToList();

                                            //排序檢查得出可先補貨的項目
                                            foreach (var faitem in NFATargetList)
                                            {
                                                var current_onhand = CTargetItemGroupList.Find(x => x.ItemUID.Equals(faitem.ItemUID));
                                                if (current_onhand != null)
                                                {
                                                    if (faitem.EachQuantity <= current_onhand.TotalEachQty)
                                                    {
                                                        //處理配貨列表
                                                        var vm_list = VesselManifestList.FindAll(x => x.UID.Equals(faitem.VesselManifestUID));
                                                        //如果VesselManifest qty 與 WorkOrderPayload qty 不一致，WorkOrderPayload qty 為主
                                                        if (vm_list != null && vm_list.Count > 0)
                                                        {
                                                            vm_list.ForEach(x => x.Qty = faitem.Qty);
                                                        }
                                                        var FillPayloadList = ProcessFillAllocatedAndGetWorkOrderPayload(
                                                            warehouse_uid,
                                                            faitem.WorkOrderPodUID,
                                                            vm_list,
                                                            faitem.ItemGroupUID,
                                                            true, false
                                                            );
                                                        if (FillPayloadList != null && FillPayloadList.Count > 0)
                                                        {
                                                            var current_na_item = new WMSChangeAllocatedInfoModel()
                                                            {
                                                                OldAllocatedItemRefUID = new Guid[] { faitem.UID },
                                                                NewAllocatedItems = FillPayloadList.Select(x => new NewAllocatedItem()
                                                                {
                                                                    BolUID = vm_list.FirstOrDefault().BolUID,
                                                                    WorkOrderPodUid = x.WorkOrderPodUID,
                                                                    WorkOrderPayloadUID = x.UID,
                                                                    ItemUID = x.ItemUID,
                                                                    ItemNo = ProductCacheManager.GetItem(x.ItemUID).ID,
                                                                    Qty = x.Qty,
                                                                    SlotUID = x.SlotUID.Value,
                                                                    SlotName = ""
                                                                }).ToArray()
                                                            };
                                                            this.TracingAgent.Trace("Prepare for shipping to clear future allocated items", beforeObject: current_na_item);
                                                            shipping_deallocate_list.Add(current_na_item);

                                                            var fa_ti_list = TIList.FindAll(x => x.WorkOrderPayloadUID.Equals(faitem.UID));
                                                            if (fa_ti_list != null && fa_ti_list.Count > 0)
                                                            {
                                                                var parameter = new ReplicateDataParameter();
                                                                parameter.TicketInfoUID = fa_ti_list.Select(x => x.UID);
                                                                IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = this.TicketInfoRepository.GetAllocatedData(parameter).Content;
                                                                var rp_ti_list = SetPrepareCopyTicketInfo(fa_ti_list, WorkerList, FillPayloadList);
                                                                //完成補貨後的任務項目調整
                                                                if (rp_ti_list != null && rp_ti_list.Count > 0)
                                                                {
                                                                    this.TracingAgent.Trace("Prepare for deallocate in replenishment", beforeObject: allocatedReplicateModels);
                                                                    //Deallocated
                                                                    actions.Add(() => this.ReplicationManager.Deallocated(null, allocatedReplicateModels));

                                                                    var allocate_ti_list = rp_ti_list.Select(x => x.UID);
                                                                    this.TracingAgent.Trace("Prepare for allocate in replenishment", beforeObject: allocate_ti_list);
                                                                    //Allocated
                                                                    actions.Add(() => this.ReplicationManager.Allcoated(
                                                                        new ReplicateDataParameter() { WorkOrderPayloadUID = FillPayloadList.Select(p => p.UID) }));

                                                                    this.WorkOrderPayloadRepository.ChangeStatus(faitem.UID, WorkOrderPayloadStatus.Inactive);
                                                                    current_onhand.TotalEachQty -= faitem.EachQuantity;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else { break; }
                                                }
                                            }
                                        }
                                    }
                                }
                                //this.DbEntities.Commit();
                                this.CommitTransaction();
                                this.DbEntities.DbAdapter.Dispose();
                                this.DbEntities.ReInitConnectionInstance();
                                //更新同步
                                if (actions.Count > 0)
                                {
                                    foreach (var item in actions)
                                    {
                                        results.Add(item.Invoke());
                                    }
                                    if (results.All(p => p.Success))
                                    {
                                        string shipping_service_url = this.AppConfigure.ShippingManagementWebServiceUrl;
                                        if (!String.IsNullOrEmpty(shipping_service_url))
                                        {
                                            ShippingService.WebService ss = new ShippingService.WebService();
                                            ss.Url = shipping_service_url;

                                            var slot_info_list = SlotRepository.GetSlotMappingList(shipping_deallocate_list.SelectMany(x => x.NewAllocatedItems.Select(c => c.SlotUID))).Content;
                                            shipping_deallocate_list.SelectMany(x => x.NewAllocatedItems).ToList().ForEach(p =>
                                            {
                                                p.SlotName = slot_info_list.Where(c => c.UID.Equals(p.SlotUID)).FirstOrDefault().SlotName;
                                            });
                                            using (var act3 = this.TracingAgent.StartActivity($"Ready to launch for shipping to clear future allocated items"))
                                            {
                                                this.TracingAgent.Trace("Ready to launch for shipping to clear future allocated items", beforeObject: shipping_deallocate_list);
                                                var shipping_response = ss.ChangeAllocatedInfo(shipping_deallocate_list.ToArray());

                                                if (shipping_response != null)
                                                {
                                                    act3.AddTag("Sync result", shipping_response.IsComplete);
                                                    act3.AddTag("Sync result message", shipping_response.Message);
                                                    if (shipping_response.IsComplete)
                                                    {
                                                        rs.Success = true;
                                                        rs.Content = (int)ReplenishmentProcessStatus.ReplenishmentCompleted;
                                                        rs.Message = ReplenishmentProcessStatus.ReplenishmentCompleted.ToString();
                                                        return rs;
                                                    }
                                                    else
                                                    {
                                                        this.TracingAgent.Trace(String.Format("Sync Shipping ChangeAllocatedInfo : {0}", shipping_response.Message));
                                                    }
                                                }
                                                else
                                                {
                                                    rs.Success = true;
                                                    rs.Content = (int)ReplenishmentProcessStatus.ReplenishmentFailed;
                                                    rs.Message = "Fail to sync Shipping ChangeAllocatedInfo.";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            rs.Success = true;
                                            rs.Content = (int)ReplenishmentProcessStatus.ReplenishmentFailed;
                                            rs.Message = "Shipping Service URL is empty.";
                                        }
                                    }
                                    else
                                    {
                                        rs.Success = true;
                                        rs.Content = (int)ReplenishmentProcessStatus.ReplenishmentFailed;
                                        rs.Message = "Fail to sync PBSC onhand for replenishment.";
                                    }
                                }
                                else
                                {
                                    rs.Success = true;
                                    rs.Content = (int)ReplenishmentProcessStatus.NoOnHandToDoReplenishment;
                                    rs.Message = "There is no available onhand for future allocated.";
                                }
                            }
                            else
                            {
                                rs.Success = true;
                                rs.Content = (int)ReplenishmentProcessStatus.NoOnHandToDoReplenishment;
                                rs.Message = "There is no available onhand for future allocated.";
                            }
                        }
                        else
                        {
                            rs.Success = true;
                            rs.Content = (int)ReplenishmentProcessStatus.NoOnHandToDoReplenishment;
                            rs.Message = "There is no available onhand for future allocated.";
                        }
                    }
                    else
                    {
                        rs.Success = true;
                        rs.Content = (int)ReplenishmentProcessStatus.NoNeedToDoReplenishment;
                        rs.Message = "Input object is empty.";
                    }
                }
                else
                {
                    rs.Success = true;
                    rs.Content = (int)ReplenishmentProcessStatus.NoNeedToDoReplenishment;
                    rs.Message = "Input object is empty.";
                }
            }
            catch (Exception ex)
            {
                if (this.DbEntities.Transaction != null)
                {
                    //this.DbEntities.Rollback();
                    this.RollbackTransaction();
                }
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.Content = (int)ReplenishmentProcessStatus.NoNeedToDoReplenishment;
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = Resource.COMMON_RETRY;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<int> SetNominateFillFutureAllocated(INominateReplenishmentModel target_future_allocated)
        {
            var rs = ActionResultTemplates.Result<int>();
            if (target_future_allocated != null)
            {
                //列出相對應的WorkOrderPayloadList
                var task_payload_list = this.WorkOrderPayloadRepository.GetList(new { UID = target_future_allocated.WorkOrderPayloadUIDList }).Content;
                if (task_payload_list != null && task_payload_list.Count() > 0)
                {
                    var payload_list = task_payload_list.Select(x => new ReplenishmentModel(target_future_allocated.WarehouseUID.Value, x.ItemUID));
                    rs = SetFillFutureAllocated(payload_list, target_future_allocated.WorkOrderPayloadUIDList);

                    if (rs != null && !rs.Content.Equals((int)ReplenishmentProcessStatus.ReplenishmentCompleted))
                    {
                        var fail_payload_list = this.WorkOrderPayloadRepository.GetList(new { UID = target_future_allocated.WorkOrderPayloadUIDList }).Content
                            .GroupBy(x => x.ItemUID)
                            .Select(x => new { ItemUID = x.FirstOrDefault().ItemUID, Qty = x.Sum(d => d.Qty) });
                        var item_list = this.ProductCacheManager.GetItems(fail_payload_list.Select(x => x.ItemUID)).ToList();
                        string msg = "Failed to allocate. The following item with insufficient on-hand.\n";

                        foreach (var fail_payload_item in fail_payload_list)
                        {
                            var item = item_list.Find(x => x.UID.Equals(fail_payload_item.ItemUID));
                            msg += String.Format("{0} - Qty:{1}", item.ID, fail_payload_item.Qty);
                        }

                        rs.Message = msg;
                    }
                }
                else
                {
                    rs.Content = (int)ReplenishmentProcessStatus.DataHasDamage;
                    rs.Message = "Can not find any of WorkOrderPayload.";
                }
            }
            return rs;
        }
        #endregion

        #region 私有方法

        private Guid? GetShipviaUIDByCarrier(PackingStationCarrier carrier)
        {
            ///為了讓BulkPick 判斷指定的customer 故設定此Carrier
            PartyParameterize param = new PartyParameterize();
            if (carrier == PackingStationCarrier.Fedex)
            {
                param.GroupUID = new Guid("C69136EF-9141-4ED2-AA24-9C23B6A14CF2");
                param.ID = "Fedex  (Packing Station)";

            }
            else if (carrier == PackingStationCarrier.UPS)
            {
                param.GroupUID = new Guid("C69136EF-9141-4ED2-AA24-9C23B6A14CF2");
                param.ID = "UPS (Packing Station)";
            }
            var rs = this.PartyManager.GetParties(param);
            if (rs.Content != null && rs.Content.Count() > 0)
            {
                return rs.Content.First().UID;
            }
            return null;
        }
        private List<IWorkOrderPayloadModel> ProcessFillAllocatedAndGetWorkOrderPayload(Guid WarehouseUID, Guid WorkOrderPodUID,
            IEnumerable<IVesselManifestModel> VesselItems, Guid? ItemGroupUID, bool PassPackageVersion, bool isChinaWarehouse)
        {
            List<IWorkOrderPayloadModel> result = null;

            if (VesselItems != null && VesselItems.Count() > 0)
            {
                var param = new AllocateExecutorParameters
                {
                    InventoryManager = this.InventoryManager,
                    ProductUtility = new ProductUtility(),
                    WorkOrderPayloadRepository = this.WorkOrderPayloadRepository,
                    LabelManager = this.LabelManager,
                    PackageMappingCache = this.PackageCacheManager,
                    SequenceAgent = this.SequenceAgent,
                    TracingAgent = this.TracingAgent
                };


                var executor = new FullAllocatedTemporaryOnhandExecutor(param);
                //確認庫存是否足夠
                AllocatedPlannerInitParameters initParameters = new AllocatedPlannerInitParameters
                {
                    PackageManager = this.PackageManager,
                    PackageMappingCache = this.PackageCacheManager,
                    PackageUomManager = this.PackageUomManager,
                    PackageVersionManager = this.PackageVersionManager,
                    PackageVersionRepository = this.PackageVersionRepository,
                    VesselManager = this,
                    ProductCache = this.ProductCacheManager,
                    WarehouseManger = this.WarehouseManager,
                    OrderType = (int)OrderType.Truckload,
                    TracingAgent = this.TracingAgent,
                    AllocatedExecutor = executor
                };
                //只能補1stock 的貨
                VesselItems.ToList().ForEach(p =>
                {
                    p.OnhandType = (int)InventoryType.Stock;
                });
                var allocatePlanner = AbstractAllocatePlanner.GetInstance(initParameters, AllocateType.GeneralAllocate);

                //取𢔽配貨規劃表
                var allcoatedResult = allocatePlanner.ExternalOrderPlanByWMS(
                    WarehouseUID, VesselItems, PassPackageVersion, isChinaWarehouse
                    );

                //以規劃表設定WorkOrder
                var wparameters = new WorkOrderAssignAgentParameters();
                wparameters.AuthenticationInfo = this.AuthProvider.GetAuthenticationInfo();
                wparameters.SequenceAgent = this.SequenceAgent;
                wparameters.warehouseManger = this.WarehouseManager;
                wparameters.WorkOrderPayloadRepository = this.WorkOrderPayloadRepository;
                wparameters.WorkOrderPodRepository = this.WorkOrderPodRepository;
                wparameters.WorkOrderRepository = this.WorkOrderRepository;
                wparameters.ItemManager = this.ItemManager;
                wparameters.PackageManager = this.PackageManager;
                wparameters.ProductCacheManager = this.ProductCacheManager;
                wparameters.PackageCacheManager = this.PackageCacheManager;
                wparameters.InventoryManager = this.InventoryManager;
                wparameters.WorkOrderManager = this as IWorkOrderManager;
                wparameters.BulkPickWorkOrdrPayloadRelationRepository = this.BulkPickWorkOrdrPayloadRelationRepository;
                wparameters.VesselManifestRepository = this.VesselManifestRepository;
                wparameters.LabelManager = this.LabelManager;
                wparameters.PackageUomManager = this.PackageUomManager;
                wparameters.VesselRepository = this.VesselRepository;
                //wparameters.TransacationScope = this.TransacationScopeModel.Value;
                wparameters.TracingAgent = this.TracingAgent;
                var manifestType = ManifestType.Outbound;
                var agent = AbstractWorkOrderAssignAgent.GetAgent(manifestType, wparameters);


                var outboundWorkOrder = new AssignedOutboundWorkOrderCollection();
                outboundWorkOrder.VesselUID = VesselItems.FirstOrDefault().VesselUID;
                outboundWorkOrder.ServiceType = manifestType;

                outboundWorkOrder.Items = new List<IAssignedOutboundWorkOrderPayload>();
                foreach (var plan in allcoatedResult)
                {
                    // Payload
                    foreach (var paitem in plan.Items)
                    {
                        var payloadModel = new AssignedOutboundWorkOrderPayload()
                        {
                            PayloadUID = paitem.PayloadUID,
                            ItemUID = plan.ItemUID,
                            ItemGroupUID = ItemGroupUID,
                            VesselMainifestUID = plan.VesselManifestUID,
                            AllocatedQty = paitem.AllocatedQty,
                            OnhandPayloadItems = plan.OnhandPayloadItems
                        };
                        outboundWorkOrder.Items.Add(payloadModel);
                    }
                }

                //outboundWorkOrder.Items = items;
                var converter = new AssignedParameterConverter();
                var workOrder = converter.OutboundParameterConvert(outboundWorkOrder);
                workOrder.StorageMethod = (int)StorageMethod.NewPallet;

                //agent.ExistTransactionScope = true;
                var wresult = agent.Execute(workOrder, null);
                List<IActionResult<bool>> allocatedResult = new List<IActionResult<bool>>();
                if (wresult.Success)
                {
                    var WPList = agent.workOrderPayloadModelList;
                    using (var activity2 = this.TracingAgent.StartActivity($"執行生成 Workor order SQL &  執行"))
                    {

                        using (var w2 = this.TracingAgent
                            .StartActivity($"generate  &execute work order payload "))
                        {
                            #region generate  &execute work order payload
                            var wpayloadobj = wresult.Content.WorkOrderPayload;
                            allocatedResult.Add(this.WorkOrderPayloadRepository.AddPayload(wpayloadobj));
                            #endregion
                        }




                        if (allocatedResult.All(p => p.Success))
                        {
                            #region execute payload sql


                            using (var w4 = this.TracingAgent
                            .StartActivity($"execute payload sql count:{wresult.Content.AllocatedExecutorResult.Payloads.Count()}"))
                            {
                                allocatedResult.Add(this
                                .InventoryManager
                                .BatchAddPayload(wresult.Content.AllocatedExecutorResult.Payloads));

                                this.LabelManager
                             .BatchReturnCloneLabel(wresult.Content.AllocatedExecutorResult.CloneLabels);
                            }
                            #endregion

                            WorkOrderManager.AssignedPayloadtoPod(WorkOrderPodUID, WPList.Select(p => p.UID));
                            WPList.ForEach(x =>
                            {
                                x.WorkOrderPodUID = WorkOrderPodUID;
                            });
                            //取回新增的WorkOrderPayload
                            result = WPList;
                        }
                    }

                }
            }

            return result;
        }
        private List<ITicketInfoModel> SetPrepareCopyTicketInfo(List<ITicketInfoModel> TIList, List<ITicketInfoAssigneeRelationModel> worker_list, List<IWorkOrderPayloadModel> FillPayloadList)
        {
            List<ITicketInfoModel> result = null;

            if (TIList != null && TIList.Count > 0 && FillPayloadList != null && FillPayloadList.Count > 0)
            {
                foreach (var current_fpitem in FillPayloadList)
                {
                    //設定 TicketInfo、TicketAssigned 為多筆
                    //讀取FutureAllocated TicketInfo List
                    //var TIList = this.TicketInfoRepository.GetList(new { WorkOrderPayloadUID = OriginalWorkOrderPayloadUID });
                    //複製並建立新的 Ticketinfo
                    result = GetCopyToNewTicketInfo(TIList, current_fpitem.UID, current_fpitem.Qty);
                    //var worker_list = this.TicketInfoAssigneeRelationRepository.GetAssignedList(new Guid[] { TIList.FirstOrDefault().UID });

                    var copy_ob_ticketinfo = this.TicketInfoRepository.AddTickInfos(result);
                    if (copy_ob_ticketinfo.Success && worker_list != null && worker_list.Count > 0)
                    {
                        //加入指派Worker
                        var param = new MaintainWorkderInnerParameters();
                        param.GroupUID = worker_list.Select(p => p.GroupUID).ToArray();
                        param.TicketInfoUID = result.Select(p => p.UID).ToArray();

                        var copy_worker = this.TicketManager.AddWorkder(param);
                        if (copy_worker.Success)
                        {
                            //清除原TicketInfo
                            this.TicketInfoRepository.UpdateTicketInfoStatus(TIList.Select(x => x.UID), TicketInfoStatus.Void);
                            //清除原Worker
                            this.TicketInfoAssigneeRelationRepository.ClearAllWorkder(TIList.Select(x => x.UID).ToArray());
                        }
                    }
                }
            }

            return result;
        }
        private List<ITicketInfoModel> GetCopyToNewTicketInfo(List<ITicketInfoModel> TicketInList, Guid WorkPayloadUID, int Qty)
        {
            List<ITicketInfoModel> _ticketInfos = new List<ITicketInfoModel>();
            if (TicketInList != null && TicketInList.Count > 0)
            {
                var tckSeq = this.SequenceAgent.GetTicketInfoSeqence(TicketInList.FirstOrDefault().TicketUID, TicketInList.Count);
                foreach (var info_item in TicketInList)
                {
                    TicketInfoInnerModel _info = new TicketInfoInnerModel();
                    var _itemSequence = tckSeq.Dequeue();
                    _info.UID = Guid.NewGuid();
                    _info.TicketUID = info_item.TicketUID;
                    _info.Status = (int)TicketInfoStatus.Open;
                    _info.Type = info_item.Type;
                    _info.EstQty = info_item.EstQty;
                    _info.WorkOrderPayloadUID = WorkPayloadUID;
                    _info.EstQty = Qty;
                    _info.ID = _info.Name = _itemSequence;
                    _ticketInfos.Add(_info);
                }
            }
            return _ticketInfos;
        }
        #endregion

    }
}

