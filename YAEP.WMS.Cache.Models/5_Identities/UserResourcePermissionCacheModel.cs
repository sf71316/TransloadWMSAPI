using System;
using YAEP.Identities.Constants;
using YAEP.Identities.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UserResourcePermissionCacheModel : AbstractCacheModel, IUserResourcePermissionModel
    {
        public Guid UserUID { get; set; }
        public string UserAccount { get; set; }
        public Guid ResourceUID { get; set; }
        public string ResourceID { get; set; }
        public string ResourceName { get; set; }
        public ResourceTypes ResourceType { get; set; }
        public Permissions Permission { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.UserAccount} / {this.ResourceID} / {this.Permission} ";
        }


    }
}