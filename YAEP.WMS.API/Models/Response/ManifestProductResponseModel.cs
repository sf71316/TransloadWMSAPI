using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Core.Item.Constants;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class ManifestProductResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid ItemUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ImageUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ManifestProductPackageResponseModel> Packages { get; set; } = new List<ManifestProductPackageResponseModel>();
    }
    /// <summary>
    /// 
    /// </summary>
    public class ManifestProductPackageResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VersionId { get; set; }
        
    }
}