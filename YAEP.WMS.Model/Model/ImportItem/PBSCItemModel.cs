using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class PBSCItemModel : IPBSCItemModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string CustomerID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CustomerUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid GroupUID { get; set; }
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string UPC { get; set; }
        public string EAN { get; set; }
        public bool IsBOM { get; set; }
        public string Description { get; set; }
        public decimal? LengthInch { get; set; }
        public decimal? WidthInch { get; set; }
        public decimal? HeightInch { get; set; }
        public decimal? LengthCM { get; set; }
        public decimal? WidthCM { get; set; }
        public decimal? HeightCM { get; set; }
        public decimal? NetWeightKG { get; set; }
        public decimal? NetWeightLB { get; set; }
        public decimal? GrossWeightKG { get; set; }
        public decimal? GrossWeightLB { get; set; }
        public string StockUOM { get; set; }
        public string PurchaseUOM { get; set; }
        public string SellingUOM { get; set; }
        public string ShipUOM { get; set; }
        public string ActualProduct { get; set; }
        public bool Is_VirtualItem { get; set; }
        public string PUOM { get; set; }
        public int BoxQuantity { get; set; }

        public override string ToString()
        {
            return $"{this.CategoryName} - {this.ID}";
        }
    }
}
