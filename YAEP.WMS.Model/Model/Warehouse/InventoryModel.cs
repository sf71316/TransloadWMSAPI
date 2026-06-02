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
    [Table("IWMS_nventory")]
    [DbTable("WMS_Inventory")]
    public class InventoryModel : IInventoryModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid ItemUID { get; set; }
        public int Type { get; set; }
        public int Qty { get; set; }
        public Guid PackageUID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int Status { get; set; }
    }
}
