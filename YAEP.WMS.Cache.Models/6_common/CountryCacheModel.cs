using System;
using YAEP.Common.Interfaces.Models; 

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CountryCacheModel : AbstractCacheModel, ICountryModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public string DisplayName { get; set; }
        public string CultureName { get; set; }
        public string ISO2 { get; set; }
        public string ISO3 { get; set; }
        public string Language2 { get; set; }
        public string Language3 { get; set; }
        public string ISOCurrencySymbol { get; set; }
        public string Description { get; set; }
        public int? Sort { get; set; }
        public int? Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.ID} / {this.Name}";
        }

    
    }
}