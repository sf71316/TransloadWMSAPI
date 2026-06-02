using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class VendorRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ext { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Fax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Zip { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public Guid GroupUID { get; set; }
    }

}