using System; 
using YAEP.WMS.Cache.Models;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public class ProductRedisController : AbstractDefaultConnectSettingController<ProductCacheModel>
    {
        public ProductRedisController() : base(o => o.UID)
        {
            this.AppendIndex(nameof(ProductCacheModel.ID), o => o.ID);
            this.AppendIndex(nameof(ProductCacheModel.Name), o => o.Name);
            this.AppendIndex(nameof(ProductCacheModel.Type), o => o.Type);
            this.AppendIndex(nameof(ProductCacheModel.Status), o => o.Status);
            this.AppendIndex(nameof(ProductCacheModel.GroupUID), o => o.GroupUID);
            this.AppendIndex(nameof(ProductCacheModel.CustomerUID), o => o.CustomerUID);
            this.AppendIndex(nameof(ProductCacheModel.CustomerName), o => o.CustomerName);
            this.AppendIndex(nameof(ProductCacheModel.CategoryUID), o => o.CategoryUID);
            this.AppendIndex(nameof(ProductCacheModel.IsBOM), o => o.IsBOM);
            this.AppendIndex(nameof(ProductCacheModel.IsVirtualItem), o => o.IsVirtualItem);
        }

        public void Replace(Guid uid, ProductCacheModel newData)
        {
            this.Delete(uid);
            if (newData != null)
            {
                this.Create(newData);
            }
        }


    }

}
