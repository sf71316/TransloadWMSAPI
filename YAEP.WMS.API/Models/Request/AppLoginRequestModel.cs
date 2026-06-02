using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class AppLoginRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ApplicationName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClientIP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OsType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OsVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceIDl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceModel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NotificationToken { get; set; }
    }
}