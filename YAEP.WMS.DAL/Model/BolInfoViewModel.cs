using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class BolInfoViewModel : IBolInfoViewModel
    {
        public string BolName { get; set; }
        public string BolRefNo { get; set; }
        public string VesselRefNo { get; set; }
        public DateTime ETA { get; set; }
        public string ShipVia { get; set; }
        public string WarehouseName { get; set; }
        public Guid WarehouseGroupUID { get; set; }
    }
}
