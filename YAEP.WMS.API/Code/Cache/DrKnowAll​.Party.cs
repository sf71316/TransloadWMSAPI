using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.DI;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Utilities;

namespace YAEP.WMS.Api.Code.Cache
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
        public static IEnumerable<IPartyModel> GetCustomer()
        {
            var enumType = DrKnowAllKeys.Customer;

            var knowledge = Instance.Recollect<IPartyModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetPartyFactory().CreatePartyManager();
                var dataResult = manager.GetParties(PartyTypeCategories.Customer);
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// get customer
        /// </summary>
        /// <param name="customerUID"></param>
        /// <returns></returns>
        public static IPartyModel GetCustomer(Guid customerUID)
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
        public static IPartyModel GetCustomer(string customerID)
        {
            if (String.IsNullOrWhiteSpace(customerID))
            {
                return null;
            }

            return GetCustomer().FirstOrDefault(o => o.ID.Equals(customerID, StringComparison.OrdinalIgnoreCase));
        }

       

    }

}