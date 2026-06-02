using System; 

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string applicationName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string clientIP { get; set; }

    }
}