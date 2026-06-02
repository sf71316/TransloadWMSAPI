using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductSearchRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ItemID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Guid> PHierarchy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Guid> CHierarchy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? CustomerUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid[] ItemUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? PageSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? PageNumber { get; set; } 
    }

}