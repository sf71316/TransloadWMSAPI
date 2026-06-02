using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.NotificationReceiver.Common
{
    public interface IOutboundTicketInfoProcessingRequest : ICommonRequest
    {
        IList<INotificationProcessInfo> ProcessItems { get; set; }
    }
    public interface INotificationProcessInfo
    {
        Guid ProcessItemUID { get; set; }
        int PickQty { get; set; }
    }
}
