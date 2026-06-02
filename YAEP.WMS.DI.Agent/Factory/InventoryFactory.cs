using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using YAEP.Data.ORM.Dapper.Contrib;
using YAEP.Data.ORM.Interfaces;
using YAEP.Data.ORM.Templates;
using YAEP.Templates.DI.Unity;
using YAEP.WMS.BLL.Manager;
using YAEP.WMS.DAL.Repository;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;

namespace YAEP.WMS.DI.Agent.Factory
{
    public class InventoryFactory : AbstractMiddleLayerFactory
    {
        public InventoryFactory(IUnityContainer container) : base(container)
        {
            this.CallPublicDIPool();
            this.registerModelDefinition<ItemModel>();
            this.registerModelDefinition<PackageVersionViewModel>();
            this.registerModelDefinition<TicketInfoModel>();
            base.RegisterType<ITicketInfoRepository, TicketInfoRepository<TicketInfoModel>>();
        }
        private void registerModelDefinition<TModel>() where TModel : class
        {
            // 注入 Model 定義  
            base.RegisterType<IModelDescriptor<TModel>, GenericModelDescriptor<TModel>>();
            // 注入 Repository 處理器
            base.RegisterType<IRepositoryHandler<TModel>, GenericRepositoryHandler<TModel>>();
        }
        public IInventoryManager CreateInventoryManager()
        {
            var instance = base.Resolve<IInventoryManager>();
            return instance;
        }
    }
}
