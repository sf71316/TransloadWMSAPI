using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Cache.Redis.Controllers;

namespace YAEP.WMS.Cache.Redis
{
    /// <summary>
    /// 全知博士
    /// </summary>
    public static partial class DrKnowAll
    {
        static DrKnowAll()
        {
            setReloadDelegate();
            setForgetDelegate();
        }

        #region Core Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public static bool Reload(DrKnowAllKeys key = DrKnowAllKeys.ALL)
        {
            if (key == DrKnowAllKeys.ALL)
            {
                foreach (var mapping in ReloadMappings)
                {
                    try
                    {
                        Forget(mapping.Key);
                        mapping.Value.Invoke();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
            {
                try
                {
                    Forget(key);
                    ReloadMappings[key].Invoke();
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public static void Forget(DrKnowAllKeys key = DrKnowAllKeys.ALL)
        {
            if (key == DrKnowAllKeys.ALL)
            {
                foreach (var mapping in ForgetMappings)
                {
                    try
                    {
                        mapping.Value.Invoke();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
            {
                try
                {
                    ForgetMappings[key].Invoke();
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>  
        private readonly static Dictionary<DrKnowAllKeys, Action> ReloadMappings = new Dictionary<DrKnowAllKeys, Action>();
        /// <summary>
        /// 
        /// </summary>
        private static void setReloadDelegate()
        {
            ReloadMappings.Add(DrKnowAllKeys.Country, () => GetCountry());
            ReloadMappings.Add(DrKnowAllKeys.State, () => GetState());
            ReloadMappings.Add(DrKnowAllKeys.City, () => GetCity());
            ReloadMappings.Add(DrKnowAllKeys.Zip, () => GetZip());
            ReloadMappings.Add(DrKnowAllKeys.Group, () => GetGroup());
            ReloadMappings.Add(DrKnowAllKeys.User, () => GetUser());
            ReloadMappings.Add(DrKnowAllKeys.Customer, () => GetCustomer());
            ReloadMappings.Add(DrKnowAllKeys.Warehouse, () => GetWarehouse());

            // Product
            ReloadMappings.Add(DrKnowAllKeys.ProductCategory, () => ReloadProductCategory());
            ReloadMappings.Add(DrKnowAllKeys.ProductCategoryRelation, () => ReloadProductCategoryRelation());
            ReloadMappings.Add(DrKnowAllKeys.Product, () => ReloadAllProduct());

            // Package
            ReloadMappings.Add(DrKnowAllKeys.PackageUom, () => ReloadPackageUom());
            ReloadMappings.Add(DrKnowAllKeys.PackageVersion, () => ReloadPackageVersion());
            ReloadMappings.Add(DrKnowAllKeys.Package, () => ReloadPackage());
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly static Dictionary<DrKnowAllKeys, Action> ForgetMappings = new Dictionary<DrKnowAllKeys, Action>();
        /// <summary>
        /// 
        /// </summary>
        private static void setForgetDelegate()
        {
            ForgetMappings.Add(DrKnowAllKeys.Country, () => (new CountryRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.State, () => (new StateRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.City, () => (new CityRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.Zip, () => (new ZipRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.Group, () => (new GroupRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.User, () => (new UserRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.Customer, () => (new CustomerRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.Warehouse, () => (new WarehouseRedisController()).DeleteAll());

            // Product
            ForgetMappings.Add(DrKnowAllKeys.ProductCategory, () => (new ProductCategoryRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.ProductCategoryRelation, () => (new ProductCategoryRelationRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.Product, () => (new ProductRedisController()).DeleteAll());

            // Package
            ForgetMappings.Add(DrKnowAllKeys.PackageUom, () => (new PackageUomRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.PackageVersion, () => (new PackageVersionRedisController()).DeleteAll());
            ForgetMappings.Add(DrKnowAllKeys.Package, () => (new PackageRedisController()).DeleteAll());
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return CheckIsInitialized();
            }
            set
            {
                SetIsInitialized(value);
            }
        }


        private static bool CheckIsInitialized()
        {
            var controller = new BoardRedisController();
            var boardData = controller.RetrieveAll();
            return (boardData?.Any(b => b.ID.Equals("IsInitialized") && b.IsLoaded) ?? false);

        }
        private static void SetIsInitialized(bool isInitialized)
        {
            var controller = new BoardRedisController();
            controller.SetLoaded("IsInitialized", isInitialized);
        }
    }

}