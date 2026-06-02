using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Api.Code.Cache.Models;

namespace YAEP.WMS.Api.Code.Cache
{
    /*
    *  Package 
    */
    public static partial class DrKnowAll
    {
        /// <summary>
        /// 
        /// </summary>
        public static DrKnowLoadingStatus PackageLoadingStatus { get; set; } = DrKnowLoadingStatus.Pending;
        /// <summary>
        /// 
        /// </summary>
        public static DrKnowLoadingStatus PackageVersionLoadingStatus { get; set; } = DrKnowLoadingStatus.Pending;
        /// <summary>
        /// 
        /// </summary>
        public static DrKnowLoadingStatus PackageUomLoadingStatus { get; set; } = DrKnowLoadingStatus.Pending;

        #region Package 

        /// <summary>
        /// get package list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PackageCacheModel> GetPackage()
        {
            var enumType = DrKnowAllKeys.Package;

            var knowledge = Instance.Recollect<PackageCacheModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetPackageFactory().CreatePackageManager();

                var dataResult = manager.GetAllPackages();
                if (dataResult.Success)
                {
                    var data = GetPackageCacheList(dataResult.Content);

                    knowledge.SetData(data);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
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

            return GetPackage().FirstOrDefault(o => o.UID == packageUID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageUID"></param>
        public static void RefreshPackage(Guid packageUID)
        {
            var enumType = DrKnowAllKeys.Package;

            var collection = GetPackage();

            var itemCached = collection.Where(o => o.UID == packageUID);

            var manager = GetPackageFactory().CreatePackageManager();
            var dataResult = manager.GetPackage(packageUID);

            var data = dataResult.Content;

            var cacheData = new HashSet<PackageCacheModel>(collection);

            if (data == null)
            {
                cacheData.RemoveWhere(o => o.UID == packageUID);
            }
            else
            {
                if (itemCached == null)
                {
                    // add 
                    var p = GetPackageCache(data);
                    cacheData.Add(p);
                }
                else
                {
                    // update 
                    cacheData.RemoveWhere(o => o.UID == packageUID);
                    var p = GetPackageCache(data);
                    cacheData.Add(p);
                }
            }

            var knowledge = Instance.Recollect<PackageCacheModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
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

            var enumType = DrKnowAllKeys.Package;

            var collection = GetPackage() ?? (new PackageCacheModel[] { });
            var cacheData = new HashSet<PackageCacheModel>(collection);
            // remove
            cacheData.RemoveWhere(o => o.ItemUID == itemUID);

            // load from db
            var manager = GetPackageFactory().CreatePackageManager();
            var dataResult = manager.GetPackagesByItem(itemUID);
            var dataInDB = dataResult.Content;

            if ((dataInDB?.Count() ?? 0) > 0)
            {
                // add
                foreach (var package in dataInDB)
                {
                    var cachePackage = GetPackageCache(package);
                    cacheData.Add(cachePackage);
                }
            }

            var knowledge = Instance.Recollect<PackageCacheModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);

            if (isRefreshPackageUOM)
            {
                var uomUID = dataInDB?.Select(o => o.UOM).Where(o => o != Guid.Empty).ToArray();

                RefreshPackageUom(uomUID);
            }
            if (isRefreshPackageVersion)
            {
                RefreshPackageVersionByItem(itemUID);
            }
        }
        private static IEnumerable<PackageCacheModel> GetPackageCacheList(IEnumerable<IPackageModel> source)
        {
            var result = new HashSet<PackageCacheModel>(source.Select(o =>
            {
                var cacheModel = new PackageCacheModel();
                cacheModel.UID = o.UID;
                cacheModel.ID = o.ID;
                cacheModel.Name = o.Name;
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

                var version = GetPackageVersion(o.VersionUID);
                if (version != null)
                {
                    cacheModel.VersionId = $"{version.VersionId} ver.{version.SerialNumber}";
                }

                cacheModel.UomName = GetPackageUom(o.UOM)?.Name;

                if (o.ParentUID.HasValue)
                {
                    var parentPackage = source.FirstOrDefault(p => p.UID == o.ParentUID.Value);
                    if (parentPackage != null)
                    {
                        var parentUOM = GetPackageUom(parentPackage.UOM);
                        cacheModel.ParentUOM = parentUOM?.UID;
                        cacheModel.ParentUomName = parentUOM?.Name;
                    }
                }

                return cacheModel;
            }));

            return result;
        }
        private static PackageCacheModel GetPackageCache(IPackageModel o)
        {
            var cacheModel = new PackageCacheModel();
            cacheModel.UID = o.UID;
            cacheModel.ID = o.ID;
            cacheModel.Name = o.Name;
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

            var version = GetPackageVersion(o.VersionUID);
            if (version != null)
            {
                cacheModel.VersionId = $"{version.VersionId} ver.{version.SerialNumber}";
            }

            cacheModel.UomName = GetPackageUom(o.UOM)?.Name;

            if (o.ParentUID.HasValue)
            {
                var parentPackage = GetPackage().FirstOrDefault(p => p.UID == o.ParentUID.Value);
                if (parentPackage != null)
                {
                    var parentUOM = GetPackageUom(parentPackage.UOM);
                    cacheModel.ParentUOM = parentUOM?.UID;
                    cacheModel.ParentUomName = parentUOM?.Name;
                }
            }

            return cacheModel;
        }
        private static PackageCacheModel GetPackageCache(IPackageViewModel o)
        {
            var cacheModel = new PackageCacheModel();
            cacheModel.UID = o.UID;
            cacheModel.ID = o.ID;
            cacheModel.Name = o.Name;
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

            cacheModel.VersionId = o.VersionId;
            cacheModel.UomName = o.UomName;
            cacheModel.ParentUOM = o.ParentUOM;
            cacheModel.ParentUomName = o.ParentUomName;

            return cacheModel;
        }

        #endregion

        #region Package Version 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IPackageVersionModel> GetPackageVersion()
        {
            var enumType = DrKnowAllKeys.PackageVersion;

            var knowledge = Instance.Recollect<IPackageVersionModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetPackageFactory().CreatePackageVersionManager(null);
                var dataResult = manager.GetAllPackageVersion();
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// get package version
        /// </summary>
        /// <param name="versionUID">
        /// Version UID
        /// <para /><see cref="IPackageVersionModel.UID"/>
        /// </param>
        /// <returns></returns>
        public static IPackageVersionModel GetPackageVersion(Guid versionUID)
        {
            if (versionUID == Guid.Empty)
            {
                return null;
            }

            return GetPackageVersion().FirstOrDefault(o => o.UID == versionUID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionUID"></param>
        public static void RefreshPackageVersion(Guid versionUID)
        {
            var enumType = DrKnowAllKeys.PackageVersion;

            var collection = GetPackageVersion();

            var versionCached = collection.Where(o => o.UID == versionUID);

            var manager = GetPackageFactory().CreatePackageVersionManager(null);
            var dataResult = manager.GetPackageVersion(versionUID);

            var itemInDB = dataResult.Content;

            var cacheData = new List<IPackageVersionModel>(collection);

            if (itemInDB == null)
            {
                cacheData.RemoveAll(o => o.UID == versionUID);
            }
            else
            {
                if (versionCached == null)
                {
                    // add 
                    cacheData.Add(itemInDB);
                }
                else
                {
                    // update 
                    cacheData.RemoveAll(o => o.UID == versionUID);
                    cacheData.Add(itemInDB);
                }
            }

            var knowledge = Instance.Recollect<IPackageVersionModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
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

            var enumType = DrKnowAllKeys.PackageVersion;

            // load from db
            var manager = GetPackageFactory().CreatePackageVersionManager(null);
            var dataResult = manager.GetPackageVersionList(itemUID);
            var dataInDB = dataResult.Content;

            var collection = GetPackageVersion() ?? (new IPackageVersionModel[] { });
            var cacheData = new HashSet<IPackageVersionModel>(collection);


            if ((dataInDB?.Count() ?? 0) > 0)
            {
                // remove
                cacheData.RemoveWhere(o => dataInDB.Any(g => g.UID == o.UID));

                // add
                foreach (var version in dataInDB)
                {
                    cacheData.Add(version);
                }
            }

            var knowledge = Instance.Recollect<IPackageVersionModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
        }

        #endregion

        #region UOM

        /// <summary>
        /// get package UOM list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IPackageUomModel> GetPackageUom()
        {
            var enumType = DrKnowAllKeys.PackageUom;

            var knowledge = Instance.Recollect<IPackageUomModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetPackageFactory().CreatePackageUomManager();
                var dataResult = manager.GetPackageUomList();
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// get package UOM
        /// </summary>
        /// <param name="uomUID">
        /// UOM UID
        /// <para /><see cref="IPackageUomModel.UID"/>
        /// </param>
        /// <returns></returns>
        public static IPackageUomModel GetPackageUom(Guid uomUID)
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
            var enumType = DrKnowAllKeys.PackageUom;

            var collection = GetPackageUom();

            var cached = collection.Where(o => o.UID == uomUID);

            var manager = GetPackageFactory().CreatePackageUomManager();
            var dataResult = manager.GetPackageUom(uomUID);

            var model = dataResult.Content;

            var cacheData = new List<IPackageUomModel>(collection);

            if (model == null)
            {
                cacheData.RemoveAll(o => o.UID == uomUID);
                return;
            }

            if (cached == null)
            {
                // add 
                cacheData.Add(model);
            }
            else
            {
                // update 
                cacheData.RemoveAll(o => o.UID == uomUID);
                cacheData.Add(model);
            }

            var knowledge = Instance.Recollect<IPackageUomModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
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

            var enumType = DrKnowAllKeys.PackageUom;

            var collection = GetPackageUom() ?? (new IPackageUomModel[] { });
            var cacheData = new HashSet<IPackageUomModel>(collection);
            // remove
            cacheData.RemoveWhere(o => uomUID.Any(g => g == o.UID));

            // load from db
            var manager = GetPackageFactory().CreatePackageUomManager();
            var dataResult = manager.GetPackageUomList(uomUID);
            var dataInDB = dataResult.Content;

            if ((dataInDB?.Count() ?? 0) > 0)
            {
                // add
                foreach (var uom in dataInDB)
                {
                    cacheData.Add(uom);
                }
            }

            var knowledge = Instance.Recollect<IPackageUomModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
        }

        #endregion

        /// <summary>
        /// reload package UOM list
        /// </summary>
        /// <returns></returns>
        private static void ReloadPackageUom()
        {
            var enumType = DrKnowAllKeys.PackageUom;

            var knowledge = Instance.Recollect<IPackageUomModel>(enumType.ToString());

            var manager = GetPackageFactory().CreatePackageUomManager();
            var dataResult = manager.GetPackageUomList();
            if (dataResult.Success)
            {
                knowledge.SetData(dataResult.Content);
            }
            Instance.Remember(enumType.ToString(), knowledge);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static void ReloadPackageVersion()
        {
            var enumType = DrKnowAllKeys.PackageVersion;

            var knowledge = Instance.Recollect<IPackageVersionModel>(enumType.ToString());

            var manager = GetPackageFactory().CreatePackageVersionManager(null);
            var dataResult = manager.GetAllPackageVersion();
            if (dataResult.Success)
            {
                knowledge.SetData(dataResult.Content);
            }
            Instance.Remember(enumType.ToString(), knowledge);
        }
        /// <summary>
        /// reload package list
        /// </summary>
        /// <returns></returns>
        private static void ReloadPackage()
        {
            var enumType = DrKnowAllKeys.Package;

            var knowledge = Instance.Recollect<PackageCacheModel>(enumType.ToString());

            var manager = GetPackageFactory().CreatePackageManager();

            var dataResult = manager.GetAllPackages();
            if (dataResult.Success)
            {
                var data = GetPackageCacheList(dataResult.Content);

                knowledge.SetData(data);
            }
            Instance.Remember(enumType.ToString(), knowledge);
        }
    }

}