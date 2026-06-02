using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickModel
    { 
        /// <summary>
        /// 識別碼
        /// </summary>
        Guid UID { get; set; }

        /// <summary>
        /// 代碼
        /// </summary>
        String ID { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        Int32 Type { get; set; }

        /// <summary>
        /// <para />狀態, 預設 100
        /// <para />Void = 0
        /// <para />Open= 100
        /// <para />Processing=200
        /// <para />Complete=300
        /// <para />refenerce: <see cref="YAEP.WMS.Constant.Enums.BulkPickStatus"/>
        /// </summary>
        Int32 Status { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        String Name { get; set; }

        /// <summary>
        /// 客戶名稱
        /// </summary>
        String PartyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid TicketUID { get; set; } 

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
