using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IMaintainWorkderParameters
    {
        Guid[] TicketInfoUID { get; set; }
        Guid[] GroupUID { get; set; }
       
    }
}
