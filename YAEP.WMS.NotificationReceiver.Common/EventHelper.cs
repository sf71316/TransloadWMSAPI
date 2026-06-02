using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.NotificationReceiver.Common
{
    public class EventHelper
    {
        public const string INBOUND_TICKET_COMPLETED = "Inbound_Ticket_Completed";
        public const string INBOUND_TICKET_INFO_COMPLETED = "Inbound_Ticket_Info_Completed";
        public const string INBOUND_TICKET_INFO_PROCESSING = "Inbound_Ticket_Info_Processing";
        public const string OUTBOUND_TICKET_COMPLETED = "Outbound_Ticket_Completed";
        public const string OUTBOUND_TICKET_INFO_COMPLETED = "Outbound_Ticket_Info_Completed";
        public const string OUTBOUND_TICKET_INFO_PROCESSING = "Outbound_Ticket_Info_Processing";
        public const string OUTBOUND_MOVE_TICKET_INFO_COMPLETED = "Outbound_Move_Ticket_Info_Completed";
        public const string MANIFEST_DELETED = "Manifest_Deleted";
        public const string BOL_DELETED = "BOL_Deleted";
    }
}
