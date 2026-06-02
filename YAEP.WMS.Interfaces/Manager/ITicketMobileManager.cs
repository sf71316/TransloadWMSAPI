using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities.Model;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketMobileManager: ITraceInfiltrator
    {
        IActionResult<bool> UploadTicketByPodBarcode(IUploadTicketDataParameter parameter);
        IActionResult<IEnumerable<ITicketListViewModel>> GetTicketList(IGetTicketListParameters parameters);
        IActionResult<IEnumerable<dynamic>> GetTicketInfo(IEnumerable<Guid> ticketUIDs, IEnumerable<Guid> ticketinfoguids = null);
        IActionResult<dynamic> UploadTicketData(IUploadTicketDataParameter parameter);
        IActionResult<dynamic> BatchUploadTicketData(IEnumerable<IUploadTicketDataParameter> parameters);        
        IActionResult<dynamic> UploadTicketDataByPodBarcode(IUploadTicketDataParameter parameter);
        IActionResult<IEnumerable<ITicketProcessModel>> GetTicketProcessModel(Guid[] TicketInfoUIDs);
        IActionResult<IEnumerable<dynamic>> GetTickeInfotListDetail(Guid[] TicketInfoUID, Guid WorkOrderPodUID);
        IActionResult<IEnumerable<ITicketSummaryViewModel>> GetTicketSummaryData(Guid TicketUID, IEnumerable<Guid> groupIDs);
        IEnumerable<dynamic> GetAttachmentTypeList(int belongToType);
        IActionResult<IAttachmentFilesInfoModel> DownloadAttachment(Guid attachmentUID);
        IActionResult<bool> UploadAttachment(ITicketUploadAttachmentParameters param);
        IActionResult<dynamic> ChangeToSlot(IChangeToSlotParameter parameters);
        IActionResult<dynamic> BatchChangeToSlotAPI(IBatchChangeToSlotParameter parameters);
        IActionResult<dynamic> ChangeFromSlotAPI(IChangeFromSlotParameter parameters);
        IActionResult<IEnumerable<IPodBarcodeInfo>> GetReceivingQtyBarcodeInfo(ICheckPodBarcodeInfoParameters parameters);
        IActionResult<dynamic> CompleteTicketData(String[] TicketIDs);
    }
}
