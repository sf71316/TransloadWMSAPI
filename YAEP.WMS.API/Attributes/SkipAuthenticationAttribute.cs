using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;

namespace YAEP.WMS.Controllers.Api.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public class SkipAuthenticationAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authenticationInfo = actionContext.Request.AuthenticateUseJwt();

            base.OnActionExecuting(actionContext);
        }

    }
}