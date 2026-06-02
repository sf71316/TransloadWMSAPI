using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace YAEP.WMS.Controllers.Api.Attributes
{
    public class AllowCrossSiteAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
     
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        { 
            try
            {
                // 允許跨網域
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            catch
            {

            }

            base.OnActionExecuted(actionExecutedContext);
        }

    }
}