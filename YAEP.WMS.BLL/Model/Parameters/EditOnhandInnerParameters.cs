using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class EditOnhandInnerParameters : IEditOnhandParameters
    {
        public Guid WarehouseUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid TargetPackageUID { get; set; }
        public Guid SlotUID { get; set; }
        public int Onhand { get; set; }
    }
}
