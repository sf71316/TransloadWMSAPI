using YAEP.Common.DI;
using YAEP.Core.Item.DI;
using YAEP.Core.Party.DI;
using YAEP.Data.ORM.Dapper.Contrib;
using YAEP.Data.ORM.Interfaces;
using YAEP.Data.ORM.Templates;
using YAEP.Identities.DI;
using YAEP.Interfaces;
using YAEP.Package.DI;
using YAEP.Templates.DI.Unity;
using YAEP.WMS.DAL.Repository;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;

namespace YAEP.WMS.Cache
{
    internal static class FactoryUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PackageFactory GetPackageFactoryInstance()
        {
            return initFactory(new PackageFactory());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ItemFactory GetItemFactoryInstance()
        {
            return initFactory(new ItemFactory());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CommonFactory GetCommonFactoryInstance()
        {
            return initFactory(new CommonFactory());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PartyFactory GetPartyFactoryInstance()
        {
            return initFactory(new PartyFactory());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IdentityFactory GetIdentityFactoryInstance(IAuthenticationInfo authenticationInfo = null)
        {
            return initFactory(new IdentityFactory(), authenticationInfo);
        }

        private static T initFactory<T>(T abstractFactory, IAuthenticationInfo authenticationInfo = null) where T : AbstractFactory
        {
            if (authenticationInfo == null)
            {
                abstractFactory.RegisterInstance<IAuthenticationInfo>(GetAuthenticationInfo());
            }
            else
            {
                abstractFactory.RegisterInstance<IAuthenticationInfo>(authenticationInfo);
            }
            abstractFactory.RegisterInstance<IConnectionSettings>(GetConnectionSettings());
            return abstractFactory;
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

        private static IAuthenticationProvider _AuthenticationProvider = null;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IAuthenticationProvider GetAuthenticationInfoProvider()
        {
            if (_AuthenticationProvider == null)
            {
                _AuthenticationProvider = new AuthenticationInfoProvider(GetAuthenticationInfo());
            }

            return _AuthenticationProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IWarehouseRepository GetWarehouseRepository()
        {
            var authenticationInfoProvider = GetAuthenticationInfoProvider();

            var dbEntities = new DbEntities(new ConnectionSettings());
            var modelDescriptor = new GenericModelDescriptor<WarehouseModel>();
            var repositoryHandler = new GenericRepositoryHandler<WarehouseModel>(dbEntities, authenticationInfoProvider, modelDescriptor);
            var repository = new WarehouseRepository<WarehouseModel>(repositoryHandler);

            return repository;
        }

    }
}
