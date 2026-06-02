using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
    public partial class PBSCPackagingModel : IPBSCPackagingModel
    {
        /// <summary>
        /// 
        /// </summary>
        public String LINE_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String PROD_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String MEASURE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String PARENT_MEASURE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? QTY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? HEIGHT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? WIDTH { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? DEPTH { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? WEIGHT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? GIRTH { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String SCC14 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String ADDBY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADDDATIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String CreatedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String ModifiedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int32? LevelIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? MetricHeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? MetricWidth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? MetricDepth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? MetricWeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String UPC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String PUOM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? ProductLengthInch { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? ProductWidthInch { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? ProductHeightInch { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal? ProductGrossWeight { get; set; }
    }
}
