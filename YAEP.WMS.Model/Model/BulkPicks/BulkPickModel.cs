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
    [Table("WMS_BulkPick")]
    [DbTable("WMS_BulkPick")]
    public class BulkPickModel : IBulkPickModel
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
        /// <para />Open= 100
        /// <para />Processing=200
        /// <para />Complete=300
        /// <para />refenerce: <see cref="YAEP.WMS.Constant.Enums.BulkPickStatus"/>
        /// </summary>
        public Int32 Status { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// 客戶名稱
        /// </summary>
        public String PartyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid TicketUID { get; set; }

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
