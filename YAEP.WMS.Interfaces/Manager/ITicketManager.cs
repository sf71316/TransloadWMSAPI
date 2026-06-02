using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Identities.Interfaces;
using YAEP.Identities.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.Utilities.Model;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketManager : ITicketMobileManager, ILogInfiltrator, ITraceInfiltrator, IDisposable
    {
        IActionResult<Guid> GenerateBulkPickTicket(IEnumerable<Guid> TicketInfoUID);
        IActionResult<bool> RemoveBulkPickTicket(IEnumerable<IBulkPickTicketInfoRelationModel> Models);
        IActionResult<bool> GeneratreTicket(ITicketGenerateParameter parameter);
        IActionResult<bool> VoidTicket(IVoidTicketParameters Parameters);
        IEnumerable<IEnumFieldInfo> GetServiceItemNameList();
        IActionResult<IEnumerable<IComponentViewModel>> GetBolNameList(Guid ManifestUID);
        IActionResult<IEnumerable<IBolInfoViewModel>> GetBolInfo(Guid BolUID);
        IActionResult<IEnumerable<IComponentViewModel>> GetTicketIDList(Guid BolUID);
        IActionResult<IEnumerable<IComponentViewModel>> GetVesselRefNoList(Guid BolUID);
        IActionResult<IEnumerable<ITicketAssignedListViewModel>> GetTicketAssignedList
            (ITicketAssignedListParameters Parameters, IGroupManager groupManager);
        IActionResult<bool> AddWorkder(IMaintainWorkderParameters Parametes, bool isIgnoreCheck = false);
        IActionResult<bool> AddWorkderAPI(IMaintainWorkderParameters Parametes, bool isIgnoreCheck = false);
        IActionResult<bool> RemoveWorkderAPI(Guid[] tauid, Guid TicketInfoUID);
        IActionResult<bool> BatchAssignWorkerAPI(IMaintainWorkderParameters Parametes);
        IActionResult<bool> BatchAssignWorker(IMaintainWorkderParameters Parametes);
        IActionResult<bool> IsTicketComplete(Guid TicketUID, Guid[] notContainsTicketInfoUID = null);
        IActionResult<bool> UpdateTicketInfo(ITicketInfoModel model);
        IActionResult<bool> ChangeTicketStatus(Guid ticketUID, TicketStatus status);
        IActionResult<IEnumerable<IStatusCheckModel>> GetManifestStatusCollection(Guid TicketUID);
        IActionResult<IEnumerable<IStatusCheckModel>> GetManifestStatusCollection(IEnumerable<Guid> TicketUIDs);
        IActionResult<IEnumerable<IStatusCheckModel>> GetBatchManifestStatusCollection(IEnumerable<Guid> TicketUIDCollection);
        IActionResult<IEnumerable<ITicketInfoModel>> GetPodBelongTicket(IEnumerable<string> enumerable);
        IActionResult<IEnumerable<dynamic>> GetAttachmentList(Guid belongTouid, int belongToType, Guid? attachmentTypeUID);
        IActionResult<IEnumerable<ITicketGroupAssignedModel>> GetTicketGroupAssignedList
            (Guid ticketinfouid, IGroupManager groupManager);
        IActionResult<ITicketModel> GetTicketModel(object condition);
        IActionResult<IEnumerable<ITicketRelationModel>> GetTicketRelationList(object condition);
        IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoList(object condition);
        IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoByPickAll(IEnumerable<Guid> vesselUID = null,
            IEnumerable<Guid> workorderPayloadUID = null);
        IEnumerable<IEnumFieldInfo> GetTicketTypeList();
        IEnumerable<IEnumFieldInfo> GetTicketStatusList();
        IActionResult<IEnumerable<ITicketSearchListViewModel>> GetTicketSearchList(ITicketSearchListParameters Parameters, IGroupManager groupManager);


        IActionResult<bool> CheckTicketStatus();
    }
}
