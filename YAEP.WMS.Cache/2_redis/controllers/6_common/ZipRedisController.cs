using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class ZipRedisController : AbstractDefaultConnectSettingController<ZipCacheModel>
    {
        public ZipRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(ZipCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(ZipCacheModel.City), o => o.City);
            this.AppendIndex(nameof(ZipCacheModel.State), o => o.State);
            this.AppendIndex(nameof(ZipCacheModel.Country), o => o.Country);
            this.AppendIndex(nameof(ZipCacheModel.Latitude), o => o.Latitude);
            this.AppendIndex(nameof(ZipCacheModel.Longtitude), o => o.Longtitude);
        }

    }

}
