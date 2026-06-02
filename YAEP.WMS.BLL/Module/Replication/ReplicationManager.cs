using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.LittleBird.CapPBSC;
using YAEP.LittleBird.CapPBSC.Models;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Extension;
using Newtonsoft.Json;
using YAEP.WMS.Constant.Enums;
using System.Web.Hosting;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Module
{
    internal class ReplicationManager
    {
        AbstractReplication<WmsInventoryModel> _InventoryReplicateClient;
        AbstractReplication<WmsReceivingModel> _ReceiviedReplicateClient;
        AbstractReplication<WmsAllocatedModel> _AllocatedReplicateClient;
        ITicketInfoRepository _TicketInfoRepository;
        IInventoryManager _InventoryManager;
        IReplicationlogRepository _ReplicationlogRepository;
        IAuthenticationInfo _Authentication;
        ProductCacheManager _ProductCacheManager;
        PackageCacheManager _PackageCacheManager;
        ITracingAgent _TracingAgent;
        public ReplicationManager(IReplicationManagerInitParameters initParameters)
        {
            _InventoryReplicateClient = new OnhandReplication(initParameters.TracingAgent);
            _ReceiviedReplicateClient = new ReceiviedReplication(initParameters.TracingAgent);
            _AllocatedReplicateClient = new AllocatedReplication(initParameters.TracingAgent);
            _TicketInfoRepository = initParameters.TicketInfoRepository;
            _InventoryManager = initParameters.InventoryManager;
            _ProductCacheManager = initParameters.ProductCacheManager;
            _PackageCacheManager = initParameters.PackageCacheManager;
            _Authentication = initParameters.AuthenticationInfo;
            _ReplicationlogRepository = initParameters.ReplicationlogRepository;
            _TracingAgent = initParameters.TracingAgent;
        }
        public IActionResult<bool> CancelReceiving(Guid manifestUID,
            IEnumerable<IReceiviedReplicateModel> receivingReplicateModels = null)
        {

            List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                Dictionary<string, Guid> logmapping = new Dictionary<string, Guid>();
                ReplicateDataParameter parameter = new ReplicateDataParameter();
                List<WmsReceivingModel> sendData = new List<WmsReceivingModel>();
                parameter.ManifestUID = new Guid[] { manifestUID };
                IEnumerable<IReceiviedReplicateModel> receivingdCollection = null;
                if (receivingReplicateModels != null)
                {
                    receivingdCollection = receivingReplicateModels;
                }
                else
                {
                    receivingdCollection = this._TicketInfoRepository.GetReceiviedData(parameter).Content;
                }
                var itemInfoList = this._ProductCacheManager.GetItems(receivingdCollection.Select(x => x.ItemUID));
                var receivingDataGrp = receivingdCollection.GroupBy(g => g.Barcode);
                foreach (var pallet in receivingDataGrp)
                {
                    var combineItems = pallet.Select(p =>
                    {
                        var vi = new VirtualItemInfo();
                        var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                        vi.ActualProduct = iteminfo.ActualProduct;
                        vi.ProductId = iteminfo.ID;
                        vi.Quantity = p.Qty;
                        vi.ProductUID = iteminfo.UID;
                        vi.CustomerUID = pallet.FirstOrDefault().PartyUID;
                        vi.PUOM = iteminfo.PUOM;
                        return vi;
                    });
                    var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                    if (combineItem.Success)
                    {
                        WmsReceivingModel rm = new WmsReceivingModel();
                        rm.Barcode = pallet.First().Barcode;
                        rm.ItemNo = combineItem.Content.Name;
                        rm.Quantity = combineItem.Content.CombinedQuantity * -1;
                        rm.WmsLocationID = pallet.First().OriginalSlotName;
                        rm.ReceivedDate = DateTime.Now;
                        rm.WarehouseID = pallet.First().WarehouseID;
                        rm.LocationID = pallet.First().LocationID;
                        rm.Marks = pallet.First().ExternalOrderNo;
                        //TODO 虛擬item 暫不轉換UOM名稱，目前並沒使用
                        rm.UOM = WMSAPIParameters.SET_UOM_KEYNAME;
                        sendData.Add(rm);
                    }
                    else
                    {
                        foreach (var item in pallet)
                        {
                            var pkgInfo = this._PackageCacheManager.GetPackage(item.PackageUID);
                            var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == item.ItemUID);
                            WmsReceivingModel rm = new WmsReceivingModel();
                            rm.Barcode = item.Barcode;
                            rm.ItemNo = iteminfo.Name;
                            rm.Quantity = item.Qty * -1;
                            rm.WmsLocationID = item.OriginalSlotName;
                            rm.ReceivedDate = DateTime.Now;
                            rm.WarehouseID = item.WarehouseID;
                            rm.LocationID = item.LocationID;
                            rm.Marks = item.ExternalOrderNo;
                            rm.UOM = this._PackageCacheManager.GetUOM(pkgInfo.UOM).Name;
                            sendData.Add(rm);
                        }
                    }
                }
                var syncResult = this._ReceiviedReplicateClient.Sync(sendData, replicateuid.ToString("D"));
                this._TracingAgent.Trace($"sync receivied result:{syncResult.Content}", syncResult);
                var requestJson = "";// JsonConvert.SerializeObject(sendData);
                foreach (var item in receivingdCollection)
                {
                    // var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.BelongToUID = item.TicketInfoUID;
                    log.Action = (int)ReplicationAction.CancelReceiving;
                    log.ReplicateUID = replicateuid;
                    log.Operate = (int)ReplicationOperate.Receive;
                    log.IsComplete = syncResult.Success;
                    log.ItemUID = item.ItemUID;
                    log.Quantity = item.Qty;
                    log.OriginalData = requestJson;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;

                    logcollcetion.Add(log);
                }
                var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                rs.Success = syncResult.Success;
                if (!rs.Success)
                {
                    rs.Message = syncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                }
                else
                {
                    this._TracingAgent.Trace("CancelReceiving sync complete");
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("CancelReceiving sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }

            return rs;
        }
        public IActionResult<bool> Receiving(IEnumerable<Guid> ticketInfoUIDs)
        {
            //HostingEnvironment.QueueBackgroundWorkItem(ct =>
            //{
            List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                ReplicateDataParameter parameter = new ReplicateDataParameter();
                List<WmsReceivingModel> sendData = new List<WmsReceivingModel>();
                parameter.TicketInfoUID = ticketInfoUIDs;
                var receivingCollection = this._TicketInfoRepository.GetReceiviedData(parameter);
                var itemInfoList = this._ProductCacheManager.GetItems(receivingCollection.Content.Select(x => x.ItemUID));
                var receivingdDataGrp = receivingCollection.Content.GroupBy(g => new
                { Barcode = g.Barcode, ItemGroupUID = g.ItemGroupUID });
                foreach (var pallet in receivingdDataGrp)
                {
                    var combineItems = pallet.Select(p =>
                    {
                        var vi = new VirtualItemInfo();
                        var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                        vi.ActualProduct = iteminfo.ActualProduct;
                        vi.ProductId = iteminfo.ID;
                        vi.Quantity = p.Qty;
                        vi.ProductUID = iteminfo.UID;
                        vi.CustomerUID = pallet.FirstOrDefault().PartyUID;
                        vi.PUOM = iteminfo.PUOM;
                        return vi;
                    });
                    var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                    if (combineItem.Success)
                    {
                        WmsReceivingModel rm = new WmsReceivingModel();
                        rm.Barcode = pallet.First().Barcode;
                        rm.ItemNo = combineItem.Content.Name;
                        rm.Quantity = combineItem.Content.CombinedQuantity;
                        rm.WmsLocationID = pallet.First().LandingZoneSlotName;
                        rm.ReceivedDate = DateTime.Now;
                        rm.WarehouseID = pallet.First().WarehouseID;
                        rm.LocationID = pallet.First().LocationID;
                        rm.Marks = pallet.First().ExternalOrderNo;
                        //TODO 虛擬item 暫不轉換UOM名稱，目前並沒使用
                        rm.UOM = WMSAPIParameters.SET_UOM_KEYNAME;
                        sendData.Add(rm);
                    }
                    else
                    {
                        foreach (var item in pallet)
                        {
                            var pkgInfo = this._PackageCacheManager.GetPackage(item.PackageUID);
                            var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == item.ItemUID);
                            WmsReceivingModel rm = new WmsReceivingModel();
                            rm.Barcode = item.Barcode;
                            rm.ItemNo = iteminfo.Name;
                            rm.Quantity = item.Qty;
                            rm.WmsLocationID = item.LandingZoneSlotName;
                            rm.ReceivedDate = DateTime.Now;
                            rm.WarehouseID = item.WarehouseID;
                            rm.LocationID = item.LocationID;
                            rm.Marks = item.ExternalOrderNo;
                            rm.UOM = this._PackageCacheManager.GetUOM(pkgInfo.UOM).Name;
                            sendData.Add(rm);
                        }
                    }

                }
                var syncResult = this._ReceiviedReplicateClient.Sync(sendData, replicateuid.ToString("D"));
                this._TracingAgent.Trace($"sync receivied result:{syncResult.Content}", syncResult);
                var requestJson = "";// JsonConvert.SerializeObject(sendData);
                foreach (var item in receivingCollection.Content)
                {
                    //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.BelongToUID = item.TicketInfoUID;
                    log.Action = (int)ReplicationAction.Receiving;
                    log.ReplicateUID = replicateuid;
                    log.Operate = (int)ReplicationOperate.Receive;
                    log.IsComplete = syncResult.Success;
                    log.ItemUID = item.ItemUID;
                    log.Quantity = item.Qty;
                    log.OriginalData = requestJson;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;
                    logcollcetion.Add(log);
                }
                var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                rs.Success = syncResult.Success;
                if (!rs.Success)
                {
                    rs.Message = syncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                }
                else
                {
                    this._TracingAgent.Trace("Receiving sync complete");
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Receiving sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            //});
            return rs;
        }
        public IActionResult<bool> Receivied(Guid CustomerUID, IEnumerable<Guid> ticketInfoUIDs)
        {
            this._TracingAgent.Trace("ready to receivied sync received parameter", ticketInfoUIDs);
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                ReplicateDataParameter parameter = new ReplicateDataParameter();
                List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
                List<WmsReceivingModel> receiviedsendData = new List<WmsReceivingModel>();
                List<WmsInventoryModel> inventorysendData = new List<WmsInventoryModel>();
                parameter.TicketInfoUID = ticketInfoUIDs;
                var receiviedCollection = this._TicketInfoRepository.GetReceiviedData(parameter);
                var itemInfoList = this._ProductCacheManager.GetItems(receiviedCollection.Content.Select(x => x.ItemUID));

                var receivingdDataGrp = receiviedCollection.Content.GroupBy(g => new
                { Barcode = g.Barcode, ItemGroupUID = g.ItemGroupUID });
                foreach (var pallet in receivingdDataGrp)
                {
                    var combineItems = pallet.Select(p =>
                    {
                        var vi = new VirtualItemInfo();
                        var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                        vi.ActualProduct = iteminfo.ActualProduct;
                        vi.ProductId = iteminfo.ID;
                        vi.Quantity = p.ActQty;
                        vi.ProductUID = iteminfo.UID;
                        vi.CustomerUID = pallet.FirstOrDefault().PartyUID;
                        vi.PUOM = iteminfo.PUOM;
                        return vi;
                    });
                    //qty =-1 delete receiving data
                    var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                    if (combineItem.Success)
                    {
                        WmsReceivingModel rm = new WmsReceivingModel();
                        rm.Barcode = pallet.First().Barcode;
                        rm.ItemNo = combineItem.Content.Name;
                        rm.Quantity = combineItem.Content.CombinedQuantity * -1;
                        rm.WmsLocationID = pallet.First().LandingZoneSlotName;
                        rm.ScanDate = DateTime.Now;
                        rm.ScanBy = this._Authentication.Account;
                        rm.WarehouseID = pallet.First().WarehouseID;
                        rm.LocationID = pallet.First().LocationID;
                        rm.Marks = pallet.First().ExternalOrderNo;
                        //TODO 虛擬item 暫不轉換UOM名稱，目前並沒使用
                        rm.UOM = WMSAPIParameters.SET_UOM_KEYNAME;
                        receiviedsendData.Add(rm);
                    }
                    else
                    {
                        foreach (var item in pallet)
                        {
                            var pkgInfo = this._PackageCacheManager.GetPackage(item.PackageUID);
                            var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == item.ItemUID);
                            WmsReceivingModel rm = new WmsReceivingModel();
                            rm.Barcode = item.Barcode;
                            rm.ItemNo = iteminfo.Name;
                            rm.Quantity = item.ActQty * -1;
                            rm.WmsLocationID = item.LandingZoneSlotName;
                            rm.ScanDate = DateTime.Now;
                            rm.ScanBy = this._Authentication.Account;
                            rm.WarehouseID = item.WarehouseID;
                            rm.LocationID = item.LocationID;
                            rm.Marks = item.ExternalOrderNo;
                            rm.UOM = this._PackageCacheManager.GetUOM(pkgInfo.UOM).Name;
                            receiviedsendData.Add(rm);
                        }
                    }
                }
                //var onhandList = receiviedCollection.Content.GroupBy(p => p.ItemUID);
                //入庫Inventory type 一律是onhand
                foreach (var item in receiviedsendData)
                {
                    var info = item;
                    var im = new WmsInventoryModel();
                    im.ItemNo = item.ItemNo;
                    im.LocationID = info.LocationID;
                    im.Quantity = Math.Abs(item.Quantity);
                    im.UpdateDate = DateTime.Now;
                    im.WarehouseID = info.WarehouseID;
                    im.WmsLocationID = item.WmsLocationID;
                    im.InventoryType = (int)InventoryType.Stock;
                    inventorysendData.Add(im);
                }

                var receiviedSyncResult = this._ReceiviedReplicateClient.Sync(receiviedsendData, replicateuid.ToString("D"));
                var inventorySyncResult = this._InventoryReplicateClient.Sync(inventorysendData, replicateuid.ToString("D"));
                this._TracingAgent.Trace($"sync receivied result:{receiviedSyncResult.Content}", receiviedSyncResult);
                this._TracingAgent.Trace($"sync onhand result:{inventorySyncResult.Content}", inventorySyncResult);
                var requestJson = "";// JsonConvert.SerializeObject(receiviedsendData);
                var requestJsonInventory = "";// JsonConvert.SerializeObject(inventorysendData);
                foreach (var item in receiviedCollection.Content)
                {
                    //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.BelongToUID = item.TicketInfoUID;
                    log.Action = (int)ReplicationAction.Receivied;
                    log.ReplicateUID = replicateuid;
                    log.Operate = (int)ReplicationOperate.Receive;
                    log.IsComplete = receiviedSyncResult.Success;
                    log.ItemUID = item.ItemUID;
                    log.Quantity = item.ActQty;
                    log.OriginalData = requestJson;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;
                    logcollcetion.Add(log);
                }
                foreach (var item in receiviedCollection.Content)
                {
                    //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.BelongToUID = item.TicketInfoUID;
                    log.Action = (int)ReplicationAction.Receivied;
                    log.Operate = (int)ReplicationOperate.Inventory;
                    log.IsComplete = inventorySyncResult.Success;
                    log.ItemUID = item.ItemUID;
                    log.Quantity = item.ActQty;
                    log.ReplicateUID = replicateuid;
                    log.OriginalData = requestJsonInventory;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;
                    logcollcetion.Add(log);
                }
                var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                rs.Success = receiviedSyncResult.Success && inventorySyncResult.Success;
                if (!rs.Success)
                {
                    rs.Message = receiviedSyncResult.Message + " " + inventorySyncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                }
                else
                {
                    this._TracingAgent.Trace("Receivied sync complete");
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Receivied sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> Allcoated(IGetReplicateDataParameter parameter)
        {
            this._TracingAgent.Trace("ready to allocated sync received parameter", parameter);
            //HostingEnvironment.QueueBackgroundWorkItem(ct =>
            //{
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {

                List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
                List<WmsAllocatedExtendModel> allocatedsendData = new List<WmsAllocatedExtendModel>();
                var allocatedData = retryProcess<IEnumerable<IAllocatedReplicateModel>>(
                    () => this._TicketInfoRepository.GetAllocatedData(parameter));
                var itemInfoList = this._ProductCacheManager.GetItems(allocatedData.Select(x => x.ItemUID));
                var syncdataGrp = allocatedData.GroupBy(p => p.WorkOrderPodUID);
                foreach (var pod in syncdataGrp)
                {
                    //區分虛擬實體產品
                    var ActualProductGrp = pod.GroupBy(g => new
                    {
                        ActualProduct =
                        itemInfoList.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct,
                        OnhandType = g.OriginalPayloadType
                    });
                    foreach (var itemgrp in ActualProductGrp)
                    {
                        if (string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                        {
                            foreach (var item in itemgrp)
                            {
                                var pkgInfo = this._PackageCacheManager.GetPackage(item.PackageUID);
                                var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == item.ItemUID);
                                var am = new WmsAllocatedExtendModel();
                                am.BelongTo = item.UID;
                                am.ItemNo = iteminfo.Name;
                                am.QuantityToAllocated = item.Quantity;
                                am.QuantityToPick = 0;
                                am.Status = (int)WMSReplicateAllocatedStatus.Allocated;
                                am.LocationID = item.LocationID;
                                am.OrderNo = Convert.ToInt32(item.ExternalOrderNo);
                                am.WmsLocationID = item.OriginalSlotName ?? "";
                                am.WarehouseID = item.WarehouseID;
                                am.LocationID = item.LocationID;
                                am.OrderQuantity = item.Quantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = item.WorkOrderPayloadUID;
                                am.inventory_type = itemgrp.Key.OnhandType.Value;
                                allocatedsendData.Add(am);
                            }
                        }
                        else
                        {
                            var combineItems = itemgrp.Select(p =>
                            {
                                var vi = new VirtualItemInfo();
                                var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                                vi.ActualProduct = iteminfo.ActualProduct;
                                vi.ProductId = iteminfo.ID;
                                vi.Quantity = p.Quantity;
                                vi.CustomerUID = itemgrp.FirstOrDefault().PartyUID;
                                vi.ProductUID = iteminfo.UID;
                                vi.PUOM = iteminfo.PUOM;
                                return vi;
                            });
                            var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                            //this._TracingAgent.Trace("Combine virtual item", combineItem);
                            if (combineItem.Success)
                            {
                                var am = new WmsAllocatedExtendModel();
                                var _belongtoUID = itemgrp.OrderBy(p => p.UID).FirstOrDefault().UID;
                                var _wpayloaduid = itemgrp.OrderBy(p => p.WorkOrderPayloadUID).FirstOrDefault().WorkOrderPayloadUID;
                                am.BelongTo = _belongtoUID;
                                am.ItemNo = combineItem.Content.Name;
                                am.QuantityToAllocated = combineItem.Content.CombinedQuantity;
                                am.QuantityToPick = 0;
                                am.Status = (int)WMSReplicateAllocatedStatus.Allocated;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderNo = Convert.ToInt32(itemgrp.First().ExternalOrderNo);
                                am.WmsLocationID = itemgrp.First().OriginalSlotName ?? "";
                                am.WarehouseID = itemgrp.First().WarehouseID;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderQuantity = combineItem.Content.CombinedQuantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = _wpayloaduid;
                                am.inventory_type = itemgrp.Key.OnhandType.Value;
                                allocatedsendData.Add(am);
                            }
                        }
                    }

                }
                if (allocatedsendData.Count > 0)
                {
                    rs.Success = true;
                    var allocatedSyncResult = this._AllocatedReplicateClient.Sync(allocatedsendData, replicateuid.ToString("D"));
                    this._TracingAgent.Trace($"sync allocated result:{allocatedSyncResult.Content}", allocatedSyncResult);
                    var requestJson = "";// JsonConvert.SerializeObject(allocatedsendData);
                    foreach (var item in allocatedData)
                    {
                        //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                        var log = new ReplicationlogModel();
                        log.UID = Guid.NewGuid();
                        log.BelongToUID = item.UID;//Ticket Info UID
                        log.Action = (int)ReplicationAction.Allocated;
                        log.ReplicateUID = replicateuid;
                        log.Operate = (int)ReplicationOperate.Allocated;
                        log.IsComplete = allocatedSyncResult.Success;
                        log.ItemUID = item.ItemUID;
                        log.Quantity = item.Quantity;
                        log.OriginalData = requestJson;
                        log.CreatedBy = this._Authentication.Account;
                        log.CreatedOn = DateTime.Now;
                        logcollcetion.Add(log);
                    }
                    var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                    this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                    rs.Success = allocatedSyncResult.Success;
                    if (!rs.Success)
                    {
                        rs.Message = allocatedSyncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                    }
                    else
                    {
                        this._TracingAgent.Trace("Allocated sync complete");
                    }
                }
                else
                {
                    rs.Message = "allocatedsendData no data " + Resource.COMMON_SYNC_ERROR;
                }

            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Allocated sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            //});
            return rs;
        }
        public IActionResult<bool> Deallocated(IGetReplicateDataParameter parameter,
            IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = null)
        {
            this._TracingAgent.Trace("ready to deallocated sync received parameter", parameter);
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
                List<WmsAllocatedExtendModel> allocatedsendCollection = new List<WmsAllocatedExtendModel>();
                List<WmsInventoryModel> wmsInventoryCollection = new List<WmsInventoryModel>();

                IEnumerable<IAllocatedReplicateModel> allocatedCollection = null;
                if (allocatedReplicateModels != null)
                {
                    allocatedCollection = allocatedReplicateModels;
                }
                else
                {
                    allocatedCollection = retryProcess<IEnumerable<IAllocatedReplicateModel>>(
                        () => this._TicketInfoRepository.GetAllocatedData(parameter)
                        );
                }
                if (allocatedCollection.Any(p => !p.OriginalPayloadType.HasValue))
                {
                    //2021/11/22 決議當Allocated Payload 為Future Allocated 時則Onhand Payload Type固定為Stock
                    foreach (var item in allocatedCollection.Where(p => p.PayloadType == (int)PayloadType.FutureAllocated))
                    {
                        item.OriginalPayloadType = (int)PayloadType.Stock;
                    }
                    //rs.Success = false;
                    //rs.Message = "Error some data missing payloadtype";
                    //this._TracingAgent.Trace($"have some data not find originalpayloadtype", allocatedCollection);
                    //return rs;
                }
                this._TracingAgent.Trace("Start get item info", allocatedCollection);
                var itemInfoList = this._ProductCacheManager.GetItems(allocatedCollection.Select(x => x.ItemUID));
                this._TracingAgent.Trace("End get item info", DateTime.Now);
                var syncdata = allocatedCollection.GroupBy(p => p.WorkOrderPayloadUID);
                var syncdataGrp = allocatedCollection.GroupBy(p => p.WorkOrderPodUID);
                foreach (var pod in syncdataGrp)
                {
                    //區分虛擬實體產品
                    var ActualProductGrp = pod.GroupBy(g => new
                    {
                        ActualProduct =
                        itemInfoList.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct,
                        OnhandType = g.OriginalPayloadType
                    });
                    foreach (var itemgrp in ActualProductGrp)
                    {
                        if (string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                        {
                            foreach (var item in itemgrp)
                            {
                                var pkgInfo = this._PackageCacheManager.GetPackage(item.PackageUID);
                                var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == item.ItemUID);
                                var am = new WmsAllocatedExtendModel();
                                am.BelongTo = item.UID;
                                am.ItemNo = iteminfo.Name;
                                am.QuantityToAllocated = item.Quantity * -1;
                                am.QuantityToPick = 0;
                                am.Status = (int)WMSReplicateAllocatedStatus.Void;
                                am.LocationID = item.LocationID;
                                am.OrderNo = Convert.ToInt32(item.ExternalOrderNo);
                                am.WmsLocationID = item.CurrentSlotName ?? "";
                                am.WarehouseID = item.WarehouseID;
                                am.LocationID = item.LocationID;
                                am.OrderQuantity = item.Quantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = item.WorkOrderPayloadUID;
                                am.inventory_type = itemgrp.Key.OnhandType.Value;
                                allocatedsendCollection.Add(am);
                            }
                        }
                        else
                        {
                            var combineItems = itemgrp.Select(p =>
                            {
                                var vi = new VirtualItemInfo();
                                var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                                vi.ActualProduct = iteminfo.ActualProduct;
                                vi.ProductId = iteminfo.ID;
                                vi.Quantity = p.Quantity;
                                vi.ProductUID = iteminfo.UID;
                                vi.CustomerUID = itemgrp.FirstOrDefault().PartyUID;
                                vi.PUOM = iteminfo.PUOM;
                                return vi;
                            });
                            var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                            //this._TracingAgent.Trace("Combine virtual item", combineItem);
                            if (combineItem.Success)
                            {
                                var am = new WmsAllocatedExtendModel();
                                var _belongtoUID = itemgrp.OrderBy(p => p.UID).FirstOrDefault().UID;
                                var _wpayloaduid = itemgrp.OrderBy(p => p.WorkOrderPayloadUID).FirstOrDefault().WorkOrderPayloadUID;
                                am.BelongTo = _belongtoUID;
                                am.ItemNo = combineItem.Content.Name;
                                am.QuantityToAllocated = combineItem.Content.CombinedQuantity * -1;
                                am.QuantityToPick = 0;
                                am.Status = (int)WMSReplicateAllocatedStatus.Void;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderNo = Convert.ToInt32(itemgrp.First().ExternalOrderNo);
                                am.WmsLocationID = itemgrp.First().CurrentSlotName ?? "";
                                am.WarehouseID = itemgrp.First().WarehouseID;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderQuantity = combineItem.Content.CombinedQuantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = _wpayloaduid;
                                am.inventory_type = itemgrp.Key.OnhandType.Value;
                                allocatedsendCollection.Add(am);
                            }
                        }
                    }

                }
                var allocatedSyncResult = this._AllocatedReplicateClient.Sync(allocatedsendCollection, replicateuid.ToString("D"));
                this._TracingAgent.Trace($"sync allocated result:{allocatedSyncResult.Content}", allocatedSyncResult);
                var requestJson = "";// JsonConvert.SerializeObject(allocatedsendCollection);
                foreach (var item in allocatedCollection)
                {
                    //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.BelongToUID = item.UID;//Ticket Info UID
                    log.Action = (int)ReplicationAction.Deallocated;
                    log.ReplicateUID = replicateuid;
                    log.Operate = (int)ReplicationOperate.Allocated;
                    log.IsComplete = allocatedSyncResult.Success;
                    log.ItemUID = item.ItemUID;
                    log.Quantity = item.Quantity * -1;
                    log.OriginalData = requestJson;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;
                    logcollcetion.Add(log);
                }
                //如果TicketInfo已經完成出貨，需要將onhand 還回去
                var syncdata2 = allocatedCollection.Where(p => p.TicketInfoStatus >= (int)TicketInfoStatus.Glitch
                   && p.TicketInfoType == (int)TicketInfoType.Outbound)
                   .GroupBy(p => p.WorkOrderPodUID);
                allocatedsendCollection.Clear();
                foreach (var pod in syncdata2)
                {
                    var ActualProductGrp = pod.GroupBy(g => new
                    {
                        ActualProduct =
                       itemInfoList.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct,
                        InventoryType = g.OriginalPayloadType.Value
                    });
                    foreach (var itemgrp in ActualProductGrp)
                    {
                        if (string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                        {
                            foreach (var item in itemgrp)
                            {
                                var syncInfo = item;
                                //var pkgInfo = this._PackageCacheManager.GetPackage(syncInfo.PackageUID);
                                var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == syncInfo.ItemUID);
                                var im = new WmsInventoryModel();
                                im.ItemNo = iteminfo.Name;
                                im.Quantity = syncInfo.PickQuantity;
                                im.LocationID = syncInfo.LocationID;
                                im.WmsLocationID = syncInfo.CurrentSlotName;
                                im.WarehouseID = syncInfo.WarehouseID;
                                im.LocationID = syncInfo.LocationID;
                                im.UpdateDate = DateTime.Now;
                                im.InventoryType = itemgrp.Key.InventoryType;
                                wmsInventoryCollection.Add(im);
                            }
                        }
                        else
                        {
                            var combineItems = itemgrp.Select(p =>
                            {
                                var vi = new VirtualItemInfo();
                                var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                                vi.ActualProduct = iteminfo.ActualProduct;
                                vi.ProductId = iteminfo.ID;
                                vi.Quantity = p.Quantity;
                                vi.ProductUID = iteminfo.UID;
                                vi.CustomerUID = itemgrp.FirstOrDefault().PartyUID;
                                vi.PUOM = iteminfo.PUOM;
                                return vi;
                            });
                            var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                            if (combineItem.Success)
                            {
                                var im = new WmsInventoryModel();
                                im.ItemNo = combineItem.Content.Name;
                                im.Quantity = combineItem.Content.CombinedQuantity;
                                im.LocationID = itemgrp.First().LocationID;
                                im.WmsLocationID = itemgrp.First().CurrentSlotName ?? "";
                                im.WarehouseID = itemgrp.First().WarehouseID;
                                im.LocationID = itemgrp.First().LocationID;
                                im.UpdateDate = DateTime.Now;
                                im.InventoryType = itemgrp.Key.InventoryType;
                                wmsInventoryCollection.Add(im);

                            }
                        }
                    }


                }
                if (wmsInventoryCollection.Count > 0)
                {
                    var onhandSyncResult = this._InventoryReplicateClient.Sync(wmsInventoryCollection, replicateuid.ToString("D"));
                    this._TracingAgent.Trace($"sync onhand result:{onhandSyncResult.Content}", onhandSyncResult);
                    var requestJsonInventory = "";// JsonConvert.SerializeObject(wmsInventoryCollection);
                    foreach (var item in wmsInventoryCollection)
                    {
                        var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                        var log = new ReplicationlogModel();
                        log.UID = Guid.NewGuid();
                        log.Action = (int)ReplicationAction.Deallocated;
                        log.ReplicateUID = replicateuid;
                        log.Operate = (int)ReplicationOperate.Inventory;
                        log.IsComplete = allocatedSyncResult.Success;
                        //log.ItemUID = iteminfo.UID;
                        log.Quantity = item.Quantity;
                        log.OriginalData = requestJsonInventory;
                        log.CreatedBy = this._Authentication.Account;
                        log.CreatedOn = DateTime.Now;
                        logcollcetion.Add(log);
                    }

                }
                var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                rs.Success = allocatedSyncResult.Success;
                if (!rs.Success)
                {
                    rs.Message = allocatedSyncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                }
                else
                {
                    this._TracingAgent.Trace("Deallocated sync complete");
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Deallocated sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> Outbound(IGetReplicateDataParameter parameter)
        {
            this._TracingAgent.Trace("ready to outbound sync received parameter", parameter);
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
                List<WmsAllocatedExtendModel> allocatedsendData = new List<WmsAllocatedExtendModel>();
                List<WmsInventoryModel> inventorysendData = new List<WmsInventoryModel>();

                var allocatedCollection = retryProcess<IEnumerable<IAllocatedReplicateModel>>(() => this._TicketInfoRepository.GetAllocatedData(parameter));
                if (allocatedCollection.Any(p => !p.OriginalPayloadType.HasValue))
                {
                    rs.Success = false;
                    rs.Message = "Error some data missing payloadtype";
                    this._TracingAgent.Trace($"have some data not find originalpayloadtype", allocatedCollection);
                    return rs;
                }
                var itemInfoList = this._ProductCacheManager.GetItems(allocatedCollection.Select(x => x.ItemUID));
                this._TracingAgent.Trace("get item collection", itemInfoList);
                var syncdataGrp = allocatedCollection.GroupBy(p => p.WorkOrderPodUID);
                foreach (var pod in syncdataGrp)
                {
                    //區分虛擬實體產品
                    var ActualProductGrp = pod.GroupBy(g => new
                    {
                        ActualProduct =
                        itemInfoList.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct,
                        InventoryType = g.OriginalPayloadType
                    });
                    foreach (var itemgrp in ActualProductGrp)
                    {
                        if (string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                        {
                            foreach (var item in itemgrp)
                            {
                                var pkgInfo = this._PackageCacheManager.GetPackage(item.PackageUID);
                                var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == item.ItemUID);
                                var am = new WmsAllocatedExtendModel();
                                am.BelongTo = item.UID;
                                am.ItemNo = iteminfo.Name;
                                am.QuantityToAllocated = item.Quantity;
                                am.QuantityToPick = item.Quantity;
                                am.Status = (int)WMSReplicateAllocatedStatus.Picked;
                                am.LocationID = item.LocationID;
                                am.OrderNo = Convert.ToInt32(item.ExternalOrderNo);
                                am.WmsLocationID = item.CurrentSlotName ?? "";  //目的Slot
                                am.WarehouseID = item.WarehouseID;
                                am.LocationID = item.LocationID;
                                am.OrderQuantity = item.Quantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = item.WorkOrderPayloadUID;
                                am.inventory_type = item.OriginalPayloadType.Value;
                                allocatedsendData.Add(am);
                            }
                        }
                        else
                        {
                            var combineItems = itemgrp.Select(p =>
                            {
                                var vi = new VirtualItemInfo();
                                var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                                vi.ActualProduct = iteminfo.ActualProduct;
                                vi.ProductId = iteminfo.ID;
                                vi.Quantity = p.Quantity;
                                vi.ProductUID = iteminfo.UID;
                                vi.CustomerUID = itemgrp.FirstOrDefault().PartyUID;
                                vi.PUOM = iteminfo.PUOM;
                                return vi;
                            });
                            var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                            //this._TracingAgent.Trace("Combine virtual item", combineItem);
                            if (combineItem.Success)
                            {
                                var am = new WmsAllocatedExtendModel();
                                var _belongtoUID = itemgrp.OrderBy(p => p.UID).FirstOrDefault().UID;
                                var _wpayloaduid = itemgrp.OrderBy(p => p.WorkOrderPayloadUID).FirstOrDefault().WorkOrderPayloadUID;
                                am.BelongTo = _belongtoUID;
                                am.ItemNo = combineItem.Content.Name;
                                am.QuantityToAllocated = combineItem.Content.CombinedQuantity;
                                am.QuantityToPick = combineItem.Content.CombinedQuantity;
                                am.Status = (int)WMSReplicateAllocatedStatus.Picked;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderNo = Convert.ToInt32(itemgrp.First().ExternalOrderNo);
                                am.WmsLocationID = itemgrp.First().CurrentSlotName ?? "";  //目的Slot
                                am.WarehouseID = itemgrp.First().WarehouseID;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderQuantity = combineItem.Content.CombinedQuantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = _wpayloaduid;
                                am.inventory_type = itemgrp.Key.InventoryType.Value;
                                allocatedsendData.Add(am);
                            }
                        }
                    }

                }
                //var onhandList = allocatedCollection.Content.GroupBy(p => p.ItemUID);
                foreach (var item in allocatedsendData)
                {
                    var im = new WmsInventoryModel();
                    im.ItemNo = item.ItemNo;
                    im.LocationID = item.LocationID.Value;
                    im.Quantity = item.QuantityToAllocated * -1;
                    im.UpdateDate = DateTime.Now;
                    im.WarehouseID = item.WarehouseID;
                    im.WmsLocationID = item.WmsLocationID;
                    im.InventoryType = item.inventory_type;
                    inventorysendData.Add(im);
                }

                if (allocatedsendData.Count > 0)
                {
                    var allocatedSyncResult = this._AllocatedReplicateClient.Sync(allocatedsendData, replicateuid.ToString("D"));
                    var inventorySyncResult = this._InventoryReplicateClient.Sync(inventorysendData, replicateuid.ToString("D"));
                    this._TracingAgent.Trace($"sync allocated result:{allocatedSyncResult.Content}", allocatedSyncResult);
                    this._TracingAgent.Trace($"sync onhand result:{inventorySyncResult.Content}", inventorySyncResult);
                    this._TracingAgent.Trace("Outbound sync complete");
                    var jsonAllocated = "";// JsonConvert.SerializeObject(allocatedsendData) ?? "";
                    var jsonInventory = "";// JsonConvert.SerializeObject(inventorysendData) ?? "";
                    foreach (var item in allocatedCollection)
                    {
                        //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                        var log = new ReplicationlogModel();
                        log.UID = Guid.NewGuid();
                        log.BelongToUID = item.UID;//Ticket Info UID
                        log.Action = (int)ReplicationAction.Outbound;
                        log.ReplicateUID = replicateuid;
                        log.Operate = (int)ReplicationOperate.Allocated;
                        log.IsComplete = allocatedSyncResult.Success;
                        log.ItemUID = item.ItemUID;
                        log.Quantity = item.Quantity * -1;
                        log.OriginalData = jsonAllocated;
                        log.CreatedBy = this._Authentication.Account;
                        log.CreatedOn = DateTime.Now;
                        logcollcetion.Add(log);
                    }
                    foreach (var item in allocatedCollection)
                    {
                        //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                        var log = new ReplicationlogModel();
                        log.UID = Guid.NewGuid();
                        log.BelongToUID = item.UID;//Ticket Info UID
                        log.Action = (int)ReplicationAction.Outbound;
                        log.Operate = (int)ReplicationOperate.Inventory;
                        log.IsComplete = inventorySyncResult.Success;
                        log.ItemUID = item.ItemUID;
                        log.Quantity = item.Quantity * -1;
                        log.ReplicateUID = replicateuid;
                        log.OriginalData = jsonInventory;
                        log.CreatedBy = this._Authentication.Account;
                        log.CreatedOn = DateTime.Now;
                        logcollcetion.Add(log);
                    }
                    var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                    this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                    rs.Success = allocatedSyncResult.Success;
                    if (!rs.Success)
                    {
                        rs.Message = allocatedSyncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                    }
                    else
                    {
                        this._TracingAgent.Trace("Outbound sync complete");
                    }
                }
                else
                {
                    rs.Success = true;
                    rs.Message = "not sync";
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Outbound sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        public IActionResult<bool> Rollback(IGetReplicateDataParameter parameter,
           IEnumerable<IAllocatedReplicateModel> allocatedReplicateModels = null)
        {
            this._TracingAgent.Trace("ready to rollback sync received parameter", parameter);
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
                List<WmsAllocatedModel> allocatedsendCollection = new List<WmsAllocatedModel>();
                List<WmsInventoryModel> wmsInventoryCollection = new List<WmsInventoryModel>();

                IEnumerable<IAllocatedReplicateModel> allocatedCollection = null;
                if (allocatedReplicateModels != null)
                {
                    allocatedCollection = allocatedReplicateModels;
                }
                else
                {
                    allocatedCollection = retryProcess<IEnumerable<IAllocatedReplicateModel>>(
                        () => this._TicketInfoRepository.GetAllocatedData(parameter));
                }
                if (allocatedCollection.Any(p => !p.OriginalPayloadType.HasValue))
                {
                    rs.Success = false;
                    rs.Message = "Error some data missing payloadtype";
                    this._TracingAgent.Trace($"have some data not find originalpayloadtype", allocatedCollection);
                    return rs;
                }

                var itemInfoList = this._ProductCacheManager.GetItems(allocatedCollection.Select(x => x.ItemUID));
                var syncdataGrp = allocatedCollection.GroupBy(p => p.WorkOrderPodUID);
                foreach (var pod in syncdataGrp)
                {
                    //區分虛擬實體產品
                    var ActualProductGrp = pod.GroupBy(g => new
                    {
                        ActualProduct =
                        itemInfoList.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct
                    });
                    foreach (var itemgrp in ActualProductGrp)
                    {
                        if (string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                        {
                            foreach (var item in itemgrp)
                            {
                                var pkgInfo = this._PackageCacheManager.GetPackage(item.PackageUID);
                                var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == item.ItemUID);
                                var am = new WmsAllocatedExtendModel();
                                am.BelongTo = item.UID;
                                am.ItemNo = iteminfo.Name;
                                am.QuantityToAllocated = item.Quantity;
                                am.QuantityToPick = 0;
                                am.Status = (int)WMSReplicateAllocatedStatus.Allocated;
                                am.OrderNo = Convert.ToInt32(item.ExternalOrderNo);
                                am.WmsLocationID = item.OriginalSlotName ?? "";
                                am.WarehouseID = item.WarehouseID;
                                am.LocationID = item.LocationID;
                                am.OrderQuantity = item.Quantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = item.WorkOrderPayloadUID;
                                am.inventory_type = item.OriginalPayloadType.Value;
                                allocatedsendCollection.Add(am);
                            }
                        }
                        else
                        {
                            var combineItems = itemgrp.Select(p =>
                            {
                                var vi = new VirtualItemInfo();
                                var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                                vi.ActualProduct = iteminfo.ActualProduct;
                                vi.ProductId = iteminfo.ID;
                                vi.Quantity = p.Quantity;
                                vi.ProductUID = iteminfo.UID;
                                vi.CustomerUID = itemgrp.FirstOrDefault().PartyUID;
                                vi.PUOM = iteminfo.PUOM;
                                return vi;
                            });
                            var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                            if (combineItem.Success)
                            {
                                var am = new WmsAllocatedExtendModel();
                                var _belongtoUID = itemgrp.OrderBy(p => p.UID).FirstOrDefault().UID;
                                var _wpayloaduid = itemgrp.OrderBy(p => p.WorkOrderPayloadUID).FirstOrDefault().WorkOrderPayloadUID;
                                am.BelongTo = _belongtoUID;
                                am.ItemNo = combineItem.Content.Name;
                                am.QuantityToAllocated = combineItem.Content.CombinedQuantity;
                                am.QuantityToPick = 0;
                                am.Status = (int)WMSReplicateAllocatedStatus.Allocated;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderNo = Convert.ToInt32(itemgrp.First().ExternalOrderNo);
                                am.WmsLocationID = itemgrp.First().OriginalSlotName ?? "";
                                am.WarehouseID = itemgrp.First().WarehouseID;
                                am.LocationID = itemgrp.First().LocationID;
                                am.OrderQuantity = combineItem.Content.CombinedQuantity;
                                am.UserID = this._Authentication.Account;
                                am.UpdateDate = DateTime.Now;
                                am.UID = _wpayloaduid;
                                am.inventory_type = itemgrp.First().OriginalPayloadType.Value;
                                allocatedsendCollection.Add(am);
                            }
                        }
                    }

                }
                var allocatedSyncResult = this._AllocatedReplicateClient.Sync(allocatedsendCollection, replicateuid.ToString("D"));
                this._TracingAgent.Trace($"sync allocated result:{allocatedSyncResult.Content}", allocatedSyncResult);
                var requestJson = "";// JsonConvert.SerializeObject(allocatedsendCollection);
                foreach (var item in allocatedCollection)
                {
                    //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.BelongToUID = item.UID;//Ticket Info UID
                    log.Action = (int)ReplicationAction.Allocated;
                    log.ReplicateUID = replicateuid;
                    log.Operate = (int)ReplicationOperate.Allocated;
                    log.IsComplete = allocatedSyncResult.Success;
                    log.ItemUID = item.ItemUID;
                    log.Quantity = item.Quantity;
                    log.OriginalData = requestJson;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;
                    logcollcetion.Add(log);
                }
                //如果TicketInfo已經完成出貨，需要將onhand 還回去
                var syncdata2 = allocatedCollection.Where(p => p.TicketInfoStatus >= (int)TicketInfoStatus.Glitch
                   && p.TicketInfoType == (int)TicketInfoType.Outbound)
                   .GroupBy(p => p.WorkOrderPodUID);
                allocatedsendCollection.Clear();
                foreach (var pod in syncdata2)
                {
                    var ActualProductGrp = pod.GroupBy(g => new
                    {
                        ActualProduct =
                       itemInfoList.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct,
                        InventoryType = g.OriginalPayloadType
                    });
                    foreach (var itemgrp in ActualProductGrp)
                    {
                        if (string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                        {
                            foreach (var item in itemgrp)
                            {
                                var syncInfo = item;
                                //var pkgInfo = this._PackageCacheManager.GetPackage(syncInfo.PackageUID);
                                var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == syncInfo.ItemUID);
                                var im = new WmsInventoryModel();
                                im.ItemNo = iteminfo.Name;
                                im.Quantity = syncInfo.PickQuantity;
                                im.LocationID = syncInfo.LocationID;
                                im.WmsLocationID = syncInfo.CurrentSlotName;
                                im.WarehouseID = syncInfo.WarehouseID;
                                im.LocationID = syncInfo.LocationID;
                                im.UpdateDate = DateTime.Now;
                                im.InventoryType = item.OriginalPayloadType.Value;
                                wmsInventoryCollection.Add(im);
                            }
                        }
                        else
                        {
                            var combineItems = itemgrp.Select(p =>
                            {
                                var vi = new VirtualItemInfo();
                                var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                                vi.ActualProduct = iteminfo.ActualProduct;
                                vi.ProductId = iteminfo.ID;
                                vi.Quantity = p.Quantity;
                                vi.ProductUID = iteminfo.UID;
                                vi.CustomerUID = itemgrp.FirstOrDefault().PartyUID;
                                vi.PUOM = iteminfo.PUOM;
                                return vi;
                            });
                            var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                            if (combineItem.Success)
                            {
                                var im = new WmsInventoryModel();
                                im.ItemNo = combineItem.Content.Name;
                                im.Quantity = combineItem.Content.CombinedQuantity;
                                im.LocationID = itemgrp.First().LocationID;
                                im.WmsLocationID = itemgrp.First().CurrentSlotName ?? "";
                                im.WarehouseID = itemgrp.First().WarehouseID;
                                im.LocationID = itemgrp.First().LocationID;
                                im.UpdateDate = DateTime.Now;
                                im.InventoryType = itemgrp.Key.InventoryType.Value;
                                wmsInventoryCollection.Add(im);

                            }
                        }
                    }


                }
                //foreach (var item in syncdata2)
                //{
                //    var syncInfo = item.FirstOrDefault();
                //    var pkgInfo = this._PackageCacheManager.GetPackage(syncInfo.PackageUID);
                //    var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == syncInfo.ItemUID);
                //    var im = new WmsInventoryModel();
                //    im.ItemNo = iteminfo.Name;
                //    im.Quantity = syncInfo.PickQuantity;
                //    im.LocationID = syncInfo.LocationID;
                //    im.WmsLocationID = syncInfo.LandingZoneSlotName;
                //    im.WarehouseID = syncInfo.WarehouseID;
                //    im.LocationID = syncInfo.LocationID;
                //    im.UpdateDate = DateTime.Now;
                //    im.UID = item.Key;
                //    wmsInventoryCollection.Add(im);
                //}
                var onhandSyncResult = this._InventoryReplicateClient.Sync(wmsInventoryCollection, replicateuid.ToString("D"));
                this._TracingAgent.Trace($"sync onhand result:{onhandSyncResult.Content}", onhandSyncResult);
                var requestJsonInventory = "";// JsonConvert.SerializeObject(wmsInventoryCollection);
                foreach (var item in wmsInventoryCollection)
                {
                    var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.Action = (int)ReplicationAction.Deallocated;
                    log.ReplicateUID = replicateuid;
                    log.Operate = (int)ReplicationOperate.Inventory;
                    log.IsComplete = allocatedSyncResult.Success;
                    //log.ItemUID = iteminfo.UID;
                    log.Quantity = item.Quantity;
                    log.OriginalData = requestJsonInventory;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;
                    logcollcetion.Add(log);
                }
                var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                rs.Success = allocatedSyncResult.Success;
                if (!rs.Success)
                {
                    rs.Message = allocatedSyncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                }
                else
                {
                    this._TracingAgent.Trace("Rollback  sync complete");
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Rollback sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> ModifiedOnhand(IEnumerable<WMSReplicateOnhandModel> replicateOnhandModels)
        {
            this._TracingAgent.Trace("ready to modifiedonhand sync received parameter", replicateOnhandModels);
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            List<WmsInventoryModel> inventorysendData = new List<WmsInventoryModel>();
            List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
            var itemInfoList = this._ProductCacheManager.GetItems(replicateOnhandModels.Select(x => x.ItemUID));
            var slotMapping = this._InventoryManager.GetSlotMappingList(replicateOnhandModels.Select(x => x.SlotUID));
            try
            {
                var onhandListGrp = replicateOnhandModels.GroupBy(p => new
                {
                    ActualProduct = itemInfoList.FirstOrDefault(x => p.ItemUID == x.UID).ActualProduct,
                    SlotUID = p.SlotUID,
                    InventoryType = p.PayloadType
                });
                foreach (var itemgrp in onhandListGrp)
                {
                    if (!string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                    {
                        var isnegative = itemgrp.All(p => p.Quantity < 0);
                        var combineItems = itemgrp.Select(p =>
                        {
                            var vi = new VirtualItemInfo();
                            var subiteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                            vi.ActualProduct = subiteminfo.ActualProduct;
                            vi.ProductId = subiteminfo.ID;
                            vi.Quantity = Math.Abs(p.Quantity);
                            vi.ProductUID = subiteminfo.UID;
                            vi.CustomerUID = new Guid(subiteminfo.CustomerUID);
                            vi.PUOM = subiteminfo.PUOM;
                            return vi;
                        });
                        var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                        //this._TracingAgent.Trace("Combine virtual item", combineItem);
                        var iteminfo = combineItem.Content;
                        var slotinfo = slotMapping.Content.FirstOrDefault(p => p.UID == itemgrp.Key.SlotUID);
                        var im = new WmsInventoryModel();
                        im.ItemNo = iteminfo.Name;
                        im.LocationID = slotinfo.LocationID;
                        if (!isnegative)
                            im.Quantity = iteminfo.CombinedQuantity;
                        else
                            im.Quantity = iteminfo.CombinedQuantity * -1;
                        im.UpdateDate = DateTime.Now;
                        im.WarehouseID = slotinfo.WarehouseID;
                        im.WmsLocationID = slotinfo.SlotName;
                        im.InventoryType = itemgrp.Key.InventoryType;
                        inventorysendData.Add(im);
                    }
                    else
                    {
                        foreach (var item in itemgrp)
                        {
                            var slotinfo = slotMapping.Content.FirstOrDefault(p => p.UID == item.SlotUID);
                            var info = item;
                            var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == info.ItemUID);
                            var im = new WmsInventoryModel();
                            im.ItemNo = iteminfo.Name;
                            im.LocationID = slotinfo.LocationID;
                            im.Quantity = item.Quantity;
                            im.UpdateDate = DateTime.Now;
                            im.WarehouseID = slotinfo.WarehouseID;
                            im.WmsLocationID = slotinfo.SlotName;
                            im.InventoryType = itemgrp.Key.InventoryType;
                            inventorysendData.Add(im);

                        }

                    }

                }
                var inventorySyncResult = this._InventoryReplicateClient.Sync(inventorysendData, replicateuid.ToString("D"));
                this._TracingAgent.Trace($"sync onhand result:{inventorySyncResult.Content}", inventorySyncResult);
                var requestJsonInventory = "";// JsonConvert.SerializeObject(inventorysendData);

                foreach (var item in replicateOnhandModels)
                {

                    var log = new ReplicationlogModel();
                    log.UID = Guid.NewGuid();
                    log.Action = (int)ReplicationAction.ModitiedOnhand;
                    log.Operate = (int)ReplicationOperate.Inventory;
                    log.IsComplete = inventorySyncResult.Success;
                    log.ItemUID = item.ItemUID;
                    log.Quantity = item.Quantity;
                    log.ReplicateUID = replicateuid;
                    log.OriginalData = requestJsonInventory;
                    log.CreatedBy = this._Authentication.Account;
                    log.CreatedOn = DateTime.Now;
                    log.BelongToUID = item.PayloadUID;
                    logcollcetion.Add(log);
                }
                var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                rs.Success = inventorySyncResult.Success;
                if (!rs.Success)
                {
                    rs.Message = inventorySyncResult.Message + " " + Resource.COMMON_SYNC_ERROR;

                }
                else
                {
                    this._TracingAgent.Trace("Modified onhand sync complete");
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Modified onhand sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> Move(IEnumerable<WMSReplicateMoveModel> replicateMoveModels)
        {
            this._TracingAgent.Trace("ready to move sync received parameter", replicateMoveModels);
            var allowCombineTicketInfoStatus = new int[] { (int)TicketInfoStatus.Complete, (int)TicketInfoStatus.Glitch };
            var replicateuid = Guid.NewGuid();
            var rs = ActionResultTemplates.Result<bool>();
            List<WmsInventoryModel> inventorysendData = new List<WmsInventoryModel>();
            List<ReplicationlogModel> logcollcetion = new List<ReplicationlogModel>();
            //this._TracingAgent.Trace("Start get item info", DateTime.Now);
            var itemInfoList = this._ProductCacheManager.GetItems(replicateMoveModels.Select(x => x.ItemUID));
            //this._TracingAgent.Trace("End get item info", DateTime.Now);
            //this._TracingAgent.Trace("Start get slot list", DateTime.Now);
            var orginalslotMapping = this._InventoryManager.GetSlotMappingList(replicateMoveModels.Select(x => x.OriginalSlotUID));
            var targetslotMapping = this._InventoryManager.GetSlotMappingList(replicateMoveModels.Select(x => x.TargetSlotUID));
            //this._TracingAgent.Trace("End get slot list ", DateTime.Now);
            try
            {
                var ActualProductGrp = replicateMoveModels.GroupBy(g => new
                {
                    ActualProduct =
                        itemInfoList.FirstOrDefault(p => p.UID == g.ItemUID).ActualProduct,
                    ItemGroupUID = g.ItemGroup,
                    TicketUID = g.TicketUID,
                    InventoryType = g.PayloadType
                });
                //this._TracingAgent.Trace($"Start process sync parameters", DateTime.Now);
                foreach (var itemgrp in ActualProductGrp)
                {

                    if (!string.IsNullOrEmpty(itemgrp.Key.ActualProduct))
                    {
                        //this._TracingAgent.Trace($"Start process virtual items:{itemgrp.Key.ActualProduct} ", DateTime.Now);
                        //從ItemGroup 取得相關虛擬item TicketInfo 判斷是否完成

                        IActionResult<IEnumerable<IAllocatedReplicateModel>> processvitemCollection = null;
                        if (replicateMoveModels.FirstOrDefault().ManifestType != (int)ManifestType.Inbound)
                        {
                            processvitemCollection = this._TicketInfoRepository.GetAllocatedDataByItemGroup(itemgrp.Key.ItemGroupUID, itemgrp.Key.TicketUID);
                        }
                        else
                        {
                            processvitemCollection = this._TicketInfoRepository.GetAllocatedDataByItemGroupInbound(itemgrp.Key.ItemGroupUID, itemgrp.Key.TicketUID);
                        }
                        var originalslotinfo = orginalslotMapping.Content.FirstOrDefault(p => p.UID == itemgrp.First().OriginalSlotUID);
                        var targetslotinfo = targetslotMapping.Content.FirstOrDefault(p => p.UID == itemgrp.First().TargetSlotUID);
                        var aa = processvitemCollection.Content
                            .Where(p => allowCombineTicketInfoStatus.Contains(p.TicketInfoStatus));
                        var combineItems = aa.Select(p =>
                        {
                            var iteminfo = itemInfoList.FirstOrDefault(x => x.UID == p.ItemUID);
                            if (iteminfo == null)
                                iteminfo = this._ProductCacheManager.GetItem(p.ItemUID) as IProductExtendModel;
                            var vi = new VirtualItemInfo();
                            var subiteminfo = iteminfo;
                            vi.ActualProduct = subiteminfo.ActualProduct;
                            vi.ProductId = subiteminfo.ID;
                            vi.Quantity = p.Quantity;
                            vi.ProductUID = subiteminfo.UID;
                            vi.CustomerUID = new Guid(subiteminfo.CustomerUID);
                            vi.PUOM = iteminfo.PUOM;
                            return vi;
                        });
                        if (combineItems != null)
                        {
                            var combineItem = this._ProductCacheManager.NewCombineToActualItem(combineItems);
                            //this._TracingAgent.Trace("Combine virtual item", combineItem);
                            var info = combineItem.Content;
                            if (info != null)
                            {
                                //target slot
                                var targetim = new WmsInventoryModel();
                                targetim.ItemNo = info.Name;
                                targetim.LocationID = targetslotinfo.LocationID;
                                targetim.Quantity = info.CombinedQuantity;
                                targetim.UpdateDate = DateTime.Now;
                                targetim.WarehouseID = targetslotinfo.WarehouseID;
                                targetim.WmsLocationID = targetslotinfo.SlotName;
                                targetim.InventoryType = itemgrp.Key.InventoryType;
                                inventorysendData.Add(targetim);
                                //original slot
                                var originalim = targetim.Clone<WmsInventoryModel>();
                                originalim.LocationID = originalslotinfo.LocationID;
                                originalim.WarehouseID = originalslotinfo.WarehouseID;
                                originalim.WmsLocationID = originalslotinfo.SlotName;
                                originalim.Quantity = originalim.Quantity * -1;
                                originalim.InventoryType = itemgrp.Key.InventoryType;
                                inventorysendData.Add(originalim);
                            }
                        }
                        //this._TracingAgent.Trace("end process virtual items ", DateTime.Now);
                    }
                    else
                    {

                        foreach (var item in itemgrp)
                        {
                            //this._TracingAgent.Trace($"Start process regular item:{item.ItemUID}", DateTime.Now);
                            var originalslotinfo = orginalslotMapping.Content.FirstOrDefault(p => p.UID == item.OriginalSlotUID);
                            var targetslotinfo = targetslotMapping.Content.FirstOrDefault(p => p.UID == item.TargetSlotUID);
                            var info = item;
                            var iteminfo = itemInfoList.FirstOrDefault(p => p.UID == info.ItemUID);
                            //target slot
                            var targetim = new WmsInventoryModel();
                            targetim.ItemNo = iteminfo.Name;
                            targetim.LocationID = targetslotinfo.LocationID;
                            targetim.Quantity = item.Quantity;
                            targetim.UpdateDate = DateTime.Now;
                            targetim.WarehouseID = targetslotinfo.WarehouseID;
                            targetim.WmsLocationID = targetslotinfo.SlotName;
                            targetim.InventoryType = item.PayloadType;
                            inventorysendData.Add(targetim);
                            //original slot
                            var originalim = targetim.Clone<WmsInventoryModel>();
                            originalim.LocationID = originalslotinfo.LocationID;
                            originalim.WarehouseID = originalslotinfo.WarehouseID;
                            originalim.WmsLocationID = originalslotinfo.SlotName;
                            originalim.Quantity = originalim.Quantity * -1;
                            originalim.InventoryType = item.PayloadType;
                            inventorysendData.Add(originalim);
                            //this._TracingAgent.Trace("End process regular item", DateTime.Now);
                        }

                    }
                }
                //this._TracingAgent.Trace($"End process sync parameters", DateTime.Now);
                if (inventorysendData.Count > 0)
                {
                    var inventorySyncResult = this._InventoryReplicateClient.Sync(inventorysendData, replicateuid.ToString("D"));
                    this._TracingAgent.Trace($"sync onhand result:{inventorySyncResult.Content}", inventorySyncResult);
                    var requestJson = "";// JsonConvert.SerializeObject(inventorysendData);
                    foreach (var item in replicateMoveModels)
                    {
                        //var iteminfo = itemInfoList.FirstOrDefault(p => p.Name == item.ItemNo);
                        var log = new ReplicationlogModel();
                        log.UID = Guid.NewGuid();
                        log.Action = (int)ReplicationAction.Move;
                        log.Operate = (int)ReplicationOperate.Inventory;
                        log.IsComplete = inventorySyncResult.Success;
                        log.ItemUID = item.ItemUID;
                        log.Quantity = item.Quantity;
                        log.ReplicateUID = replicateuid;
                        log.BelongToUID = item.PayloadUID;
                        log.OriginalData = JsonConvert.SerializeObject(item);
                        log.CreatedBy = this._Authentication.Account;
                        log.CreatedOn = DateTime.Now;
                        logcollcetion.Add(log);
                    }
                    var logrs = this._ReplicationlogRepository.BatchAdd(logcollcetion);
                    this._TracingAgent.Trace($"log Result:{logrs.Success}", logrs, logcollcetion);
                    rs.Content = rs.Success = inventorySyncResult.Success;
                    if (!rs.Success)
                    {
                        rs.Message = inventorySyncResult.Message + " " + Resource.COMMON_SYNC_ERROR;
                    }
                    else
                    {
                        this._TracingAgent.Trace("Move onhand sync complete");
                    }
                }
                else
                {
                    rs.Content = rs.Success = true;

                    rs.Message = "not sync";
                }
            }
            catch (Exception ex)
            {
                this._TracingAgent.Trace("Move onhand sync occur exception", ex);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        private T retryProcess<T>(Func<IActionResult<T>> p)
        {
            int maxRetry = 3;
            int current = 0;
            while (maxRetry >= current)
            {
                var rs = p.Invoke();
                if (rs.Success)
                {
                    this._TracingAgent.Trace($"Invoke method {p.Method.Name} successfully", rs.Content);
                    return rs.Content;
                }
                else
                {
                    this._TracingAgent.Trace($"Invoke method {p.Method.Name} failure", rs.Message, rs.InnerException);
                    current++;
                }
            }
            return default(T);
        }
    }
}
