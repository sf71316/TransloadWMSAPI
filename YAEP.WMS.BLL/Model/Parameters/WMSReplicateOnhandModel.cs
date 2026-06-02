using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Model
{
    internal class WMSReplicateOnhandModel
    {
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public int Quantity { get; set; }
        public int PayloadType { get; set; }
        public Guid PayloadUID { get; set; }

    }
}
