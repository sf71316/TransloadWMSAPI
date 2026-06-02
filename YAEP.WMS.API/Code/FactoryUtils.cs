using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Common.DI;
using YAEP.Core.Item.DI;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.DI;
using YAEP.Core.Party.DI;
using YAEP.SSO.DI;
using YAEP.Package.Interfaces;
using YAEP.WMS.Interfaces;
using YAEP.Identities.DI;

namespace YAEP.WMS.Api.Code
{
    /// <summary>
    /// 
    /// </summary>
    public static class FactoryUtils
    {
        private static PackageFactory _PackageFactory = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationInfo"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        public static PackageFactory GetPackageFactory(IAuthenticationInfo authenticationInfo)
        {
            var packageFactory = new PackageFactory();
            packageFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            packageFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return packageFactory;
            //if (_PackageFactory == null)
            //{
            //    _PackageFactory = new PackageFactory();
            //    _PackageFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            //    _PackageFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            //}

            //return _PackageFactory;
        }

        private static ItemFactory _ItemFactory = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationInfo"></param>
        /// <returns></returns>
        public static ItemFactory GetItemFactory(IAuthenticationInfo authenticationInfo)
        {
            var itemFactory = new ItemFactory();
            itemFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            itemFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return itemFactory;
            //if (_ItemFactory == null)
            //{
            //    _ItemFactory = new ItemFactory();
            //    _ItemFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            //    _ItemFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            //}

            //return _ItemFactory;
        }

        private static CommonFactory _CommonFactory = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationInfo"></param>
        /// <returns></returns>
        public static CommonFactory GetCommonFactory(IAuthenticationInfo authenticationInfo)
        {
            var commonFactory = new CommonFactory();
            commonFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            commonFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return commonFactory;
            //if (_CommonFactory == null)
            //{
            //    _CommonFactory = new CommonFactory();
            //    _CommonFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            //    _CommonFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            //}

            //return _CommonFactory;
        }

        private static PartyFactory _PartyFactory = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationInfo"></param>
        /// <returns></returns>
        public static PartyFactory GetPartyFactory(IAuthenticationInfo authenticationInfo)
        {
            var partyFactory = new PartyFactory();
            partyFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            partyFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return partyFactory;
            //if (_PartyFactory == null)
            //{
            //    _PartyFactory = new PartyFactory();
            //    _PartyFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            //    _PartyFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            //}

            //return _PartyFactory;
        }

        private static Factory _SsoFactory = null;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Factory GetSsoFactory()
        {
            if (_SsoFactory == null)
            {
                _SsoFactory = new Factory();
                _SsoFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            }

            return _SsoFactory;
        }

        private static DeviceFactory _DeviceFactory = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationInfo"></param>
        /// <returns></returns>
        public static DeviceFactory GetDeviceFactory(IAuthenticationInfo authenticationInfo)
        {
            var deviceFactory = new DeviceFactory();
            deviceFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            deviceFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return deviceFactory;
            //if (_DeviceFactory == null)
            //{
            //    _DeviceFactory = new DeviceFactory();
            //    _DeviceFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            //    _DeviceFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            //}

            //return _DeviceFactory;
        }

        private static IdentityFactory _IdentityFactory = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationInfo"></param>
        /// <returns></returns>
        public static IdentityFactory GetIdentityFactory(IAuthenticationInfo authenticationInfo)
        {
            var identityFactory = new IdentityFactory();
            identityFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            identityFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return identityFactory;
            //if (_IdentityFactory == null)
            //{
            //    _IdentityFactory = new IdentityFactory();
            //    _IdentityFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            //    _IdentityFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            //}

            //return _IdentityFactory;
        }


        private static IConnectionSettings _ConnectionSettings = null;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IConnectionSettings GetConnectionSettings()
        {
            if (_ConnectionSettings == null)
            {
                _ConnectionSettings = new ConnectionSettings();
            }

            return _ConnectionSettings;
        }

    }
}