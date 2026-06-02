using System;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class UserRedisController : AbstractDefaultConnectSettingController<UserCacheModel>
    {
        public UserRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(UserCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(UserCacheModel.Account), o => o.Account);
            this.AppendIndex(nameof(UserCacheModel.Email), o => o.Email);
            this.AppendIndex(nameof(UserCacheModel.Skype), o => o.Skype);
            this.AppendIndex(nameof(UserCacheModel.FirstName), o => o.FirstName);
            this.AppendIndex(nameof(UserCacheModel.LastName), o => o.LastName);
            this.AppendIndex(nameof(UserCacheModel.Telephone), o => o.Telephone);
            this.AppendIndex(nameof(UserCacheModel.Fax), o => o.Fax);
            this.AppendIndex(nameof(UserCacheModel.CellPhone), o => o.CellPhone);
            this.AppendIndex(nameof(UserCacheModel.Theme), o => o.Theme);
            this.AppendIndex(nameof(UserCacheModel.Type), o => o.Type);
            this.AppendIndex(nameof(UserCacheModel.DefaultRoleUID), o => o.DefaultRoleUID);
        }

        public void Replace(Guid userUID, UserCacheModel newData)
        {
            this.Delete(userUID);
            if (newData != null)
            {
                this.Create(newData);
            }
        }

    }

}
