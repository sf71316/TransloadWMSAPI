using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IPayloadRepository
    {

        IActionResult<IPayloadModel> GetPayload(Guid PayloadUID);
        IActionResult<IDeallocatedPayloadInfoModel> FindDeallocatedRelatedPayloadCollection(IEnumerable<Guid> allocatedPayloadUID);
        IActionResult<IEnumerable<IPayloadModel>> FindList(IEnumerable<Guid> PayloadUID, PayloadType type);
        IActionResult<IEnumerable<IPayloadModel>> FindList(IEnumerable<Guid> PayloadUID);
        IActionResult<bool> AddPayload(IPayloadModel Model);
        IActionResult<bool> BatchAddPayload(IEnumerable<IPayloadModel> Model);
        IActionResult<bool> ChangeSlotUID(Guid PayloadUID, Guid SlotUID);
        IActionResult<bool> UpatePayload(IPayloadModel Model);
        IActionResult<bool> BatchUpatePayload(IEnumerable<IPayloadModel> Model);
        IActionResult<int> GetPayloadByPackageQty(Guid packageUID);
        IActionResult<IEnumerable<IPayloadModel>> GetList(object condition);
        IActionResult<IEnumerable<IPayloadWithOriginalPayloadTypeModel>> GetListWithOriginalPayloadType(Guid itemuid, Guid slotuid);
        IActionResult<IEnumerable<IPayloadModel>> GetListByTicket(Guid ticketUID);
        IActionResult<IEnumerable<IAllocatedModel>> GetAllocatedData(Guid[] warehouseUID, Guid[] itemUID);
        IActionResult<bool> ChangePayloadStauts(Guid payloaduid, PayloadStatus status);
        IActionResult<bool> ChangePayloadStauts(IEnumerable<Guid> payloaduid, PayloadStatus status, string modifiedBy = "");
        IActionResult<bool> ChangePayloadType(Guid payloaduid, int type);
        IActionResult<IPayloadModel> GetRecoveryPayload(object condition);

        IActionResult<bool> DeletePayloadFromDb(object condition);
        IActionResult<bool> ChangePayloadType(IEnumerable<Guid> payloaduid, int type);
        IActionResult<bool> ReplenishmentPayload(IPayloadModel payloadModel);
        IActionResult<IEnumerable<IPayloadModel>> GetOnhandPayload(Guid warehouseUID, IEnumerable<Guid> itemNo, int[] slotStatus);

        IActionResult<IEnumerable<IPayloadModel>> GetOnhandPayload(IEnumerable<Guid> warehouseUID, IEnumerable<Guid> itemUID, int[] payloadType, int[] slotType);
    }
}
