using System;
using System.Collections.Generic;
using Unity;
using YAEP.Cache;
using YAEP.Common.DI;
using YAEP.Core.Item.DI;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.DI;
using YAEP.Data.ORM.Interfaces;
using YAEP.Identities.DI;
using YAEP.Interfaces;
using YAEP.Package.DI;
using YAEP.Package.Interfaces;
using YAEP.SSO.DI;
using YAEP.Templates.DI.Unity;
using YAEP.WMS.API.Code;
using YAEP.WMS.Controllers.Api;
using YAEP.WMS.DI.Agent;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Api.Code.Cache
{
    /// <summary>
    /// 全知博士
    /// </summary>
    public static partial class DrKnowAll
    {
        private static Lazy<IdentityFactory> _IdentityFactory = new Lazy<IdentityFactory>(() => GetIdentityFactoryInstance());
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IdentityFactory GetIdentityFactory()
        {
            return _IdentityFactory.Value;
        }

        private static Lazy<CommonFactory> _CommonFactory = new Lazy<CommonFactory>(() => GetCommonFactoryInstance());
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static CommonFactory GetCommonFactory()
        {
            return _CommonFactory.Value;
        }

        private static Lazy<ItemFactory> _ItemFactory = new Lazy<ItemFactory>(() => GetItemFactoryInstance());
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static ItemFactory GetItemFactory()
        {
            return _ItemFactory.Value;
        }

        private static Lazy<PartyFactory> _PartyFactory = new Lazy<PartyFactory>(() => GetPartyFactoryInstance());
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static PartyFactory GetPartyFactory()
        {
            return _PartyFactory.Value;
        }

        private static Lazy<PackageFactory> _PackageFactory = new Lazy<PackageFactory>(() => GetPackageFactoryInstance());
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static PackageFactory GetPackageFactory()
        {
            return _PackageFactory.Value;
        }


        private static Lazy<DIRoot> DIContainer = new Lazy<DIRoot>(() =>
        {
            var connectionSetting = GetConnectionSettings();
            var auth = GetAuthenticationInfo();
            var authProvider = new AuthenticationInfoProvider();
            var defaultconfig = new DefaultAppSettings();
            var root = new DIRoot();

            root = root.InitRoot(defaultconfig, connectionSetting, auth, authProvider,
                GetIdentityFactoryInstance().CreateGroupManager(), GetPartyFactory().CreatePartyManager());
            root.Container.RegisterInstance<IItemManager>(GetItemFactory().CreateItemManager());
            root.Container.RegisterInstance<IPackageManager>(GetPackageFactory().CreatePackageManager());
            root.Container.RegisterInstance<IPackageUomManager>(GetPackageFactory().CreatePackageUomManager());
            root.Container.RegisterInstance<IRefreshDrKnowAll>(new RefreshDrKnowAll());

            return root;
        });
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static DIRoot GetDIContainer()
        {
            return DIContainer.Value;
        }


        #region Factories

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

        private static Factory GetSsoFactory()
        {
            return initFactory(new Factory());
        }

        private static DeviceFactory GetDeviceFactoryInstance()
        {
            return initFactory(new DeviceFactory());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IdentityFactory GetIdentityFactoryInstance()
        {
            return initFactory(new IdentityFactory());
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

        #endregion
    }

}