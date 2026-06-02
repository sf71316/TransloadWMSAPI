using System;
using System.Net.Http;
using System.Web;
using System.Web.Http.Description;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.Utilities;
using YAEP.SSO.DI;
using YAEP.Common.DI;
using YAEP.SSO.Interfaces.Models;
using YAEP.SSO.Interfaces;
using YAEP.Interfaces;
using YAEP.SSO.Constants;
using YAEP.WMS.API.Models.Request;
using YAEP.Common.Models;
using YAEP.Identities.DI;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// 
    /// </summary>
    [SkipAuthentication]
    [ApiExplorerSettings(IgnoreApi = true)]
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [RoutePrefix("api/Auth")]
    public class AuthController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public AuthController()
        {
            this._DeviceFactory = new Lazy<DeviceFactory>(() => FactoryUtils.GetDeviceFactory(base.GetAuthenticationInfo()));
            this._SsoFactory = new Lazy<Factory>(() => FactoryUtils.GetSsoFactory());
        }


        /// <summary>
        /// Test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Test()
        {
            return $"{this.GetType().Name.Replace("Controller", " - ")}{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AppLogin([FromBody]AppLoginRequestModel model)
        {
            if (model == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (String.IsNullOrWhiteSpace((model.Account ?? String.Empty).Trim()))
            {
                return base.GetFailureResult(-1, "incorrect parameters. (Account)");
            }
            if (String.IsNullOrWhiteSpace((model.Password ?? String.Empty).Trim()))
            {
                return base.GetFailureResult(-1, "incorrect parameters. (Password)");
            }

            var ssoFactory = this.GetSsoFactory();
            var authenticator = ssoFactory.CreateAuthenticator();

            // 2020-08-13 等待 Cache 初始化完成
            if (!DrKnowAll.IsInitialized)
            {
                authenticator.SignOut(model.Account);
                var preparingResult = base.GetSuccessResult();
                preparingResult.IsComplete = false;
                preparingResult.Message = _SYSTEM_PREPARING_MESSAGE;
                preparingResult.Code = -100;
                return base.Json(preparingResult);
            }

            string ip = this.GetClientIP();

            var ticket = ssoFactory.CreateSignInTicket();
            ticket.Account = model.Account;
            ticket.Password = model.Password;
            ticket.IP = String.IsNullOrWhiteSpace((model.ClientIP ?? String.Empty).Trim()) ? ip : model.ClientIP.Trim();

            ticket.Device = DeviceTypes.MobileApps;
            ticket.ApplicationName = model.ApplicationName ?? "Totalsolution App";

            var result = authenticator.SignIn(new SignInValidator(), ticket);

            if (result.Success)
            {
                string token = JwtHelper.Encode(result.Content.Identification);
                var apiResult = base.GetSuccessResult(token);
                return base.Json(apiResult);
            }
            else
            {
                var r = base.GetSuccessResult();
                r.IsComplete = false;
                r.Message = "Incorrect account or password.";
                r.Code = -1;
                return base.Json(r);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult AppLogout(string deviceId)
        {
            //if (!String.IsNullOrWhiteSpace((deviceId ?? String.Empty).Trim()))
            //{
            //    this.setDeviceInactive(deviceId);
            //}

            var authInfo = base.GetAuthenticationInfo();
            if (authInfo == null)
            {
                return base.GetFailureResult(401, "Unauthorized");
            }

            var ssoFactory = this.GetSsoFactory();
            var authenticator = ssoFactory.CreateAuthenticator();
            var result = authenticator.SignOutByIdentification(authInfo.Identification);

            if (result.Success)
            {
                var apiResult = base.GetSuccessResult(true);
                return base.Json(apiResult);
            }
            else
            {
                return base.GetFailureResult(-1, result.Message);
            }
        }

        private bool setDeviceInactive(string deviceId)
        {

            try
            {
                var factory = this.GetDeviceFactory();
                //factory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
                var manager = factory.CreateDeviceInfoManager();

                manager.SetInactive(deviceId);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddDeviceInfo([FromBody]DeviceInfoModel model)
        {
            if (model == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            var authInfo = base.GetAuthenticationInfo();
            if (authInfo == null)
            {
                return this.Content(System.Net.HttpStatusCode.Unauthorized, false);
            }

            try
            {
                var factory = this.GetDeviceFactory();
                var manager = factory.CreateDeviceInfoManager();

                model.UID = Guid.NewGuid();
                model.ModifiedOn = DateTime.UtcNow;
                var result = manager.Create(model);
                var updateInfoResult = manager.UpdateDeviceInfo(model);
                if (result.Success && updateInfoResult.Success)
                {
                    var apiResult = base.GetSuccessResult(model);
                    return base.Json(apiResult);
                }
            }
            catch (Exception ex)
            {
                return this.GetFailureResult(-1, ex.Message);
            }

            return this.GetFailureResult(-1, "Fail to add");
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetDeviceInfo([FromUri]string deviceId)
        {
            if (String.IsNullOrWhiteSpace(deviceId))
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            var authInfo = base.GetAuthenticationInfo();
            if (authInfo == null)
            {
                return this.Content(System.Net.HttpStatusCode.Unauthorized, false);
            }

            try
            {
                var factory = this.GetDeviceFactory();
                var manager = factory.CreateDeviceInfoManager();
                var result = manager.GetLastestDeviceInfo(deviceId);
                if (result.Success)
                {
                    var apiResult = base.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
            }
            catch (Exception ex)
            {
                return this.GetFailureResult(-1, ex.Message);
            }

            return this.GetFailureResult(-1, "Fail to add");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Login([FromBody]LoginRequestModel model)
        {
            if (model == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (String.IsNullOrWhiteSpace((model.account ?? String.Empty).Trim()))
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_ACCOUNT);
            }
            if (String.IsNullOrWhiteSpace((model.password ?? String.Empty).Trim()))
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PASSWORD);
            }

            var ssoFactory = this.GetSsoFactory();
            var authenticator = ssoFactory.CreateAuthenticator();

            // 2020-08-13 等待 Cache 初始化完成
            if (!DrKnowAll.IsInitialized)
            {
                authenticator.SignOut(model.account);
                var preparingResult = base.GetSuccessResult();
                preparingResult.IsComplete = false;
                preparingResult.Message = _SYSTEM_PREPARING_MESSAGE;
                preparingResult.Code = -100;
                return base.Json(preparingResult);
            }

            string ip = this.GetClientIP();
            var ticket = ssoFactory.CreateSignInTicket();
            ticket.Account = model.account;
            ticket.Password = model.password;
            ticket.IP = String.IsNullOrWhiteSpace((model.clientIP ?? String.Empty).Trim()) ? ip : model.clientIP.Trim();

            ticket.Device = DeviceTypes.Web;
            ticket.ApplicationName = model.applicationName ?? "WMS Web";

            var result = authenticator.SignIn(new SignInValidator(), ticket);

            if (result.Success)
            {
                string token = JwtHelper.Encode(result.Content.Identification);
                var apiResult = base.GetSuccessResult(token);
                return base.Json(apiResult);
            }
            else
            {
                return base.GetFailureResult(-1, "Fail to login.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult CheckToken([FromUri]string token)
        {
            string identification = "";

            try
            {
                var dictionary = JwtHelper.Decode(token);
                if (dictionary["data"] != null)
                {
                    identification = dictionary["data"]?.ToString();
                }
            }
            catch
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_FORMAT);
            }

            if (String.IsNullOrWhiteSpace(identification))
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_FORMAT);
            }

            // 2020-08-13 等待 Cache 初始化完成
            if (!DrKnowAll.IsInitialized)
            {
                var ssoFactory = this.GetSsoFactory();
                var authenticator = ssoFactory.CreateAuthenticator();
                authenticator.SignOutByIdentification(identification);
                return this.Content(System.Net.HttpStatusCode.Unauthorized, false);
            }

            try
            {
                var info = AuthenticationExtensions.ValidToken(identification);

                if (info == null)
                {
                    if (AuthenticationExtensions.CheckIsConflict(identification))
                    {
                        return this.Content(System.Net.HttpStatusCode.Conflict, false);
                    }
                    else if (AuthenticationExtensions.CheckIsKickout(identification))
                    {
                        return this.Content(System.Net.HttpStatusCode.Conflict, false);
                    }

                    return this.Content(System.Net.HttpStatusCode.Unauthorized, false);
                }
                else
                {
                    return this.Content(System.Net.HttpStatusCode.OK, true);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, $"Fail to check token. {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identification"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateToken([FromUri]string identification)
        {
            if (String.IsNullOrWhiteSpace(identification))
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                string token = JwtHelper.Encode(identification);
                var apiResult = base.GetSuccessResult(token);
                return base.Json(apiResult);
            }
            catch
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_FORMAT);
            }
        }

        private class SignInValidator : ISignInValidatable
        {
            private readonly IdentityFactory _IdentityFactory;
            private readonly Factory _SsoFactory;
            public SignInValidator()
            {
                this._IdentityFactory = FactoryUtils.GetIdentityFactory(new EmptyAuthenticationInfo());
                this._SsoFactory = FactoryUtils.GetSsoFactory();
            }

            public IActionResult<ISignInResult> Validate(string account, string password)
            {
                var resultContainer = new ActionResultContainer<ISignInResult>();
                // 2022-12-26 加密
                password = YAEP.Utilities.Utility.MD5Encrypt(password);

                var factory = this._IdentityFactory;
                var userManager = factory.CreateUserManager();
                var result = userManager.GetUser(account, password);
                if (result.Success)
                {
                    var user = result.Content;
                    resultContainer.Success = true;
                    resultContainer.Content = this._SsoFactory.CreateSignInResult();
                    resultContainer.Content.Account = user.Account;
                    resultContainer.Content.Name = String.Format("{0} {1}", user.FirstName, user.LastName);
                    resultContainer.Content.BelongTo = user.UID;
                }
                else
                {
                    resultContainer.Message = "The account or password is incorrect .";
                }

                return resultContainer;
            }

            private class EmptyAuthenticationInfo : IAuthenticationInfo
            {
                public Guid UID { get; set; }
                public string Account { get; set; } = "Empty";
                public string Identification { get; set; }
                public DateTime LoginedTime { get; set; } = DateTime.UtcNow;
                public string MemberName { get; set; }
                public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddDays(1);
                public string Token { get; set; }
            }
        }

        private string GetClientIP(HttpRequestMessage request = null)
        {
            return HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
        }

        #region Factories

        private readonly Lazy<Factory> _SsoFactory;
        private readonly Lazy<DeviceFactory> _DeviceFactory;

        private Factory GetSsoFactory()
        {
            return this._SsoFactory.Value;
        }
        private DeviceFactory GetDeviceFactory()
        {
            return this._DeviceFactory.Value;
        }

        #endregion


        private const string _SYSTEM_PREPARING_MESSAGE = "System is in preparation, please try to login later.";

    }
}
