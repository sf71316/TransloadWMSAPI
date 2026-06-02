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
    [Table("WMS_ShipMethod")]
    [DbTable("WMS_ShipMethod")]
    public class ShipMethodModel : IShipMethodModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid PartyUID { get; set; }
        public int Type { get; set; }
        public string MethodName { get; set; }
        public string MethodValue { get; set; }
        public bool IsSignature { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
