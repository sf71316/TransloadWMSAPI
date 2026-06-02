using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace YAEP.WMS.Controllers.Api
{
    internal static class HttpRequestMessageExtensions
    {
        public const string REQUEST_PROPERTY_KEY = "RequestKey";
        public static Guid? GetRequestKey(this HttpRequestMessage request)
        {
            object value;

            if (request.Properties.TryGetValue(REQUEST_PROPERTY_KEY, out value))
            {
                return (Guid)value;
            }

            return null;
        }

        public static void setRequestKey(this HttpRequestMessage request, Guid guid)
        {
            request.Properties.Add(REQUEST_PROPERTY_KEY, guid);
        }
    }
}