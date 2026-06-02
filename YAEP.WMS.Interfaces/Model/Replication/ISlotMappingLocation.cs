using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISlotMappingLocation
    {
        Guid UID { get; set; }
        string SlotName { get; set; }
        int WarehouseID { get; set; }
        int LocationID { get; set; }
    }
}
