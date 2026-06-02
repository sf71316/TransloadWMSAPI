using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class UserResourcePermissionRedisController : AbstractDefaultConnectSettingController<UserResourcePermissionCacheModel>
    {
        public UserResourcePermissionRedisController() : base(o => getKey(o))
        {
            this.AppendIndex(nameof(UserResourcePermissionCacheModel.UserUID), o => o.UserUID);
            this.AppendIndex(nameof(UserResourcePermissionCacheModel.UserAccount), o => o.UserAccount);
            this.AppendIndex(nameof(UserResourcePermissionCacheModel.ResourceUID), o => o.ResourceUID);
            this.AppendIndex(nameof(UserResourcePermissionCacheModel.ResourceID), o => o.ResourceID);
            this.AppendIndex(nameof(UserResourcePermissionCacheModel.ResourceName), o => o.ResourceName);
            this.AppendIndex(nameof(UserResourcePermissionCacheModel.ResourceType), o => o.ResourceType);
            this.AppendIndex(nameof(UserResourcePermissionCacheModel.Permission), o => o.Permission);
        }

        public IEnumerable<UserResourcePermissionCacheModel> GetByUser(Guid userUID)
        {
            var found = this.Retrieve("UserUID", p => (p ?? String.Empty).ToString().Equals(userUID.ToString(), StringComparison.OrdinalIgnoreCase));
            return found;
        }

        public void Replace(Guid userUID, IEnumerable<UserResourcePermissionCacheModel> newData)
        {
            var found = GetByUser(userUID);

            foreach (var p in found)
            {
                this.Delete(getKey(p));
            }

            if ((newData?.Count() ?? 0) > 0)
            {
                this.Create(newData);
            }
        }

        private static string getKey(UserResourcePermissionCacheModel source)
        {
            return $"{source.UserUID}|{source.ResourceUID}";
        }
    }

}
