using System;  
using YAEP.WMS.Interfaces; 

namespace YAEP.WMS.Cache.Redis
{
    public class RefreshDrKnowAll : IRefreshDrKnowAll
    {
        public void RefreshProduct(Guid itemUID)
        {
            // Item
            DrKnowAll.RefreshProduct(itemUID, isRefreshProductCategory: true, isRefreshProductCategoryRelation: true);

            // Package
            DrKnowAll.RefreshPackageByItem(itemUID, isRefreshPackageUOM: true, isRefreshPackageVersion: true);
        }
    }
}