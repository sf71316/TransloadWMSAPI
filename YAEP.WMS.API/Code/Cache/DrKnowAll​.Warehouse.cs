using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.WMS.Constant;
using YAEP.WMS.DI.Agent;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Api.Code.Cache
{
    /*
    *  Warehouse 
    */
    public static partial class DrKnowAll
    {
        /// <summary>
        /// get warehouse list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IWarehouseModel> GetWarehouse()
        {
            var enumType = DrKnowAllKeys.Warehouse;

            var knowledge = Instance.Recollect<IWarehouseModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetDIContainer().WarehouseFactory.CreateWarehouseManger();
                var dataResult = manager.WarehouseManager.GetWarehouseList();
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// get warehouse
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        public static IWarehouseModel GetWarehouse(Guid warehouseUID)
        {
            if (warehouseUID == Guid.Empty)
            {
                return null;
            }

            return GetWarehouse().FirstOrDefault(o => o.UID == warehouseUID);
        }
    }

}