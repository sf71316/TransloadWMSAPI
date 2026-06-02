using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickTicketInfoRelationModel
    { 
        /// <summary>
        /// 識別碼
        /// </summary>
        Guid UID { get; set; }
        /// <summary>
        /// 代碼
        /// </summary>
        string ID { get; set; }
        /// <summary>
        /// 分類
        /// </summary>
        int Type { get; set; }
        /// <summary>
        /// <para />狀態, 預設 100
        /// <para />Void = 0
        /// <para />Active = 100
        /// </summary>
        int Status { get; set; }
        /// <summary>
        /// 批次出貨單詳項識別碼
        /// </summary>
        Guid BulkPickUID { get; set; }

        /// <summary>
        /// Ticket Info 識別碼
        /// </summary>
        Guid TicketInfoUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid FromSlotUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid ToSlotUID { get; set; }
        /// <summary>
        /// 建立者
        /// </summary>
        String CreatedBy { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        DateTime? CreatedOn { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        String ModifiedBy { get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        DateTime? ModifiedOn { get; set; } 
    }


}
