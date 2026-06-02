using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    public class UploadTicketResult : IUploadTicketResult
    {
        public UploadTicketResult()
        {
            this.FailureTicket = new List<IUploadFailureResult>();
        }
        public bool IsAllComplete { get; set; }
        public string TicketNo { get; set; }
        public List<IUploadFailureResult> FailureTicket { get; set; }
    }
    public class UploadFailureResult : IUploadFailureResult
    {
        public string ProductName { get; set; }
    }
}
