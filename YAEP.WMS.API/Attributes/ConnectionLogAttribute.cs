using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using YAEP.Interfaces;
using YAEP.WMS.Controllers.Api;
using YAEP.WMS.DI.Agent;
using Newtonsoft.Json;
using YAEP.WMS.Constant;
using YAEP.WMS.Language.Resources;
using System.Diagnostics;
using System.Threading.Tasks;
using YAEP.WMS.Tracing.Jaeger;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Code;
using OpenTelemetry.Context.Propagation;

namespace YAEP.WMS.Controllers.Api.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionLogAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        Activity activity;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var Propagator = new OpenTelemetry.Context.Propagation.TraceContextPropagator();
            DefaultAppSettings defaultAppSettings = new DefaultAppSettings();
            DefaultAppConfigure Config = new DefaultAppConfigure(defaultAppSettings);
            var helper = JaegerHelper.InitActivitySource(Config);
            var Auth = actionContext.Request.GetAuthenticationInfo();
            activity = helper.GetActivity(actionContext.ActionDescriptor.ActionName);
            var ctx = Propagators.DefaultTextMapPropagator.Extract(default, actionContext.Request.Headers, extract);
            activity.SetParentId(ctx.ActivityContext.TraceId, ctx.ActivityContext.SpanId, ctx.ActivityContext.TraceFlags);
            //#if (!DEBUG)
            if (Auth != null)
            {
                var key = Guid.NewGuid();
                actionContext.Request.setRequestKey(key);
                var logger = DIRoot.GetLogger();
                var url = actionContext.Request.RequestUri;
                var actionName = url.Segments[url.Segments.Length - 1];
                string message = "Receive http request <br>" +
                                 $"Http Method:{actionContext.Request.Method.ToString()} <br>" +
                                 $"Url:{url.AbsoluteUri} ";
                var requestData = Newtonsoft.Json.JsonConvert.SerializeObject(actionContext.ActionArguments);
                logger.Log(message, actionName, Auth.Account, "info", (int)YAEP.Constants.BelongToTypes.WMSCommon
                    , jsonBefore: requestData, belongToUID: key.ToString(), application: WMSAPIParameters.CONNECT_LOG_NAME);


            }
            //#endif
            base.OnActionExecuting(actionContext);

        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            activity.Dispose();
            var Auth = actionExecutedContext.Request.GetAuthenticationInfo();
            //#if (!DEBUG)
            if (Auth != null)
            {
                var _enablelocallogging = false;
                if (actionExecutedContext.Request.Headers.Contains("EnableLocalLogging"))
                {
                    _enablelocallogging = actionExecutedContext.Request.Headers
                                            .GetValues("EnableLocalLogging").FirstOrDefault() == bool.TrueString;
                }
                var key = actionExecutedContext.Request.GetRequestKey();
                var logger = DIRoot.GetLogger();
                var url = actionExecutedContext.Request.RequestUri;
                var actionName = url.Segments[url.Segments.Length - 1];
                var response = actionExecutedContext.Response;
                var responseString = "";
                var Method = "";
                var StatusCode = "";
                try
                {

                    if (response != null)
                    {
                        //不記log
                        //Task.Run(async () =>
                        //{
                        //    responseString = actionExecutedContext.Response.StatusCode.ToString();
                        //    Method = actionExecutedContext.Request.Method.ToString();
                        //    StatusCode = actionExecutedContext.Response.StatusCode.ToString();
                        //    string message = "Response http request <br>" +
                        //                 $"Http Method:{Method} <br>" +
                        //                 $"Url:{url.AbsoluteUri} <br>" +
                        //                 $"Http Response:{StatusCode}";

                        //    string requestData = await actionExecutedContext.Response.Content.ReadAsStringAsync();

                        //    logger.Log(message, actionName, Auth.Account, logger.InfoString, (int)YAEP.Constants.BelongToTypes.WMSCommon
                        //        , jsonAfter: requestData, belongToUID: key.ToString(), application: WMSAPIParameters.CONNECT_LOG_NAME
                        //);

                        //});
                    }
                    else
                    {
                        //Method = actionExecutedContext.Request.Method.ToString();
                        //string message = "Response http request <br>" +
                        //             $"Http Method:{Method} <br>" +
                        //             $"Url:{url.AbsoluteUri} <br>" +
                        //             $"Http Response:500";
                        //string requestData = $"Expcetion:{actionExecutedContext.Exception.Message} StackTrace:{actionExecutedContext.Exception.StackTrace}";
                        //logger.Log(message, actionName, Auth.Account, logger.InfoString, (int)YAEP.Constants.BelongToTypes.WMSCommon
                        //    , jsonAfter: requestData, belongToUID: key.ToString(), application: WMSAPIParameters.CONNECT_LOG_NAME);
                    }
                }
                catch (Exception ex)
                {
                    //Method = "Unknown";
                    //string message = "Response http request <br>" +
                    //             $"Http Method:{Method} <br>" +
                    //             $"Url:{url.AbsoluteUri} <br>" +
                    //             $"Http Response:500";
                    //string requestData = $"Expcetion:{ex.Message} StackTrace:{ex.StackTrace}";
                    //logger.Log(message, actionName, Auth.Account, logger.InfoString, (int)YAEP.Constants.BelongToTypes.WMSCommon
                    //    , jsonAfter: requestData, belongToUID: key.ToString(), application: WMSAPIParameters.CONNECT_LOG_NAME
                    //    );
                }

            }
            //#endif
            base.OnActionExecuted(actionExecutedContext);

        }
        private IEnumerable<string> extract(System.Net.Http.Headers.HttpRequestHeaders arg1, string arg2)
        {
            IEnumerable<string> values;
            arg1.TryGetValues(arg2, out values);
            return values;
        }
    }
}