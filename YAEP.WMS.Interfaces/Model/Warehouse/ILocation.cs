using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ILocation
    {
        Guid SlotUID { get; set; }
        string SlotName { get; set; }
        string SlotID { get; set; }
        Guid BinUID { get; set; }
        string BinName { get; set; }
        string BinID { get; set; }
        Guid AreaUID { get; set; }
        string AreaName { get; set; }
        string AreaID { get; set; }
    }
}
