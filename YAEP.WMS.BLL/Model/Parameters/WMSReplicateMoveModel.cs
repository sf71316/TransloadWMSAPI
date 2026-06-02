using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Model
{
    internal class WMSReplicateMoveModel
    {
        public Guid PayloadUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid TicketUID { get; set; }
        public int Quantity { get; set; }
        public int PayloadType { get; set; }
        public int ManifestType { get; set; }
        public Guid OriginalSlotUID { get; set; }
        public Guid TargetSlotUID { get; set; }
        public Guid? ItemGroup { get; set; }
    }
}
