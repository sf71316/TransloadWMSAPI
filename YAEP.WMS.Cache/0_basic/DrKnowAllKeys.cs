using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.Cache
{
    /// <summary>
    /// 
    /// </summary>
    public enum DrKnowAllKeys
    {
        /// <summary>
        /// 
        /// </summary>
        ALL = 0,
        /// <summary>
        /// 
        /// </summary>
        Group = 10,
        /// <summary>
        /// 
        /// </summary>
        User = 20,
        /// <summary>
        /// 
        /// </summary>
        Role = 30,
        /// <summary>
        /// 
        /// </summary>
        Resource = 40,
        /// <summary>
        /// 
        /// </summary>
        Country = 100,
        /// <summary>
        /// 
        /// </summary>
        State = 200,
        /// <summary>
        /// 
        /// </summary>
        City = 300,
        /// <summary>
        /// 
        /// </summary>
        Zip = 400,
        /// <summary>
        /// 
        /// </summary>
        Customer = 500,
        /// <summary>
        /// UOM
        /// </summary>
        PackageUom = 600,
        /// <summary>
        /// Package
        /// </summary>
        PackageVersion = 610,
        /// <summary>
        /// Package
        /// </summary>
        Package = 620,
        /// <summary>
        /// 
        /// </summary>
        Product = 1000,
        /// <summary>
        /// 
        /// </summary>
        ProductCategory = 1010,
        /// <summary>
        /// 
        /// </summary>
        ProductCategoryRelation = 1020,
        /// <summary>
        /// 
        /// </summary>
        Warehouse = 1100,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum DrKnowLoadingStatus
    {
        /// <summary>
        /// 
        /// </summary>
        Pending = 100,
        /// <summary>
        /// 
        /// </summary>
        Loading = 200,
        /// <summary>
        /// 
        /// </summary>
        Loaded = 400,
    }
}