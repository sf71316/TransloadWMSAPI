using System;
using System.Collections.Generic;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickManager:IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IBulkPickViewModel>> GetBulkPickList(IBulkPickSearchParameters parameters);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IBulkPickManifestViewModel>> GetManifestList(IBulkPickManifestSearchParameters parameters);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketInfoUID"></param>
        /// <param name="customerName"></param>
        /// <returns></returns>
        IActionResult<bool> SaveBulkPick(IEnumerable<Guid> ticketInfoUID, string customerName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        IActionResult<bool> CreateBulkPick(IBulkPickModel model);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <returns></returns>
        IActionResult<bool> DeleteBulkPick(IEnumerable<Guid> bulkPickUID);
        /// <summary>
        /// §å¦¸ Assign
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <param name="groupUID"></param>
        /// <returns></returns>
        IActionResult<bool> BatchAddWorker(IEnumerable<Guid> bulkPickUID, IEnumerable<Guid> groupUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IBulkPickInfoModel>> GetBulkPickInfoList(Guid bulkPickUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IBulkPickInfoViewModel>> GetBulkPickInfoViewList(Guid bulkPickUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketInfoUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<string>> GetBulkPickIDByTicketInfo(IEnumerable<Guid> ticketInfoUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulkPickTicketUID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        IActionResult<bool> ChangeBulkPickStatus(IEnumerable<Guid> bulkPickTicketUID, int status);
        IActionResult<IEnumerable<IBulkPickInfobyOutboundViewModel>> GetBulkPickInfoByTicketInfo(IEnumerable<Guid> ticketInfoUID);
        IActionResult<IEnumerable<IBulkPickNotificationInfoModel>> GetBulkPickOriginalNotificationInfo(IEnumerable<Guid> ticketInfoUID);
        IActionResult<bool> IsBulkPickWorkOrderPayload(Guid WorkOrderPayloadUID);
        IActionResult<bool> AddBlukPickWorkOrderPayloadRelation(IEnumerable<IBulkPickWorkOrderPayloadRelationModel> modelCollection);
    }


}
