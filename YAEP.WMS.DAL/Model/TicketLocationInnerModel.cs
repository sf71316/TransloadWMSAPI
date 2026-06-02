using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class TicketLocationInnerModel : ILocation
    {
        public Guid SlotUID { get; set; }
        public string SlotName { get; set; }
        public Guid BinUID { get; set; }
        public string BinName { get; set; }
        public Guid AreaUID { get; set; }
        public string AreaName { get; set; }
        public string SlotID { get; set; }
        public string BinID { get; set; }
        public string AreaID { get; set; }
    }
}
