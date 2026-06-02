using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketInfoAssigneeRelationRepository
    {
        IActionResult<bool> AddWorkder(IMaintainWorkderParameters Parametes, bool isIgnore = false);
        IActionResult<bool> RemoveWorkder(Guid[] tauids);
        IActionResult<bool> ClearAllWorkder(Guid[] TicketInfoUID);
        IActionResult<bool> CheckHasWorkder(Guid TicketInfoUID, Guid GroupUID);
        IActionResult<IEnumerable<ITicketInfoAssigneeRelationModel>> GetAssignedList(Guid[] TicketInfoUID);
        IActionResult<IEnumerable<ITicketInfoModel>> GetRelationTicketInfo(Guid TicketInfoUID);
    }
}
