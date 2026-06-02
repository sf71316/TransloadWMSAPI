using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class InventorySearchParameters : IInventorySearchParameters
    {
        public IEnumerable<Guid> PHierarchy { get; set; } = null;
        public IEnumerable<string> CHierarchy { get; set; }
        public Guid? CustomerUID { get; set; }
        public Guid? WarehouseUID { get; set; }
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public Guid? SlotUID { get; set; }
    }
}
