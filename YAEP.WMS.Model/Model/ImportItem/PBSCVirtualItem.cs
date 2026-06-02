using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class PBSCVirtualItem : IPBSCVirtualItem
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid CustomerUID { get; set; }
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
        public string CategoryName { get; set; }
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
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? LengthMetric { get; set; }
        public decimal? WidthMetric { get; set; }
        public decimal? HeightMetric { get; set; }
        public decimal? LengthInch { get; set; }
        public decimal? WidthInch { get; set; }
        public decimal? HeightInch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ActualProduct { get; set; }
        public string SourcePackage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BoxQuantity { get; set; }

        public override string ToString()
        {
            return $"{this.CategoryName} - {this.ID}";
        }
    }

}
