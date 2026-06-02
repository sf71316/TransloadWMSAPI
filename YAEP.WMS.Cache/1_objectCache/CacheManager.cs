using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using YAEP.Cache;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Interfaces.Model;
using YAEP.WMS.Model;
using Redis = YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.Cache.CacheManager
{
    public sealed class CacheManager
    {
        ObjectCache _Cache = MemoryCache.Default;
        public const string PRODUCT_CACHE_KEY = "ProductCache";
        public const string PRODUCT_PACKAGE_CACHE_KEY = "ProductPackageCache";
        public const string MIN_PKG_PRODUCT_CACHE_KEY = "MIN_PKG_PRODUCT";
        public const string PKG_VER_CACHE_KEY = "PKG_VER_CACHE";
        public const string PKG_CACHE_KEY = "PKG_CACHE_KEY";
        public const string PKG_TREE_CACHE_KEY = "PKG_TREE_CACHE";
        public const string ITEM_PKG_CACHE_KEY = "ITEM_PKG_CACHE";
        public const string PKG_DATA_CACHE_KEY = "PKG_DATA_CACHE";
        public const string PKG_VER_DATA_CACHE_KEY = "PKG_VER_DATA_CACHE";
        public const string PKG_UOM_DATA_CACHE_KEY = "PKG_UOM_DATA_CACHE";
        public CacheManager()
        {

        }
        public static CacheManager CreateInstance()
        {
            return new CacheManager();
        }
        public void LoadCache()
        {
            var t1 = Task.Factory.StartNew(() =>
            {
                LoadPackageCache();
            });
            var t2 = Task.Factory.StartNew(() =>
            {
                LoadPackageVersionCache();
            });
            var t3 = Task.Factory.StartNew(() =>
            {
                LoadPackageUomCache();
            });
            var t4 = Task.Factory.StartNew(() =>
            {
                GetProductPackageCache();
            });
            var t5 = Task.Factory.StartNew(() =>
            {
                GetProductCache();
            });

            Task.WaitAll(t1, t2, t3, t4, t5);

        }
        public List<IPackageViewModel> LoadPackageCache()
        {
            if (_Cache[PKG_DATA_CACHE_KEY] == null)
            {
                _Cache.Add(PKG_DATA_CACHE_KEY
                    , Redis.DrKnowAll.GetPackage().Select(p => p as IPackageViewModel).ToList()
                    , new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[PKG_DATA_CACHE_KEY] as List<IPackageViewModel>;
        }
        public List<IProductExtendModel> GetProductCache()
        {
            if (_Cache[PRODUCT_CACHE_KEY] == null)
            {
                _Cache.Add(PRODUCT_CACHE_KEY,
                    Redis.DrKnowAll.GetProduct().Select(p => (IProductExtendModel)new ProductExtendModel(p)).ToList()
                    ,
                    new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[PRODUCT_CACHE_KEY] as List<IProductExtendModel>;
        }
        public List<IPackageVersionModel> LoadPackageVersionCache()
        {
            if (_Cache[PKG_VER_DATA_CACHE_KEY] == null)
            {
                _Cache.Add(PKG_VER_DATA_CACHE_KEY
                    , Redis.DrKnowAll.GetPackageVersion().Select(p => p as IPackageVersionModel).ToList()
                    , new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[PKG_VER_DATA_CACHE_KEY] as List<IPackageVersionModel>;
        }
        public List<IPackageUomModel> LoadPackageUomCache()
        {
            if (_Cache[PKG_UOM_DATA_CACHE_KEY] == null)
            {
                _Cache.Add(PKG_UOM_DATA_CACHE_KEY
                    , Redis.DrKnowAll.GetPackageUom().Select(p => p as IPackageUomModel).ToList()
                    , new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[PKG_UOM_DATA_CACHE_KEY] as List<IPackageUomModel>;
        }
        public List<IProductPackageExtendModel> GetProductPackageCache()
        {
            if (_Cache[PRODUCT_PACKAGE_CACHE_KEY] == null)
            {
                var items = GetProductCache();// Redis.DrKnowAll.GetProduct().Select(p => new ProductExtendModel(p));
                if (items != null && items.Count() > 0)
                {
                    var packages = LoadPackageCache();//  Redis.DrKnowAll.GetPackage().ToList();
                    var packagesgrp = packages.GroupBy(p => p.ItemUID).ToDictionary(g => g.Key,
                        g => g.OrderByDescending(o => o.VersionId).ThenByDescending(o => o.GrossWeight).ToList());
                    ConcurrentBag<IProductPackageExtendModel> item_list = new ConcurrentBag<IProductPackageExtendModel>();
                    Parallel.ForEach(items, x =>
                    {
                        ProductPackageExtendModel target = new ProductPackageExtendModel(x);
                        //var target_packages = packages.FindAll(p => p.ItemUID == x.UID).OrderByDescending(o => o.VersionId).ThenByDescending(o => o.GrossWeight);
                        List<IPackageViewModel> target_packages = null;
                        if (packagesgrp.TryGetValue(x.UID, out target_packages))
                        {
                            if (target_packages.Count() > 0)
                            {
                                ConcurrentBag<PackageExtendModel> target_package = new ConcurrentBag<PackageExtendModel>();

                                foreach (var p in target_packages)
                                {
                                    target_package.Add(new PackageExtendModel()
                                    {
                                        UID = p.UID,
                                        //ID = p.ID,
                                        Name = p.Name,
                                        VersionId = p.VersionId,
                                        SCC14 = p.SCC14,
                                        PUOM = p.PUOM
                                    });
                                }
                                target.Packages = target_package;
                            }

                            item_list.Add(target);
                        }
                    });

                    _Cache.Add(PRODUCT_PACKAGE_CACHE_KEY, item_list.ToList(),
                    new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
                }


            }
            return _Cache[PRODUCT_PACKAGE_CACHE_KEY] as List<IProductPackageExtendModel>;
        }
    }
}
