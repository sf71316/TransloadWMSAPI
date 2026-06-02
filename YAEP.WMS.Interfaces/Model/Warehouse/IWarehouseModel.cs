using System;

namespace YAEP.WMS.Interfaces
{
    public interface IWarehouseModel
    {
        /// <summary>
        /// 識別碼
        /// </summary>
        Guid UID { get; set; }
        /// <summary>
        /// 群組識別碼
        /// </summary>
        Guid GroupUID { get; set; }
        /// <summary>
        /// 代碼
        /// </summary>
        string ID { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 電話
        /// </summary>
        string Phone { get; set; }
        /// <summary>
        /// 傳真
        /// </summary>
        string Fax { get; set; }
        /// <summary>
        /// 國家
        /// </summary>
        string Country { get; set; }
        /// <summary>
        /// 州
        /// </summary>
        string State { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        string City { get; set; }
        /// <summary>
        /// 郵遞區號
        /// </summary>
        string Zip { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        decimal Volume { get; set; }
        /// <summary>
        /// 狀態
        /// <para/><see cref="YAEP.WMS.Constant.Enums.WarehouseStatus"/>
        /// </summary>
        int Status { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        string Description { get; set; }
        string Mail { get; set; }
        string Contact { get; set; }
        Guid? PhotoUID { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}