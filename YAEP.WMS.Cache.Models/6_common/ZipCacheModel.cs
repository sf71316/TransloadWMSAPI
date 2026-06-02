using System;
using YAEP.Common.Interfaces.Models;

namespace YAEP.WMS.Cache.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ZipCacheModel : AbstractCacheModel, IZipModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Latitude { get; set; }
        public string Longtitude { get; set; }
        public int Status { get; set; }
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
            return $"{this.ID} / {this.City} / {this.Latitude} / {this.Longtitude}";
        }


    }
}