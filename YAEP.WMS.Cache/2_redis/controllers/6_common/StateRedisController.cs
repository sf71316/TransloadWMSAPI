using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class StateRedisController : AbstractDefaultConnectSettingController<StateCacheModel>
    {
        public StateRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(StateCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(StateCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(StateCacheModel.Country), o => o.Country); 
        }

    }

}
