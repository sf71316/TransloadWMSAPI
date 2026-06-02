using System;
using System.Collections.Generic;
using YAEP.Cache;

namespace YAEP.WMS.Api.Code.Cache
{
    /// <summary>
    /// 全知博士
    /// </summary>
    public static partial class DrKnowAll
    {
        static DrKnowAll()
        {
            setReloadDelegate();
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
                Instance.Clean();
            }
            else
            {
                Instance.Forget(key.ToString());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private readonly static YAEP.Cache.IDrKnowAll Instance = new YAEP.Cache.DrKnowAll(new MemoryStorage());
        /// <summary>
        /// 
        /// </summary> 
        private readonly static Dictionary<DrKnowAllKeys, Func<object>> ReloadMappings1 = new Dictionary<DrKnowAllKeys, Func<object>>();
        private readonly static Dictionary<DrKnowAllKeys, Action> ReloadMappings = new Dictionary<DrKnowAllKeys, Action>();
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
            //ReloadMappings.Add(DrKnowAllKeys.Country, GetCountry);
            //ReloadMappings.Add(DrKnowAllKeys.State, GetState);
            //ReloadMappings.Add(DrKnowAllKeys.City, GetCity);
            //ReloadMappings.Add(DrKnowAllKeys.Zip, GetZip);
            //ReloadMappings.Add(DrKnowAllKeys.Group, GetGroup);
            //ReloadMappings.Add(DrKnowAllKeys.User, GetUser);
            //ReloadMappings.Add(DrKnowAllKeys.Customer, GetCustomer);
            //ReloadMappings.Add(DrKnowAllKeys.Warehouse, GetWarehouse);

            // Product
            ReloadMappings.Add(DrKnowAllKeys.ProductCategory, () =>
            {
                DrKnowAll.ProductCategoryLoadingStatus = DrKnowLoadingStatus.Loading;
                ReloadProductCategory();
                DrKnowAll.ProductCategoryLoadingStatus = DrKnowLoadingStatus.Loaded;
            });
            ReloadMappings.Add(DrKnowAllKeys.ProductCategoryRelation, () =>
            {
                DrKnowAll.ProductCategoryRelationLoadingStatus = DrKnowLoadingStatus.Loading;
                ReloadProductCategoryRelation();
                DrKnowAll.ProductCategoryRelationLoadingStatus = DrKnowLoadingStatus.Loaded;
            });
            ReloadMappings.Add(DrKnowAllKeys.Product, () =>
            {
                DrKnowAll.ProductLoadingStatus = DrKnowLoadingStatus.Loading;
                ReloadProduct();
                DrKnowAll.ProductLoadingStatus = DrKnowLoadingStatus.Loaded;
            });
            //ReloadMappings.Add(DrKnowAllKeys.ProductCategory, GetProductCategory);
            //ReloadMappings.Add(DrKnowAllKeys.ProductCategoryRelation, GetProductCategoryRelation);
            //ReloadMappings.Add(DrKnowAllKeys.Product, GetProduct);

            // Package
            ReloadMappings.Add(DrKnowAllKeys.PackageUom, () =>
            {
                DrKnowAll.PackageUomLoadingStatus = DrKnowLoadingStatus.Loading;
                ReloadPackageUom();
                DrKnowAll.PackageUomLoadingStatus = DrKnowLoadingStatus.Loaded;
            });
            ReloadMappings.Add(DrKnowAllKeys.PackageVersion, () =>
            {
                DrKnowAll.PackageVersionLoadingStatus = DrKnowLoadingStatus.Loading;
                ReloadPackageVersion();
                DrKnowAll.PackageVersionLoadingStatus = DrKnowLoadingStatus.Loaded;
            });
            ReloadMappings.Add(DrKnowAllKeys.Package, () =>
            {
                DrKnowAll.PackageLoadingStatus = DrKnowLoadingStatus.Loading;
                ReloadPackage();
                DrKnowAll.PackageLoadingStatus = DrKnowLoadingStatus.Loaded;
            });

            //ReloadMappings.Add(DrKnowAllKeys.PackageUom, GetPackageUom);
            //ReloadMappings.Add(DrKnowAllKeys.PackageVersion, GetPackageVersion);
            //ReloadMappings.Add(DrKnowAllKeys.Package, GetPackage);
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
            return true;
            if (_MemoryCache == null)
            {
                _MemoryCache = System.Runtime.Caching.MemoryCache.Default;
            }

            if (_MemoryCache.Contains(CACHE_KEY))
            {
                return (bool)_MemoryCache.Get(CACHE_KEY);
            }
            else
            {
                return false;
            }
        }
        private static void SetIsInitialized(bool isInitialized)
        {
            if (_MemoryCache == null)
            {
                _MemoryCache = System.Runtime.Caching.MemoryCache.Default;
            }

            var policy = new System.Runtime.Caching.CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24);
            _MemoryCache.Set(CACHE_KEY, isInitialized, policy);
        }

        private static System.Runtime.Caching.ObjectCache _MemoryCache = null;
        private const string CACHE_KEY = "IsInitialized";

    }

}