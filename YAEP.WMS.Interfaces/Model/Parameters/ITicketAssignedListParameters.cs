using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketAssignedListParameters
    {
        Guid? BolUID { get; set; }
        Guid? ManifestUID { get; set; }
        Guid? TicketUID { get; set; }
    }
}
