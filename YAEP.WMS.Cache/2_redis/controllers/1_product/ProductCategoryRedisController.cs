using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class ProductCategoryRedisController : AbstractDefaultConnectSettingController<ProductCategoryCacheModel>
    {
        public ProductCategoryRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(ProductCategoryCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(ProductCategoryCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(ProductCategoryCacheModel.Type), o => o.Type);
            this.AppendIndex(nameof(ProductCategoryCacheModel.Status), o => o.Status);
            this.AppendIndex(nameof(ProductCategoryCacheModel.GroupUID), o => o.GroupUID);
        }

        public void Replace(IEnumerable<Guid> keys, IEnumerable<ProductCategoryCacheModel> newData)
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
