using System;
using System.Collections.Generic;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.Api.Code.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductCacheModel : IItemModel
    {
        /// <summary>
        /// 是否為虛擬 Item
        /// </summary>
        public bool IsVirtualItem { get => (this.Type == 100); }
        /// <summary>
        /// 
        /// </summary>
        public int CombinedQuantity { get; set; }

        #region Extender Properties

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
        public Guid CategoryUID { get; set; }
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
        public Guid ImageUID { get; set; }
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

        /// <summary>
        /// 實際 Item ID
        /// </summary>
        public string ActualProduct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BoxQuantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PUOM { get; set; }

        #endregion

        #region IItemModel Members
        /// <summary>
        /// 
        /// </summary>
        public Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid GroupUID { get; set; }
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
        public int Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ModifiedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        #endregion
    }
}