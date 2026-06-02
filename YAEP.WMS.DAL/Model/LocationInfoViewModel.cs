using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class LocationInfoViewModel : ILocationInfoViewModel
    {
        public string WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public Guid AreaUID { get; set; }
        public string AreaID { get; set; }
        public string AreaName { get; set; }
        public Guid BinUID { get; set; }
        public string BinID { get; set; }
        public string BinName { get; set; }
        public string SlotID { get; set; }
        public string SlotName { get; set; }
        public Guid SlotUID { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }

    }
}
