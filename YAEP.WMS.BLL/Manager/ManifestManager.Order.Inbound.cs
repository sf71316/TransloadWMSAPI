using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Constants;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Identities.Constants;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.LittleBird.WMS.Models;
using YAEP.Package.Constants;
using YAEP.Package.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.Utilities;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IOrderManager
    {
        public IActionResult<IReceivingResponse> Receiving(IReceivingRequest request)
        {
            Func<IActionResult<bool>> syncMethod = null;
            Stopwatch sw = new Stopwatch();
            IManifestModel manifestModel = null;
            var groups = this.GetGroupUserViewByUser();
            //var _productCacheManager = new ProductCacheManager(this.ItemManager, groups);
            List<IManifestItemListModel> manifestItems = new List<IManifestItemListModel>();
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            ReceivingResult result = new ReceivingResult();
            List<ItemInfo> items = new List<ItemInfo>();
            List<string> notFinditem = new List<string>();
            List<string> duplicateitem = new List<string>();
            List<string> nothavepkg = new List<string>();
            List<string> notmatchpkg = new List<string>();
            List<string> notmatchmaxpkg = new List<string>();
            List<string> notmatchminpkg = new List<string>();
            List<IReceivingContainer> virutalItemContaier = new List<IReceivingContainer>();
            Dictionary<string, IEnumerable<Guid>> LabelMapping = new Dictionary<string, IEnumerable<Guid>>();
            var customerUIDs = new List<Guid>();
            if (!string.IsNullOrEmpty(request.CustomerPartyName))
                customerUIDs.AddRange(this.GetCustomer(groups.Content.Select(x => x.GroupUID),
                    request.CustomerPartyName).Select(p => p.UID));
            else
                customerUIDs.Add(request.CustomerUID);
            var rs = ActionResultTemplates.Result<IReceivingResponse>();
            rs.Content = result;
            rs.Success = false;
            rs.Content.IsComplete = false;
            if (!this.RequestManager.IsRequestProcessing(RequestAction.RECEIVING, request.RefNo))
            {
                this.RequestManager.AddRequest(RequestAction.RECEIVING, request.RefNo);
                if (customerUIDs.Count() > 0)
                {
                    request.Init();

                    try
                    {
                        sw.Start();
                        //if manifest exist
                        var manifestInfo = this.Repository.GetData(new
                        {
                            RefNo = request.RefNo.ToNvarchar(),
                            //PartyUID = request.CustomerUID, //Booking# +WarehouseUID 應該可以判定這個manifest唯一性
                            WarehouseUID = request.WarehouseUID
                        });
                        if (manifestInfo.Content != null)//manifest exist
                        {
                            //manifestModel = manifestInfo.Content;
                            //var mItems = this.ManifestItemListRepository.GetManifestItemList(manifestModel.UID);
                            //if (mItems.Content.Count() > 0)
                            //{
                            //    result.IsComplete = false;
                            //    result.Message = Resource.MANIFEST_ORDER_RECEIVING_EXISTITEM;
                            //    rs.Content = result;
                            //    return rs;
                            //}
                            result.IsComplete = false;
                            result.Message = Resource.MANIFEST_ORDER_RECEIVING_EXISTITEM;
                            rs.Content = result;
                            return rs;
                        }
                        //itemNo convert item Model
                        var pgrp = request.Container;//.SelectMany(x => x.Items).GroupBy(p => p.Name);
                        checkitempgkavailable(pgrp, notFinditem, duplicateitem, nothavepkg, notmatchpkg, notmatchmaxpkg, notmatchminpkg, items,
                            virutalItemContaier, customerUIDs, groups);
                        // notFinditem  找不到item
                        // duplicateitem item 重覆
                        // nothavepkg  找不到包裝

                        if (((notFinditem != null && notFinditem.Count > 0) || (nothavepkg != null && nothavepkg.Count > 0)
                            || (notmatchpkg != null && notmatchpkg.Count > 0))
                            && request.ImportItems != null)
                        {
                            var addItems = request.ImportItems.Where(x => notFinditem.Contains(x.Item.ID) || nothavepkg.Contains(x.Item.ID)
                            || notmatchpkg.Contains(x.Item.ID));
                            var addItemsTask = ProcessPBSCItemAndPackage(addItems);
                            this.DbEntities.ReInitConnectionInstance();
                            if (!addItemsTask.Success)
                            {
                                rs.Message = Resource.MANIFEST_INBOUND_ADD_ITEM_ERROR;
                                rs.Success = false;
                                return rs;
                            }
                        }

                        #region clear item vailate rs / temp data
                        notFinditem.Clear();
                        duplicateitem.Clear();
                        nothavepkg.Clear();
                        notmatchpkg.Clear();
                        notmatchmaxpkg.Clear();
                        notmatchminpkg.Clear();
                        items.Clear();
                        virutalItemContaier.Clear();
                        #endregion
                        checkitempgkavailable(pgrp, notFinditem, duplicateitem, nothavepkg, nothavepkg, notmatchmaxpkg, notmatchminpkg, items,
                            virutalItemContaier, customerUIDs, groups);

                        //var vItemcollection = request.Container.SelectMany(x => x.Items)
                        //    .Select(x => this.ProductCacheManager.);
                        sw.Stop();
                        OutpubToDebugLine($"Inbound collect item info elapsed {sw.ElapsedMilliseconds}ms");
                        sw.Restart();
                        if (notFinditem.Count == 0 && duplicateitem.Count == 0 && nothavepkg.Count == 0 && notmatchpkg.Count == 0
                            && notmatchmaxpkg.Count == 0 && notmatchminpkg.Count == 0)
                        {


                            #region Manifest 

                            if (manifestModel == null)  //manifest unexist
                            {
                                var _seq = this.SequenceAgent.GetManinfestSequence(SequenceAgent.GetManifestRootUID(), ManifestType.Inbound);
                                // create manifest
                                ManinfestInnerModel model = new ManinfestInnerModel();
                                model.UID = Guid.NewGuid();
                                model.ID = _seq;
                                model.Name = request.RefNo;
                                model.RefNo = request.RefNo;
                                model.PartyUID = customerUIDs.FirstOrDefault();//TODO 暫定
                                model.Status = ManifestStatus.Open;
                                model.Type = (int)ManifestType.Inbound;
                                model.WarehouseUID = request.WarehouseUID;
                                model.Volume = 0;
                                model.Weight = 0;
                                if (request.IsTransferOrder)
                                {
                                    model.Description = "TransferOrder";
                                }
                                _action.Push(() => this.Repository.Add(model));
                                manifestModel = model;
                            }
                            #endregion
                            #region  manifest item
                            //create mainifest item 

                            var itemCollection = request.Container.SelectMany(x => x.Items)
                               .Where(p => p.VirtualItems == null || p.VirtualItems.Count == 0).GroupBy(p => new
                               {
                                   ContainerUID = p.ContainerUID,
                                   ItemNo = p.Name,
                               }).ToList();
                            itemCollection.AddRange(virutalItemContaier.SelectMany(x => x.Items).GroupBy(p => new
                            {
                                ContainerUID = p.ContainerUID,
                                ItemNo = p.Name,
                            }));

                            var _seqCol = this.SequenceAgent.GetMainfestItemListSequence(
                    SequenceAgent.GetManifestRootUID(), itemCollection.Count());
                            foreach (var itemNo in itemCollection)
                            {
                                var itemInfo = items.FirstOrDefault(p => p.Item.Name == itemNo.Key.ItemNo);
                                if (itemInfo != null)
                                {
                                    var _seq = _seqCol.Dequeue();
                                    ManifestItemInnerModel m = new ManifestItemInnerModel();
                                    m.UID = Guid.NewGuid();
                                    m.ID = _seq;
                                    m.ItemUID = itemInfo.Item.UID;
                                    m.ManifestUID = manifestModel.UID;
                                    m.PackageQty = itemNo.Sum(s => s.PackageQty);
                                    m.PackageUID = itemInfo.Package.UID;
                                    m.Volume = this.ProductUtility.CalculateCUFT(itemInfo.Package, m.PackageQty.Value);
                                    m.Weight = this.ProductUtility.CaculateTTLWeight(itemInfo.Package, m.PackageQty.Value);
                                    m.Status = ManifestItemListStatus.Draft;
                                    var container = request.Container.FirstOrDefault(p => p.UID == itemNo.First().ContainerUID);
                                    if (container != null)
                                    {
                                        container.ManifestItemUID.Add(m.UID);
                                    }
                                    else
                                    {

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
                            #region create bol
                            List<IBolModel> bolModels = new List<IBolModel>();
                            List<IVesselModel> vesselModels = new List<IVesselModel>();
                            List<IVesselManifestModel> vesselManifestModels = new List<IVesselManifestModel>();
                            //var groupbyBol = request.Container.SelectMany(p => p.Items);
                            var extString = "";
                            if (request.Container.Any(p => !string.IsNullOrEmpty(p.ExternalData)))
                            {
                                var externaldata = request.Container
                                    .Select(p => JsonConvert.DeserializeObject<ReceivingExternalModel>(p.ExternalData));
                                extString = string.Join(",", externaldata.GroupBy(g => new { BOLNO = g.BOLNO, Container = g.Name })
                                    .Select(p => p.Key.BOLNO + "|" + p.Key.Container + "|" + string.Join("|", p.Select(x => x.SONO))));
                            }
                            var _bseq = this.SequenceAgent.GetBOLSeqence(SequenceAgent.GetManifestRootUID());
                            BolInnerModel bolModel = new BolInnerModel();
                            bolModel.UID = Guid.NewGuid();
                            bolModel.ID = _bseq;
                            bolModel.Name = "BOL-" + request.RefNo;
                            bolModel.RefNo = "BOL-" + request.RefNo + "@" + extString;
                            bolModel.ManifestUID = manifestModel.UID;
                            bolModel.Contact = "";
                            bolModel.ShipViaUID = Guid.Empty;
                            bolModel.ShipMethodUID = Guid.Empty;
                            bolModel.Status = BolStatus.Open;
                            _action.Push(() => this.BolRepository.AddBol(bolModel));
                            bolModels.Add(bolModel);
                            #endregion
                            #region create vessel

                            var _vseq =// DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            this.SequenceAgent.GetVesselSeqence(SequenceAgent.GetManifestRootUID());
                            VesselInnerModel vesselModel = new VesselInnerModel();
                            vesselModel.UID = Guid.NewGuid();
                            vesselModel.ID = _vseq;
                            vesselModel.Name = "Vessel " + request.RefNo;
                            vesselModel.RefNo = "Vessel " + request.RefNo;
                            vesselModel.Type = 1;
                            vesselModel.BolUID = bolModel.UID;
                            vesselModel.Status = (int)VesselStatus.Open;
                            _action.Push(() => this.VesselRepository.AddVessel(vesselModel));
                            vesselModels.Add(vesselModel);
                            #endregion
                            #region create vessel manifest item

                            var _seqVCol = this.SequenceAgent.GetVesselManifestSequence(
                               SequenceAgent.GetManifestRootUID(), itemCollection.Count());
                            foreach (var container in request.Container)
                            {
                                //List<Guid> vesselmanifestUID = new List<Guid>();
                                var _viseq = _seqVCol.Dequeue();
                                var mitemCollection = manifestItems.Where(p => container.ManifestItemUID.Contains(p.UID));
                                List<Guid> vesselInfoUID = new List<Guid>();
                                List<IVesselManifestModel> vesselManifestCollection = new List<IVesselManifestModel>();
                                foreach (var mitemInfo in mitemCollection)
                                {
                                    var itemInfo = items.FirstOrDefault(p => p.Item.UID == mitemInfo.ItemUID);
                                    VesselManifestItemInnerModel vitem = new VesselManifestItemInnerModel();
                                    vitem.UID = Guid.NewGuid();
                                    vitem.ID = _viseq;
                                    vitem.ItemUID = mitemInfo.ItemUID;
                                    vitem.ManifestItemUID = mitemInfo.UID;
                                    vitem.PartyUID = customerUIDs.FirstOrDefault();
                                    vitem.VesselUID = vesselModel.UID;
                                    vitem.BolUID = bolModel.UID;
                                    vitem.Status = (int)VesselManifestStatus.Open;
                                    vitem.Qty = mitemInfo.PackageQty.Value;
                                    vitem.Volume = this.ProductUtility.CalculateCUFT(itemInfo.Package, vitem.Qty);
                                    vitem.Weight = this.ProductUtility.CaculateTTLWeight(itemInfo.Package, vitem.Qty);
                                    vitem.PackageUID = mitemInfo.PackageUID;
                                    vesselManifestModels.Add(vitem);
                                    vesselManifestCollection.Add(vitem);
                                    vesselInfoUID.Add(vitem.UID);
                                }
                                _action.Push(() => this.VesselManifestRepository.BatchAddVesselManifest(vesselManifestCollection));
                                foreach (var item in container.Items)
                                {
                                    if (!LabelMapping.ContainsKey(item.Barcode))
                                        LabelMapping.Add(item.Barcode, vesselInfoUID);
                                }
                                //container.VesselManifestUID = vitem.UID;
                                //vesselmanifestUID.Add(vitem.UID);


                            }


                            #endregion
                            #region inbound auto assigned model paramters init
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
                                WorkOrderManager = this.WorkOrderManager
                            };
                            #endregion
                            sw.Stop();
                            OutpubToDebugLine($"prepare auto assigned object elapsed {sw.ElapsedMilliseconds}ms");
                            sw.Restart();
                            var autoAssignAgent = new ExternalInboundAutoAssignAgent(provider);
                            var parameters = new InboundAutoAssignedParameters();
                            parameters.ReceivingRequest = request;
                            parameters.Manifest = manifestModel;
                            parameters.ManifestItems = manifestItems;
                            parameters.Vessel = vesselModels;
                            parameters.VesselItems = vesselManifestModels;
                            parameters.Bol = bolModels;
                            parameters.LabelMapping = LabelMapping;
                            parameters.PackageCacheManager = this.PackageCacheManager;
                            parameters.WarehouseUID = request.WarehouseUID;
                            parameters.ForceWorkOrderOpen = true;
                            using (var db = this.DbEntities.DbAdapter)
                            {
                                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                                //created manifest data
                                var action1 = _action.Reverse();
                                foreach (var item in action1)
                                {
                                    Result.Add(item.Invoke());
                                }
                                sw.Stop();
                                OutpubToDebugLine($"insert manifest data elapsed {sw.ElapsedMilliseconds}ms");
                                sw.Restart();
                                if (Result.All(x => x.Success))
                                {
                                    _action.Clear();
                                    //create Workorderdd
                                    //create workorder pod
                                    //create workorder payload
                                    var response = autoAssignAgent.Execute(parameters);
                                    sw.Stop();
                                    OutpubToDebugLine($"assigned workorder data elapsed {sw.ElapsedMilliseconds}ms");
                                    sw.Restart();
                                    if (response.IsComplete)
                                    {
                                        //approve bol
                                        _action.Push(() =>
                                        {
                                            var brs = ActionResultTemplates.Result<bool>();
                                            brs.Success = true;
                                            try
                                            {
                                                foreach (var bol in bolModels)
                                                {
                                                    var resultBol = this.ForceApproveBol(bol.UID, manifestModel.WarehouseUID, manifestModel.Type);
                                                    if (resultBol.Success)
                                                    {

                                                        brs.Content &= resultBol.Content;
                                                        brs.Success &= resultBol.Success;
                                                        brs.Message += " " + result.Message;
                                                    }
                                                    else
                                                    {
                                                        brs.Content &= resultBol.Success;
                                                        brs.Success &= resultBol.Success;
                                                        brs.Message += " " + result.Message;
                                                    }
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                this.TracingAgent.Trace(ex.Message, ex);
                                                brs.Message = Resource.COMMON_RETRY;
                                                brs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                                                brs.Success = false;
                                                brs.InnerException = ex;
                                            }
                                            return brs;
                                        });


                                        var action2 = _action.Reverse();
                                        foreach (var item in action2)
                                        {
                                            Result.Add(item.Invoke());
                                        }
                                        sw.Stop();
                                        OutpubToDebugLine($"approve bol data (include ticket) elapsed {sw.ElapsedMilliseconds}ms");
                                        sw.Restart();
                                        if (Result.All(p => p.Success))//資料全部新建完成
                                        {
                                            //create ticket
                                            List<IActionResult<bool>> assignResult = new List<IActionResult<bool>>();
                                            var ticketinfocollection = this.TicketRepository.GetTicketInfoList(bolModels.Select(p => p.UID));
                                            var groupsInfo = DrKnowAll.GetGroup(groups.Content.Select(p => p.GroupUID));
                                            groupsInfo = groupsInfo.Where(x => x.Type == (int)GroupTypes.Team);
                                            foreach (var bol in bolModels)
                                            {
                                                //assigned ticket
                                                var ticketinfos = ticketinfocollection.Content.Where(p => p.BolUID == bol.UID);
                                                var param = new MaintainWorkderInnerParameters();
                                                param.GroupUID = groupsInfo.Select(p => p.UID).ToArray();
                                                param.TicketInfoUID = ticketinfos.Select(p => p.UID).ToArray();
                                                assignResult.Add(this.TicketManager.AddWorkder(param));
                                                sw.Stop();
                                                OutpubToDebugLine($"assigned ticket elapsed {sw.ElapsedMilliseconds}ms");
                                                sw.Restart();


                                                //register receiver 
                                                if (!string.IsNullOrEmpty(request.ReceiverUrl) &&
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
                                                        assignResult.Add(this.ReceiverRepository.Add(receiver));
                                                    }

                                                }

                                            }
                                            if (assignResult.All(p => p.Success))
                                            {
                                                db.Commit();
                                                //replicate to subscriber
                                                syncMethod = () => this.ReplicationManager.Receiving(ticketinfocollection.Content.Select(p => p.UID));

                                                rs.Success = true;
                                                rs.Content.IsComplete = true;
                                            }
                                            else
                                            {
                                                db.Rollback();
                                                rs.Success = false;
                                                rs.Message += string.Join(",", assignResult.Select(p => p.Message).ToArray());
                                                rs.Content.IsComplete = false;
                                                rs.Content.Message = rs.Message;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result.IsComplete = rs.Success = false;
                                        result.Message = response.Message;
                                    }
                                }
                                else
                                {
                                    result.IsComplete = rs.Success = false;
                                    result.Message = string.Join(",", Result.Select(x => x.Message));
                                }
                            }
                            this.DbEntities.ReInitConnectionInstance();
                            if (syncMethod != null)
                            {
                                var syncRs = syncMethod.Invoke();
                                this.TracingAgent.Trace($"Receiving syncResult:{syncRs.Success} Message:{syncRs.Message}");
                            }
                        }
                        else
                        {
                            result.IsComplete = false;
                            StringBuilder msg = new StringBuilder();
                            if (notFinditem.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_FIND, string.Join(",", notFinditem)));
                            }
                            if (duplicateitem.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_DUPLICATE, string.Join(",", duplicateitem)));
                            }
                            if (nothavepkg.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_PACKAGE, string.Join(",", nothavepkg)));
                            }
                            if (notmatchpkg.Count > 0)
                            {
                                msg.Append(string.Format("Not find package please check  package setting in P item", string.Join(",", notmatchpkg)));
                            }
                            if (notmatchmaxpkg.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_MATCH_PACKAGE, string.Join(",", notmatchmaxpkg)));
                            }
                            if (notmatchminpkg.Count > 0)
                            {
                                msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_MATCH_MIN_PACKAGE, string.Join(",", notmatchminpkg)));
                            }
                            result.Message = msg.ToString();
                            rs.Content = result;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.TracingAgent.Trace(ex.Message, ex);
                        rs.Message = Resource.COMMON_RETRY;
                        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                        rs.Success = false;
                        rs.Content.IsComplete = false;
                        rs.Content.Message = ex.Message;
                        rs.InnerException = ex;
                    }
                    finally
                    {
                        this.RequestManager.RemoveRequest(RequestAction.RECEIVING, request.RefNo);
                    }
                    return rs;
                }
                else
                {
                    this.RequestManager.RemoveRequest(RequestAction.RECEIVING, request.RefNo);
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_ORDER_ALLOCATED_NOT_FIND_CUSTOMERUID;
                    return rs;
                }
            }
            else
            {
                rs.Content.IsComplete = false;
                rs.Content.Message = string.Format(Resource.COMMON_REQUEST_ISPROCESSING, $"Booking#{request.RefNo}");
                return rs;
            }
        }

        /// <summary>
        /// Transload 收貨：比照既有 Receiving，一次呼叫完成「生成 + 完成 + 建庫存」。
        /// (1) 生成交易：1 Manifest(Inbound,Open) + Manifest_Item_List + 1 BOL + 每櫃 1 Vessel(含容器 5 新欄) + Vessel_Manifest，
        ///     再跑 inbound auto-assign（每櫃 scoped 一次，避開 planner 的 VesselItems.First() 多櫃問題）生成 WorkOrder/Pod/Payload，
        ///     ForceApproveBol 生成 Ticket，AddWorkder 指派 team 群組，commit。
        /// (2) 生成成功後內部呼叫 <see cref="CompleteReceivingByTransload"/>（系統 IsAllPass）→ 建 WMS_PayLoad(Stock)+WMS_Inventory。
        ///     此完成段現由內部觸發；未來改由工人/獨立 API 觸發時移除該段即可。
        /// 不動 production Receiving；重用既有 Repository / 交易 / auto-assign / CompleteTicketData。
        /// </summary>
        public IActionResult<ITransloadReceivingResult> ReceivingByTransload(ITransloadReceivingInput input)
        {
            var rs = ActionResultTemplates.Result<ITransloadReceivingResult>();
            rs.Success = false;
            try
            {
                // 基本驗證
                var containers = input?.Containers?.ToList() ?? new List<ITransloadReceivingContainer>();
                if (input == null || containers.Count == 0)
                {
                    rs.Message = "No container.";
                    return rs;
                }

                // 客戶解析（CustomerPartyName → PartyUID，跨使用者群組，與既有 Receiving 同方式）
                var groups = this.GetGroupUserViewByUser();
                var customerUID = this.GetCustomer(groups.Content.Select(x => x.GroupUID), input.CustomerPartyName)
                                      .Select(p => p.UID).FirstOrDefault();
                if (customerUID == Guid.Empty)
                {
                    rs.Message = Resource.MANIFEST_ORDER_ALLOCATED_NOT_FIND_CUSTOMERUID;
                    return rs;
                }

                // 查重複（RefNo + WarehouseUID 已存在則擋）
                var existed = this.Repository.GetData(new
                {
                    RefNo = input.RefNo.ToNvarchar(),
                    WarehouseUID = input.WarehouseUID
                });
                if (existed != null && existed.Content != null)
                {
                    rs.Message = Resource.MANIFEST_ORDER_RECEIVING_EXISTITEM;
                    return rs;
                }

                // ── 生成前先驗證所有行的「產品 + 包裝」資料（任一缺 → 整批失敗，不寫任何資料）──
                // 產品：對齊既有 checkitempgkavailable 的 ProductCacheManager.GetItem(sku, 客戶, 群組)（含客戶/群組可見性 + 重複偵測）。
                // 包裝：用 PackageUID 直驗（精確到版本，比既有 UOM 字串配對精準），再以 UID==pkg.ItemUID 綁回產品。
                var allItems = containers.SelectMany(c => c.Items ?? Enumerable.Empty<ITransloadReceivingItem>()).ToList();
                if (allItems.Count == 0)
                {
                    rs.Message = "No item.";
                    return rs;
                }
                var customerUIDList = new List<Guid> { customerUID };
                var pkgCache = new Dictionary<Guid, IPackageModel>();
                var missingPackage = new List<string>();    // 包裝不存在
                var missingProduct = new List<string>();     // 產品不存在（含客戶/群組看不到）
                var duplicateProduct = new List<string>();   // 同 SKU 多筆（跨客戶重複，無法判定）
                var skuMismatch = new List<string>();        // SKU 解析的產品 ≠ PackageUID 所屬產品
                foreach (var it in allItems)
                {
                    // 包裝（PackageUID 直驗）
                    var pkg = (it.PackageUID != Guid.Empty)
                        ? (pkgCache.ContainsKey(it.PackageUID) ? pkgCache[it.PackageUID] : this.PackageCacheManager.GetPackage(it.PackageUID))
                        : null;
                    if (pkg == null)
                    {
                        missingPackage.Add(it.Sku ?? it.PackageUID.ToString());
                        continue;
                    }
                    pkgCache[it.PackageUID] = pkg;

                    // 產品（對齊既有：SKU + 客戶 + 群組）
                    var found = this.ProductCacheManager.GetItem(it.Sku, customerUIDList, groups.Content)?.ToList();
                    var cnt = found?.Count ?? 0;
                    if (cnt == 0)
                    {
                        missingProduct.Add(string.IsNullOrWhiteSpace(it.Sku) ? "(empty)" : it.Sku);
                        continue;
                    }
                    if (cnt > 1)
                    {
                        duplicateProduct.Add(it.Sku);
                        continue;
                    }
                    // 一致性：SKU 解析到的產品要等於 PackageUID 所屬產品
                    if (found[0].UID != pkg.ItemUID)
                    {
                        skuMismatch.Add($"{it.Sku}↔package");
                    }
                }
                if (missingPackage.Count > 0 || missingProduct.Count > 0 || duplicateProduct.Count > 0 || skuMismatch.Count > 0)
                {
                    var msg = new StringBuilder();
                    if (missingProduct.Count > 0) msg.Append($"Product not found: {string.Join(",", missingProduct)}. ");
                    if (duplicateProduct.Count > 0) msg.Append($"Duplicate product: {string.Join(",", duplicateProduct)}. ");
                    if (missingPackage.Count > 0) msg.Append($"Package not found: {string.Join(",", missingPackage)}. ");
                    if (skuMismatch.Count > 0) msg.Append($"SKU/Package mismatch: {string.Join(",", skuMismatch)}. ");
                    rs.Message = msg.ToString().Trim();
                    return rs;
                }

                var rootUID = SequenceAgent.GetManifestRootUID();

                // ── Manifest ──
                var manifest = new ManinfestInnerModel
                {
                    UID = Guid.NewGuid(),
                    ID = this.SequenceAgent.GetManinfestSequence(rootUID, ManifestType.Inbound),
                    Name = input.RefNo,
                    RefNo = input.RefNo,
                    PartyUID = customerUID,
                    Status = ManifestStatus.Open,
                    Type = (int)ManifestType.Inbound,
                    WarehouseUID = input.WarehouseUID,
                    Volume = 0,
                    Weight = 0,
                };

                // ── BOL（1 筆，掛 Manifest；Vessel/VesselManifest 都引用其 UID，故先建）──
                var bol = new BolInnerModel
                {
                    UID = Guid.NewGuid(),
                    ID = this.SequenceAgent.GetBOLSeqence(rootUID),
                    Name = "BOL-" + input.RefNo,
                    RefNo = "BOL-" + input.RefNo,
                    ManifestUID = manifest.UID,
                    Contact = "",
                    ShipViaUID = Guid.Empty,
                    ShipMethodUID = Guid.Empty,
                    Status = BolStatus.Open,
                };

                // ── 每櫃：Vessel + Manifest_Item_List + Vessel_Manifest（一次處理以正確串接 Vessel↔ManifestItem↔VesselManifest）──
                // 同時組裝 auto-assign(A2) 所需：每行 1 個 pallet barcode、LabelMapping(barcode→VesselManifest UID)、內部 IReceivingRequest(planner 讀)。
                var manifestItems = new List<IManifestItemListModel>();
                var vessels = new List<IVesselModel>();
                var vesselManifests = new List<IVesselManifestModel>();
                var labelMapping = new Dictionary<string, IEnumerable<Guid>>();
                var innerRequest = new TransloadReceivingInnerRequest
                {
                    RefNo = input.RefNo,
                    WarehouseUID = input.WarehouseUID,
                    CustomerPartyName = input.CustomerPartyName,
                    CustomerUID = customerUID,
                };
                var itemLineCount = containers.Sum(c => c.Items?.Count() ?? 0);
                var itemSeq = itemLineCount > 0 ? this.SequenceAgent.GetMainfestItemListSequence(rootUID, itemLineCount) : null;
                var vmSeq = itemLineCount > 0 ? this.SequenceAgent.GetVesselManifestSequence(rootUID, itemLineCount) : null;
                foreach (var c in containers)
                {
                    var rcvContainer = new ReceivingContainer { UID = Guid.NewGuid() };
                    innerRequest.Container.Add(rcvContainer);

                    var vessel = new VesselInnerModel
                    {
                        UID = Guid.NewGuid(),
                        ID = this.SequenceAgent.GetVesselSeqence(rootUID),
                        Name = "Vessel " + (string.IsNullOrWhiteSpace(c.ConNo) ? input.RefNo : c.ConNo),
                        RefNo = c.ConNo,
                        Type = 1,
                        BolUID = bol.UID,
                        Status = (int)VesselStatus.Open,
                        // 櫃資訊(Seal/ContainerType/Loading/Stackable/Arrival)改由既有 SetContainerInfo 端點補寫(避免動 Vessel schema/AddVessel),
                        // 此處只建立 Vessel 基本欄位;SSCC barcode 不依賴這些欄。
                    };
                    vessels.Add(vessel);

                    foreach (var it in (c.Items ?? Enumerable.Empty<ITransloadReceivingItem>()))
                    {
                        var pkg = pkgCache[it.PackageUID];               // 已驗證存在
                        var vol = this.ProductUtility.CalculateCUFT(pkg, it.EnterQty);
                        var wt = this.ProductUtility.CaculateTTLWeight(pkg, it.EnterQty);

                        var mi = new ManifestItemInnerModel
                        {
                            UID = Guid.NewGuid(),
                            ID = itemSeq.Dequeue(),
                            ItemUID = pkg.ItemUID,
                            ManifestUID = manifest.UID,
                            PackageUID = it.PackageUID,
                            PackageQty = it.EnterQty,
                            Volume = vol,
                            Weight = wt,
                            Status = ManifestItemListStatus.Draft,
                        };
                        manifestItems.Add(mi);

                        var vm = new VesselManifestItemInnerModel
                        {
                            UID = Guid.NewGuid(),
                            ID = vmSeq.Dequeue(),
                            ItemUID = pkg.ItemUID,
                            ManifestItemUID = mi.UID,
                            PartyUID = customerUID,
                            VesselUID = vessel.UID,        // 綁到「這一櫃」
                            BolUID = bol.UID,
                            PackageUID = it.PackageUID,
                            Qty = it.EnterQty,
                            Volume = vol,
                            Weight = wt,
                            Status = (int)VesselManifestStatus.Open,
                        };
                        vesselManifests.Add(vm);

                        // A2 用：每行 1 個系統生成 pallet barcode（SSCC 格式，GenerateBarcode 內含 lock+毫秒戳保證唯一）→ 對應這筆 VesselManifest
                        var barcode = this.WarehouseManager.GenerateBarcode(BarcodeType.Pallet);
                        labelMapping[barcode] = new[] { vm.UID };
                        rcvContainer.Items.Add(new ReceivingContainerItem
                        {
                            UID = Guid.NewGuid(),
                            ContainerUID = rcvContainer.UID,
                            Name = it.Sku,
                            PackageQty = it.EnterQty,
                            Barcode = barcode,
                            ItemUID = new List<Guid> { pkg.ItemUID },
                        });
                    }
                }

                // ── 交易寫入：Manifest → Items → BOL → Vessels → VesselManifests（依 FK 相依順序）──
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    var results = new List<IActionResult<bool>>();
                    results.Add(this.Repository.Add(manifest));
                    if (manifestItems.Count > 0)
                    {
                        results.Add(this.ManifestItemListRepository.Add(manifestItems));
                    }
                    results.Add(this.BolRepository.AddBol(bol));
                    foreach (var v in vessels)
                    {
                        results.Add(this.VesselRepository.AddVessel(v));
                    }
                    if (vesselManifests.Count > 0)
                    {
                        results.Add(this.VesselManifestRepository.AddVesselManifest(vesselManifests));
                    }

                    if (results.All(x => x.Success))
                    {
                        // ── A2：inbound auto-assign 生成 WorkOrder/Pod/Payload，再核准 BOL 生成 Ticket（同交易內）──
                        // 為避開既有 DefaultReceivingPlanner 把 WorkOrder.VesselUID 一律設成第一個 vessel 的問題，
                        // 改成「每櫃各跑一次 Execute（scoped 單一 vessel）」，使 planner 的 VesselItems.First() 自然就是該櫃。
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
                            WorkOrderManager = this.WorkOrderManager
                        };
                        var autoAssignAgent = new ExternalInboundAutoAssignAgent(provider);

                        var assignOk = true;
                        var assignMsg = "";
                        for (int ci = 0; ci < vessels.Count && assignOk; ci++)
                        {
                            var vessel = vessels[ci];
                            var vItems = vesselManifests.Where(x => x.VesselUID == vessel.UID).ToList();
                            if (vItems.Count == 0) continue;   // 空櫃不需規畫
                            var vmUIDset = new HashSet<Guid>(vItems.Select(x => x.UID));
                            var miForVessel = manifestItems.Where(mi => vItems.Any(x => x.ManifestItemUID == mi.UID)).ToList();
                            var lblForVessel = labelMapping.Where(kv => kv.Value.Any(u => vmUIDset.Contains(u)))
                                                           .ToDictionary(kv => kv.Key, kv => kv.Value);
                            var scopedReq = new TransloadReceivingInnerRequest
                            {
                                RefNo = input.RefNo,
                                WarehouseUID = input.WarehouseUID,
                                CustomerPartyName = input.CustomerPartyName,
                                CustomerUID = customerUID,
                            };
                            scopedReq.Container.Add(innerRequest.Container[ci]);

                            var assignParam = new InboundAutoAssignedParameters
                            {
                                ReceivingRequest = scopedReq,
                                Manifest = manifest,
                                ManifestItems = miForVessel,
                                Vessel = new List<IVesselModel> { vessel },
                                VesselItems = vItems,
                                Bol = new List<IBolModel> { bol },
                                LabelMapping = lblForVessel,
                                PackageCacheManager = this.PackageCacheManager,
                                WarehouseUID = input.WarehouseUID,
                                ForceWorkOrderOpen = true,
                            };
                            var assignResp = autoAssignAgent.Execute(assignParam);
                            if (!assignResp.IsComplete)
                            {
                                assignOk = false;
                                assignMsg = assignResp.Message;
                            }
                        }

                        if (!assignOk)
                        {
                            db.Rollback();
                            rs.Success = false;
                            rs.Message = assignMsg;
                        }
                        else
                        {
                            // 核准 BOL → 生成收貨 Ticket（之後由 CompleteReceiving 觸發完成、建庫存）
                            var bolRs = this.ForceApproveBol(bol.UID, input.WarehouseUID, (int)ManifestType.Inbound);
                            if (!bolRs.Success)
                            {
                                db.Rollback();
                                rs.Success = false;
                                rs.Message = bolRs.Message;
                            }
                            else
                            {
                                // Assign 工單：把 ForceApproveBol 生成的 ticketinfo 指派給 team 群組
                                //（CompleteReceiving 完成時 GetInfoData 需有指派關係才撈得到；不 register receiver、不 replication）
                                var ticketInfoColl = this.TicketRepository.GetTicketInfoList(new[] { bol.UID });
                                // Team 群組改由「倉庫」解析（warehouse.GroupUID → 子群組 Type=Team），
                                // 不依賴登入者(T2user 為 API 帳號、不在任何 team)是否在 team。
                                //var whGroupUID = this.WarehouseManager.GetWarehouse(input.WarehouseUID)?.Content?.GroupUID ?? Guid.Empty;
                                var groupsInfo = DrKnowAll.GetGroup(groups.Content.Select(p => p.GroupUID));
                                groupsInfo = groupsInfo.Where(x => x.Type == (int)GroupTypes.Team);
                                //var teamGroupUIDs = DrKnowAll.GetGroup()
                                //                             .Where(g => g.Status > 0 && g.Type == (int)GroupTypes.Team && g.ParentUID == whGroupUID)
                                //                             .Select(g => g.UID).ToArray();
                                var ticketInfoUIDs = (ticketInfoColl.Content ?? Enumerable.Empty<IAssignedTicketInfoModel>())
                                                             .Where(p => p.BolUID == bol.UID)
                                                             .Select(p => p.UID).ToArray();
                                var workerParam = new MaintainWorkderInnerParameters
                                {
                                    GroupUID = groupsInfo.Select(g => g.UID).ToArray(),
                                    TicketInfoUID = ticketInfoUIDs,
                                };
                                var assignWorkerRs = this.TicketManager.AddWorkder(workerParam);
                                if (assignWorkerRs.Success)
                                {
                                    db.Commit();
                                    rs.Success = true;
                                    rs.Content = new TransloadReceivingResult
                                    {
                                        ManifestUID = manifest.UID,
                                        Vessels = vessels,
                                    };
                                }
                                else
                                {
                                    db.Rollback();
                                    rs.Success = false;
                                    rs.Message = assignWorkerRs.Message;
                                }
                            }
                        }
                    }
                    else
                    {
                        db.Rollback();
                        rs.Success = false;
                        rs.Message = string.Join(",", results.Where(x => !x.Success).Select(x => x.Message));
                    }
                }
                this.DbEntities.ReInitConnectionInstance();

                // 生成成功(含 SSCC pallet barcode 落 WorkOrder_Pod)。
                // 完成段(建 PayLoad Stock+Inventory)改由 facade 接既有 CompleteInbound(會建 home address+上架到可揀位),
                // 故此處不再內部呼叫 CompleteReceivingByTransload(其 CompleteTicketData 對 transload 生成的票會缺 home address 而 NRE)。
                return rs;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                return rs;
            }
        }

        /// <summary>
        /// Transload 收貨完成（系統觸發、IsAllPass）：manifest → BOL → Ticket，依 Type 分「收貨/移動」兩段完成。
        /// 第一段完成收貨票（建 WMS_PayLoad(Stock) 落在 InboundTemp 暫存區）；
        /// 第二段完成移動票（把 PayLoad 從暫存區搬到實際儲位，成為可用庫存）。
        /// 兩段必須分開呼叫 CompleteTicketData：收貨與移動票塞同一次呼叫時，移動票會抓不到
        /// 收貨剛建、尚未對該次呼叫可見的 PayLoad 而靜默假成功（不搬貨）。
        /// </summary>
        public IActionResult<bool> CompleteReceivingByTransload(Guid manifestUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = false;
            try
            {
                if (manifestUID == Guid.Empty)
                {
                    rs.Message = "ManifestUID is required.";
                    return rs;
                }

                // 1. 取該 manifest 的 BOL（Status>0）
                var bols = this.BolRepository.GetList(new { ManifestUID = manifestUID });
                var bolUIDs = (bols.Content ?? Enumerable.Empty<IBolModel>())
                                  .Where(b => b.Status > 0).Select(b => b.UID).ToList();
                if (bolUIDs.Count == 0)
                {
                    rs.Message = "No BOL found for manifest.";
                    return rs;
                }

                // 2. 取每 BOL 的 Ticket（含 Type，BOL→Vessel→WorkOrder→Ticket），依 Type 分成「收貨」與「移動」兩組
                var receivingIDs = new List<string>();
                var moveIDs = new List<string>();
                foreach (var bolUID in bolUIDs)
                {
                    var ticketRs = this.TicketRepository.GetTicketByBol(bolUID);
                    if (!ticketRs.Success || ticketRs.Content == null) continue;
                    foreach (var t in ticketRs.Content.Where(x => x.Status > 0 && !string.IsNullOrEmpty(x.ID)))
                    {
                        if (t.Type == (int)TicketType.Receiving) receivingIDs.Add(t.ID);
                        else if (t.Type == (int)TicketType.Move) moveIDs.Add(t.ID);
                    }
                }
                receivingIDs = receivingIDs.Distinct().ToList();
                moveIDs = moveIDs.Distinct().ToList();
                if (receivingIDs.Count == 0 && moveIDs.Count == 0)
                {
                    rs.Message = "No ticket to complete (already completed?).";
                    return rs;
                }

                // 3. 第一段：完成「收貨」票（IsAllPass、ActQty=EstQty）→ 建 PayLoad(Stock)+Inventory 落在 InboundTemp 暫存區
                if (receivingIDs.Count > 0)
                {
                    var recvRs = this.CompleteTicketData(receivingIDs.ToArray());
                    if (!recvRs.Success)
                    {
                        rs.Success = false;
                        rs.Message = "Receiving complete failed: " + recvRs.Message;
                        return rs;
                    }
                }

                // 4. 重新初始化連線，確保收貨剛建的 PayLoad 已 commit、對接下來的移動票可見
                this.DbEntities.ReInitConnectionInstance();

                // 5. 第二段：完成「移動」票 → 把 PayLoad 從暫存區搬到實際儲位（可用庫存）
                if (moveIDs.Count > 0)
                {
                    var moveRs = this.CompleteTicketData(moveIDs.ToArray());
                    if (!moveRs.Success)
                    {
                        rs.Success = false;
                        rs.Message = "Move complete failed: " + moveRs.Message;
                        return rs;
                    }
                }

                rs.Success = true;
                rs.Content = true;
                return rs;
            }
            catch (Exception ex)
            {
                this.TracingAgent.Trace(ex.Message, ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                return rs;
            }
        }

        private void checkitempgkavailable(IList<IReceivingContainer> pgrp, List<string> notFinditem,
            List<string> duplicateitem, List<string> nothavepkg, List<string> notmatchpkg, List<string> notmatchmaxpkg,
             List<string> notmatchminpkg, List<ItemInfo> items, List<IReceivingContainer> virutalItemContaier, List<Guid> customerUIDs,
            IActionResult<IEnumerable<Identities.Interfaces.Models.IGroupUserViewModel>> groups)
        {
            foreach (var container in pgrp)
            {
                foreach (var item in container.Items)
                {
                    ItemInfo i = new ItemInfo();
                    var itemOriginals = this.ProductCacheManager.GetItem(item.Name, customerUIDs, groups.Content);
                    if (itemOriginals != null && itemOriginals.Count() > 1)//是否有重覆的Item#
                    {
                        if (!duplicateitem.Any(x => x == item.Name))
                            duplicateitem.Add(item.Name);
                    }
                    if (itemOriginals != null && itemOriginals.Count() == 1)
                    {
                        //Get Package
                        var itemOrg = itemOriginals.FirstOrDefault();
                        var pkg = FindEqualUOMPackage(container.PackageUOM,
                            itemOrg.UID, itemOrg.ID, item.InventoryRatio,
                            item.PackageUOM,
                            item.UseMinUOM, notmatchmaxpkg, notmatchminpkg);
                        if (pkg != null && pkg.Count() > 0)
                        {
                            //取得最新版本全部包裝並取得該版本指定包裝
                            i.Item = itemOrg;
                            i.Package = pkg.FirstOrDefault();
                            //取得虛擬Item
                            var virtualItems = this.ProductCacheManager.GetVirtualItems(itemOrg.Name, customerUIDs);
                            if (virtualItems.Content?.Count() > 0)
                            {
                                var _itemgrpUID = Guid.NewGuid();
                                item.VirtualItems = virtualItems.Content.Select(p => p as IItemModel).ToList();
                                i.VirtualItems = virtualItems.Content;
                                item.ItemGroupUID = _itemgrpUID;
                                var receivingContainer = new ReceivingContainer();
                                receivingContainer.ExternalData = container.ExternalData;
                                //receivingContainer.ManifestItemUID.Add(container.ManifestItemUID);
                                receivingContainer.PackageUOM = container.PackageUOM;
                                receivingContainer.VesselManifestUID = container.VesselManifestUID;
                                foreach (var vitem in virtualItems.Content)
                                {
                                    var viitem = new ReceivingContainerItem();
                                    viitem.InventoryRatio = item.InventoryRatio;
                                    viitem.ExternalData = item.ExternalData;
                                    viitem.Barcode = item.Barcode;
                                    viitem.ContainerUID = item.ContainerUID;
                                    viitem.Name = vitem.Name;
                                    viitem.PackageQty = item.PackageQty;
                                    viitem.PackageUOM = item.PackageUOM;
                                    viitem.UseMinUOM = item.UseMinUOM;
                                    viitem.UID = item.UID;
                                    viitem.ItemGroupUID = _itemgrpUID;
                                    receivingContainer.Items.Add(viitem);

                                    ItemInfo vii = new ItemInfo();
                                    vii.Item = vitem;
                                    var pkgcollection = this.PackageCacheManager.GetPackagesByItem(vitem.UID);
                                    var latestPkg = pkgcollection.GroupBy(g => g.VersionUID)
                                    .OrderByDescending(o => o.OrderByDescending(o1 => o1.CreatedOn).First().CreatedOn)
                                    .Select(p => p).FirstOrDefault();
                                    vii.Package = this.PackageCacheManager.GetMinPackage(latestPkg);
                                    items.Add(vii);
                                    item.ItemUID.Add(vitem.UID);
                                }
                                virutalItemContaier.Add(receivingContainer);

                            }
                            else
                            {
                                i.VirtualItems = new List<ProductExtendModel>();
                                item.ItemUID.Add(itemOrg.UID);
                            }
                            items.Add(i);
                        }
                        else
                        {
                            var pkgs = this.PackageCacheManager.GetPackagesByItem(itemOrg.UID).GroupBy(grp => grp.VersionUID);
                            if (!nothavepkg.Any(x => x == item.Name) && pkgs.Count() == 0)
                            {
                                nothavepkg.Add(item.Name);
                            }
                            else
                            {
                                //找不到包裝的產品
                                //移至FindEqualUOMPackage 設定
                                //以判斷下是以防萬一
                                if (!notmatchpkg.Any(p => p == item.Name) && notmatchminpkg.Count == 0
                                    && notmatchmaxpkg.Count == 0)
                                    notmatchpkg.Add(item.Name);
                            }

                        }

                    }
                    if (itemOriginals == null || (itemOriginals != null && itemOriginals.Count() == 0)) //item 是否存在
                    {
                        if (!notFinditem.Any(x => x == item.Name))
                            notFinditem.Add(item.Name);
                    }
                }

            }
        }

        public IActionResult<ICancelReceivingResponse> CancelReceiving(ICancelReceivingRequest request)
        {

            var rs = ActionResultTemplates.Result<ICancelReceivingResponse>();
            try
            {
                if (!this.RequestManager.IsRequestProcessing(RequestAction.CANCEL_RECEIVING, request.RefNo))
                {
                    this.RequestManager.AddRequest(RequestAction.CANCEL_RECEIVING, request.RefNo);
                    var groups = this.GetGroupUserViewByUser();
                    var customerUIDs = new List<Guid>();
                    if (!string.IsNullOrEmpty(request.CustomerPartyName))
                        customerUIDs.AddRange(this.GetCustomer(groups.Content.Select(x => x.GroupUID),
                            request.CustomerPartyName).Select(p => p.UID));
                    else
                        customerUIDs.Add(request.CustomerUID);
                    CancelReceivingResponse response = new CancelReceivingResponse();
                    DeleteManifestRequest deleteManifestRequest = new DeleteManifestRequest();
                    deleteManifestRequest.RefNo = request.RefNo;
                    deleteManifestRequest.CustomerUID = customerUIDs.FirstOrDefault();
                    deleteManifestRequest.WarehouseUID = request.WarehouseUID;
                    var deleteResult = this.DeleteManifestByOrder(deleteManifestRequest);
                    response.IsComplete = deleteResult.Success;
                    response.Message = deleteResult.Message;
                    rs.Content = response;
                    rs.Success = true;
                    this.RequestManager.RemoveRequest(RequestAction.CANCEL_RECEIVING, request.RefNo);
                }
                else
                {
                    rs.Content.IsComplete = false;
                    rs.Content.Message = string.Format(Resource.COMMON_REQUEST_ISPROCESSING, $"Booking#{request.RefNo}");
                    return rs;
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
        public IActionResult<bool> ImportInboundData(IImportInboundParameter parameter)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var param = new InboundInitImportParameters();
                param.CustomerUID = parameter.CustomerUID;
                param.WarehouseUID = parameter.WarehouseUID;
                param.PackageCacheManager = this.PackageCacheManager;
                param.ProductCacheManager = this.ProductCacheManager;
                param.GroupUserViews = this.GetGroupUserViewByUser().Content;
                var defaultImportModule = new DefaultInboundImportModule(param);
                rs = defaultImportModule.Execute(parameter.File);
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

        private IEnumerable<IPackageModel> FindEqualUOMPackage(string ContainerUOM, Guid itemUID, string itemNO, int inventoryRatio,
            string packageUOM, bool useMinUOM, List<string> notmatchmaxpkg, List<string> notmatchminpkg)
        {
            //TODO 先暫時不判斷包裝
            //var _result = new List<IPackageModel>();
            //var pkgs = this.PackageCacheManager.GetPackagesByItem(itemUID).GroupBy(grp => grp.VersionUID);
            //var maxuomUnique = this.PackageCacheManager.GetUomUniqueFromName(ContainerUOM);
            //var minuomUnique = this.PackageCacheManager.GetUomUniqueFromName(packageUOM);
            //foreach (var pkglist in pkgs)
            //{
            //    //1.比對所有版本包裝集合是否有符合的UOM
            //    var uomamx = this.PackageCacheManager.GetMaxUomUnique();
            //    var minPkg = this.PackageCacheManager.GetMinPackage(pkglist);
            //    var cPkg = pkglist.FirstOrDefault(x => x.UOM == maxuomUnique);
            //    var eqmin = (minPkg.UOM == minuomUnique) || useMinUOM;
            //    var eqmax = cPkg != null;
            //    //2.從1的包裝結果中找出有相符的換算比率
            //    if (eqmin && eqmax)
            //    {
            //        var ratio = this.PackageCacheManager.GetReceivePackageUomQuantity(cPkg.UID, minPkg.UID, 1, pkglist);
            //        if (ratio.Success)
            //        {
            //            if (ratio.Content == inventoryRatio)
            //            {
            //                _result.Add(minPkg);
            //            }
            //        }
            //    }
            //}
            //if (_result.Count > 0)
            //{
            // return _result.OrderByDescending(o => o.CreatedOn).FirstOrDefault();
            //}
            //return null;


            var pkgs = this.PackageCacheManager.GetPackagesByItem(itemUID).GroupBy(grp => grp.VersionUID);

            var maxuomUnique = WMSAPIParameters.MAX_PACKAGE_UOM.Select(p =>
                                this.PackageCacheManager.GetUomUniqueFromName(p));
            var minuomUnique = this.PackageCacheManager.GetUomUniqueFromName(packageUOM);
            var _result = new List<IPackageModel>();
            var _allminpkg = false;
            var _allmaxpkg = false;
            foreach (var pkglist in pkgs)
            {
                //1.比對所有版本包裝集合是否有符合的UOM
                var uomamx = this.PackageCacheManager.GetMaxUomUnique();
                var minPkg = this.PackageCacheManager.GetMinPackage(pkglist);
                var cPkg = pkglist.FirstOrDefault(x => maxuomUnique.Any(p => p.Value == x.UOM));
                var eqmin = (minPkg.UOM == minuomUnique) || useMinUOM;
                var eqmax = cPkg != null;
                if (eqmin && eqmax)
                {
                    _result.Add(minPkg);
                }
                _allminpkg = _allminpkg || eqmin;
                _allmaxpkg = _allmaxpkg || eqmax;
            }
            if (_result.Count() == 0)
            {
                //當找不到適合版本的包裝，就以最新版本包裝做為錯誤訊息的依據
                if (this.PackageCacheManager.GetPackagesByItem(itemUID).Count() > 0)
                {
                    var lastestVerUID = this.PackageCacheManager
                        .GetPackagesByItem(itemUID).OrderByDescending(o => o.CreatedOn).FirstOrDefault().VersionUID;
                    var lastestpkg = this.PackageCacheManager.GetPackagesByVersion(lastestVerUID);
                    var uomamx = this.PackageCacheManager.GetMaxUomUnique();
                    var minPkg = this.PackageCacheManager.GetMinPackage(lastestpkg);
                    var cPkg = lastestpkg.FirstOrDefault(x => maxuomUnique.Any(p => p.Value == x.UOM));
                    var eqmin = (minPkg.UOM == minuomUnique) || useMinUOM;
                    var eqmax = cPkg != null;
                    if (!eqmin)
                    {
                        notmatchminpkg.Add(itemNO);
                    }
                    if (!eqmax)
                    {
                        notmatchmaxpkg.Add(itemNO);
                    }
                }
                else
                {
                    notmatchmaxpkg.Add(itemNO);
                }
            }
            return _result.OrderByDescending(o => o.CreatedOn);
        }

        public IActionResult<bool> CheckReplenishmentSync(IEnumerable<IReplenishmentModel> Data)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                if (Data != null && Data.Count() > 0)
                {
                    var rs_client = new ReplenishmentSyncer();

                    var TargetList = Data.Select(x =>
                    {
                        return new SyncReplenishmentModel()
                        {
                            ItemGroupUID = x.ItemGroupUID,
                            SeparateByUID = x.SeparateByUID,
                            PayloadPackageUID = x.PayloadPackageUID,
                            Weight = x.Weight,
                            VesselManifestUID = x.VesselManifestUID,
                            TargetSlotUID = x.TargetSlotUID,
                            ModifiedOn = x.ModifiedOn,
                            ModifiedBy = x.ModifiedBy,
                            CreatedOn = x.CreatedOn,
                            CreatedBy = x.CreatedBy,
                            Volume = x.Volume,
                            Status = x.Status,
                            Qty = x.Qty,
                            LoadingZoneSlotUID = x.LoadingZoneSlotUID,
                            PackageUID = x.PackageUID,
                            SlotUID = x.SlotUID,
                            ItemUID = x.ItemUID,
                            PayloadUID = x.PayloadUID,
                            WorkOrderPodUID = x.WorkOrderPodUID,
                            WorkOrderUID = x.WorkOrderUID,
                            Type = x.Type,
                            Name = x.Name,
                            ID = x.ID,
                            UID = x.UID,
                            WarehouseUID = x.WarehouseUID
                        };
                    });

                    rs = rs_client.Sync(TargetList, Guid.NewGuid().ToString());
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
        public IActionResult<bool> CheckReplenishmentSync(IEnumerable<ITicketProcessModel> Data)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                if (Data != null && Data.Count() > 0)
                {
                    var rs_client = new ReplenishmentSyncer();
                    var WorkOrderPayloadList = WorkOrderPayloadRepository.GetList(new
                    {
                        WorkOrderPodUID = Data.Where(x => x.WorkOrderPodUID != null).Select(x => x.WorkOrderPodUID.Value)
                    });
                    if (WorkOrderPayloadList != null && WorkOrderPayloadList.Content != null)
                    {
                        var TargetList = WorkOrderPayloadList.Content.Select(x =>
                        {
                            return new SyncReplenishmentModel()
                            {
                                ItemGroupUID = x.ItemGroupUID,
                                SeparateByUID = x.SeparateByUID,
                                PayloadPackageUID = x.PayloadPackageUID,
                                Weight = x.Weight,
                                VesselManifestUID = x.VesselManifestUID,
                                TargetSlotUID = x.TargetSlotUID,
                                ModifiedOn = x.ModifiedOn,
                                ModifiedBy = x.ModifiedBy,
                                CreatedOn = x.CreatedOn,
                                CreatedBy = x.CreatedBy,
                                Volume = x.Volume,
                                Status = x.Status,
                                Qty = x.Qty,
                                LoadingZoneSlotUID = x.LoadingZoneSlotUID,
                                PackageUID = x.PackageUID,
                                SlotUID = x.SlotUID,
                                ItemUID = x.ItemUID,
                                PayloadUID = x.PayloadUID,
                                WorkOrderPodUID = x.WorkOrderPodUID,
                                WorkOrderUID = x.WorkOrderUID,
                                Type = x.Type,
                                Name = x.Name,
                                ID = x.ID,
                                UID = x.UID,
                                WarehouseUID = Data.Where(c => c.WorkOrderPodUID.Equals(x.WorkOrderPodUID)).Select(c => c.WarehouseUID).FirstOrDefault()
                            };
                        });

                        rs = rs_client.Sync(TargetList, Guid.NewGuid().ToString());
                    }
                    else
                    {
                        rs.Success = false;
                    }
                }
                else
                {
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

        public IActionResult<bool> ProcessPBSCItemAndPackage(IEnumerable<IPBSCItemPackagingModel> Data)
        {
            var rs = ActionResultTemplates.Result<bool>();
            using (var db = this.DbEntities.DbAdapter)
            {
                try
                {
                    if (Data != null && Data.Count() > 0)
                    {

                        //  this.TracingAgent.Trace("新增/更新 產品包裝版本", Data);
                        this.DbEntities.BeginTranaction(System.Data.IsolationLevel.ReadCommitted);
                        //using (var scope = this.GetNewTransactionScope(System.Transactions.IsolationLevel.ReadCommitted, 60 * 60))
                        //{
                        foreach (var item_data in Data)
                        {
                            bool has_multibox = (item_data.MultiBoxItem != null && item_data.MultiBoxItem.Count() > 0);
                            //一般Item Or MultiBoxItem 主體
                            var gitem_task = SyncPackage(item_data, has_multibox);
                            if (gitem_task.Success)
                            {
                                if (has_multibox)
                                {
                                    //MultiBoxItem Detail
                                    List<IPBSCVirtualItem> vitem_list = item_data.MultiBoxItem.ToList();
                                    foreach (IPBSCVirtualItem vitem in vitem_list)
                                    {
                                        //Virtual Item 轉 ItemPackaging Model
                                        SyncPackage(ConvertVirtualItemToItemPackaging(vitem));
                                    }
                                }
                            }
                            else if (!String.IsNullOrEmpty(gitem_task.Message))
                            {
                                rs.Message += String.Format("{0}\n", gitem_task.Message);
                            }
                        }

                        //scope.Complete();
                        //}
                        //this.DbEntities.Commit();
                        this.CommitTransaction();
                    }
                    rs.Content = true;
                    rs.Message = "Success";
                    rs.Success = true;
                }
                catch (Exception ex)
                {
                    if (this.DbEntities.Transaction != null)
                    {
                        //this.DbEntities.Rollback();
                        this.RollbackTransaction();
                    }
                    this.TracingAgent.Trace("Add Item / Package fail", ex);
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    rs.Message = Resource.COMMON_RETRY; ;
                    rs.InnerException = ex;
                }
            }
            return rs;
        }

        /*
                 1. 檢查 Product 是否存在
                 2. 確認WMS中沒有相同架構的包裝
                 3. 檢查 UOM 是否存在, 不存在則建立
                 4. 建立 Package Version
                 5. 建立 Root Package
                 6. 建立 Children Packages 
        */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item_data"></param>
        /// <returns>有新增Item需RefreshCache則為True，否則為False</returns>
        private IActionResult<bool> SyncPackage(IPBSCItemPackagingModel item_data, bool has_multi_box = false)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var data = this.PreparePackageData(item_data.Packages.ToList());
            int syncedCount = 0;
            int totalCount = data.Count();
            string productId = item_data.Item.ID;

            //產品
            Guid item_group_uid = Guid.Parse("C69136EF-9141-4ED2-AA24-9C23B6A14CF2");
            Guid item_customer_uid = Guid.Parse("632689E6-C643-43EE-AE85-7384E20E587A");
            //確認產品是否存在
            var item = this.checkProductExists(item_group_uid, productId);
            if (item == null)
            {
                //continue;
                //2020-06-20 Kevin 說要新增Item
                //建立一般產品 & 多箱產品(Detail) (假使多箱產品(主體)剛重建)
                item = CreateWMSItem(item_data.Item, item_group_uid, item_customer_uid, item_data.Item.Is_VirtualItem);
                //this.refreshProduct(item_group_uid);
            }
            else
            {
                //(一般Item/多箱產品(主體)) & 確認沒有Ticket使用過的紀錄也沒有OnHand -> 刪除原本Item & Package 然後重建
                if (!item_data.Item.Is_VirtualItem && !CheckMultiBoxItemHasUseOnTicketOrOnhand(item))
                {
                    //Inactive Item 及其相關 MultiBoxItem Detail
                    InactiveWMSItem(item);

                    //只建立 多箱產品(主體)
                    item = CreateWMSItem(item_data.Item, item_group_uid, item_customer_uid);
                    //this.refreshProduct(item_group_uid);
                }
                else if (item_data.Item.Is_VirtualItem)
                {
                    //新增或更新 多箱產品(Detail)
                    this.TracingAgent.Trace(String.Format("新增/更新 產品包裝版本 {0}", item.ID), item);
                    item = CreateWMSItem(item_data.Item, item_group_uid, item_customer_uid, item_data.Item.Is_VirtualItem);
                    //this.refreshProduct(item_group_uid);
                }
            }
            // this.refreshProduct(item_group_uid);
            if (item != null)
            {
                //包裝
                foreach (var model in data)
                {
                    if (checkIfAlreadyHasPackage(item))
                    {
                        this.TracingAgent.Trace(String.Format("已有包裝 {0}", item.ID), item);
                        //(一般Item/多箱產品(主體)/多箱產品(Detail)) 比較架構及Qty
                        List<IPackageViewModel> same_packages = checkIfSamePackageStructure(item, model);
                        if (same_packages != null)
                        {
                            this.TracingAgent.Trace(String.Format("已有同樣架構的包裝 {0}", item.ID), item);
                            //同步SCC14 & PUOM
                            if (item_data.Item.Is_VirtualItem && item_data.Packages != null && item_data.Packages.Count().Equals(1))
                            {
                                this.TracingAgent.Trace(String.Format("做多箱產品(Detail)時更新主產品包裝 {0}", item.ID), item);
                                //架構一樣的多箱產品(主體)更新包裝須由多箱產品(Detail)PUOM找到相關多箱產品(主體)包裝
                                //需在更新Detail前更新主體的該包裝資訊
                                var multibox_detail_extension = getItemByUID(item.UID);
                                if (multibox_detail_extension != null)
                                {
                                    this.TracingAgent.Trace(String.Format("找到多箱產品(Detail) {0} & ActualProduct={1}, PUOM={2}", item.ID, multibox_detail_extension.ActualProduct, multibox_detail_extension.PUOM), multibox_detail_extension);
                                    //因為多箱產品(Detail)已經先更新了所以須從包裝取得'原'PUOM
                                    var multibox_detail_packages = getNewestVersionOfPackageStructure(multibox_detail_extension);
                                    if (multibox_detail_packages != null)
                                    {
                                        string target_puom = multibox_detail_packages.FirstOrDefault().PUOM;

                                        if (!multibox_detail_extension.PUOM.Equals(target_puom))
                                        {
                                            this.TracingAgent.Trace(String.Format("找到多箱產品(Detail) {0} 原 PUOM={1}", item.ID, target_puom), item);
                                            var main_item = checkProductExists(item_group_uid, multibox_detail_extension.ActualProduct);
                                            if (main_item != null)
                                            {
                                                this.TracingAgent.Trace(String.Format("找到多箱產品(主體) {0}", main_item.ID), item);
                                                //用多箱產品(Detail)尚未更新的PUOM找到多箱產品(主體)的相關包裝
                                                List<IPackageViewModel> puom_packages = getPackageByPUOM(main_item, target_puom);
                                                if (puom_packages != null && puom_packages.Count > 0)
                                                {
                                                    this.TracingAgent.Trace(String.Format("找到多箱產品(主體) {0} 所有包裝PUOM為 {1}", main_item.ID, target_puom), item);
                                                    foreach (var puom_package_item in puom_packages)
                                                    {
                                                        if (!puom_package_item.PUOM.Equals(multibox_detail_extension.PUOM))
                                                        {
                                                            this.TracingAgent.Trace(String.Format("更新多箱產品(主體) {0} 包裝({1}) PUOM為 {2}", main_item.ID, puom_package_item.UID, multibox_detail_extension.PUOM), item);
                                                            puom_package_item.PUOM = multibox_detail_extension.PUOM;
                                                            this.PackageManager.UpdatePackage(puom_package_item);
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.TracingAgent.Trace(String.Format("PUOM無須調整 原({0}) -> 改({1})", target_puom, multibox_detail_extension.PUOM), item);
                                        }
                                    }
                                }
                            }

                            //一般產品/多箱產品(Detail)
                            if (!has_multi_box && SyncBarcodeOfPackages(same_packages, model.Package))
                            {
                                //Console.WriteLine("更新產品Barcode {0}", item.ID);
                                this.TracingAgent.Trace(String.Format("Same Package Structure But Barcode. ({0})", item.ID), model.Package);
                                //this.OnError(ErrorType.SamePackageStructureButBarcode, productId, model.Package);
                                continue;
                            }
                            else
                            {
                                this.TracingAgent.Trace(String.Format("Same Package Structure Exists. ({0})", item.ID), model.Package);
                                //this.OnError(ErrorType.SamePackageStructureExists, productId, model.Package);
                                continue;
                            }
                        }
                        else if (!item_data.Item.Is_VirtualItem && has_multi_box)
                        {
                            this.TracingAgent.Trace(String.Format("main item different package structure {0}", item.ID), item);
                            //多箱產品(主體)架構與現行不同 -> 不允許(包裝階層)修改並通知 User
                            //SendMail("Varys Item Package Syncer BOM Item Warning",
                            //    String.Format("There is already exist a package structure for item ({0}), please update packages manually.", productId),
                            //    System.Configuration.ConfigurationManager.AppSettings["CAP.WMS.Error.MailTo"],
                            //    System.Configuration.ConfigurationManager.AppSettings["CAP.WMS.Error.MailCc"]
                            //    );
                            //this.OnError(ErrorType.PackageStructureAlreadyExistNotAllowUpdateMultiBoxItem, productId, model.Package);

                            //停止繼續處裡多箱產品(Detail)
                            rs.Success = false;
                            rs.Message = String.Format("main item different package structure {0}", item.ID);
                            return rs;
                        }
                    }

                    this.TracingAgent.Trace(String.Format("新增產品包裝版本 {0}", item.ID), item);

                    string uom = model.Package.UOM;
                    bool uomExists = this.checkUomExists(uom);
                    if (!uomExists)
                    {
                        this.PackageUomManager.CreateUom(uom, uom);
                        this.refreshUom();
                    }
                    var uomModel = this._PackageUOMList?.FirstOrDefault(o => o.ID.Equals(uom, StringComparison.OrdinalIgnoreCase));
                    if (uomModel == null)
                    {
                        continue;
                    }

                    var addVersionResult = this.PackageVersionManager.AddPackageVersion(item.UID, item.ID);
                    if (!addVersionResult.Success)
                    {
                        continue;
                    }

                    var versionUID = addVersionResult.Content;

                    var rootPackage = new PackageModel()
                    {
                        UID = Guid.NewGuid(),
                        VersionUID = versionUID,
                        ItemUID = item.UID,
                        UOM = uomModel.UID,
                        ID = model.Package.Name,
                        Name = model.Package.Name,
                        Length = model.Package.Length,
                        Width = model.Package.Width,
                        Height = model.Package.Height,
                        GrossWeight = (model.Package.GrossWeight ?? 0m),
                        Quantity = model.Package.Quantity,
                        SCC14 = model.Package.SCC14,
                        PUOM = model.Package.PUOM,
                        Status = (int)PackageStatus.Active,
                        Type = 1,
                    };

                    if (model.Package.Children.Count() == 0)
                    {
                        rootPackage.CreatedBy = "Lst-Pkg";
                    }

                    var addRootPackageResult = this.PackageManager.AddPackage(rootPackage);

                    if (!addRootPackageResult.Success)
                    {
                        continue;
                    }

                    var parentPackageUID = rootPackage.UID;
                    this.createChildren(model.Package.Children, item.UID, versionUID, parentPackageUID, productId);

                    syncedCount++;
                }

                //更新快取
                RefreshProductCache(item.UID);
                // this.refreshProduct(item_group_uid);
            }

            rs.Success = true;
            return rs;
        }

        #region Product

        private IItemModel CreateWMSItem(IPBSCItemModel pbsc_item, Guid item_group_uid, Guid item_customer_uid, bool is_virtual_item = false)
        {
            IItemModel item = null;

            pbsc_item.UID = Guid.NewGuid();
            pbsc_item.GroupUID = item_group_uid;
            pbsc_item.CustomerUID = item_customer_uid;
            item = CreateItem(pbsc_item, is_virtual_item);

            return item;
        }
        private void InactiveWMSItem(IItemModel item)
        {
            if (item != null)
            {
                this.ItemManager.Delete(item.UID);

                //Inactive 該 Item 的所有版本的所有包裝
                var versions = this.PackageVersionManager.GetPackageVersionList(item.UID);
                if (versions != null && versions.Content != null)
                {
                    var version_list = versions.Content.ToList();
                    if (version_list.Count > 0)
                    {
                        //每個版本
                        foreach (var version_package_item in version_list)
                        {
                            //取得該版本的包裝
                            var packages = this.PackageManager.GetPackagesByVersion(version_package_item.UID);
                            if (packages != null && packages.Content != null)
                            {
                                List<IPackageViewModel> package_list = packages.Content.ToList();
                                foreach (IPackageViewModel p_item in package_list)
                                {
                                    p_item.Status = 0;
                                    this.PackageManager.UpdatePackage(p_item);
                                }
                            }
                        }
                    }
                }

                //MultiBoxItem Detail
                IEnumerable<IItemModel> vitem_list = getVirtualItems(item);
                if (vitem_list != null)
                {
                    foreach (IItemModel vitem in vitem_list)
                    {
                        InactiveWMSItem(vitem);
                        //更新快取
                        RefreshProductCache(item.UID);
                    }
                }

                //更新快取
                RefreshProductCache(item.UID);
            }
        }
        private IItemModel CreateItem(IPBSCItemModel product, bool is_virtual_item)
        {
            // 處理 Item Category 
            Guid categoryUID = this.handleItemCategory(product.GroupUID, product);

            if (categoryUID == Guid.Empty)
            {
                return null;
            }

            // 搜尋 Item
            var searchItemResult = this.ItemManager.GetItem(product.GroupUID, product.ID);
            var foundItem = searchItemResult.Content;

            if (foundItem == null)
            {
                if (this.addProduct(product, categoryUID, is_virtual_item))
                {
                    return this.ItemManager.GetItem(product.UID)?.Content;
                }
            }
            else
            {
                product.UID = foundItem.UID;

                if (this.updateProduct(product, categoryUID))
                {
                    return this.ItemManager.GetItem(product.UID)?.Content;
                }
            }

            return null;
        }
        private Guid handleItemCategory(Guid groupUID, IPBSCItemModel product)
        {
            string categoryName = product.CategoryName;

            Guid categoryUID = Guid.Empty;

            // 處理 Item Category
            var categoryParameters = new ItemCategoryInnerParameterize();
            categoryParameters.GroupUID = groupUID;
            categoryParameters.ID = categoryName;
            var searchCategoryResult = this.ItemManager.GetCategories(categoryParameters);
            var category = searchCategoryResult.Content?.FirstOrDefault(o => o.ID.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (category == null)
            {
                var newCategory = new ItemCategoryModel()
                {
                    UID = Guid.NewGuid(),
                    GroupUID = groupUID,
                    ID = categoryName,
                    Name = categoryName,
                    Description = categoryName,
                    Status = 1,
                };
                var createCategoryResult = this.ItemManager.CreateCategory(newCategory);
                if (createCategoryResult.Success)
                {
                    categoryUID = (createCategoryResult.Content?.UID ?? Guid.Empty);
                }
                else
                {
                    //  處理建立新 Item Category 失敗的狀況
                    this.TracingAgent.Trace("FailToCreateCategory", beforeObject: product);
                }
            }
            else
            {
                categoryUID = category.UID;
            }

            return categoryUID;
        }
        private bool addProduct(IPBSCItemModel product, Guid categoryUID, bool is_virtual_item)
        {
            if (product.UID == Guid.Empty)
            {
                product.UID = Guid.NewGuid();
            }

            var item = new Model.ItemModel()
            {
                UID = product.UID,
                GroupUID = product.GroupUID,
                ID = product.ID,
                Name = product.Name,
                Description = product.Description,
                Status = (int)YAEP.Core.Item.Constants.ItemStatus.Active,
                Type = (is_virtual_item ? 100 : 1),
            };

            var properties = (!product.Is_VirtualItem ? this.parseToProductProperties(item.UID, product) : this.parseToVirtualProductProperties(item.UID, product));

            var result = this.ItemManager.Create(item, properties, categoryUID);

            if (result.Success)
            {
                return true;
            }
            else
            {
                //this.OnError(ErrorType.FailToCreateProduct, product.ID, null);
                return false;
            }
        }
        private bool updateProduct(IPBSCItemModel product, Guid categoryUID)
        {
            var item = new Model.ItemModel()
            {
                UID = product.UID == Guid.Empty ? Guid.NewGuid() : product.UID,
                GroupUID = product.GroupUID,
                ID = product.ID,
                Name = product.Name,
                Description = product.Description,
                Status = (int)ItemStatus.Active,
                Type = 1,
            };

            var properties = (!product.Is_VirtualItem ? this.parseToProductProperties(item.UID, product) : this.parseToVirtualProductProperties(item.UID, product));

            var result = this.ItemManager.Update(item, properties);

            if (result.Success)
            {
                // 檢查是否已經有 Category Relation
                var checkResult = this.ItemManager.CheckHasCategoryRelation(item.UID, categoryUID);
                if (checkResult.Content == ResultOfCheckBelongCategory.Yes)
                {
                    return true;
                }
                else
                {
                    // 更新 Category : 1. 刪除原有 Category 關聯, 2. 新增 Category 關聯  
                    var clearCategoryRelationResult = this.ItemManager.ClearCategoryRelationByItem(item.UID);

                    if (clearCategoryRelationResult.Success)
                    {
                        var setRelationResult = this.ItemManager.SetCategoryRelation(item.UID, categoryUID);
                        if (setRelationResult.Success)
                        {
                            return true;
                        }
                        else
                        {
                            // 設定產品分類失敗
                            this.TracingAgent.Trace("FailToSetCategoryRelation", beforeObject: product);
                        }
                    }
                    else
                    {
                        // 清除產品分類失敗
                        this.TracingAgent.Trace("FailToClearCategoryRelation", beforeObject: product.ID);
                    }
                }
            }
            else
            {
                this.TracingAgent.Trace("FailToUpdateProduct", beforeObject: product.ID);
            }

            return false;
        }
        private IEnumerable<ItemPropertiesModel> parseToProductProperties(Guid itemUID, IPBSCItemModel product)
        {
            var properties = new List<ItemPropertiesModel>();

            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.CustomerUID),
                Value = product.CustomerUID.ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.BOOLEAN,
                Name = nameof(product.IsBOM),
                Value = product.IsBOM ? "1" : "0",
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.EAN),
                Value = product.EAN,
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.UPC),
                Value = product.UPC,
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.NetWeightLB),
                Value = (product.NetWeightLB ?? 0m).ToString(),
            });


            return properties;
        }
        private IEnumerable<ItemPropertiesModel> parseToVirtualProductProperties(Guid itemUID, IPBSCItemModel product)
        {
            var properties = new List<ItemPropertiesModel>();

            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.CustomerUID),
                Value = product.CustomerUID.ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.EAN),
                Value = product.EAN,
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.UPC),
                Value = product.UPC,
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.LengthInch),
                Value = (product.LengthInch ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.WidthInch),
                Value = (product.WidthInch ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.HeightInch),
                Value = (product.HeightInch ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.GrossWeightLB),
                Value = (product.GrossWeightLB ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.NetWeightLB),
                Value = (product.NetWeightLB ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.ActualProduct),
                Value = product.ActualProduct,
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.PUOM),
                Value = product.PUOM,
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.INT32,
                Name = nameof(product.BoxQuantity),
                Value = product.BoxQuantity.ToString(),
            });


            return properties;
        }

        private IEnumerable<IItemModel> _ProductList = null;

        private IItemModel checkProductExists(Guid group_uid, string productId)
        {
            //if (this._ProductList == null)
            //{
            //    this.refreshProduct(group_uid);
            //}

            var foundProduct = this._ProductList?.FirstOrDefault(o => o.ID.Equals(productId, StringComparison.OrdinalIgnoreCase));

            return foundProduct;
        }

        private WMSItemExtendModel getItemByUID(Guid itemUID)
        {
            var parameters = new ItemInnerParameterize();
            parameters.ListOfGroupUID = new List<Guid>(new Guid[] { Guid.Parse("C69136EF-9141-4ED2-AA24-9C23B6A14CF2") });
            parameters.Status = 1;
            parameters.UID = itemUID;

            IEnumerable<WMSItemExtendModel> target_items = this.ItemManager.GetItems<WMSItemExtendModel>(parameters)?.Content;

            if (target_items != null)
            {
                return target_items.FirstOrDefault() as WMSItemExtendModel;
            }

            return null;
        }

        private IEnumerable<IItemModel> getVirtualItems(IItemModel main_item)
        {
            var parameters = new ItemInnerParameterize();
            parameters.ListOfGroupUID = new List<Guid>(new Guid[] { Guid.Parse("C69136EF-9141-4ED2-AA24-9C23B6A14CF2") });
            parameters.Status = 1;
            ItemPropertySearchModel property_item = new ItemPropertySearchModel()
            {
                Name = "ActualProduct",
                Value = main_item.ID
            };

            parameters.ItemProperties = new List<IItemPropertySearchModel>() { property_item };

            return this.ItemManager.GetItems(parameters)?.Content;
        }

        #endregion

        #region Package

        private bool checkIfAlreadyHasPackage(IItemModel item)
        {
            bool result = false;

            if (getNewestVersionOfPackageStructure(item) != null)
            {
                return true;
            }

            return result;
        }

        /// <summary>
        /// 檢查WMS是否有相同包裝結構的版本
        /// </summary>
        /// <param name="packageManager"></param>
        /// <param name="versionManager"></param>
        /// <param name="item"></param>
        /// <param name="model"></param>
        /// <returns>有則是包裝列表 無則是null</returns>
        private List<IPackageViewModel> checkIfSamePackageStructure(IItemModel item, PackageSyncModel model)
        {
            List<IPackageViewModel> package_list = getNewestVersionOfPackageStructure(item);

            if (package_list != null && package_list.Count > 0)
            {
                //比較包裝架構
                if (this.checkCompareChildren(package_list, model.Package))
                {
                    return package_list;
                }
            }

            return null;
        }

        private bool checkCompareChildren(List<IPackageViewModel> wms_packages, PackageSyncEntity model)
        {
            Console.WriteLine("UOM -> {0}", model.UOM);
            bool result = false;
            var wms_model_list = wms_packages.Where(o =>
            o.UomName.Equals(model.UOM, StringComparison.OrdinalIgnoreCase) &&
            o.Quantity.Equals(model.Quantity)
            //&& o.ParentUID.Equals(model.PUOM)
            //&& o.ParentUID.Equals(model.SCC14)
            );
            var wms_model = wms_model_list.FirstOrDefault();
            if (wms_model != null)
            {
                var children = wms_packages.Where(o => o.ParentUID.Equals(wms_model.UID));

                if (children != null && model.Children != null)
                {
                    if (children.Count() == model.Children.Count)
                    {
                        foreach (PackageSyncEntity children_data in model.Children)
                        {
                            if (!checkCompareChildren(wms_packages, children_data))
                            {
                                return false;
                            }
                        }

                        //Console.WriteLine("{0} -> All Children Pass", model.UOM);
                        return true;
                    }
                    else
                    {
                        //Console.WriteLine("{0} -> Children Count Is Different", model.UOM);
                        return false;
                    }
                }
                else if (children == null && model.Children == null)
                {
                    //Console.WriteLine("{0} -> No Children", model.UOM);
                    return true;
                }
            }

            return result;
        }

        private List<IPackageViewModel> getNewestVersionOfPackageStructure(IItemModel item)
        {
            List<IPackageViewModel> result = null;
            var versions = this.PackageVersionManager.GetPackageVersionList(item.UID);
            if (versions != null && versions.Content != null)
            {
                var version_list = versions.Content.ToList();
                if (version_list.Count > 0)
                {
                    //每個版本
                    //foreach (var version_package_item in version_list)
                    {
                        //2020-06-20 Kevin 說只比對最新版本的
                        version_list.Sort((x, y) => y.SerialNumber.CompareTo(x.SerialNumber));
                        var version_package_item = version_list.FirstOrDefault();
                        //取得該版本的包裝
                        var packages = this.PackageManager.GetPackagesByVersion(version_package_item.UID);
                        if (packages != null && packages.Content != null)
                        {
                            return packages.Content.ToList();
                        }
                    }

                }
            }

            return result;
        }

        /// <summary>
        /// 同步SCC14 or PUOM
        /// </summary>
        /// <param name="packageManager"></param>
        /// <param name="wms_packages"></param>
        /// <param name="model"></param>
        /// <returns>有更新Barcode則回True，否則False</returns>
        private bool SyncBarcodeOfPackages(List<IPackageViewModel> wms_packages, PackageSyncEntity model)
        {
            bool result = false;

            var wms_model_list = wms_packages.Where(o =>
            o.UomName.Equals(model.UOM, StringComparison.OrdinalIgnoreCase)
            && o.Quantity.Equals(model.Quantity)
            //&& o.ParentUID.Equals(model.PUOM)
            //&& o.ParentUID.Equals(model.SCC14)
            );
            var wms_model = wms_model_list.FirstOrDefault();
            if (wms_model != null)
            {
                //Barcode不同則Update
                bool diff_flag = false;
                if (model.SCC14 != null &&
                    wms_model.SCC14 != null &&
                    !model.SCC14.Equals(wms_model.SCC14))
                {
                    wms_model.SCC14 = model.SCC14;
                    diff_flag = true;
                }
                if (model.PUOM != null &&
                    wms_model.PUOM != null &&
                    !model.PUOM.Equals(wms_model.PUOM))
                {
                    wms_model.PUOM = model.PUOM;
                    diff_flag = true;
                }

                if (diff_flag)
                {
                    this.PackageManager.UpdatePackage(wms_model);
                    result = true;
                }

                var children = wms_packages.Where(o => o.ParentUID.Equals(wms_model.UID));
                if (children != null && model.Children != null && children.Count() == model.Children.Count)
                {
                    foreach (PackageSyncEntity children_data in model.Children)
                    {
                        //只要有一層包裝有更新則為True
                        if (SyncBarcodeOfPackages(wms_packages, children_data) || result)
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private void createChildren(IEnumerable<PackageSyncEntity> children, Guid itemUID, Guid versionUID, Guid parentPackageUID, string productId)
        {
            if ((children?.Count() ?? 0) == 0)
            {
                return;
            }

            foreach (var childPackageEntity in children)
            {
                string uom = childPackageEntity.UOM;
                bool uomExists = this.checkUomExists(uom);
                if (!uomExists)
                {
                    this.PackageUomManager.CreateUom(uom, uom);
                    this.refreshUom();
                }
                var uomModel = this._PackageUOMList?.FirstOrDefault(o => o.ID.Equals(uom, StringComparison.OrdinalIgnoreCase));
                if (uomModel == null)
                {
                    return;
                }

                var childPackage = new PackageViewInnerModel()
                {
                    UID = Guid.NewGuid(),
                    ParentUID = parentPackageUID,
                    VersionUID = versionUID,
                    ItemUID = itemUID,
                    UOM = uomModel.UID,
                    ID = childPackageEntity.Name,
                    Name = childPackageEntity.Name,
                    Length = childPackageEntity.Length,
                    Width = childPackageEntity.Width,
                    Height = childPackageEntity.Height,
                    GrossWeight = (childPackageEntity.GrossWeight ?? 0m),
                    Quantity = childPackageEntity.Quantity,
                    SCC14 = childPackageEntity.SCC14,
                    PUOM = childPackageEntity.PUOM,
                    Status = (int)PackageStatus.Active,
                    Type = 1,
                };

                if (childPackageEntity.Children.Count() == 0)
                {
                    childPackage.CreatedBy = "Lst-Pkg";
                }

                var addChildPackageResult = this.PackageManager.AddPackage(childPackage);

                if (addChildPackageResult.Success)
                {
                    //this.OnSuccess(productId, childPackageEntity);
                }
                else
                {
                    this.TracingAgent.Trace("FailToCreateChildPackage", productId, childPackageEntity);
                    continue;
                }

                this.createChildren(childPackageEntity.Children, itemUID, versionUID, childPackage.UID, productId);
            }
        }

        private IEnumerable<IPackageUomModel> _PackageUOMList = null;
        private void refreshUom()
        {
            this._PackageUOMList = this.PackageUomManager.GetPackageUomList()?.Content;
        }

        /// <summary>
        /// 取得該產品所有版本相同PUOM的包裝
        /// </summary>
        /// <param name="item"></param>
        /// <param name="puom"></param>
        /// <returns></returns>
        private List<IPackageViewModel> getPackageByPUOM(IItemModel item, string puom)
        {
            List<IPackageViewModel> result = null;

            if (!String.IsNullOrEmpty(puom))
            {
                puom = puom.Trim();
                var versions = this.PackageVersionManager.GetPackageVersionList(item.UID);
                if (versions != null && versions.Content != null)
                {
                    var version_list = versions.Content.ToList();
                    if (version_list.Count > 0)
                    {
                        //每個版本
                        foreach (var version_package_item in version_list)
                        {
                            //取得該版本的包裝
                            var packages = this.PackageManager.GetPackagesByVersion(version_package_item.UID);
                            if (packages != null && packages.Content != null)
                            {
                                var package_list = packages.Content.ToList();

                                foreach (IPackageViewModel package_item in package_list)
                                {
                                    //Console.WriteLine("{0} Package {1}, PUOM {2}", item.ID, version_package_item.VersionId, package_item.ID, package_item.PUOM);
                                }

                                var specific_puom_package = package_list.Where(o => !String.IsNullOrEmpty(o.PUOM) && o.PUOM.Trim().Equals(puom));
                                if (specific_puom_package != null)
                                {
                                    result = new List<IPackageViewModel>();

                                    foreach (IPackageViewModel package_item in specific_puom_package)
                                    {
                                        //Console.WriteLine("Check {0} Package {1}, PUOM {2}", version_package_item.VersionId, package_item.ID, package_item.PUOM);
                                    }

                                    result.AddRange(specific_puom_package);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool checkUomExists(string uom)
        {
            if (this._PackageUOMList == null)
            {
                this.refreshUom();
            }

            bool uomExists = this._PackageUOMList?.Any(o => o.ID.Equals(uom, StringComparison.OrdinalIgnoreCase)) ?? false;

            return uomExists;
        }

        #endregion

        #region Check & Prepare Model
        /// <summary>
        /// 檢查產品是否有Ticket的使用紀錄或庫存
        /// (若是多箱產品，主體為全不會被使用，所以檢查時須一併與Detail同時檢查)
        /// </summary>
        /// <param name="item"></param>
        /// <returns>若其中一項有使用紀錄則回True 否則False</returns>
        private bool CheckMultiBoxItemHasUseOnTicketOrOnhand(IItemModel item)
        {
            if (item != null)
            {
                List<IItemModel> check_item_list = new List<IItemModel>();

                //主體
                check_item_list.Add(item);

                //MultiBoxItem Detail
                IEnumerable<IItemModel> vitem_list = getVirtualItems(item);
                if (vitem_list != null)
                {
                    check_item_list.AddRange(vitem_list);
                }

                foreach (IItemModel check_item in check_item_list)
                {
                    if (!CheckHasUseOnTicket(check_item.UID).Equals(ItemStorageStatus.Noneonahnd_Unused))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private ItemStorageStatus CheckHasUseOnTicket(Guid itemUID)
        {
            var response = this.InventoryManager.GetItemUsageStatus(itemUID);
            if (response.Success)
            {
                return (ItemStorageStatus)response.Content;
            }

            return ItemStorageStatus.Noneonahnd_Unused;
        }

        private PBSCItemPackagingModel ConvertVirtualItemToItemPackaging(IPBSCVirtualItem vitem)
        {
            PBSCItemModel target_item = new PBSCItemModel();
            target_item.UID = vitem.UID;
            target_item.ID = vitem.ID;
            target_item.Name = vitem.Name;
            target_item.CategoryName = vitem.CategoryName;
            target_item.UPC = vitem.UPC;
            target_item.EAN = vitem.EAN;
            target_item.IsBOM = false;
            target_item.Description = vitem.Description;
            target_item.LengthInch = vitem.LengthInch;
            target_item.WidthInch = vitem.WidthInch;
            target_item.HeightInch = vitem.HeightInch;
            target_item.LengthCM = vitem.LengthMetric;
            target_item.WidthCM = vitem.WidthMetric;
            target_item.HeightCM = vitem.HeightMetric;
            target_item.NetWeightLB = vitem.NetWeight;
            target_item.GrossWeightLB = vitem.GrossWeight;
            target_item.ActualProduct = vitem.ActualProduct;
            target_item.Is_VirtualItem = true;
            target_item.PUOM = vitem.PUOM;
            target_item.BoxQuantity = vitem.BoxQuantity;

            List<PBSCPackagingModel> target_packages = new List<PBSCPackagingModel>();
            target_packages.Add(new PBSCPackagingModel()
            {
                PROD_ID = vitem.ID,
                PARENT_MEASURE = "",
                //多箱產品特有包裝
                MEASURE = "EACH",
                QTY = vitem.BoxQuantity,
                HEIGHT = vitem.HeightInch,
                WIDTH = vitem.WidthInch,
                DEPTH = vitem.LengthInch,
                WEIGHT = vitem.GrossWeight,
                MetricHeight = vitem.HeightMetric,
                MetricWidth = vitem.WidthMetric,
                MetricDepth = vitem.LengthMetric,
                MetricWeight = vitem.GrossWeight,
                ProductLengthInch = vitem.LengthInch,
                ProductWidthInch = vitem.WidthInch,
                ProductHeightInch = vitem.HeightInch,
                ProductGrossWeight = vitem.GrossWeight,
                UPC = vitem.UPC,
                PUOM = vitem.PUOM
            });

            return new PBSCItemPackagingModel(target_item, null, target_packages);
        }



        private List<PackageSyncModel> PreparePackageData(List<IPBSCPackagingModel> pbsc_packaging_list)
        {
            var list = new List<PackageSyncModel>();
            if (pbsc_packaging_list != null && pbsc_packaging_list.Count > 0)
            {
                var group = pbsc_packaging_list.GroupBy(o => o.PROD_ID);
                foreach (var g in group)
                {
                    string productId = g.Key;

                    var model = new PackageSyncModel()
                    {
                        ProductId = productId,
                    };

                    var rootRows = g.Where(o => o.PARENT_MEASURE == null || String.IsNullOrWhiteSpace(o.PARENT_MEASURE));

                    if ((rootRows?.Count() ?? 0) == 0)
                    {
                        continue;
                    }
                    else if ((rootRows?.Count() ?? 0) > 1)
                    {
                        continue;
                    }

                    var root = rootRows.FirstOrDefault();
                    string uom = root.MEASURE;

                    this.setPackageProperties(root, model.Package);

                    this.setChildren(model.Package, uom, g);

                    list.Add(model);
                }
            }
            return list;
        }

        private void setChildren(PackageSyncEntity parentPackage, string parentUOM, IEnumerable<IPBSCPackagingModel> source)
        {
            var children = source.Where(o => o.PARENT_MEASURE != null && o.PARENT_MEASURE.Equals(parentUOM, StringComparison.OrdinalIgnoreCase));

            //if (children.GetEnumerator().Current != null)
            {
                foreach (var data in children)
                {
                    string uom = data.MEASURE;

                    var package = new PackageSyncEntity();
                    this.setPackageProperties(data, package);

                    this.setChildren(package, uom, source);

                    parentPackage.Children.Add(package);
                }
            }
        }

        private void setPackageProperties(IPBSCPackagingModel row, PackageSyncEntity package)
        {
            string name = row.MEASURE;
            string uom = row.MEASURE;
            if (uom.StartsWith("BOX", StringComparison.OrdinalIgnoreCase))
            {
                uom = "BOX";
            }

            package.Name = uom;
            package.UOM = uom;
            package.Length = row.DEPTH ?? 0m;
            package.Width = row.WIDTH ?? 0m;
            package.Height = row.HEIGHT ?? 0m;
            package.GrossWeight = row.WEIGHT ?? 0m;
            package.Quantity = Convert.ToInt32(row.QTY ?? 1);
            package.SCC14 = row.SCC14;
            package.PUOM = row.PUOM;

            if (uom.StartsWith("EACH", StringComparison.OrdinalIgnoreCase))
            {
                if (package.Length <= 0)
                {
                    package.Length = row.ProductLengthInch ?? 0m;
                }
                if (package.Width <= 0)
                {
                    package.Width = row.ProductWidthInch ?? 0m;
                }
                if (package.Height <= 0)
                {
                    package.Height = row.ProductHeightInch ?? 0m;
                }
                if (package.GrossWeight <= 0)
                {
                    package.GrossWeight = row.ProductGrossWeight ?? 0m;
                }
            }

        }

        #endregion
    }
}
