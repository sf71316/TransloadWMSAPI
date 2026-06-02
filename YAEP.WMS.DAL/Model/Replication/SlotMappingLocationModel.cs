using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class SlotMappingLocationModel : ISlotMappingLocation
    {
        public Guid UID { get; set; }
        public string SlotName { get; set; }
        public int WarehouseID { get; set; }
        public int LocationID { get; set; }
    }
}
