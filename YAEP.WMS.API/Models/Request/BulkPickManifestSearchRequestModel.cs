using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickManifestSearchRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RefNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DateBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OptionText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OptionValue { get; set; }

    }

}


