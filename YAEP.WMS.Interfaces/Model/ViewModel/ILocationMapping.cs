using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ILocationMapping
    {
        Guid UID { get; set; }
        string Name { get; set; }
        Guid WarehouseUID { get; set; }
        string LocationID { get; set; }
        string WarehouseID { get; set; }
    }
}
