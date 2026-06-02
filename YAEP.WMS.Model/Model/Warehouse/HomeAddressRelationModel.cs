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
    [Table("WMS_HomeAddressRelation")]
    [DbTable("WMS_HomeAddressRelation")]
    public class HomeAddressRelationModel : IHomeAddressRelationModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid ItemCategoryUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public int Type { get; set; }
        public int Sequence { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int OutboundType { get; set; }
    }
}
