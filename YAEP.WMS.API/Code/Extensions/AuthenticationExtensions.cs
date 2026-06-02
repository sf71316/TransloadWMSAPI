using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using YAEP.Interfaces;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using System.IO;

namespace YAEP.WMS.Controllers.Api
{
    internal static class AuthenticationExtensions
    {
        public const string AUTH_PROPERTY_KEY = "AuthenticationInfo";
        private static object LOCK_Obj = new object();
        public static APIResult<IAuthenticationInfo> GetMobileAuthenticationUseJwt(this HttpRequestMessage request)
        {
            var result = new APIResult<IAuthenticationInfo>();
            result.Code = (int)HttpStatusCode.Unauthorized;

            string token = request.GetToken();

            string identification = "";
            try
            {
                var dictionary = JwtHelper.Decode(token);
                if (dictionary["data"] != null)
                {
                    identification = dictionary["data"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                result.Code = (int)HttpStatusCode.BadRequest;
                result.Message = $"incorrect format. ({ex.Message})";
                return result;
            }

            if (String.IsNullOrWhiteSpace(identification))
            {
                result.Code = (int)HttpStatusCode.BadRequest;
                result.Message = $"incorrect format.";
                return result;
            }

            try
            {

                var info = AuthenticationExtensions.ValidToken(identification);

                if (info == null)
                {
                    if (AuthenticationExtensions.CheckIsConflict(identification))
                    {
                        result.Code = (int)HttpStatusCode.Conflict;
                    }
                    else if (AuthenticationExtensions.CheckIsKickout(identification))
                    {
                        result.Code = (int)HttpStatusCode.Conflict;
                    }
                    //else if (AuthenticationExtensions.CheckIsTimeout(identification))
                    //{
                    //    return this.Content(System.Net.HttpStatusCode.Unauthorized, false);
                    //} 
                }
                else
                {
                    info.Token = token;

                    result.IsComplete = true;
                    result.Data = info;
                    result.Code = (int)HttpStatusCode.OK;
                    request.setAuthInfoProperty(info);
                }
            }
            catch (Exception ex)
            {
                result.Code = (int)HttpStatusCode.InternalServerError;
                result.Message = ex.Message;
            }

            return result;
        }

        public static IAuthenticationInfo AuthenticateUseJwt(this HttpRequestMessage request)
        {
            string token = request.GetToken();

            if (String.IsNullOrWhiteSpace((token ?? String.Empty).Trim()))
            {
                return null;
            }

            var dictionary = JwtHelper.Decode(token);

            if (dictionary?["data"] != null)
            {
                string identification = dictionary["data"]?.ToString();

                if (!String.IsNullOrWhiteSpace((identification ?? String.Empty).Trim()))
                {
                    return request.CheckToken(identification);
                }
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetToken(this HttpRequestMessage request)
        {
            string token = "";
            if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
            {
                var key = request.GetQueryNameValuePairs().FirstOrDefault(p => p.Key.ToLower() == "accessKey".ToLower());
                if (!string.IsNullOrEmpty(key.Value))
                {
                    token = key.Value;
                }

            }
            else
            {
                token = request.Headers.Authorization.Parameter;
            }
            if (String.IsNullOrWhiteSpace((token ?? String.Empty).Trim()) || (token?.Equals("null", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return null;
            }

            return token;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IAuthenticationInfo CheckToken(this HttpRequestMessage request, string token)
        {
            var result = ValidToken(token);

            if (result != null)
            {
                result.Token = request.GetToken();
                request.setAuthInfoProperty(result);
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IAuthenticationInfo GetAuthenticationInfo(this HttpRequestMessage request)
        {
            object value;

            if (request.Properties.TryGetValue(AUTH_PROPERTY_KEY, out value))
            {
                var authenticationInfo = value as IAuthenticationInfo;

                return authenticationInfo;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IAuthenticationInfo ValidToken(string token)
        {
            var ssoFactory = FactoryUtils.GetSsoFactory();
            var authenticator = ssoFactory.CreateAuthenticator();
            var result = authenticator.CheckIdentification(token);
            //if (!result.Success)
            writelog(token, result);
            return result?.Content;
        }

        public static void writelog(string token, IActionResult<IAuthenticationInfo> result)
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
                    $"[{DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")}] token {token} result:{result.Success} Message:{result.Message} Exception:{result.InnerException?.Message} " + Environment.NewLine);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CheckIsConflict(string token)
        {
            var ssoFactory = FactoryUtils.GetSsoFactory();
            var authenticator = ssoFactory.CreateAuthenticator();
            var result = authenticator.CheckIsConflict(token);
            return result?.Content ?? false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CheckIsTimeout(string token)
        {
            var ssoFactory = FactoryUtils.GetSsoFactory();
            var authenticator = ssoFactory.CreateAuthenticator();
            var result = authenticator.CheckIsTimeout(token);
            return result?.Content ?? false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CheckIsKickout(string token)
        {
            var ssoFactory = FactoryUtils.GetSsoFactory();
            var authenticator = ssoFactory.CreateAuthenticator();
            var result = authenticator.CheckIsKickout(token);
            return result?.Content ?? false;
        }

        private static void setAuthInfoProperty(this HttpRequestMessage request, IAuthenticationInfo info)
        {
            var ainfo = request.GetAuthenticationInfo();
            if (ainfo == null)
            {
                request.Properties.Add(AUTH_PROPERTY_KEY, info);
            }
        }

    }
}
