using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductNameResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ItemID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid ItemUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CustomerUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ProductNamePackageResponseModel> Package { get; set; } = new List<ProductNamePackageResponseModel>();
    }
    /// <summary>
    /// 
    /// </summary>
    public class ProductNamePackageResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string VersionID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PackageName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid PackageUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid ItemUID { get; set; }
    }
}