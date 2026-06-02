using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Cache.Redis.Controllers;

namespace YAEP.WMS.Cache.Redis
{
    /*
    *  Party 
    */
    public static partial class DrKnowAll
    {
        /// <summary>
        /// get customer list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CustomerCacheModel> GetCustomer()
        {
            var customerController = new CustomerRedisController();
            var allCustomer = customerController.RetrieveAll();

            if ((allCustomer?.Count() ?? 0) == 0)
            {
                var manager = FactoryUtils.GetPartyFactoryInstance().CreatePartyManager();
                var dataResult = manager.GetParties(PartyTypeCategories.Customer);
                if (dataResult.Success)
                {
                    allCustomer = Copy(dataResult.Content);

                    customerController.Create(allCustomer);
                }
            }

            return allCustomer;
        }
        /// <summary>
        /// get customer
        /// </summary>
        /// <param name="customerUID"></param>
        /// <returns></returns>
        public static CustomerCacheModel GetCustomer(Guid customerUID)
        {
            if (customerUID == Guid.Empty)
            {
                return null;
            }

            return GetCustomer().FirstOrDefault(o => o.UID == customerUID);
        }
        /// <summary>
        /// get customer
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public static CustomerCacheModel GetCustomer(string customerID)
        {
            if (String.IsNullOrWhiteSpace(customerID))
            {
                return null;
            }

            return GetCustomer().FirstOrDefault(o => o.ID.Equals(customerID, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<CustomerCacheModel> Copy(IEnumerable<IPartyModel> source)
        {
            return (source?.Select(o => new CustomerCacheModel()
            {
                UID = o.UID,
                GroupUID = o.GroupUID,
                ID = o.ID,
                Description = o.Description,
                Name = o.Name,
                Status = o.Status,
                Country = o.Country,
                State = o.State,
                City = o.City,
                Zip = o.Zip,
                Address = o.Address,
                Phone = o.Phone,
                PhoneExtension = o.PhoneExtension,
                Fax = o.Fax,
                Email = o.Email,
                CreatedBy = o.CreatedBy,
                CreatedOn = o.CreatedOn,
                ModifiedBy = o.ModifiedBy,
                ModifiedOn = o.ModifiedOn,
            }) ?? new CustomerCacheModel[] { }).ToArray();
        }

    }

}