using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.NotificationSender.Client
{
    internal class SenderAPI : AbstractSenderAPI
    {
        public SenderAPI(ISenderConfigure configure) : base(configure)
        {
            _Client = new RestClient();
            _Client.BaseUrl = new Uri(configure.ReceiverUrl);
            this.RetryCount = 1;
            this.RetryInterval = 1 * 1000;
            _Client.Timeout = 30 * 1000;
        }
        public override IAPIResult<bool> SendNotify(INotificationRequest request)
        {
            var response = InnerActionPostMethod<ActionResult<bool>>(NOTIFICATION_COMMAND, request, Method.POST, DataFormat.Json);
            if (response.Item1 != null)
            {
                return response.Item1;
            }
            else
            {
                ActionResult<bool> result = new ActionResult<bool>();
                if (!string.IsNullOrEmpty(response.Item2.ErrorMessage))
                    result.Message = response.Item2.ErrorMessage;
                return result;
            }
        }
    }
}
