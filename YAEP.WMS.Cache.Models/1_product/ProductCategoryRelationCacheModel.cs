using System;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductCategoryRelationCacheModel : AbstractCacheModel, IItemCategoryRelationModel
    {
        public Guid ItemUID { get; set; }
        public Guid CategoryUID { get; set; }

        public override string ToString()
        {
            return $"{this.ItemUID}|{this.CategoryUID}";
        }
    }
}