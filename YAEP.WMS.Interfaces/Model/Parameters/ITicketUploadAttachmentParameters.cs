using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketUploadAttachmentParameters
    {
        Guid BelongToGuid { get; set; }
        int BelongToType { get; set; }
        Guid? AttachmentTypeUID { get; set; }
        HttpPostedFile File { get; set; }
    }
}
