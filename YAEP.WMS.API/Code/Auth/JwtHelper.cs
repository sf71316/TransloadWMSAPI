using System;
using System.Collections.Generic;
using System.Text;
using Jose;

namespace YAEP.WMS.Api.Code
{
    /// <summary>
    /// 
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encode(object data)
        {
            var payload = new Dictionary<string, object>() {
                { "sub", "yaep.wms.com" },
                { "exp", DateTime.UtcNow.AddHours(1).Ticks },
                { "data", data }
            };

            var secretKey = GetSecretKey();

            string token = JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
            return token;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Dictionary<string, object> Decode(string token)
        {
            var secretKey = GetSecretKey();

            try
            {
                var jwtObject = JWT.Decode<Dictionary<string, object>>(token, secretKey, JwsAlgorithm.HS256);
                return jwtObject;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        private static byte[] GetSecretKey()
        {
            string key = "yaep_wms_test_jose_jwt";

            return Encoding.UTF8.GetBytes(key);
        }
    }
}