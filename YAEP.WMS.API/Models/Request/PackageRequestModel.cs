using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid VersionUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid ItemUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid UOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ParentUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
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
        public decimal GrossWeight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ImageUID { get; set; } 
    }

    /// <summary>
    /// 
    /// </summary>
    public class PackageUpdateRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid UID { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public Guid UOM { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
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
        public decimal GrossWeight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ImageUID { get; set; }
    }

}