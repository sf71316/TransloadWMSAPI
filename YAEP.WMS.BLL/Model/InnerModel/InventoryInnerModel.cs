using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    public class InventoryInnerModel : IInventoryModel
    {
        public Guid UID { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid PackageUID { get; set; }
        public Guid ItemUID { get; set; }
        public int Type { get; set; }
        public int Qty { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int Status { get; set; }
    }
}
