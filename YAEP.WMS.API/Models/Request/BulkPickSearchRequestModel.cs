using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickSearchRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string BulkPickNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerPartyName { get; set; }        
        /// <summary>
        /// 
        /// </summary>
        public int[] Status { get; set; } = null;
    }

}


