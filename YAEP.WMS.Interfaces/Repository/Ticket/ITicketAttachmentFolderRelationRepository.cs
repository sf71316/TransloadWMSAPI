using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketAttachmentFolderRelationRepository
    {
        IActionResult<bool> Add(dynamic model);
        IActionResult<IEnumerable<ITicketAttachmentFolderRelationModel>> GetAttachmentFolderUID(Guid belongtouid,int belongtotype);
    }
}
