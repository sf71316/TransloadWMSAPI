using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;
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
    public class WarehouseFactory : AbstractMiddleLayerFactory
    {
        public WarehouseFactory(IUnityContainer container) : base(container)
        {
            this.CallPublicDIPool();
            this.registerModelDefinition<ItemModel>();
            this.registerModelDefinition<PackageVersionViewModel>();
            //// 注入 Model 定義 
            //base.RegisterType<IModelDescriptor<WarehouseModel>, GenericModelDescriptor<WarehouseModel>>();
            //base.RegisterType<IModelDescriptor<BinModel>, GenericModelDescriptor<BinModel>>();
            //base.RegisterType<IModelDescriptor<AreaModel>, GenericModelDescriptor<AreaModel>>();
            //base.RegisterType<IModelDescriptor<SlotModel>, GenericModelDescriptor<SlotModel>>();
            //base.RegisterType<IModelDescriptor<LabelModel>, GenericModelDescriptor<LabelModel>>();
            //// 注入 Repository 處理器
            //container.RegisterType<IRepositoryHandler<WarehouseModel>, GenericRepositoryHandler<WarehouseModel>>();
            //container.RegisterType<IRepositoryHandler<BinModel>, GenericRepositoryHandler<BinModel>>();
            //container.RegisterType<IRepositoryHandler<AreaModel>, GenericRepositoryHandler<AreaModel>>();
            //container.RegisterType<IRepositoryHandler<SlotModel>, GenericRepositoryHandler<SlotModel>>();
            //container.RegisterType<IRepositoryHandler<LabelModel>, GenericRepositoryHandler<LabelModel>>();
            //// 注入 Repository 實作
            //container.RegisterType<IWarehouseRepository, WarehouseRepository<WarehouseModel>>();
            //container.RegisterType<IAreaRepository, AreaRepository<AreaModel>>();
            //container.RegisterType<IBinRepository, BinRepository<BinModel>>();
            //container.RegisterType<ISlotRepository, SlotRepository<SlotModel>>();
            //container.RegisterType<ILabelRepository, LabelRepository<LabelModel>>();

            // 注入 物件
            container.RegisterType<IWarehouseDeleteParameters, WarehouseDeleteParamters>();
            container.RegisterType<IWarehouseComponentParameters, WarehouseComponentParameters>();

        }
        public IWarehouseAgent CreateWarehouseManger()
        {
            var instance = base.Resolve<IWarehouseAgent>();
            return instance;
        }
        public ILabelManager CreateLabelManager()
        {
            var instance = base.Resolve<ILabelManager>();
            return instance;
        }
        public IWarehouseDeleteParameters GenerateDeleteWarehouseParameters()
        {
            var instance = base.Resolve<IWarehouseDeleteParameters>();
            return instance;
        }
        private void registerModelDefinition<TModel>() where TModel : class
        {
            // 注入 Model 定義  
            base.RegisterType<IModelDescriptor<TModel>, GenericModelDescriptor<TModel>>();
            // 注入 Repository 處理器
            base.RegisterType<IRepositoryHandler<TModel>, GenericRepositoryHandler<TModel>>();
        }
    }
}
