using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Warehouse")]
    [DbTable("WMS_Warehouse")]
    public class WarehouseModel : IWarehouseModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        [ExplicitKey]
        public string ID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Address { get; set; }
        public decimal Volume { get; set; }
        public int Status { get; set; } = (int)WarehouseStatus.Active;
        public string Description { get; set; }
        public Guid? PhotoUID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid GroupUID { get; set; }
        public string Contact { get; set; }
        public string Mail { get; set; }
        public string Fax { get; set; }
    }
}