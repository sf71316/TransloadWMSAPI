using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Identities.Constants;
using YAEP.Interfaces;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]

    [RoutePrefix("api/Order")]
    public partial class OrderController : AbstractApiController
    {
        /// <summary>
        /// 出貨配置產品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Allocated")]
        public IHttpActionResult Allocated(AllocatedRequest request)
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();
            InitDIRoot();

            if (request != null && !string.IsNullOrEmpty(request.CustomerPartyName) && request.WarehouseUID != Guid.Empty)
            {
                var factory = this.GetIdentityFactory();
                var manager = factory.CreateGroupManager();
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
                {

                    _instance.TracingAgent.BeginTracing("", request);
                    var rs = _instance.Allocated(request);
                    _instance.TracingAgent.EndTracing(rs);
                    var result = this.GetSuccessResult<IAllocatedResponse>(rs.Content);
                    sw.Stop();
                    Debug.WriteLine($"Allocated total elapsed {sw.ElapsedMilliseconds}ms");
                    return this.Json(result);
                }
            }
            else
            {
                return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
            }

        }


        /// <summary>
        /// 出貨配置產品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("FutureAllocated")]
        public IHttpActionResult FutureAllocated(AllocatedRequest request)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            InitDIRoot();
            using (this.DIContainer.Container)
            {
                if (request != null && !string.IsNullOrEmpty(request.CustomerPartyName) && request.WarehouseUID != Guid.Empty)
                {
                    //if (!request.IsChinaWarehouse)
                    //{
                        var factory = this.GetIdentityFactory();
                        var manager = factory.CreateGroupManager();
                        var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                        _instance.TracingAgent.BeginTracing("", request);
                        var rs = _instance.FutureAllocated(request);
                        _instance.TracingAgent.EndTracing(rs);
                        var result = this.GetSuccessResult<IAllocatedResponse>(rs.Content);
                        sw.Stop();
                        Debug.WriteLine($"FutureAllocated total elapsed {sw.ElapsedMilliseconds}ms");
                        return this.Json(result);
                    //}
                    //else
                    //{
                    //    return this.BadRequest(Resource.MANIFEST_FUTURE_ALLOCATED_NOT_SUPPORT);
                    //}
                }
                else
                {
                    return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("DeleteManifest")]
        public IHttpActionResult DeleteManifest(DeleteManifestRequest request)
        {
            InitDIRoot();
            using (this.DIContainer.Container)
            {
                if (request != null && request.CustomerUID != Guid.Empty && request.WarehouseUID != Guid.Empty)
                {
                    var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                    request.IgnoreCheckManifest = true;
                    _instance.TracingAgent.BeginTracing("", request);
                    var rs = _instance.DeleteManifestByOrder(request);
                    _instance.TracingAgent.EndTracing(rs);
                    if (rs.Success)
                    {
                        var result = this.GetSuccessResult<dynamic>(rs.Content);
                        return this.Json(result);
                    }
                    else
                    {
                        var result = this.GetFailureResult<dynamic>(rs.Content, iscomplete: false);
                        return result;
                    }

                }
                else
                {
                    return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
                }
            }
        }
        [HttpPost]
        [ActionName("BatchDeAllocated")]
        public IHttpActionResult BatchDeAllocated(IEnumerable<DeallocatedRequest> request)
        {
            InitDIRoot();
            using (this.DIContainer.Container)
            {
                if (request != null && request.Count() > 0)
                {
                    if (request.All(p => p.CustomerUID != Guid.Empty && p.WarehouseUID != Guid.Empty
                        && (p.BolUID.HasValue && p.BolUID.Value != Guid.Empty || !p.BolUID.HasValue)))
                    {
                        var factory = this.GetIdentityFactory();
                        var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                        _instance.TracingAgent.BeginTracing("", request);
                        var rs = _instance.Deallocated(request);
                        _instance.TracingAgent.EndTracing(rs);
                        if (rs.Success)
                        {
                            var result = this.GetSuccessResult<dynamic>(rs.Content);
                            return this.Json(result);
                        }
                        else
                        {
                            var result = this.GetSuccessResult<dynamic>(rs.Content, iscomplete: false);
                            return this.Json(result);
                        }
                    }
                    else
                    {
                        return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
                    }
                }
                else
                {
                    return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("DeAllocated")]
        public IHttpActionResult DeAllocated(DeallocatedRequest request)
        {

            InitDIRoot();
            using (this.DIContainer.Container)
            {
                if (request != null)
                {
                    if (request.CustomerUID != Guid.Empty && request.WarehouseUID != Guid.Empty
                        && (request.BolUID.HasValue && request.BolUID.Value != Guid.Empty || !request.BolUID.HasValue))
                    {
                        var factory = this.GetIdentityFactory();
                        var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                        _instance.TracingAgent.BeginTracing("", request);
                        var rs = _instance.Deallocated(request);
                        _instance.TracingAgent.EndTracing(rs);
                        if (rs.Success)
                        {
                            var result = this.GetSuccessResult<dynamic>(rs.Content);
                            return this.Json(result);
                        }
                        else
                        {
                            var result = this.GetSuccessResult<dynamic>(rs.Content, iscomplete: false);
                            return this.Json(result);
                        }
                    }
                    else
                    {
                        var result = this.GetFailureResult(-1, Resource.MANIFEST_COMMON_REQUEST_NULL);
                        return result;
                    }
                }
                else
                {
                    return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
                }
            }
        }
        /// <summary>
        /// Pick by item (Only change ticket status)
        /// </summary>
        /// <param name="request">
        /// <![CDATA[
        /// RefNo:External order serial no
        /// ChangeStatus:狀態值 1:Processing 2:Complete
        /// ItemRefUID:WorkOrderPayload UID
        /// ]]>
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("PickItem")]
        public IHttpActionResult PickItem(PickItemRequest request)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
            _instance.TracingAgent.BeginTracing("", request);
            if (string.IsNullOrEmpty(request.RefNo))
            {
                _instance.TracingAgent.EndTracing($"{nameof(request.RefNo)}:" + Resource.COMMON_DATA_NOT_FOUND);
                return this.BadRequest($"{nameof(request.RefNo)}:" + Resource.COMMON_DATA_NOT_FOUND);
            }
            var response = _instance.PickItem(request);
            _instance.TracingAgent.EndTracing(response);
            if (response.Success)
            {
                var result = this.GetSuccessResult<dynamic>(response.Content);
                return this.Json(result);
            }
            else
            {
                var result = this.GetFailureResult<IPickItemResponse>(response.Content);
                return result;
            }
        }
        [ConnectionLog]
        [HttpPost]
        [ActionName("SyncTrackingNo")]
        public IHttpActionResult SyncTrackingNo(SyncTrackingNoRequest request)
        {
            InitDIRoot();

            var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
            var response = _instance.SyncTrackingNoAPI(request);

            if (response.Success)
            {
                var result = this.GetSuccessResult<dynamic>(response.Content);
                return this.Json(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, response.Message);
                return result;
            }
        }
        [ConnectionLog]
        [HttpPost]
        [ActionName("SyncProNo")]
        public IHttpActionResult SyncProNo(SyncProNoRequest request)
        {
            InitDIRoot();

            var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;

            var response = _instance.SyncProNoAPI(request);

            if (response.Success)
            {
                var result = this.GetSuccessResult<dynamic>(response.Content);
                return this.Json(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, response.Message);
                return result;
            }
        }

    }
}
