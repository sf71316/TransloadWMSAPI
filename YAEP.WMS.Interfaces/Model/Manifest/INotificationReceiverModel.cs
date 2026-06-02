using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface INotificationReceiverModel
    {
        Guid UID { get; set; }
        Guid BelongToUID { get; set; }
        string ReceiverUrl { get; set; }
        string ReceiverSecret { get; set; }
        int Status { get; set; }
    }
}
