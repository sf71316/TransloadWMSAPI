using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketRelationRepository
    {

        IActionResult<bool> Add(IEnumerable<ITicketRelationModel> Collection);
        IActionResult<bool> Delete(Guid uid);
        IActionResult<int?> GetParentTicketServiceType(Guid ticketUID);
        IActionResult<IEnumerable<ITicketRelationModel>> GetTicketRelationList(object condition);
    }
}
