using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Core.Item.Constants;
using YAEP.Core.Item.DI;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.DI;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Identities.DI;
using YAEP.Interfaces;
using YAEP.Package.DI;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Api.Code;
using YAEP.WMS.API.Models.Response;

namespace YAEP.WMS.API.Code
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductManager
    {
        private readonly bool _UseCache;
        private readonly IAuthenticationInfo _AuthenticationInfo;
        private readonly Lazy<IdentityFactory> _IdentityFactory;
        private readonly Lazy<PartyFactory> _PartyFactory;
        private readonly Lazy<ItemFactory> _ItemFactory;
        private readonly Lazy<PackageFactory> _PackageFactory;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationInfo"></param>
        /// <param name="useCache"></param>
        public ProductManager(IAuthenticationInfo authenticationInfo, bool useCache = true)
        {
            this._UseCache = useCache;
            this._AuthenticationInfo = authenticationInfo ?? throw new ArgumentNullException(nameof(authenticationInfo));
            this._IdentityFactory = new Lazy<IdentityFactory>(() => FactoryUtils.GetIdentityFactory(authenticationInfo));
            this._PartyFactory = new Lazy<PartyFactory>(() => FactoryUtils.GetPartyFactory(authenticationInfo));
            this._PackageFactory = new Lazy<PackageFactory>(() => FactoryUtils.GetPackageFactory(authenticationInfo));
            this._ItemFactory = new Lazy<ItemFactory>(() => FactoryUtils.GetItemFactory(authenticationInfo));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ManifestProductResponseModel> GetProductList()
        {
            var groupUIDs = getGroupsByUser();
            var customerUIDs = getCustomersByUser().Select(o => o.UID).ToArray();

            var colletion = this.getCacheProductList(customerUIDs, groupUIDs);

            var arrItemUID = colletion.Select(o => o.UID).ToArray();
            var allCachedPackages = this.getCachePackageList(arrItemUID);

            var list = colletion.Select(o =>
            {
                Guid itemUID = o.UID;
                var item = new ManifestProductResponseModel
                {
                    ItemUID = itemUID,
                    ID = o.ID,
                    Name = o.Name,
                    Status = YAEP.Utilities.EnumerableData.GetName<ItemStatus>(o.Status),
                    ImageUID = o.ImageUID,
                    CustomerName = o.CustomerName,
                    Description = o.Description,
                };

                if (allCachedPackages.Any(p => p.ItemUID == itemUID))
                {
                    var packages = allCachedPackages.Where(p => p.ItemUID == itemUID);
                    foreach (var p in packages)
                    {
                        var package = new ManifestProductPackageResponseModel
                        {
                            UID = p.UID,
                            ID = p.ID,
                            Name = p.Name,
                            VersionId = p.VersionId,
                        };
                        item.Packages.Add(package);
                    }
                }

                return item;
            });

            list = list.OrderBy(o => o.ID).ToList();

            return list;
        }

        private IAuthenticationInfo GetAuthenticationInfo()
        {
            return this._AuthenticationInfo;
        }
        private IdentityFactory GetIdentityFactory()
        {
            return this._IdentityFactory.Value;
        }
        private PartyFactory GetPartyFactory()
        {
            return this._PartyFactory.Value;
        }
        private PackageFactory GetPackageFactory()
        {
            return this._PackageFactory.Value;
        }
        private ItemFactory GetItemFactory()
        {
            return this._ItemFactory.Value;
        }

        private IEnumerable<YAEP.WMS.Cache.Models.ProductCacheModel> getCacheProductList(IEnumerable<Guid> customerUIDs = null, IEnumerable<Guid> groupUIDs = null)
        {
            if (!this._UseCache)
            {
                return this.getCacheProductListByDB(customerUIDs, groupUIDs);
            }

            var emptyCustomerParameters = (customerUIDs?.Count() ?? 0) == 0;
            var emptyGroupParameters = (groupUIDs?.Count() ?? 0) == 0;

            var cacheArray = YAEP.WMS.Cache.Redis.DrKnowAll.GetProduct().ToArray();

            if (emptyCustomerParameters && emptyGroupParameters)
            {
                return cacheArray;
            }

            var result = cacheArray.Where(o =>
            {
                bool r = true;

                if (!emptyGroupParameters)
                {
                    r = groupUIDs.Any(g => g == o.GroupUID);
                }

                if (r)
                {
                    if (!emptyCustomerParameters)
                    {
                        r = customerUIDs.Any(g => g == o.CustomerUID);
                    }
                }

                return r;
            });

            return result.ToArray();
        }
        private IEnumerable<YAEP.WMS.Cache.Models.ProductCacheModel> getCacheProductListByDB(IEnumerable<Guid> customerUIDs = null, IEnumerable<Guid> groupUIDs = null)
        {
            var factory = this.GetItemFactory();
            var manager = factory.CreateItemManager();
            var parameters = factory.CreateItemSearchParameters();

            if ((customerUIDs?.Count() ?? 0) > 0)
            {
                foreach (var customerUID in customerUIDs)
                {
                    var propertySearchParameters = factory.CreateItemPropertiesSearchParameters();
                    propertySearchParameters.Name = "CustomerUID";
                    propertySearchParameters.Value = customerUID.ToString();
                    parameters.ItemProperties.Add(propertySearchParameters);
                }
            }
            if ((groupUIDs?.Count() ?? 0) > 0)
            {
                parameters.ListOfGroupUID.AddRange(groupUIDs);
            }

            var dataResult = manager.GetItems<YAEP.WMS.Cache.Models.ProductCacheModel>(parameters);

            return dataResult.Content?.ToArray() ?? new YAEP.WMS.Cache.Models.ProductCacheModel[] { };
        }
        private IEnumerable<YAEP.WMS.Cache.Models.PackageCacheModel> getCachePackageList(IEnumerable<Guid> itemUID)
        {
            if (!this._UseCache)
            {
                return this.getCachePackageListByDB(itemUID);
            }

            var colletion = YAEP.WMS.Cache.Redis.DrKnowAll.GetPackage().Where(p => itemUID.Any(uid => uid == p.ItemUID)).ToList();

            return colletion.ToArray();
        }
        private IEnumerable<YAEP.WMS.Cache.Models.PackageCacheModel> getCachePackageListByDB(IEnumerable<Guid> itemUID)
        {
            if ((itemUID?.Count() ?? 0) > 0)
            {
                var packageFactory = this.GetPackageFactory();
                var packageManager = packageFactory.CreatePackageManager();
                var searchPackageResult = packageManager.GetPackagesByItem(itemUID.ToArray());
                if (searchPackageResult.Success)
                {
                    return this.parsePackageCacheList(searchPackageResult.Content);
                }
            }

            return new YAEP.WMS.Cache.Models.PackageCacheModel[] { };
        }
        private IEnumerable<YAEP.WMS.Cache.Models.PackageCacheModel> parsePackageCacheList(IEnumerable<IPackageViewModel> source)
        {
            var result = source.Select(o =>
            {
                var cacheModel = new YAEP.WMS.Cache.Models.PackageCacheModel();
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
            }).ToArray();

            return result;
        }


        private IEnumerable<Guid> getGroupsByUser()
        {
            var authInfo = this.GetAuthenticationInfo();
            var manager = this.GetIdentityFactory().CreateGroupManager();
            var result = manager.GetGroupKeysByUser(authInfo.UID);
            if (result.Success)
            {
                return result.Content;
            }
            return null;
        }

        private IEnumerable<IPartyModel> getCustomersByUser()
        {
            var groups = this.getGroupsByUser() ?? new Guid[] { };

            if (groups.Count() > 0)
            {
                var partyManager = this.GetPartyFactory().CreatePartyManager();
                var searchPartyParameters = this.GetPartyFactory().CreatePartyParameter();
                searchPartyParameters.PartyTypeCategory = PartyTypeCategories.Customer;
                if (searchPartyParameters.ListOfGroupUID == null)
                {
                    searchPartyParameters.ListOfGroupUID = new List<Guid>();
                }
                searchPartyParameters.ListOfGroupUID.AddRange(groups);
                var searchPartyResult = partyManager.GetParties(searchPartyParameters);

                if (searchPartyResult.Success)
                {
                    return searchPartyResult.Content;
                }
            }

            return null;
        }

    }
}