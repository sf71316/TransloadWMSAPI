using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketGroupAssignedModel
    {
        Guid UID { get; set; }
        string GroupName { get; set; }
        string Members { get; set; }
    }
}
