using System;
using YAEP.Identities.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupCacheModel : AbstractCacheModel, IGroupModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public Guid? ParentUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Abbrev { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public int? Sort { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid DefaultRoleUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.ID} / {this.Name} / {this.Type}";
        }


    }
}