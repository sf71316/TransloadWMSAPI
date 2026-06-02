using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using YAEP.WMS.Api.Code;

namespace YAEP.WMS.Controllers.Api.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        static object LOCK_Obj = new object();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // 2020-08-13 等待 Cache 初始化完成
            if (!YAEP.WMS.Cache.Redis.DrKnowAll.IsInitialized)
            {
                var preparingResult = new WMS.Api.Models.APIResult<string>();
                preparingResult.IsComplete = false;
                preparingResult.ResponseTime = DateTime.Now;
                preparingResult.Message = "System is in preparation, please try it later.";
                preparingResult.Code = -100;
                actionContext.Response = actionContext.Request.CreateResponse<WMS.Api.Models.APIResult<string>>(HttpStatusCode.Unauthorized, preparingResult);
                //writelog(actionContext.Request.GetToken(), preparingResult.Message);
                return;
            }

            var authenticationInfo = actionContext.Request.AuthenticateUseJwt();

            if (authenticationInfo == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized");
            }
            else
            {
                // Controller 啟用 權限驗證   
                // string controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName; 
                //var code = authenticationInfo.ControllerEnable(controllerName: controllerName);
                //TODO 暫時拿掉驗証
                var code = 1;
                if (code == 1)
                {
                    base.OnActionExecuting(actionContext);
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized");
                }
            }
        }

        public void writelog(string token, string result)
        {
            lock (LOCK_Obj)
            {
                var logPath = "~/App_Data/log/";
                var phylogPath = System.Web.HttpContext.Current.Server.MapPath(logPath);
                if (!Directory.Exists(phylogPath))
                {
                    Directory.CreateDirectory(phylogPath);
                }
                var logfile = $"{System.DateTime.Now.ToString("yyyyMMdd")}.log";
                File.AppendAllText(Path.Combine(phylogPath, logfile),
                    $"[{DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")}] token {token} result:{result} " + Environment.NewLine);
            }
        }
    }
}