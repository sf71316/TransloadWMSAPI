using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Model
{
    internal class OutboundMoveTicketMappingModel
    {
        public Guid TicketUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid PackageUID { get; set; }
    }
}
