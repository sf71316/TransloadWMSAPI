using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL
{
    public static class RequestAction
    {
        public static string PICK_ALL = "PICK_ALL";
        public static string RECEIVING = "Receiving";
        public static string ALLOCATED = "Allocated";
        public static string DEALLOCATED = "deallocated";
        public static string FUTUREALLOCATED = "Futureallocated";
        public static string CANCEL_RECEIVING = "CancelReceiving";
        public static string DELETE_MANIFEST = "DeleteManifest";
        public static string UPLOAD_OUTBOUND_TICKET_BY_POD = "UploadOutboundTicketDataByPod";
    }
}
