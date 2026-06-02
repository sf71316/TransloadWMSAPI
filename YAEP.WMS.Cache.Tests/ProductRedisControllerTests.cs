using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Common.DI;
using YAEP.Core.Item.DI;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.DI;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.DI;
using YAEP.Templates.DI.Unity;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Cache.Redis.Controllers;

namespace YAEP.WMS.Cache.Tests
{
    [TestClass()]
    public class ProductRedisControllerTests
    {
        [TestMethod()]
        public void GetAllTest()
        {
            var productController = new ProductRedisController();
            var all = productController.RetrieveAll();

            if ((all?.Count() ?? 0) == 0)
            {
                reloadAllProduct();
            }

            all = productController.RetrieveAll();
        

            Assert.IsTrue((all?.Count() ?? 0) > 0);
        }

        private void reloadAllProduct()
        {
            var itemFactory = GetItemFactoryInstance();

            var itemManager = itemFactory.CreateItemManager();
            var itemParameters = itemFactory.CreateItemSearchParameters();
            var items = itemManager.GetItems<ProductCacheModel>(itemParameters).Content;
            var categoryRelations = itemManager.GetAllCategoryRelation().Content;

            var partyFactory = GetPartyFactoryInstance();
            var partyManager = partyFactory.CreatePartyManager();
            var customers = partyManager.GetParties(PartyTypeCategories.Customer).Content;

            var cacheData = new HashSet<ProductCacheModel>(items.Select(o =>
            {
                // Customer Name
                o.CustomerName = customers.FirstOrDefault(c => c.UID == o.CustomerUID)?.Name;

                // Category
                o.CategoryUID = (categoryRelations.FirstOrDefault(r => r.ItemUID == o.UID)?.CategoryUID ?? Guid.Empty);

                return o;
            }));

            var productController = new ProductRedisController();
            productController.Create(cacheData);
        }

        private static T initFactory<T>(T abstractFactory) where T : AbstractFactory
        {
            abstractFactory.RegisterInstance<IAuthenticationInfo>(GetAuthenticationInfo());
            abstractFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return abstractFactory;
        }

        private static PackageFactory GetPackageFactoryInstance()
        {
            return initFactory(new PackageFactory());
        }

        private static ItemFactory GetItemFactoryInstance()
        {
            return initFactory(new ItemFactory());
        }

        private static CommonFactory GetCommonFactoryInstance()
        {
            return initFactory(new CommonFactory());
        }

        private static PartyFactory GetPartyFactoryInstance()
        {
            return initFactory(new PartyFactory());
        }

        private static IConnectionSettings _ConnectionSettings = null;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IConnectionSettings GetConnectionSettings()
        {
            if (_ConnectionSettings == null)
            {
                _ConnectionSettings = new ConnectionSettings();
            }

            return _ConnectionSettings;
        }

        private static IAuthenticationInfo _AuthenticationInfo = null;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IAuthenticationInfo GetAuthenticationInfo()
        {
            if (_AuthenticationInfo == null)
            {
                _AuthenticationInfo = new DrKnowAllAuthenticationInfo();
            }

            return _AuthenticationInfo;
        }

    }
}