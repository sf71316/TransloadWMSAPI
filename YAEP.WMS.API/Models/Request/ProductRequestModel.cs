using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductRequestModel
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
        public Guid CustomerUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CategoryUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid GroupUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsBOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ImageUID { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public decimal? LengthInch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? WidthInch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? HeightInch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? LengthCM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? WidthCM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? HeightCM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? NetWeightKG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? NetWeightLB { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? GrossWeightKG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? GrossWeightLB { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string StockUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PurchaseUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SellingUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ShipUOM { get; set; }
    } 
}