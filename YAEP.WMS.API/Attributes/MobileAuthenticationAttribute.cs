using System;
using System.Collections.Generic;
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
    public class MobileAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        /// <summary>
        /// Controller 進入前會先驗證登入權限
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
                preparingResult.Message = "System is in preparation, please try to login later.";
                preparingResult.Code = -100;
                actionContext.Response = actionContext.Request.CreateResponse<WMS.Api.Models.APIResult<string>>(HttpStatusCode.ServiceUnavailable, preparingResult);
                return;
            }

            var result = actionContext.Request.GetMobileAuthenticationUseJwt();
    
            if ((result?.IsComplete ?? false))
            {
                base.OnActionExecuting(actionContext); 
            }
            else
            {
                switch (result.Code)
                {
                    case (int)HttpStatusCode.Conflict:
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Conflict, "Conflict");
                        break;
                    case (int)HttpStatusCode.Unauthorized: 
                    default:
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized");
                        break;
                }
       
            }
        }

    }
}