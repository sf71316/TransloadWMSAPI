using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageSyncRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid ItemUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PackageSyncRequestEntity Package { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class PackageSyncRequestEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// gross weight
        /// </summary>
        public decimal? GrossWeight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SCC14 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PackageSyncRequestEntity[] Children { get; set; }
    }
}