using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class TicketUploadAttachmentParameters : ITicketUploadAttachmentParameters
    {
        public Guid BelongToGuid { get; set; }
        public int BelongToType { get; set; }
        public Guid? AttachmentTypeUID { get; set; }
        public HttpPostedFile File { get; set; }
    }
}
