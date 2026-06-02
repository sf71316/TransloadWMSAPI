using System.Collections.Generic;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class CustomerRedisController : AbstractDefaultConnectSettingController<CustomerCacheModel>
    {
        public CustomerRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(CustomerCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(CustomerCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(CustomerCacheModel.Status), o => o.Status);
            this.AppendIndex(nameof(CustomerCacheModel.GroupUID), o => o.GroupUID);
            this.AppendIndex(nameof(CustomerCacheModel.Type), o => o.Type);
        }

 

    }

}
