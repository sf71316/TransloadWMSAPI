using System;
using YAEP.Core.Party.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PartyCacheModel : AbstractCacheModel, IPartyModel
    {
        public Guid UID { get; set; }
        public Guid GroupUID { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string PhoneExtension { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
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