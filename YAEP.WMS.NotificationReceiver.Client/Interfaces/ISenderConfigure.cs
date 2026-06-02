using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  YAEP.WMS.NotificationSender.Client
{
    public interface ISenderConfigure
    {
        string ReceiverUrl { get; }
        string ReceiverSecret { get; }
    }
}
