using System;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomerCacheModel : PartyCacheModel, IPartyModel
    {
        public PartyTypeCategories Type { get; set; } = PartyTypeCategories.Customer;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.ID} / {this.Name} / {this.Type}";
        }


    }
}