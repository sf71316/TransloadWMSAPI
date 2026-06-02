using System;
using System.Collections.Generic;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickTicketInfoRelationRepository
    {
        IActionResult<IEnumerable<IBulkPickTicketInfoRelationModel>> GetTicketRelations(IEnumerable<Guid> bulkPickUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        IActionResult<bool> Create(IBulkPickTicketInfoRelationModel model);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        IActionResult<bool> Create(IEnumerable<IBulkPickTicketInfoRelationModel> collection);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        IActionResult<bool> Delete(Guid uid);
        IActionResult<bool> DeleteByBulkPick(Guid bulkPickUID);
    }


}
