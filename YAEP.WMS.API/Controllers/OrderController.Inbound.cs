using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using YAEP.Identities.Constants;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Model;

namespace YAEP.WMS.API.Controllers
{
    public partial class OrderController : AbstractApiController
    {
        /// <summary>
        /// 新增入庫單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Receiving")]
        public IHttpActionResult Receiving(ReceivingRequest request)
        {

            InitDIRoot();

            if (request != null && request.WarehouseUID != Guid.Empty)
            {
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
                {
                    _instance.TracingAgent.BeginTracing("", request);
                    var rs = _instance.Receiving(request);
                    _instance.TracingAgent.EndTracing(rs);
                    if (rs.Success)
                    {
                        var result = this.GetSuccessResult<IReceivingResponse>(rs.Content);
                        return this.Json(result);
                    }
                    else
                    {
                        var result = this.GetFailureResult<IReceivingResponse>(-1, rs.Content.Message, rs.Content);
                        return result;
                    }
                }

            }
            else
            {
                return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
            }

        }
        /// <summary>
        /// 取消入庫單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CancelReceiving")]
        public IHttpActionResult CancelReceiving(CancelReceivingRequest request)
        {
            InitDIRoot();
            if (request != null && (request.CustomerUID != Guid.Empty || !string.IsNullOrEmpty(request.CustomerPartyName)) && request.WarehouseUID != Guid.Empty)
            {

                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
                {
                    _instance.TracingAgent.BeginTracing("", request);
                    var rs = _instance.CancelReceiving(request);
                    _instance.TracingAgent.EndTracing(rs);
                    if (rs.Success)
                    {
                        var result = this.GetSuccessResult<ICancelReceivingResponse>(rs.Content);
                        return this.Json(result);
                    }
                    else
                    {
                        var result = this.GetFailureResult<ICancelReceivingResponse>(-1, rs.Content.Message, rs.Content);
                        return result;
                    }
                }
            }
            else
            {
                return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ImportInboundData")]
        public IHttpActionResult ImportInboundData()
        {
            InitDIRoot();
            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var param = new ImportInboundParameter();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                param.File = HttpContext.Current.Request.Files[0];
                if (!string.IsNullOrEmpty(HttpContext.Current.Request["CustomerUID"]) &&
                    !string.IsNullOrEmpty(HttpContext.Current.Request["WarehouseUID"]))
                {
                    param.CustomerUID = new Guid(HttpContext.Current.Request["CustomerUID"]);
                    param.WarehouseUID = new Guid(HttpContext.Current.Request["WarehouseUID"]);
                    var rs = _instance.ImportInboundData(param);
                    var result = this.GetSuccessResult<bool>(rs.Content, rs.Message, rs.Success);

                    return this.Json(result);
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
}
