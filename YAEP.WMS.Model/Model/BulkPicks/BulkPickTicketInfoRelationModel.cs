using System;
using YAEP.Data.ORM.Attributes;
using Dapper.Contrib.Extensions;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
    [Table("WMS_BulkPick_TicketInfoRelation")]
    [DbTable("WMS_BulkPick_TicketInfoRelation")]
    public class BulkPickTicketInfoRelationModel : IBulkPickTicketInfoRelationModel
    {
        /// <summary>
        /// 識別碼
        /// </summary>
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }

        /// <summary>
        /// 代碼
        /// </summary>
        public String ID { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public Int32 Type { get; set; }

        /// <summary>
        /// <para />狀態, 預設 100
        /// <para />Void = 0
        /// <para />Active = 100
        /// </summary>
        public Int32 Status { get; set; }

        /// <summary>
        /// 批次出貨單詳項識別碼
        /// </summary>
        public Guid BulkPickUID { get; set; }

        /// <summary>
        /// Ticket Info 識別碼
        /// </summary>
        public Guid TicketInfoUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid FromSlotUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid ToSlotUID { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public String CreatedBy { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public String ModifiedBy { get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        public DateTime? ModifiedOn { get; set; }
    }
}
