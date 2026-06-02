using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class BulkPickNotificationInfoModel : IBulkPickNotificationInfoModel
    {
        public Guid TicketInfoUID {get;set;}
        public Guid WorkOrderPayloadUID {get;set;}
        public string RefNo {get;set;}
        public string ReceiverUrl {get;set;}
        public string ReceiverSecret {get;set;}
        public int ActQty {get;set;}
    }
}
