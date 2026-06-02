using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid BulkPickUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BulkPickNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TicketID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid TicketUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string StatusName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AssignedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? AssignedTime { get; set; }
    }

}