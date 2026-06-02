using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class CityRedisController : AbstractDefaultConnectSettingController<CityCacheModel>
    {
        public CityRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(CityCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(CityCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(CityCacheModel.Country), o => o.Country);
            this.AppendIndex(nameof(CityCacheModel.State), o => o.State);
        }

    }

}
