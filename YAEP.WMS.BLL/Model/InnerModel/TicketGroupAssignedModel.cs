using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class TicketGroupAssignedInnerModel : ITicketGroupAssignedModel
    {
        public Guid UID { get; set; }
        public string GroupName { get; set; }
        public string Members { get; set; }
    }
}
