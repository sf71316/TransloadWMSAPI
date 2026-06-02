using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Cache.Redis.Controllers;


namespace YAEP.WMS.Cache.Redis
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
        public static IEnumerable<WarehouseCacheModel> GetWarehouse()
        {
            var controller = new WarehouseRedisController();
            var allWarehouse = controller.RetrieveAll();

            if ((allWarehouse?.Count() ?? 0) == 0)
            {
                var repository = FactoryUtils.GetWarehouseRepository();

                var data = repository.GetWarehouseList()?.Content;
                if ((data?.Count() ?? 0) > 0)
                {
                    allWarehouse = data.Select(o => new WarehouseCacheModel()
                    {
                        UID = o.UID,
                        GroupUID = o.GroupUID,
                        ID = o.ID,
                        Name = o.Name,
                        Phone = o.Phone,
                        Fax = o.Fax,
                        Country = o.Country,
                        State = o.State,
                        City = o.City,
                        Zip = o.Zip,
                        Address = o.Address,
                        Volume = o.Volume,
                        Status = o.Status,
                        Description = o.Description,
                        Mail = o.Mail,
                        Contact = o.Contact,
                        PhotoUID = o.PhotoUID,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        ModifiedBy = o.ModifiedBy,
                        ModifiedOn = o.ModifiedOn,
                    }).ToArray();

                    controller.Create(allWarehouse);
                }
            }

            return allWarehouse;
        }
        /// <summary>
        /// get warehouse
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        public static WarehouseCacheModel GetWarehouse(Guid warehouseUID)
        {
            if (warehouseUID == Guid.Empty)
            {
                return null;
            }

            return GetWarehouse().FirstOrDefault(o => o.UID == warehouseUID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupUIDs"></param>
        /// <returns></returns>
        public static IEnumerable<WarehouseCacheModel> GetWarehouse(IEnumerable<Guid> groupUIDs)
        {

            if ((groupUIDs?.Count() ?? 0) == 0)
            {
                return new WarehouseCacheModel[] { };
            }

            var allWarehouse = GetWarehouse();

            var warehouse = allWarehouse.Where(o => groupUIDs.Any(guid => guid == o.GroupUID)).ToArray();

            return warehouse;
        }
    }

}