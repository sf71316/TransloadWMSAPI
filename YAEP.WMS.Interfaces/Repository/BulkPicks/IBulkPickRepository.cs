using System;
using System.Collections.Generic;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <returns></returns>
        IActionResult<IBulkPickModel> GetBulkPickModel(Guid bulkPickUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IBulkPickModel>> GetBulkPickCollection(IEnumerable<Guid> bulkPickUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IBulkPickViewModel>> GetList(IBulkPickSearchParameters parameters);
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
        /// <returns></returns>
        IActionResult<IEnumerable<IBulkPickSaveModel>> GetBulkPickSaveDataByTicket(IEnumerable<Guid> ticketInfoUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        IActionResult<bool> Create(IBulkPickModel model);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IActionResult<bool> Create(IEnumerable<IBulkPickModel> collection);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        IActionResult<bool> Delete(IEnumerable<Guid> bulkPickUID);
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
        IActionResult<IEnumerable<IBulkPickModel>> GetBulkPickByTicketInfo(IEnumerable<Guid> ticketInfoUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulkPickTicketUID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        IActionResult<bool> ChangeBulkPickStatus(IEnumerable<Guid> bulkPickTicketUID, int status, string modifiedBy = "");
        IActionResult<IEnumerable<IBulkPickInfobyOutboundViewModel>> GetBulkPickInfoByTicketInfo(IEnumerable<Guid> ticketInfoUID);
        IActionResult<IEnumerable<IBulkPickModel>> GetBulkPickByTicketCollection(IEnumerable<Guid> ticketUID);
    }


}
