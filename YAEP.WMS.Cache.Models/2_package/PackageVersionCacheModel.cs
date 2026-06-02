using System;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageVersionCacheModel : AbstractCacheModel, IPackageVersionModel
    {
        public Guid UID { get; set; }
        public Guid ItemUID { get; set; }
        public string VersionId { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long SerialNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.VersionId}";
        }


    }
}