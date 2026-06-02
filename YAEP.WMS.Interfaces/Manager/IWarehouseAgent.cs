using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWarehouseAgent
    {
        IWarehouseManger WarehouseManager { get; set; }
        IAreaManager AreaManager { get; set; }
        ISlotManager SlotManager { get; set; }
        IBinManager BinManager { get; set; }
        IHomeAddressRelationManager HomeAddressRelationManager { get; set; }
    }
}
