using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IUploadTicketResult
    {
        bool IsAllComplete { get; set; }
        string TicketNo { get; set; }
        List<IUploadFailureResult> FailureTicket { get; set; }
    }
    public interface IUploadFailureResult
    {
        string ProductName { get; set; }
    }
}
