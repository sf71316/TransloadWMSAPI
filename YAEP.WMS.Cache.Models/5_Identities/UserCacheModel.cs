using System;
using YAEP.Identities.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UserCacheModel : AbstractCacheModel, IUserModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Skype { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string CellPhone { get; set; }
        public string Theme { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public Guid DefaultRoleUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.ID} / {this.Account} / {this.Type} ";
        }


    }
}