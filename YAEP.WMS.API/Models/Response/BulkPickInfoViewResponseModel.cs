using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickInfoViewResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid BulkPickUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ManifestNo { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public Guid TicketInfoUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TicketInfoID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RefNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int EstQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ActQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FromSlot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ToSlot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Shipvia { get; set; }

    }

}