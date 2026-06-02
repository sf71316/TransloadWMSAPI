using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Pod")]
    [DbTable("WMS_Pod")]
    public class PodModel : IPodModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public bool IsPack { get; set; }
        //public Guid VesselUID { get; set; }
        public decimal? VolumeLimit { get; set; }
        public decimal? WeightLimit { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
