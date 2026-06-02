using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Interfaces;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Model;

namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// WorkOrder相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/WorkOrder")]
    public class WorkOrderController : AbstractApiController
    {
        /// <summary>
        ///  下拉選單
        /// </summary>
        /// <param name="vuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetWorkOrderPodNameList")]
        public IHttpActionResult GetWorkOrderPodNameList(Guid? vuid)
        {
            if (this.IsLegalGuid(vuid))
            {
                InitDIRoot();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
                var rs = _instance.GetWorkOrderPodList(vuid.Value);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, Resource.COMMON_ILLEGAL_GUID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wuid"></param>
        /// <returns></returns>
        [Compression]
        [HttpGet]
        [ActionName("GetLandingZoneList")]
        public IHttpActionResult GetLandingZoneList(Guid? wuid)
        {
            if (this.IsLegalGuid(wuid))
            {
                InitDIRoot();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
                var rs = _instance.GetLandingZoneList(wuid.Value);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, Resource.COMMON_ILLEGAL_GUID);
            }
        }
        /// <summary>
        /// 取得WorkOrder Pod 列表
        /// </summary>
        /// <param name="vuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetWorkOrderPod")]
        public IHttpActionResult GetWorkOrderPod(Guid? vuid)
        {
            if (this.IsLegalGuid(vuid))
            {
                InitDIRoot();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
                var rs = _instance.GetWorkOrderPod(vuid.Value);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, Resource.COMMON_ILLEGAL_GUID);
            }
        }
        /// <summary>
        /// 取得WorkOrder Payload 列表
        /// </summary>
        /// <param name="vuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetWorkOrderPayload")]
        public IHttpActionResult GetWorkOrderPayload(Guid? vuid)
        {
            if (this.IsLegalGuid(vuid))
            {
                InitDIRoot();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
                var rs = _instance.GetWorkOrderPayload(vuid.Value);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, Resource.COMMON_ILLEGAL_GUID);
            }
        }
        /// <summary>
        /// 儲存 Work Order Pod/ Payload
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SaveAssignedItem")]
        public IHttpActionResult SaveAssignedItem([FromBody]AssignedWorkOrderCollection Model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.SaveAssignedWorkItmes(Model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 儲存 Outbound Work Order Pod/ Payload
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SaveOutboundAssignedItem")]
        public IHttpActionResult SaveOutboundAssignedItem([FromBody]AssignedOutboundWorkOrderCollection Model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.SaveOutboundAssignedWorkItems(Model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 合併WorkOrder Pod
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("MergePallet")]
        public IHttpActionResult MergePallet(WorkOrderMergePalletParameter parameter)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.MergePalletAPI(parameter);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 儲存 Work Order Pod
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddWorkOrderPod")]
        public IHttpActionResult AddWorkOrderPod(WorkOrderPodParameter parameter)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.AddWorkOrderPodAPI(parameter);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <param name="workOrderPodUID"></param>
        /// <param name="podName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("EditWorkOrderPod")]
        public IHttpActionResult EditWorkOrderPod(EditWorkOrderPodParameters parameters)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            dynamic entity = new ExpandoObject();
            StorageMethod t = StorageMethod.NewPallet;
            if (parameters.Type.HasValue
                &&
                Enum.GetValues(typeof(StorageMethod))
                .Cast<StorageMethod>().Any(p => (int)p == parameters.Type.Value))
            {
                entity.Type = parameters.Type;
            }
            if (!string.IsNullOrEmpty(parameters.Name))
            {
                entity.Name = parameters.Name;
            }
            if (!string.IsNullOrEmpty(parameters.OperationSuggestion))
            {
                entity.OperationSuggestion = parameters.OperationSuggestion;
            }
            if (parameters.ContainerType.HasValue)
            {
                entity.ContainerType = parameters.ContainerType;
            }
            if (parameters.UID != Guid.Empty)
            {
                entity.UID = parameters.UID;
            }
            else
            {
                return this.GetFailureResult(-1, "illegal UID");

            }
            var rs = _instance.EditWorkOrderPod(entity);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 編輯操作指示
        /// </summary>
        /// <param name="workorderpodUID"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("EditOperationSuggestion")]
        public IHttpActionResult EditOperationSuggestion(Guid workorderpodUID, string content)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            dynamic entity = new ExpandoObject();

            if (!string.IsNullOrEmpty(content))
            {
                entity.OperationSuggestion = content;
            }
            else
            {
                return this.GetFailureResult(-1, Resource.COMMON_ERROR_OCCURED);

            }
            if (workorderpodUID != Guid.Empty)
            {
                entity.UID = workorderpodUID;
            }
            else
            {
                return this.GetFailureResult(-1, Resource.COMMON_ILLEGAL_GUID);

            }
            var rs = _instance.EditWorkOrderPod(entity);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[ActionName("GeteAvailableInventoryList")]
        //public IHttpActionResult GeteAvailableInventoryList(GetAvailableInventoryRequest parameters)
        //{
        //    InitDIRoot();
        //    var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
        //    var rs = _instance.GeteAvailableInventoryList(parameters);
        //    if (rs.Success)
        //    {
        //        var result = this.GetSuccessResult<IEnumerable<IAvailableInventoryModel>>(rs.Content);
        //        return this.Json<APIResult<IEnumerable<IAvailableInventoryModel>>>(result);
        //    }
        //    else
        //    {
        //        return this.GetFailureResult(-1, rs.Message);
        //    }
        //}

        /// <summary>
        /// 指派 Slot  給Work Order pod
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SetSlot")]
        public IHttpActionResult SetSlot(SetSlotParameters Model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.SetSlot(Model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<bool>(rs.Content);
                return this.Json<APIResult<bool>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        ///  指定LoadingZone
        /// </summary>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SetLoadingZoneSlot")]
        public IHttpActionResult SetLoadingZoneSlot(SetSlotParameters Parameters)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.SetLoadingZoneSlot(Parameters);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<bool>(rs.Content);
                return this.Json<APIResult<bool>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        ///  指定LoadingZone
        /// </summary>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AssignedPayloadtoPod")]
        public IHttpActionResult AssignedPayloadtoPod(AssignedPayloadtoPodParameters parameters)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            IActionResult<bool> rs = _instance.AssignedPayloadtoPod(parameters.WorkOrderPodUID, new Guid[] { parameters.WorkOrderPayloadUID });
            if (rs.Success)
            {
                var result = this.GetSuccessResult<bool>(rs.Content);
                return this.Json<APIResult<bool>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 指派 Barocde  給Work Order pod
        /// </summary>
        /// <param name="WorkOrderPodUID"></param>
        /// <param name="customerBarcode"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SetWorkOrderPodBarcode")]
        public IHttpActionResult SetWorkOrderPodBarcode(Guid WorkOrderPodUID, string customerBarcode)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.SetWorkOrderPodBarcode(WorkOrderPodUID, customerBarcode);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<bool>(rs.Content);
                return this.Json<APIResult<bool>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 移除WorkOrder Pod
        /// </summary>
        /// <param name="wuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("RemoveWorkOrderPod")]
        public IHttpActionResult RemoveWorkOrderPod([FromUri]Guid[] wuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.RemoveWorkOrderPodFromUI(wuid);
            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json<APIResult<bool>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wpluid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("RemoveWorkOrderPayload")]
        public IHttpActionResult RemoveWorkOrderPayload([FromUri]Guid[] wpuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.RemoveWorkOrderPayloadFromUI(wpuid);
            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json<APIResult<bool>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 取得Outbound估算值
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CheckOutboundAvailabilityQty")]
        public IHttpActionResult CheckOutboundAvailabilityQty([FromBody]CheckOutboundAvailabilityRequest parameters)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
            var rs = _instance.CheckOutboundAvailabilityQty(parameters);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vesselUID"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ExecuteInboundAutoAssign")]
        public IHttpActionResult ExecuteInboundAutoAssign([FromUri]Guid vesselUID)
        {
            InitDIRoot();

            var manager = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;

            var result = manager.ExecuteInboundAutoAssign(vesselUID);

            if (result.Success)
            {
                var apiResult = this.GetSuccessResult(result.Content);
                return this.Json(apiResult);
            }
            else
            {
                return this.GetFailureResult(-1, result.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vesselUID"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ExecuteOutboundAutoAssign")]
        public IHttpActionResult ExecuteOutboundAutoAssign([FromUri]Guid vesselUID)
        {
            InitDIRoot();

            var manager = this.DIContainer.ManifestFactory.CreateManger().WorkOrderManager;

            var result = manager.ExecuteOutboundAutoAssign(vesselUID);

            if (result.Success)
            {
                var apiResult = this.GetSuccessResult(result.Content);
                return this.Json(apiResult);
            }
            else
            {
                return this.GetFailureResult(-1, result.Message);
            }
        }
    }
}
