using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class PackageRedisController : AbstractDefaultConnectSettingController<PackageCacheModel>
    {
        public PackageRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(PackageCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(PackageCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(PackageCacheModel.Type), o => o.Type);
            this.AppendIndex(nameof(PackageCacheModel.Status), o => o.Status);
            this.AppendIndex(nameof(PackageCacheModel.ItemUID), o => o.ItemUID);
            this.AppendIndex(nameof(PackageCacheModel.VersionUID), o => o.VersionUID);
            this.AppendIndex(nameof(PackageCacheModel.VersionId), o => o.VersionId);
            this.AppendIndex(nameof(PackageCacheModel.UOM), o => o.UOM);
            this.AppendIndex(nameof(PackageCacheModel.UomName), o => o.UomName);
            this.AppendIndex(nameof(PackageCacheModel.ParentUID), o => o.ParentUID);
            this.AppendIndex(nameof(PackageCacheModel.ParentUOM), o => o.ParentUOM);
            this.AppendIndex(nameof(PackageCacheModel.ParentUomName), o => o.ParentUomName);
            this.AppendIndex(nameof(PackageCacheModel.PUOM), o => o.PUOM);

        }

        public void Replace(Guid packageUID, PackageCacheModel newData)
        {
            this.Delete(packageUID);
            if (newData != null)
            {
                this.Create(newData);
            }
        }
        public void Replace(IEnumerable<Guid> packageUIDs, IEnumerable<PackageCacheModel> newData)
        {
            this.Remove(packageUIDs);
            if ((newData?.Count() ?? 0) > 0)
            {
                this.Create(newData);
            }
        }
        public void Remove(IEnumerable<Guid> packageUIDs)
        {
            foreach (var packageUID in packageUIDs)
            {
                this.Delete(packageUID);
            }
        }

    }

}
