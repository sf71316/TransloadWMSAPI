using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Core.Item.Constants;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Code;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Model;
using YAEP.WMS.Cache;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// 手持相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Compression]
    [ConnectionLog]
    [MobileAuthentication]
    [RoutePrefix("api/Mobile")]
    public partial class MobileController : AbstractApiController
    {
        /// <summary>
        /// 查詢 Ticket 資料列表
        /// </summary>
        /// <param name="parameters">Ticket 項目</param>
        /// <returns></returns>

        [HttpGet]
        [ActionName("GetTicketList")]
        public IHttpActionResult GetTicketList([FromUri] GetTicketListParameters parameters)
        {
            InitDIRoot();
            parameters = this.AntiXSSEncode(parameters);
            var _groups = IdentityHelper.GetGroupKeys(this.GetAuthenticationInfo());
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {

                parameters.groupIds = _groups.ToArray();
                _instance.TracingAgent.BeginTracing("", parameters);

                var rs = _instance.GetTicketList(parameters);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success)
                {

                    var result = this.GetSuccessResult<IEnumerable<ITicketListViewModel>>(rs.Content);
                    return this.Json<APIResult<IEnumerable<ITicketListViewModel>>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
        }
        /// <summary>
        /// 查詢 Ticket Info 資料列表
        /// </summary>
        /// <param name="tuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetTicketInfoList")]
        public IHttpActionResult GetTicketInfoList([FromUri] Guid[] tuid)
        {
            InitDIRoot();
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                _instance.TracingAgent.BeginTracing(tuid.ToString(), tuid);
                var rs = _instance.GetTicketInfo(tuid);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<IEnumerable<dynamic>>(rs.Content);
                    return this.Json<APIResult<IEnumerable<dynamic>>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
        }
        /// <summary>
        /// 查詢 Ticket Info Items資料列表
        /// </summary>
        /// <param name="ticketinfouid"></param>
        /// <param name="workorderpoduid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetTickeInfotListDetail")]
        public IHttpActionResult GetTickeInfotListDetail([FromUri] Guid[] ticketinfouid, Guid workorderpoduid)
        {
            InitDIRoot();
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                _instance.TracingAgent.BeginTracing($"{ticketinfouid} {workorderpoduid}", ticketinfouid);
                var rs = _instance.GetTickeInfotListDetail(ticketinfouid, workorderpoduid);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<IEnumerable<dynamic>>(rs.Content);
                    return this.Json<APIResult<IEnumerable<dynamic>>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
        }
        /// <summary>
        /// Ticket Summary
        /// </summary>
        /// <param name="ticketuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetTicketSummary")]
        public IHttpActionResult GetTicketSummary(Guid ticketuid)
        {
            InitDIRoot();
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                var _groups = IdentityHelper.GetGroupKeys(this.GetAuthenticationInfo());
                _instance.TracingAgent.BeginTracing($"{ticketuid}", ticketuid);
                var rs = _instance.GetTicketSummaryData(ticketuid, _groups);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<IEnumerable<dynamic>>(rs.Content);
                    return this.Json<APIResult<IEnumerable<dynamic>>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
        }
        /// <summary>
        /// 使用地方：
        /// Inbound : 收貨明細頁面Sync 
        /// Outbound : 出貨明細頁面Sync
        /// Move : 所有move 上/下架的動作
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("UploadTicketData")]
        public IHttpActionResult UploadTicketData(UploadTicketDataParameter parameters)
        {
            InitDIRoot();
            if (parameters != null && parameters.Item != null)//&& parameters.Items.Count() > 0
            {
                parameters = this.AntiXSSEncode(parameters);
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
                {
                    _instance.TracingAgent.BeginTracing("", parameters);
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                    _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                    var rs = _instance.UploadTicketData(parameters);
                    _instance.TracingAgent.EndTracing(rs);
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, Resource.MANIFEST_COMMON_PARAMETERS_NULL);
                return result;
            }
        }

        /// <summary>
        /// 批次上傳 Ticket 結果
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("BatchUploadTicketData")]
        public IHttpActionResult BatchUploadTicketData(IEnumerable<UploadTicketDataParameter> parameters)
        {
            //TODO 改一半，批次修改ticket，待討論
            InitDIRoot();
            if (parameters != null && parameters.Count() > 0)
            {
                parameters = this.AntiXSSEncode(parameters);
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
                {
                    _instance.TracingAgent.BeginTracing("", parameters);
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                    _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                    var rs = _instance.BatchUploadTicketData(parameters);
                    _instance.TracingAgent.EndTracing(rs);
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, Resource.MANIFEST_COMMON_PARAMETERS_NULL);
                return result;
            }
        }
        /// <summary>
        /// Receiving  Ticket : 收貨後加Payload，改Onhand，呼叫PBSC WMS同步改Onhand
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("UploadTicketDataByPod")]
        public IHttpActionResult UploadTicketDataByPod(dynamic parameters)
        {
            InitDIRoot();
            var barcode = Convert.ToString(parameters.barcode);
            if (!string.IsNullOrEmpty(barcode))
            {
                parameters.barcode = this.AntiXSSEncode(barcode);
                TicketInfoParameter titem = new TicketInfoParameter();
                UploadTicketDataParameter parm = new UploadTicketDataParameter();
                titem.IsPodScan = true;
                parm.ServiceItem = Constant.Enums.TicketType.Receiving;
                titem.Barcode = new UploadTicketBarcode[] { new UploadTicketBarcode {
                    Barcode=barcode,
                    ScanQty=1
                } };
                parm.Item = titem;
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
                {
                    _instance.TracingAgent.BeginTracing("", parameters);
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                    _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                    var rs = _instance.UploadTicketDataByPodBarcode(parm);
                    _instance.TracingAgent.EndTracing(rs);
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, Resource.MANIFEST_COMMON_PARAMETERS_NULL);
                return result;
            }
        }
        /// <summary>
        /// Receiving  Ticket 整盤少收 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("UploadTicketShortageByPod")]
        public IHttpActionResult UploadTicketShortageByPod(dynamic parameters)
        {
            InitDIRoot();
            var barcode = Convert.ToString(parameters.barcode);
            if (!string.IsNullOrEmpty(barcode))
            {
                parameters.barcode = this.AntiXSSEncode(barcode);
                TicketInfoParameter titem = new TicketInfoParameter();
                UploadTicketDataParameter parm = new UploadTicketDataParameter();
                titem.IsAllShortage = true;
                titem.IsPodScan = true;
                parm.ServiceItem = TicketType.Receiving;
                titem.Barcode = new UploadTicketBarcode[] { new UploadTicketBarcode {
                    Barcode=barcode,
                    ScanQty=1
                } };
                parm.Item = titem;
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
                {
                    _instance.TracingAgent.BeginTracing("", parameters);
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                    _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                    var rs = _instance.UploadTicketDataByPodBarcode(parm);
                    _instance.TracingAgent.EndTracing(rs);
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, Resource.MANIFEST_COMMON_PARAMETERS_NULL);
                return result;
            }
        }
        /// <summary>
        /// Complete Outbound Ticket : 出貨後刪除Payload，減Onhand，呼叫PBSC WMS同步改Onhand，Complete Allocated
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("UploadOutboundTicketDataByPod")]
        public IHttpActionResult UploadOutboundTicketDataByPod(dynamic parameters)
        {
            InitDIRoot();
            var barcode = Convert.ToString(parameters.barcode);
            if (!string.IsNullOrEmpty(barcode))
            {

                parameters.barcode = this.AntiXSSEncode(barcode);
                TicketInfoParameter titem = new TicketInfoParameter();
                UploadTicketDataParameter parm = new UploadTicketDataParameter();
                titem.IsPodScan = true;
                parm.ServiceItem = Constant.Enums.TicketType.Outbound;
                titem.Barcode = new UploadTicketBarcode[] { new UploadTicketBarcode {
                    Barcode=barcode,
                    ScanQty=1
                } };
                parm.Item = titem;
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
                {
                    _instance.TracingAgent.BeginTracing("", parameters);
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                    _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                    var rs = _instance.UploadTicketDataByPodBarcode(parm);
                    _instance.TracingAgent.EndTracing(rs);
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, Resource.MANIFEST_COMMON_PARAMETERS_NULL);
                return result;
            }
        }

        /// <summary>
        /// 批次修改Ticket目的地位置
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("BatchChangeToSlot")]
        public IHttpActionResult BatchChangeToSlot(BatchChangeToSlotParameter parameters)
        {
            InitDIRoot();
            if (string.IsNullOrEmpty(parameters.SlotName))
            {
                return GetFailureResult(-1, "SlotName is null");
            }
            if ((parameters.TicketInfoUIDs?.Any(p => p == Guid.Empty) ?? false))
            {
                return GetFailureResult(-1, "TicketInfoUID is null");
            }

            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                _instance.TracingAgent.BeginTracing("", parameters);
                var rs = _instance.BatchChangeToSlotAPI(parameters);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
        }

        /// <summary>
        /// 改變Ticket目的地位置
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ChangeToSlot")]
        public IHttpActionResult ChangeToSlot(ChangeToSlotParameter parameters)
        {
            InitDIRoot();
            if (string.IsNullOrEmpty(parameters.SlotName))
            {
                return GetFailureResult(-1, "SlotName is null");
            }
            if (parameters.TicketInfoUID == Guid.Empty)
            {
                return GetFailureResult(-1, "TicketInfoUID is null");
            }

            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                _instance.TracingAgent.BeginTracing("", parameters);
                var rs = _instance.ChangeToSlot(parameters);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
        }
        /// <summary>
        /// 改變Ticket來源位置(重新Allocated)
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ChangeFromSlot")]
        public IHttpActionResult ChangeFromSlot(ChangeFromSlotParameter parameters)
        {
            InitDIRoot();
            if (string.IsNullOrEmpty(parameters.SlotName))
            {
                return GetFailureResult(-1, "SlotName is null");
            }
            if (parameters.TicketInfoUID == Guid.Empty)
            {
                return GetFailureResult(-1, "TicketInfoUID is null");
            }

            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                _instance.TracingAgent.BeginTracing("", parameters);
                var rs = _instance.ChangeFromSlotAPI(parameters);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
        }
        /// <summary>
        /// 加log 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddLog")]
        public IHttpActionResult AddLog(LogParameters data)
        {
            InitDIRoot();
            this.AntiXSSEncode(data);
            var userInfo = this.GetAuthenticationInfo();
            if (data != null)
            {
                if (data.belongtouid == Guid.Empty)
                {
                    return this.GetFailureResult(-1, "belong to UID invalid");
                }
                if (data.belongtotype < 0)
                {
                    return this.GetFailureResult(-1, "belong to type must more than zero");
                }

                //data.belongtotype
                // string message = this.AntiXSSEncode(System.Web.HttpContext.Current.Server.UrlDecode(data.message));
                var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
                _instance.TracingAgent.BeginTracing("", data);
                _instance.Log("Receivied WMS APP upload log", "Mobile", userInfo.Account,
                            "info", data.belongtotype, data.belongtouid.ToString(), jsonBefore: data.message);
                _instance.TracingAgent.EndTracing();
                return this.Ok("");
            }
            else
            {
                return GetFailureResult(-1, "parameter is null");
            }
        }
        /// <summary>
        /// 批次新增log
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("BatchAddLog")]
        public IHttpActionResult BatchAddLog(LogParameters[] logs)
        {
            InitDIRoot();
            this.AntiXSSEncode(logs);
            var userInfo = this.GetAuthenticationInfo();
            if (logs != null)
            {
                if (logs.Any(p => p.belongtouid == Guid.Empty))
                {
                    return this.GetFailureResult(-1, "belong to UID invalid");
                }
                if (logs.Any(p => p.belongtotype < 0))
                {
                    return this.GetFailureResult(-1, "belong to type must more than zero");
                }
                var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
                _instance.TracingAgent.BeginTracing("", logs);
                foreach (var data in logs)
                {
                    _instance.Log(data.message, "Mobile", userInfo.Account, "info", data.belongtotype);
                }
                _instance.TracingAgent.EndTracing();
                return this.Ok("");
            }
            else
            {
                return GetFailureResult(-1, "parameter is null");
            }
        }
        ///// <summary>
        ///// 檢查Pod 原始資訊
        ///// </summary>
        ///// <param name="ticketinfouid"></param>
        ///// <param name="barcode"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[ActionName("CheckPodBarcodeInfo")]
        //public IHttpActionResult CheckPodBarcodeInfo([FromUri]Guid ticketinfouid, string barcode)
        //{
        //    InitDIRoot();
        //    var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
        //    var rs = _instance.CheckPodBarcodeInfo(ticketinfouid, barcode);
        //    if (rs.Success)
        //    {
        //        var result = this.GetSuccessResult<IPodBarcodeInfo>(rs.Content);
        //        return this.Json<APIResult<IPodBarcodeInfo>>(result);
        //    }
        //    else
        //    {
        //        var result = this.GetFailureResult(-1, rs.Message);
        //        return result;
        //    }
        //}

        /// <summary>
        /// 自動完成Ticket 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CompleteTicket")]
        public IHttpActionResult CompleteTicket([FromUri] String[] data)
        {
            InitDIRoot();
            this.AntiXSSEncode(data);
            var userInfo = this.GetAuthenticationInfo();
            if (data != null && data.Length > 0)
            {
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
                {
                    var rs = _instance.CompleteTicketData(data);
                    var result = this.GetSuccessResult<dynamic>(rs.Content, rs.Message, rs.Success);
                    return this.Json<APIResult<dynamic>>(result);
                }
            }
            else
            {
                return GetFailureResult(-1, "parameter is null");
            }
        }


        [HttpGet]
        [ActionName("GetWarehouseList")]
        public IHttpActionResult GetWarehouseList()
        {
            //InitDIRoot();
            //using (var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager)
            {
                //var rs = _instance.GetWarehouseNameList();
                var groups = IdentityHelper.GetGroupKeys(this.GetAuthenticationInfo());
                var warehouses = DrKnowAll.GetWarehouse(groups);
                if (warehouses != null)
                {
                    warehouses = warehouses.OrderBy(x => x.ID);
                    var result = this.GetSuccessResult(warehouses);
                    return this.Json(result);
                }
                else
                {
                    return this.GetFailureResult(-1, Resource.MANIFEST_NOT_FIND_DATA);
                }
            }
        }

        [HttpGet]
        [ActionName("GetSlotList")]
        public IHttpActionResult GetSlotList([FromUri] Guid[] wuids)
        {
            InitDIRoot();

            using (var slot_manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager)
            {
                var result = slot_manager.GetSlotListFromCache(wuids);
                if (result.Success)
                {
                    var collection = result.Content;
                    collection = collection.OrderBy(o => o.ID);

                    var actionResult = this.GetSuccessResult(result.Content);
                    return this.Json(actionResult);
                }
                else
                {
                    return base.GetDataNotFoundResult();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetInventoryPayloadList")]
        public IHttpActionResult GetInventoryPayloadList(ModifyPayloadListParameters parameters)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("GetInventoryPayloadList", parameters);

                //manager.TracingAgent.Trace("Start to prepare for items which user is relate to.");
                //ProductManager pmgr = new ProductManager(base.GetAuthenticationInfo());
                //if (parameters.ItemNoList == null || parameters.ItemNoList.Length == 0)
                //{
                //    parameters.ItemNoList = pmgr.GetProductList().Select(p => p.Name).ToArray();
                //}
                //else
                //{
                //    parameters.ItemNoList = pmgr.GetProductList()
                //        .Select(p => p.Name)
                //        .Where(x => parameters.ItemNoList.Contains(x)).ToArray();
                //}
                //manager.TracingAgent.Trace("Done prepare for items which user is relate to.");

                parameters = this.AntiXSSEncode(parameters);
                var result = manager.GetModifyPayloadList(parameters);
                var colletion = result.Content;
                if (colletion != null)
                {
                    colletion = colletion.OrderBy(o => o.PayloadType).ThenBy(o => o.ItemName);
                }
                manager.TracingAgent.EndTracing(result);

                var apiResult = this.GetSuccessResult<dynamic>(colletion);
                if (!result.Success)
                {
                    apiResult.IsComplete = result.Success;
                    apiResult.Message = result.Message;
                }
                return this.Json(apiResult);
            }
        }

        [HttpPost]
        [ActionName("GetInventoryItemList")]
        public IHttpActionResult GetInventoryItemList()
        {
            try
            {
                InitDIRoot();

                using (var manager = this.DIContainer.InventoryFactory.CreateInventoryManager())
                {
                    manager.TracingAgent.BeginTracing("GetInventoryItemList");
                    var result = manager.GetItemListFromCache(null);
                    manager.TracingAgent.EndTracing();
                    if (result.Success)
                    {
                        var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                        apiResult.Message = result.Message;
                        return this.Json(apiResult);
                    }
                    else
                    {
                        return base.GetDataNotFoundResult();
                    }

                    //ProductManager pmgr = new ProductManager(base.GetAuthenticationInfo());
                    //var colletion = pmgr.GetProductList();
                    //if (colletion != null)
                    //{
                    //    colletion = colletion.OrderBy(o => o.ID);
                    //}
                    //var apiResult = base.GetSuccessResult(colletion);
                    //return base.Json(apiResult);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SetInventoryPayload")]
        public IHttpActionResult SetInventoryPayload(List<CretaeAdjustmentTicket> parameters)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                parameters = this.AntiXSSEncode(parameters);
                manager.TracingAgent.BeginTracing("", parameters);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.InventoryCounting;
                var result = manager.CreateAdjustmentTicket(parameters);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SetPayloadGroupMoveAdjustment")]
        public IHttpActionResult SetPayloadGroupMoveAdjustment(List<SetGroupMoveAdjustmentRequest> parameters)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                parameters = this.AntiXSSEncode(parameters);
                manager.TracingAgent.BeginTracing("", parameters);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.InventoryCounting;
                var result = manager.SetGroupMoveAdjustment(parameters);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }
        }

        [HttpGet]
        [ActionName("GetCurrentUserRoles")]
        public IHttpActionResult GetCurrentUserRoles()
        {
            var authInfo = base.GetAuthenticationInfo();

            var factory = this.GetIdentityFactory();
            var manager = factory.CreateUserManager();

            var result = manager.GetUserRoles(authInfo.UID);
            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }

            return base.GetDataNotFoundResult();
        }

        /// <summary>
        /// 取得Carrier Truck 列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetCarrierTruckList")]
        public IHttpActionResult GetCarrierTruckList(SearchCarrierTruckParameters parameters)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                parameters = this.AntiXSSEncode(parameters);
                manager.TracingAgent.BeginTracing("", parameters);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                if (string.IsNullOrEmpty(parameters.TimeZone))
                {
                    var tz = Request.Headers.FirstOrDefault(p => p.Key.Equals("TimeZone", StringComparison.OrdinalIgnoreCase));
                    if (tz.Equals(default(KeyValuePair<string, IEnumerable<string>>)))
                    {
                        parameters.TimeZone = tz.Value.FirstOrDefault();
                    }
                }
                var result = manager.GetCarrierTruckList(parameters);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 取得Carrier Type 列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetShipCarrierCategories")]
        public IHttpActionResult GetShipCarrierCategories()
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("");
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.GetShipCarrierCategories();
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 新增 Carrier Truck 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddCarrierTruck")]
        public IHttpActionResult AddCarrierTruck(AddCarrierTruckDTO request)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                request = this.AntiXSSEncode(request);
                manager.TracingAgent.BeginTracing("", request);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.AddCarrierTruck(request);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 新增 Carrier Pallet
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddShipCarrierPallet")]
        public IHttpActionResult AddShipCarrierPallet(AddCarrierPalletDTO request)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                request = this.AntiXSSEncode(request);
                manager.TracingAgent.BeginTracing("", request);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.AddShipCarrierPallet(request);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 刪除 Carrier Truck 
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteCarrierTruck")]
        public IHttpActionResult DeleteCarrierTruck([FromUri] Guid[] carrierTruckUIds)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", carrierTruckUIds);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.DeleteCarrierTruck(carrierTruckUIds);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 指派 Carrier Pallet 給 Carrier Truck 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AssignedPalletToTruck")]
        public IHttpActionResult AssignedPalletToTruck(AssignedPalletToTruckRequest request)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", request);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.AssignedPalletToTruck(request);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 指定Carrier Truck 送 ASN
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CarrierTruckDepartured")]
        public IHttpActionResult CarrierTruckDepartured(DeparturedRequest request)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", request);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                if (request.carrierTruckUID != null)
                {
                    var result = manager.CarrierTruckDepartured(request.carrierTruckUID.ToList());
                    manager.TracingAgent.EndTracing(result);
                    var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                    apiResult.Message = result.Message;
                    apiResult.IsComplete = result.Success;
                    return this.Json(apiResult);
                }
                else
                {
                    var apiResult = this.GetSuccessResult();
                    apiResult.Message = "Must select one truck.";
                    apiResult.IsComplete = false;
                    return this.Json(apiResult);
                }
            }
        }
        /// <summary>
        /// 指定Carrier Pallet 移動到另外一台Carrier Truck
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ChangePalletToOtherTruck")]
        public IHttpActionResult ChangePalletToOtherTruck(ChangePalletToOtherTruckRequest request)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", request);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.ChangePalletToOtherTruck(request);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 刪除Carrier Pallet 與Truck 關聯
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("RemovePalletFromTruck")]
        public IHttpActionResult RemovePalletFromTruck([FromUri] Guid[] carrierPalletUIDs)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", carrierPalletUIDs);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.RemovePalletFromTruck(carrierPalletUIDs.ToList());
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 取得Carrier Pallet 列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetCarrierPallets")]
        public IHttpActionResult GetCarrierPallets(SearchCarrierPalletParameters paramters)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", paramters);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.GetCarrierPallets(paramters);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }

        }
        /// <summary>
        /// 指派Carrier Package給Pallet
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AssignedPackageToPallet")]
        public IHttpActionResult AssignedPackageToPallet(AssignedPackageToPalletRequest request)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", request);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.AssignedPackageToPallet(request);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 刪除 Carrier Pallet (包含carrier package)
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteCarrierPallet")]
        public IHttpActionResult DeleteCarrierPallet([FromUri] Guid[] carrierPalletUIds)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", carrierPalletUIds);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.DeleteCarrierPallet(carrierPalletUIds.ToList());
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 刪除 Carrier Pallet (包含carrier package)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ChangePackageToOtherPallet")]
        public IHttpActionResult ChangePackageToOtherPallet(ChangePackageToOtherPalletRequest request)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", request);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.ChangePackageToOtherPallet(request);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);
            }
        }
        /// <summary>
        /// 刪除 Carrier pacge
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("RemovePackageFromPallet")]
        public IHttpActionResult RemovePackageFromPallet([FromUri] Guid[] palletinfoUIDs)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                if (palletinfoUIDs != null)
                {
                    manager.TracingAgent.BeginTracing("", palletinfoUIDs);
                    manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                    manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                    var result = manager.RemovePackageFromPallet(palletinfoUIDs.ToList());
                    manager.TracingAgent.EndTracing(result);
                    var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                    apiResult.Message = result.Message;
                    apiResult.IsComplete = result.Success;
                    return this.Json(apiResult);
                }
                else
                {
                    var apiResult = this.GetSuccessResult();
                    apiResult.IsComplete = false;
                    apiResult.Message = "uid can't null";
                    return this.Json(apiResult);
                }

            }
        }
        /// <summary>
        /// 取得Carrier package 列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetCarrierPalletInfos")]
        public IHttpActionResult GetCarrierPalletInfos(SearchCarrierPalletInfoParameters parameters)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager)
            {
                manager.TracingAgent.BeginTracing("", parameters);
                manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;

                var result = manager.GetCarrierPalletInfos(parameters);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                apiResult.IsComplete = result.Success;
                return this.Json(apiResult);

            }
        }
    }
}
