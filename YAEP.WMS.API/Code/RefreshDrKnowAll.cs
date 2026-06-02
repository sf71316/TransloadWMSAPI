using System; 
using YAEP.WMS.Api.Code;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.API.Code
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