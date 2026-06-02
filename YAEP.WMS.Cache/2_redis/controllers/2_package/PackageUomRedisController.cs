using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class PackageUomRedisController : AbstractDefaultConnectSettingController<PackageUomCacheModel>
    {
        public PackageUomRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(PackageUomCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(PackageUomCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(PackageUomCacheModel.Type), o => o.Type);
            this.AppendIndex(nameof(PackageUomCacheModel.Status), o => o.Status);
            this.AppendIndex(nameof(PackageUomCacheModel.Type), o => o.Type);

        }
        public void Replace(Guid uomUID, PackageUomCacheModel newData)
        {
            this.Delete(uomUID);
            if (newData != null)
            {
                this.Create(newData);
            }
        }

        public void Replace(IEnumerable<Guid> keys, IEnumerable<PackageUomCacheModel> newData)
        {
            this.Remove(keys);
            if ((newData?.Count() ?? 0) > 0)
            {
                this.Create(newData);
            }
        }
        public void Remove(IEnumerable<Guid> keys)
        {
            foreach (var key in keys)
            {
                this.Delete(key);
            }
        }

    }

}
