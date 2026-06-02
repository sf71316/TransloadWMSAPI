  using System;
  using Unity;
  using Unity.Injection;
  using YAEP.Data.ORM.Interfaces;
  using YAEP.Interfaces;
  using YAEP.WMS.BLL.Module;
  using YAEP.WMS.Interfaces;
  using YAEP.WMS.DAL.Repository;
  using YAEP.Data.ORM.Templates;
  using YAEP.WMS.Model;
  using YAEP.Data.ORM.Dapper.Contrib;
  using YAEP.Identities.Interfaces;
  using YAEP.WMS.DI.Agent.Factory;
  using YAEP.Core.Party.Interfaces;
namespace YAEP.WMS.DI.Agent
{
	public sealed class DIRoot 
	{
		private static readonly Lazy<DIRoot > _LazyInstance = new Lazy<DIRoot  >(() => new DIRoot ());
		private  IUnityContainer _Container;
		#region Factory Variable
						private   Lazy<InventoryFactory > _InventoryFactory;
								private   Lazy<ManifestFactory > _ManifestFactory;
								private   Lazy<WarehouseFactory > _WarehouseFactory;
						#endregion
		public DIRoot  InitRoot(IAppSettings appSettings,IConnectionSettings connectionsettings,IAuthenticationInfo AuthenticationInfo, IAuthenticationProvider AuthenticationProvider, IGroupManager groupManager, IPartyManager partyManager)
      {
      _Container= new UnityContainer();
	   //注入 AppSettings 
            this._Container.RegisterInstance<IAppSettings>(appSettings);
			 //注入 IGroupManager 
            this._Container.RegisterInstance<IGroupManager>(groupManager);
            //注入 IPartyManager 
            this._Container.RegisterInstance<IPartyManager>(partyManager);
      // 注入 登入驗證提供者
            this._Container.RegisterInstance<IAuthenticationInfo>(AuthenticationInfo);
            this._Container.RegisterInstance<IAuthenticationProvider>(AuthenticationProvider);
        // 注入 連線資訊提供者
        this._Container.RegisterInstance<IConnectionSettings>(connectionsettings);
          #region Factory init
            _InventoryFactory  = new Lazy<InventoryFactory  >(() => new InventoryFactory (this._Container));
  _ManifestFactory  = new Lazy<ManifestFactory  >(() => new ManifestFactory (this._Container));
  _WarehouseFactory  = new Lazy<WarehouseFactory  >(() => new WarehouseFactory (this._Container));
		
				

				#endregion 
         // 注入Sequence Agent
		 DbEntities dbEntities = new DbEntities(connectionsettings);
		this._Container.RegisterInstance<IObjectRelationalMappingLayer>(dbEntities);
        //this._Container.RegisterType<IObjectRelationalMappingLayer, DbEntities>();
        this._Container.RegisterType<IModelDescriptor<SeqenceModel>, GenericModelDescriptor<SeqenceModel>>();
        this._Container.RegisterType<IRepositoryHandler<SeqenceModel>, GenericRepositoryHandler<SeqenceModel>>();
        this._Container.RegisterType<ISequenceRepository, SequenceRepository<SeqenceModel>>();
        this._Container.RegisterType<ISequenceAgent, SequenceAgent>();
        return this;
		}
        public ISequenceAgent GetSequenceAgent()
        {
            return this._Container.Resolve<ISequenceAgent>();
        }
        public static ILogInfiltrator GetLogger()
        {
            return Logger.GetLogger();
        }
	//	public static DIRoot  Instance { get { return _LazyInstance.Value; } }
		#region Factory Proerpties
			public   InventoryFactory  InventoryFactory
	{
		get
			{
						 return _InventoryFactory.Value;
			}
	}
			public   ManifestFactory  ManifestFactory
	{
		get
			{
						 return _ManifestFactory.Value;
			}
	}
			public   WarehouseFactory  WarehouseFactory
	{
		get
			{
						 return _WarehouseFactory.Value;
			}
	}
				#endregion
		public IUnityContainer Container
        {
            get
            {
                return this._Container;
            }
        }
	}
}
