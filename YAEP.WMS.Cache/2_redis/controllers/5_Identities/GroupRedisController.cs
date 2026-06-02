using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class GroupRedisController : AbstractDefaultConnectSettingController<GroupCacheModel>
    {
        public GroupRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(GroupCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(GroupCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(GroupCacheModel.ParentUID), o => o.ParentUID);
            this.AppendIndex(nameof(GroupCacheModel.Abbrev), o => o.Abbrev);
            this.AppendIndex(nameof(GroupCacheModel.Type), o => o.Type);
        }

    }

}
