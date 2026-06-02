using System;
using System.Collections.Generic;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class PackageVersionRedisController : AbstractDefaultConnectSettingController<PackageVersionCacheModel>
    {
        public PackageVersionRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(PackageVersionCacheModel.VersionId), o => o.VersionId);
            this.AppendIndex(nameof(PackageVersionCacheModel.ItemUID), o => o.ItemUID);
            this.AppendIndex(nameof(PackageVersionCacheModel.SerialNumber), o => o.SerialNumber);
            this.AppendIndex(nameof(PackageVersionCacheModel.Status), o => o.Status);

        }

        public void Replace(Guid versionUID, PackageVersionCacheModel newData)
        {
            this.Delete(versionUID);
            if (newData != null)
            {
                this.Create(newData);
            }
        }
        public void ReplaceByItem(Guid itemUID, IEnumerable<PackageVersionCacheModel> newData)
        {
            this.Remove(itemUID);
            if (newData != null)
            {
                this.Create(newData);
            }
        }
        public void Remove(Guid itemUID)
        {
            var found = this.Retrieve("ItemUID", u => (u ?? String.Empty).ToString().Equals(itemUID.ToString(), StringComparison.OrdinalIgnoreCase));
            foreach (var version in found)
            {
                this.Delete(version.UID);
            }
        }

    }

}
