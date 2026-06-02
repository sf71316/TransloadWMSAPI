using YAEP.Data.NoSql.Redis.ServiceStack;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class WarehouseRedisController : AbstractDefaultConnectSettingController<WarehouseCacheModel>
    {
        public WarehouseRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(WarehouseCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(WarehouseCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(WarehouseCacheModel.Status), o => o.Status);
            this.AppendIndex(nameof(WarehouseCacheModel.GroupUID), o => o.GroupUID);
        }

    }

}
