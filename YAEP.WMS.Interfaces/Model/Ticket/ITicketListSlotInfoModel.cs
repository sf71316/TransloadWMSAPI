using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketListSlotInfoModel
    {
        Guid TicketUID { get; set; }
        int Type { get; set; }
        int ManifestType { get; set; }
        Guid SlotUID { get; set; }
        string SlotName { get; set; }
        Guid LoadingZoneSlotUID { get; set; }
        string LoadingZoneSlotName { get; set; }
    }
}
