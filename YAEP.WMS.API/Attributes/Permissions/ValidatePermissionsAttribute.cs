using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using YAEP.Identities.Constants;
using YAEP.WMS.Api.Code;

namespace YAEP.WMS.Controllers.Api.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidatePermissionsAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        private readonly Permissions _Permissions;
        /// <summary>
        /// 
        /// </summary>
        public ValidatePermissionsAttribute(Permissions permissions = Permissions.None)
        {
            this._Permissions = permissions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authenticationInfo = actionContext.Request.AuthenticateUseJwt();

            if (authenticationInfo == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized");
            }
            else
            {
                // 預設值 則略過驗證
                if (this._Permissions == Permissions.None)
                {
                    base.OnActionExecuting(actionContext);
                }
                else
                {
                    // 權限驗證 
                    // 1. Controller: Enable
                    // 2 Action: [Parameter]
                    string controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
                    string actionName = actionContext.ActionDescriptor.ActionName;
                    var code = authenticationInfo.ActionEnablePermissions(controllerName: controllerName, actionName: actionName, permissions: this._Permissions);

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
        }

    }
}