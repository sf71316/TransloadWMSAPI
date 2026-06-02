using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IRollbackTicketRequest
    {
        string RequestBy { get; set; }
        Guid[] BolRefUID { get; set; }
        string RefNo { get; set; }
    }
}
