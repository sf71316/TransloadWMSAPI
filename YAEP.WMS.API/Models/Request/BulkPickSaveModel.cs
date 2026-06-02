using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Description;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class BulkPickSaveModel
    {
        /// <summary>
        /// 
        /// </summary> 
        private Guid[] ManifestItemUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public Guid[] TicketInfoUID { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        private string CustomerPartyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string CustomerName { get; set; }
    }

}


