using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketParentViewModel
    {
        bool IsLock { get; set; }
        string ParentTicketID { get; set; }
        string ParentTicketStatusName { get; set; }
        int? ParentTicketStatus { get; set; }
        Guid ParentTicketUID { get; set; }
        Guid TicketUID { get; set; }
    }
}
