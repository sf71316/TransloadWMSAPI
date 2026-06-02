using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public class WarehouseAgent : IWarehouseAgent
    {
        public WarehouseAgent(IAreaManager areaManager,
            IBinManager binManger,
            ISlotManager slotManager,
            IWarehouseManger warehouseManager,
            IHomeAddressRelationManager homeAddressRelationManager)
        {
            this.WarehouseManager = warehouseManager;
            this.AreaManager = areaManager;
            this.BinManager = binManger;
            this.SlotManager = slotManager;
            this.HomeAddressRelationManager = homeAddressRelationManager;
        }

        public IWarehouseManger WarehouseManager { get; set; }
        public IAreaManager AreaManager { get; set; }
        public ISlotManager SlotManager { get; set; }
        public IBinManager BinManager { get; set; }
        public IHomeAddressRelationManager HomeAddressRelationManager { get; set; }
    }
}
