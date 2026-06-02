using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IShipCarrierCategory
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
    }
}
