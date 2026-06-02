using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickBathAddWorkModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid[] BulkPickUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid[] GroupUID { get; set; }
    }

}


