using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickInfoModel
    {
        /// <summary>
        /// 識別碼
        /// </summary>
        Guid BulkPickUID { get; set; }
        /// <summary>
        /// 代碼
        /// </summary>
        string BulkPickID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid ItemUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string PartyName { get; set; } 
        /// <summary>
        ///  
        /// </summary>
        string ItemNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int? EstQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int? ActQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int? ShtQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int? SavQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string From { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string To { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid TicketInfoUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string TicketInfoRelationID { get; set; }
    }


}
