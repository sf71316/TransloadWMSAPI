using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ILoadingZoneSelectModel
    {
        Guid UID { get; set; }
        int SlotType { get; set; }
        string AreaName { get; set; }
        string BinName { get; set; }
        string SlotName { get; set; }
        bool IsDefaultLoadingZone { get; set; }
    }
}
