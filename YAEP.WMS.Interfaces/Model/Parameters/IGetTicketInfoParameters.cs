using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetTicketInfoParameters
    {
        IEnumerable<Guid> TicketUIDs { get; set; }
        IEnumerable<Guid> TicketInfoUIDs { get; set; }
        IEnumerable<String> TicketIDs { get; set; }
    }
}
