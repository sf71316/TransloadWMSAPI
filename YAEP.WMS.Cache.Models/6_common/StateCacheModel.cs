using System;
using YAEP.Common.Interfaces.Models; 

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class StateCacheModel : AbstractCacheModel, IStateModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Country { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Sort { get; set; }
        public int? Status { get; set; }
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
            return $"{this.ID} / {this.Name} / {this.Country}";
        }


    }
}