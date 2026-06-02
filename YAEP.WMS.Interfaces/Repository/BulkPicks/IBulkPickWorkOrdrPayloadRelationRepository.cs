using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IBulkPickWorkOrdrPayloadRelationRepository
    {
        IActionResult<bool> Create(IEnumerable<IBulkPickWorkOrderPayloadRelationModel> modelCollection);
        IActionResult<bool> Exist(Guid WorkOrderPayloadUID);
        IActionResult<IEnumerable<IBulkPickNotificationInfoModel>> GetBulkPickOriginalNotificationInfo(IEnumerable<Guid> ticketInfoUID);
    }
}
