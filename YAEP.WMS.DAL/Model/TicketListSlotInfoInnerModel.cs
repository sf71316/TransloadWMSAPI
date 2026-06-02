using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    public class TicketListSlotInfoInnerModel : ITicketListSlotInfoModel
    {
        public Guid TicketUID { get; set; }
        public int Type { get; set; }
        public Guid SlotUID { get; set; }
        public string SlotName { get; set; }
        public Guid LoadingZoneSlotUID { get; set; }
        public string LoadingZoneSlotName { get; set; }
        public int ManifestType { get; set; }
    }
}
