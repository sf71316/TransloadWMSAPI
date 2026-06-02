using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Data.NoSql.Redis;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Cache.Redis.Controllers;

namespace YAEP.WMS.Cache.Redis
{
    /*
    *  Package 
    */
    public static partial class DrKnowAll
    {
        #region Package 

        /// <summary>
        /// get package list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PackageCacheModel> GetPackage()
        {
            var packageController = new PackageRedisController();
            var allPackages = packageController.RetrieveAll();

            if ((allPackages?.Count() ?? 0) == 0)
            {
                allPackages = ReloadPackage();
            }

            return allPackages;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageUID"></param>
        /// <returns></returns>
        public static PackageCacheModel GetPackage(Guid packageUID)
        {
            if (packageUID == Guid.Empty)
            {
                return null;
            }

            var packageController = new PackageRedisController();

            return packageController.Retrieve(packageUID).FirstOrDefault();
            //return GetPackage().FirstOrDefault(o => o.UID == packageUID);
        }

        public static IEnumerable<PackageCacheModel> GetPackageByItem(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return null;
            }

            string itemUIDString = itemUID.ToString();
            var condition1 = SearchCondition.AND("ItemUID", _ =>
            {
                return (_ ?? String.Empty).ToString().Equals(itemUIDString, StringComparison.OrdinalIgnoreCase);
            });

            var packageController = new PackageRedisController();

            var found = packageController.RetrieveByConditions(new SearchCondition[] { condition1 });

            return found.ToArray();
            //return GetPackage().FirstOrDefault(o => o.UID == packageUID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageUID"></param>
        public static void RefreshPackage(Guid packageUID)
        {
            if (packageUID == Guid.Empty)
            {
                return;
            }

            // load from db
            var manager = FactoryUtils.GetPackageFactoryInstance().CreatePackageManager();
            var dataResult = manager.GetPackage(packageUID);

            var packageController = new PackageRedisController();
            if (dataResult?.Content != null)
            {
                var data = dataResult.Content;
                // load from db
                var versionManager = FactoryUtils.GetPackageFactoryInstance().CreatePackageVersionManager(null);
                var uomManager = FactoryUtils.GetPackageFactoryInstance().CreatePackageUomManager();

                var version = versionManager.GetPackageVersion(data.VersionUID)?.Content;
                var uom = uomManager.GetPackageUom(data.UOM)?.Content;

                var cacheModel = new PackageCacheModel();
                cacheModel.UID = data.UID;
                cacheModel.ID = data.ID;
                cacheModel.Name = data.Name;
                cacheModel.Quantity = data.Quantity;
                cacheModel.Length = data.Length;
                cacheModel.Width = data.Width;
                cacheModel.Height = data.Height;
                cacheModel.GrossWeight = data.GrossWeight;
                cacheModel.SCC14 = data.SCC14;
                cacheModel.Status = data.Status;
                cacheModel.Type = data.Type;
                cacheModel.ItemUID = data.ItemUID;
                cacheModel.VersionUID = data.VersionUID;
                cacheModel.UOM = data.UOM;
                cacheModel.ParentUID = data.ParentUID;
                cacheModel.CreatedBy = data.CreatedBy;
                cacheModel.CreatedOn = data.CreatedOn;
                cacheModel.ModifiedBy = data.ModifiedBy;
                cacheModel.ModifiedOn = data.ModifiedOn;

                if (version != null)
                {
                    cacheModel.VersionId = $"{version.VersionId} ver.{version.SerialNumber}";
                }

                if (uom != null)
                {
                    cacheModel.UomName = uom.Name;
                }

                if (data.ParentUID.HasValue)
                {
                    var parentPackage = manager.GetPackage(data.ParentUID.Value)?.Content;
                    if (parentPackage != null)
                    {
                        var parentUOM = uomManager.GetPackageUom(parentPackage.UOM)?.Content;
                        cacheModel.ParentUOM = parentUOM?.UID;
                        cacheModel.ParentUomName = parentUOM?.Name;
                    }
                }

                packageController.Replace(packageUID, cacheModel);
            }
            else
            {
                packageController.Delete(packageUID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        /// <param name="isRefreshPackageUOM"></param>
        /// <param name="isRefreshPackageVersion"></param>
        public static void RefreshPackageByItem(Guid itemUID, bool isRefreshPackageUOM = false, bool isRefreshPackageVersion = false)
        {
            if (itemUID == Guid.Empty)
            {
                return;
            }

            var foundPackage = GetPackage()?.Where(o => o.ItemUID == itemUID)?.ToArray();
            var foundPackageUID = (foundPackage?.Select(o => o.UID)?.ToArray() ?? new Guid[] { });

            var packageController = new PackageRedisController();

            // load from db
            var packageManager = FactoryUtils.GetPackageFactoryInstance().CreatePackageManager();
            var data = packageManager.GetPackagesByItem(itemUID)?.Content;

            if ((data?.Count() ?? 0) > 0)
            {
                var versionManager = FactoryUtils.GetPackageFactoryInstance().CreatePackageVersionManager(null);
                var versions = versionManager.GetPackageVersionList(itemUID)?.Content;

                var uomManager = FactoryUtils.GetPackageFactoryInstance().CreatePackageUomManager();
                var uoms = uomManager.GetPackageUomList(data.Select(o => o.UOM).ToArray())?.Content;

                var cacheData = new HashSet<PackageCacheModel>(data.Select(o =>
                {
                    var cacheModel = new PackageCacheModel();
                    cacheModel.UID = o.UID;
                    cacheModel.ID = o.ID;
                    cacheModel.Name = o.Name;
                    cacheModel.Quantity = o.Quantity;
                    cacheModel.Length = o.Length;
                    cacheModel.Width = o.Width;
                    cacheModel.Height = o.Height;
                    cacheModel.GrossWeight = o.GrossWeight;
                    cacheModel.SCC14 = o.SCC14;
                    cacheModel.Status = o.Status;
                    cacheModel.Type = o.Type;
                    cacheModel.ItemUID = o.ItemUID;
                    cacheModel.VersionUID = o.VersionUID;
                    cacheModel.UOM = o.UOM;
                    cacheModel.ParentUID = o.ParentUID;
                    cacheModel.CreatedBy = o.CreatedBy;
                    cacheModel.CreatedOn = o.CreatedOn;
                    cacheModel.ModifiedBy = o.ModifiedBy;
                    cacheModel.ModifiedOn = o.ModifiedOn;

                    var version = versions?.FirstOrDefault(v => v.UID == o.VersionUID);
                    if (version != null)
                    {
                        cacheModel.VersionId = $"{version.VersionId} ver.{version.SerialNumber}";
                    }

                    cacheModel.UomName = uoms?.FirstOrDefault(u => u.UID == o.UOM)?.Name;

                    if (o.ParentUID.HasValue)
                    {
                        var parentPackage = packageManager.GetPackage(o.ParentUID.Value)?.Content;
                        if (parentPackage != null)
                        {
                            var parentUOM = uomManager.GetPackageUom(parentPackage.UOM)?.Content;
                            cacheModel.ParentUOM = parentUOM?.UID;
                            cacheModel.ParentUomName = parentUOM?.Name;
                        }
                    }

                    return cacheModel;
                })).ToArray();

                // add
                packageController.Replace(foundPackageUID, cacheData);
            }
            else
            {
                packageController.Remove(foundPackageUID);
            }

        }
        /// <summary>
        /// reload package list
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<PackageCacheModel> ReloadPackage()
        {
            var factory = FactoryUtils.GetPackageFactoryInstance();

            var packageManager = factory.CreatePackageManager();
            var packages = (packageManager.GetAllPackages()?.Content ?? new PackageCacheModel[] { });

            var packageVersionManager = factory.CreatePackageVersionManager(null);
            var versions = (packageVersionManager.GetAllPackageVersion()?.Content ?? new PackageVersionCacheModel[] { });

            var packageUomManager = factory.CreatePackageUomManager();
            var uoms = (packageUomManager.GetPackageUomList()?.Content ?? new PackageUomCacheModel[] { });

            var cacheData = packages.Select(o =>
            {
                var cacheModel = new PackageCacheModel()
                {
                    UID = o.UID,
                    ID = o.ID,
                    Name = o.Name,
                    Quantity = o.Quantity,
                    Length = o.Length,
                    Width = o.Width,
                    Height = o.Height,
                    GrossWeight = o.GrossWeight,
                    SCC14 = o.SCC14,
                    Status = o.Status,
                    Type = o.Type,
                    ItemUID = o.ItemUID,
                    ImageUID = o.ImageUID,
                    ParentUID = o.ParentUID,
                    VersionUID = o.VersionUID,
                    UOM = o.UOM,
                    PUOM = o.PUOM,
                    CreatedBy = o.CreatedBy,
                    CreatedOn = o.CreatedOn,
                    ModifiedBy = o.ModifiedBy,
                    ModifiedOn = o.ModifiedOn,
                };


                var version = versions.FirstOrDefault(v => v.UID == o.VersionUID);
                if (version != null)
                {
                    cacheModel.VersionId = $"{version.VersionId} ver.{version.SerialNumber}";
                }

                cacheModel.UomName = uoms.FirstOrDefault(u => u.UID == o.UOM)?.Name;

                if (o.ParentUID.HasValue)
                {
                    var parentPackage = packages.FirstOrDefault(p => p.UID == o.ParentUID.Value);
                    if (parentPackage != null)
                    {
                        var parentUOM = uoms.FirstOrDefault(u => u.UID == parentPackage.UOM);
                        cacheModel.ParentUOM = parentUOM?.UID;
                        cacheModel.ParentUomName = parentUOM?.Name;
                    }
                }

                return cacheModel;
            }).ToArray();

            var controller = new PackageRedisController();
            controller.Create(cacheData);

            return cacheData;
        }


        #endregion

        #region Package Version 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PackageVersionCacheModel> GetPackageVersion()
        {
            var controller = new PackageVersionRedisController();
            var allPackageVersions = controller.RetrieveAll();

            if ((allPackageVersions?.Count() ?? 0) == 0)
            {
                allPackageVersions = ReloadPackageVersion();
            }

            return allPackageVersions;
        }
        /// <summary>
        /// get package version
        /// </summary>
        /// <param name="versionUID">
        /// Version UID
        /// <para /><see cref="IPackageVersionModel.UID"/>
        /// </param>
        /// <returns></returns>
        public static PackageVersionCacheModel GetPackageVersion(Guid versionUID)
        {
            if (versionUID == Guid.Empty)
            {
                return null;
            }

            return GetPackageVersion().FirstOrDefault(o => o.UID == versionUID);
        }

        public static IEnumerable<PackageVersionCacheModel> GetPackageVersionByItem(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return null;
            }

            string itemUIDString = itemUID.ToString();
            var condition1 = SearchCondition.AND("ItemUID", _ =>
            {
                return (_ ?? String.Empty).ToString().Equals(itemUIDString, StringComparison.OrdinalIgnoreCase);
            });

            var packageVersionController = new PackageVersionRedisController();

            var found = packageVersionController.RetrieveByConditions(new SearchCondition[] { condition1 });

            return found.ToArray();
            //return GetPackage().FirstOrDefault(o => o.UID == packageUID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionUID"></param>
        public static void RefreshPackageVersion(Guid versionUID)
        {
            if (versionUID == Guid.Empty)
            {
                return;
            }

            // load from db
            var manager = FactoryUtils.GetPackageFactoryInstance().CreatePackageVersionManager(null);
            var data = manager.GetPackageVersion(versionUID)?.Content;

            var versionController = new PackageVersionRedisController();
            if (data != null)
            {
                var cacheModel = Copy(data);
                versionController.Replace(versionUID, cacheModel);
            }
            else
            {
                versionController.Delete(versionUID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        public static void RefreshPackageVersionByItem(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return;
            }

            // load from db
            var manager = FactoryUtils.GetPackageFactoryInstance().CreatePackageVersionManager(null);
            var data = manager.GetPackageVersionList(itemUID)?.Content;

            var versionController = new PackageVersionRedisController();

            if ((data?.Count() ?? 0) > 0)
            {
                var cacheData = Copy(data);
                versionController.ReplaceByItem(itemUID, cacheData);
            }
            else
            {
                versionController.Remove(itemUID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<PackageVersionCacheModel> ReloadPackageVersion()
        {
            var factory = FactoryUtils.GetPackageFactoryInstance();

            var packageVersionManager = factory.CreatePackageVersionManager(null);
            var versions = (packageVersionManager.GetAllPackageVersion()?.Content ?? new PackageVersionCacheModel[] { });
            var cacheData = Copy(versions);

            var controller = new PackageVersionRedisController();
            controller.Create(cacheData);

            return cacheData;
        }
        private static IEnumerable<PackageVersionCacheModel> Copy(IEnumerable<IPackageVersionModel> source)
        {
            return (source?.Select(o => Copy(o)) ?? new PackageVersionCacheModel[] { }).ToArray();
        }
        private static PackageVersionCacheModel Copy(IPackageVersionModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new PackageVersionCacheModel()
            {
                UID = source.UID,
                VersionId = source.VersionId,
                ItemUID = source.ItemUID,
                SerialNumber = source.SerialNumber,
                Status = source.Status,
                CreatedBy = source.CreatedBy,
                CreatedOn = source.CreatedOn,
                ModifiedBy = source.ModifiedBy,
                ModifiedOn = source.ModifiedOn,
            };
        }

        #endregion

        #region UOM

        /// <summary>
        /// get package UOM list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PackageUomCacheModel> GetPackageUom()
        {
            var controller = new PackageUomRedisController();
            var allPackageUoms = controller.RetrieveAll();

            if ((allPackageUoms?.Count() ?? 0) == 0)
            {
                allPackageUoms = ReloadPackageUom();
            }

            return allPackageUoms;
        }
        /// <summary>
        /// get package UOM
        /// </summary>
        /// <param name="uomUID">
        /// UOM UID
        /// <para /><see cref="IPackageUomModel.UID"/>
        /// </param>
        /// <returns></returns>
        public static PackageUomCacheModel GetPackageUom(Guid uomUID)
        {
            if (uomUID == Guid.Empty)
            {
                return null;
            }

            return GetPackageUom().FirstOrDefault(o => o.UID == uomUID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uomUID"></param>
        public static void RefreshPackageUom(Guid uomUID)
        {
            if (uomUID == Guid.Empty)
            {
                return;
            }

            // load from db
            var manager = FactoryUtils.GetPackageFactoryInstance().CreatePackageUomManager();
            var data = manager.GetPackageUom(uomUID)?.Content;

            var uomController = new PackageUomRedisController();
            if (data != null)
            {
                var cacheModel = Copy(data);
                uomController.Replace(uomUID, cacheModel);
            }
            else
            {
                uomController.Delete(uomUID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uomUID"></param>
        public static void RefreshPackageUom(IEnumerable<Guid> uomUID)
        {
            if ((uomUID?.Count() ?? 0) == 0)
            {
                return;
            }

            // load from db
            var manager = FactoryUtils.GetPackageFactoryInstance().CreatePackageUomManager();
            var data = manager.GetPackageUomList(uomUID)?.Content;

            var uomController = new PackageUomRedisController();
            if ((data?.Count() ?? 0) == 0)
            {
                var cacheData = Copy(data);
                uomController.Replace(uomUID, cacheData);
            }
            else
            {
                uomController.Remove(uomUID);
            }
        }
        /// <summary>
        /// reload package UOM list
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<PackageUomCacheModel> ReloadPackageUom()
        {
            var factory = FactoryUtils.GetPackageFactoryInstance();

            var packageUomManager = factory.CreatePackageUomManager();
            var uoms = (packageUomManager.GetPackageUomList()?.Content ?? new PackageUomCacheModel[] { });
            var cacheData = Copy(uoms);

            var controller = new PackageUomRedisController();
            controller.Create(cacheData);

            return cacheData;
        }
        private static IEnumerable<PackageUomCacheModel> Copy(IEnumerable<IPackageUomModel> source)
        {
            return (source?.Select(o => Copy(o)) ?? new PackageUomCacheModel[] { }).ToArray();
        }
        private static PackageUomCacheModel Copy(IPackageUomModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new PackageUomCacheModel()
            {
                UID = source.UID,
                ID = source.ID,
                Name = source.Name,
                Status = source.Status,
                Type = source.Type,
                CreatedBy = source.CreatedBy,
                CreatedOn = source.CreatedOn,
                ModifiedBy = source.ModifiedBy,
                ModifiedOn = source.ModifiedOn,
            };
        }


        #endregion

    }

}