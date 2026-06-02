using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickManifestResponseModel
    {
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
        public Guid ManifestUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ManifestNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid ManifestItemListUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ManifestItemListID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerPartyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; }
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
        public decimal EstQty { get; set; } = 0m;
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
        public string ShipVia { get; set; }
    }

}