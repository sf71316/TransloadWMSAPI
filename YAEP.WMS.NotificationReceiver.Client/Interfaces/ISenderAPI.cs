using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.NotificationSender.Client
{
    public interface ISenderAPI
    {
        int Timeout { get; set; }
        int RetryCount { get; set; }
        int RetryInterval { get; set; }
        IAPIResult<bool> SendNotify(INotificationRequest request);

    }
}
