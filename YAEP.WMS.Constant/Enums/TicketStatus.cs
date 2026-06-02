using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum TicketStatus
    {
        Summary = 50,
        /// <summary>
        /// 初始狀態
        /// </summary>
        Draft = 100,
        /// <summary>
        /// 被assigned workorder 後
        /// </summary>
        Assigned = 200,
        /// <summary>
        /// Ticket加入BulkPick後原生Move Ticket 被會指定該狀態
        /// </summary>
        AssignedBulkPick = 290,
        /// <summary>
        /// BOL Approve 後
        /// </summary>
        Open = 300,
        /// <summary>
        /// Ticket 處理中
        /// </summary>
        Processing = 400,
        /// <summary>
        /// 發生缺貨時
        /// </summary>
        Glitch = 500,
        /// <summary>
        /// Ticket 完成
        /// </summary>
        Complete = 600,
        /// <summary>
        /// 刪除
        /// </summary>
        Void = 0
    }
}
