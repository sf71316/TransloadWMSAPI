using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketRepository
    {

        IActionResult<IEnumerable<IComponentViewModel>> GetBolNameList(Guid ManifestUID);
        IActionResult<IEnumerable<IBolInfoViewModel>> GetBolInfo(Guid BolUID);
        IActionResult<IEnumerable<IComponentViewModel>> GetTicketIDList(Guid BolUID);
        IActionResult<IEnumerable<IComponentViewModel>> GetVesselRefNoList(Guid BolUID);
        IActionResult<IBulkPickDataCollection> GetBulkPickOrignalData(IEnumerable<Guid> ticketInfoUIDs);
        IActionResult<bool> AddTicket(IEnumerable<ITicketModel> Collection);
        IActionResult<bool> DeleteTicket(Guid TicketUID);
        IActionResult<bool> VoidTicket(IVoidTicketParameters Parameters);
        IActionResult<ITicketAssignedListViewModelCollection> GetTicketAssignedList(ITicketAssignedListParameters Parameters);
        IActionResult<IEnumerable<ITicketGeneratoreDataModel>> GetGeneratoreTicketData(object condition);
        IActionResult<IEnumerable<ITicketGeneratoreDataModel>> GetGeneratoreTicketDataByMoveManifest(object condition);
        IActionResult<bool> UpdateTicketStatus(IEnumerable<Guid> ticketUID, TicketStatus status, string modifiedBy="");
        IActionResult<IEnumerable<ITicketListViewModel>> GetTicketList(IGetTicketListParameters parameters);
        IActionResult<IEnumerable<IStatusCheckModel>> GetManifestStatusCollection(IEnumerable<Guid> TicketUID);
        IActionResult<IEnumerable<ITicketSearchListViewModel>> GetTicketSearchList(ITicketSearchListParameters Parameters);
        IActionResult<IEnumerable<ITicketModel>> GetList(object condition);
        IActionResult<IEnumerable<ITicketSummaryViewModel>> GetTicketSummaryData(Guid TicketUID, Guid[] groupIDs);
        IActionResult<IEnumerable<ITicketModel>> GetTicketByBol(Guid boluid);
        IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoList(Guid bolUID);
        IActionResult<IEnumerable<IAssignedTicketInfoModel>> GetTicketInfoList(IEnumerable<Guid> bolUID);
        IActionResult<IEnumerable<IDeallocatedInfoDataModel>> GetDeallocatedInfoList(IEnumerable<Guid> workorderguids);
        IActionResult<ISlotModel> CheckSlotExistByTicketInfo(Guid ticketInfoUID, string slotName);

        IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoListByWorkOrderUID(Guid workorderUID);
        IActionResult<ITicketModel> GetTicketByBulkPick(Guid bulkPickUID);
        IActionResult<bool> VoidTicketByWorkOrder(IVoidTicketParameters Parameters);
    }
}
