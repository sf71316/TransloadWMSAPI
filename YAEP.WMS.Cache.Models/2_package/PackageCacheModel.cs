using System;
using System.Collections.Generic;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageCacheModel : AbstractCacheModel, IPackageViewModel
    {
        #region Extender Properties

        /// <summary>
        /// 
        /// </summary>
        public string VersionId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UomName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ParentUomName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ParentUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool CanEdit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool CanDelete { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AssignedCount { get; set; }

        #endregion

        #region IPackageModel

        /// <summary>
        /// 
        /// </summary>
        public Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ParentUID { get; set; }
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
        public int Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid VersionUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid UOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal GrossWeight { get; set; }
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
        public decimal? LengthCM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? GrossWeightKG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ImageUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SCC14 { get; set; }
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
        public string PUOM { get; set; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.VersionId} / {this.ParentUomName} / {this.UomName}";
        }


    }
}