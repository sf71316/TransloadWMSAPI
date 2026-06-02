using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class MaintainWorkderInnerParameters : IMaintainWorkderParameters
    {
        public Guid[] TicketInfoUID { get; set; }
        public Guid[] GroupUID { get; set; }
    }
}
