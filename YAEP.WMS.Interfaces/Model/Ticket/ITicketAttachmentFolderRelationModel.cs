using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketAttachmentFolderRelationModel
    {
        Guid UID { get; set; }
        Guid BelongToUID { get; set; }
        int BelongToType { get; set; }
        Guid AttachmentFolderUID { get; set; }
        int Status { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}
