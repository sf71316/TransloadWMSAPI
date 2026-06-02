using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickInfoResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid BulkPickUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BulkPickID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PartyName { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? EstQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ActQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ShtQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? SavQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid TicketInfoUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TicketInfoRelationID { get; set; }         
    }

}