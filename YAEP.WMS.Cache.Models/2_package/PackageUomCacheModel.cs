using System;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageUomCacheModel : AbstractCacheModel, IPackageUomModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.ID} / {this.Name}";
        }


    }
}