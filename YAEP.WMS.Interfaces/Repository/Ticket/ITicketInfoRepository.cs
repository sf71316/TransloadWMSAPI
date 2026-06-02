using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketInfoRepository
    {

        IActionResult<bool> AddTickInfos(IEnumerable<ITicketInfoModel> Collection);

        IActionResult<ITicketInfoModel> GetData(Guid TicketInfoUID);
        IActionResult<IEnumerable<ITicketInfoModel>> GetDataByBol(Guid BOLID);

        IActionResult<IEnumerable<ITicketInfoModel>> GetList(Guid[] TicketInfoUID);
        IActionResult<IEnumerable<ITicketInfoModel>> GetList(object condition);
        IActionResult<IManifestModel> GetManifest(Guid TicketInfoUID);

        IActionResult<IEnumerable<ITicketInfoModel>> GetPodBelongTicket(IEnumerable<string> enumerable);

        IActionResult<ITicketInfoCollection> GetTicketInfo(IGetTicketInfoParameters TicketUIDs);
        IActionResult<IEnumerable<ITicketInfoModel>> GetBelongToMoveTicketInfo(IEnumerable<Guid> ticketInfoUIDs);

        IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoByPickAll(IEnumerable<Guid> vesselUID = null, IEnumerable<Guid> workorderPayloadUID = null);

        IActionResult<IEnumerable<ITicketProcessModel>> GetTicketProcessModel(Guid[] TicketInfoUIDs);

        IActionResult<bool> IsTicketComplete(Guid TicketUID, Guid[] notContainsTicketInfoUID);

        IActionResult<bool> RollbackTicketInfo(IEnumerable<Guid> TicketUID, TicketInfoStatus Status);

        IActionResult<Tuple<bool, TicketInfoStatus>> TicketIsAllComplete(Guid TicketUID);

        IActionResult<bool> UpdateTicketInfoStatus(IEnumerable<Guid> TicketInfoUID, TicketInfoStatus Status);

        IActionResult<bool> UpdateTicketInfoStatusByTicket(IEnumerable<Guid> TicketUID, TicketInfoStatus Status);

        IActionResult<bool> UpdateTickInfo(ITicketInfoModel Model);
        IActionResult<bool> UpdateTickInfoProcessQty(ITicketInfoParameter parameters);
        IActionResult<IEnumerable<IReceiviedReplicateModel>> GetReceiviedData(IGetReplicateDataParameter parameter);
        IActionResult<IEnumerable<IAllocatedReplicateModel>> GetAllocatedData(IGetReplicateDataParameter parameter);
        IActionResult<IEnumerable<IAllocatedReplicateModel>> GetAllocatedDataByItemGroup(Guid? itemGroupUID, Guid TicketUID);
        IActionResult<IEnumerable<IAllocatedReplicateModel>> GetAllocatedDataByItemGroupInbound(Guid? itemGroupUID, Guid TicketUID);
        IActionResult<IEnumerable<IPickallViewModel>> GetTicketInfoByPickAll(IPickAllParameters parameters);
        IActionResult<bool> CompleteTicketInfoByTicket(IEnumerable<Guid> TicketUID, string modifiedBy = "");
        IActionResult<IEnumerable<ITicketInfoModel>> GetListByTicket(Guid[] TicketsUID);
    }
}
