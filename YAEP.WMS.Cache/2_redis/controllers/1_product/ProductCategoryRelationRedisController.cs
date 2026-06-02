using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class ProductCategoryRelationRedisController : AbstractDefaultConnectSettingController<ProductCategoryRelationCacheModel>
    {
        public ProductCategoryRelationRedisController() : base(o => getKey(o))
        {
            this.AppendIndex(nameof(ProductCategoryRelationCacheModel.ItemUID), o => o.ItemUID);
            this.AppendIndex(nameof(ProductCategoryRelationCacheModel.CategoryUID), o => o.CategoryUID);
        }

        public void Replace(Guid itemUID, IEnumerable<ProductCategoryRelationCacheModel> newData)
        {
            this.Remove(itemUID);
            if ((newData?.Count()??0) >0)
            {
                this.Create(newData);
            }
        }

        public void Remove(Guid itemUID)
        {
            var found = this.Retrieve("ItemUID", u => (u ?? String.Empty).ToString().Equals(itemUID.ToString(), StringComparison.OrdinalIgnoreCase));
            foreach (var item in found)
            {
                this.Delete(getKey(item));
            }
        }

        private static string getKey(ProductCategoryRelationCacheModel source)
        {
            return $"{source.ItemUID}|{source.CategoryUID}";
        }
    }

}
