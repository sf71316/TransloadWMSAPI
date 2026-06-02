using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using YAEP.Identities.Constants;
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
    public partial class OrderController : AbstractApiController
    {
        /// <summary>
        /// Pick complete by order
        /// </summary>
        /// <param name="request">RefNo:External order serial no</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("PickAll")]
        public IHttpActionResult PickAll(PickAllRequest request)
        {

            InitDIRoot();
            if (string.IsNullOrEmpty(request.RefNo))
            {
                return this.BadRequest($"{nameof(request.RefNo)}:" + Resource.COMMON_DATA_NOT_FOUND);
            }
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {

                _instance.TracingAgent.BeginTracing("", request);
                if (request.RequestFunction == (int)RequestFunction.PackingStation)
                {
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.PackingStation;
                }
                else if (request.RequestFunction == (int)RequestFunction.Web)
                {
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.Web;
                }
                else if (request.RequestFunction == (int)RequestFunction.ShippingWebService)
                {
                    _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.ExternalService;
                }
                _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                _instance.TracingAgent.TransactionInfo.Action = TransactionlogAction.PickAll;
                var response = _instance.PickAll(request);
                _instance.TracingAgent.EndTracing(response);
                if (response.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(response.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, response.Message, response.Content);
                    return result;
                }
            }
        }

        /// <summary>
        /// 退回 Ticket 處理狀態
        /// </summary>
        /// <param name="request">
        /// <![CDATA[
        /// BolRefUID:BolUID
        /// RefNo:External order serial no
        /// ]]>
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("RollbackTicket")]
        public IHttpActionResult RollbackTicket(RollbackTicketRequest request)
        {

            InitDIRoot();
            if (request.BolRefUID == null)
            {
                return this.BadRequest($"{nameof(request.BolRefUID)}:" + Resource.COMMON_DATA_NOT_FOUND);
            }
            //if (string.IsNullOrEmpty(request.RefNo))
            //{
            //    return this.BadRequest($"{nameof(request.RefNo)}:" + Resource.COMMON_DATA_NOT_FOUND);
            //}
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {

                _instance.TracingAgent.BeginTracing("", request);
                _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.Web;
                _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                var response = _instance.RollbackTicket(request);
                _instance.TracingAgent.EndTracing(response);
                if (response.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(response.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, response.Message);
                    return result;
                }
            }
        }
        [HttpPost]
        [ActionName("GetOnhand")]
        public IHttpActionResult GetOnhand(GetOnhandRequest request)
        {
            InitDIRoot();
            if (request.CustomerUID == Guid.Empty)
            {
                return this.BadRequest($"{nameof(request.CustomerUID)}:" + Resource.COMMON_DATA_NOT_FOUND);
            }
            if (request.WarehouseUID == Guid.Empty)
            {
                return this.BadRequest($"{nameof(request.WarehouseUID)}:" + Resource.COMMON_DATA_NOT_FOUND);
            }
            if (request.Items == null)
            {
                return this.BadRequest($"{nameof(request.Items)}:" + Resource.COMMON_DATA_NOT_FOUND);
            }
            //if (string.IsNullOrEmpty(request.RefNo))
            //{
            //    return this.BadRequest($"{nameof(request.RefNo)}:" + Resource.COMMON_DATA_NOT_FOUND);
            //}
            var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
            _instance.TracingAgent.BeginTracing("", request);
            var response = _instance.GetOnhand(request);
            _instance.TracingAgent.EndTracing(response);
            if (response.Success)
            {
                var result = this.GetSuccessResult<dynamic>(response.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, response.Message);
                return result;
            }
        }
        [HttpGet]
        [ActionName("ClearProductCache")]
        public IHttpActionResult ClearProductCache()
        {
            InitDIRoot();

            var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
            var response = _instance.ClearProductCache();

            if (response.Success)
            {
                var result = this.GetSuccessResult<dynamic>(response.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, response.Message);
                return result;
            }
        }
        [HttpGet]
        [ActionName("ClearPackageCache")]
        public IHttpActionResult ClearPackageCache()
        {
            InitDIRoot();

            var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
            var response = _instance.ClearPackageCache();

            if (response.Success)
            {
                var result = this.GetSuccessResult<dynamic>(response.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, response.Message);
                return result;
            }
        }
        [HttpPost]
        [ActionName("CheckBolExist")]
        public IHttpActionResult CheckBolExist(IEnumerable<string> request)
        {
            InitDIRoot();

            //if (string.IsNullOrEmpty(request.RefNo))
            //{
            //    return this.BadRequest($"{nameof(request.RefNo)}:" + Resource.COMMON_DATA_NOT_FOUND);
            //}
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {
                _instance.TracingAgent.BeginTracing("", request);
                var response = _instance.CheckBolExist(request);
                _instance.TracingAgent.EndTracing(response);
                var result = this.GetSuccessResult<ICheckBolExistResponse>(response.Content);
                return this.Json<APIResult<ICheckBolExistResponse>>(result);
            }
        }
        [HttpGet]
        [ActionName("GetAllCurrentRequest")]
        public IHttpActionResult GetAllCurrentRequest()
        {
            InitDIRoot();
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {
                var response = _instance.GetAllCurrentProeccingRequest();
                if (response.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(response.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, response.Message, response.Content);
                    return result;
                }
            }
        }
        [HttpGet]
        [ActionName("RemoveProcessingRequestStatus")]
        public IHttpActionResult RemoveProcessingRequestStatus(string actionkey, string requestKey)
        {
            InitDIRoot();
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {
                var response = _instance.RemoveProcessingRequestStatus(actionkey, requestKey);
                var result = this.GetSuccessResult<bool>(response.Content);
                return this.Json<APIResult<bool>>(result);
            }
        }
        [HttpGet]
        [ActionName("AddProcessingRequestStatus")]
        public IHttpActionResult AddProcessingRequestStatus(string actionkey, string requestKey)
        {
            InitDIRoot();
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {
                var response = _instance.AddProcessingRequestStatus(actionkey, requestKey);
                var result = this.GetSuccessResult<bool>(response.Content);
                return this.Json<APIResult<bool>>(result);
            }
        }
    }
}