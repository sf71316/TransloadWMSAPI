using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IBolInfoViewModel
    {
        string BolName { get; set; }
        string BolRefNo { get; set; }
        string VesselRefNo { get; set; }
        string WarehouseName { get; set; }
        Guid WarehouseGroupUID { get; set; }
        DateTime ETA { get; set; }
        string ShipVia { get; set; }
    }
}
