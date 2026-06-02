using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
     public interface IPBSCPackagingModel
    {
        /// <summary>
        /// 
        /// </summary>
         String LINE_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String PROD_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String MEASURE { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String PARENT_MEASURE { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? QTY { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? HEIGHT { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? WIDTH { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? DEPTH { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? WEIGHT { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? GIRTH { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String SCC14 { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String ADDBY { get; set; }

        /// <summary>
        /// 
        /// </summary>
         DateTime? ADDDATIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String CreatedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
         DateTime? CreatedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String ModifiedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
         DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Int32? LevelIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? MetricHeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? MetricWidth { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? MetricDepth { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? MetricWeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String UPC { get; set; }

        /// <summary>
        /// 
        /// </summary>
         String PUOM { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? ProductLengthInch { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? ProductWidthInch { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? ProductHeightInch { get; set; }

        /// <summary>
        /// 
        /// </summary>
         Decimal? ProductGrossWeight { get; set; }
    }
}
