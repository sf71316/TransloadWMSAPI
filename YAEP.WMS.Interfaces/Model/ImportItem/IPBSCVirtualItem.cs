using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
     public interface IPBSCVirtualItem
    {
         Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
         Guid CustomerUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
         Guid GroupUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string CategoryName { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string UPC { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string EAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
         decimal? NetWeight { get; set; }
         decimal? GrossWeight { get; set; }
         decimal? LengthMetric { get; set; }
         decimal? WidthMetric { get; set; }
         decimal? HeightMetric { get; set; }
         decimal? LengthInch { get; set; }
         decimal? WidthInch { get; set; }
         decimal? HeightInch { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string PUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string ActualProduct { get; set; }
         string SourcePackage { get; set; }
        /// <summary>
        /// 
        /// </summary>
         int BoxQuantity { get; set; }
    }
}
