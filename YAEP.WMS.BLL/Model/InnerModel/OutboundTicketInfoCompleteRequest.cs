using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.BLL.Model
{
    internal class OutboundTicketInfoCompleteRequest : IOutboundTicketInfoCompleteRequest
    {
        public OutboundTicketInfoCompleteRequest()
        {
            this.ProcessItems = new List<INotificationProcessInfo>();
        }

        public IList<INotificationProcessInfo> ProcessItems { get; set; }
        public string RefNo { get; set; }
        public string Sender { get; set; }

    }
}
