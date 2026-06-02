using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class RollbackTicketRequest : IRollbackTicketRequest
    {
        /// <summary>
        /// Bol Ref UID (Ship via UID)
        /// </summary>
        public Guid[] BolRefUID { get; set; }
        /// <summary>
        /// Syspon
        /// </summary>
        public string RefNo { get; set; }
        public string RequestBy { get; set; }
    }
}