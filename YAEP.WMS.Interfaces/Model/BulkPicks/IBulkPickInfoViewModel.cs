using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickInfoViewModel
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
        string CustomerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ManifestNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string RefNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid ItemUID { get; set; }
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
        string FromSlot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ToSlot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string TicketInfoID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Guid TicketInfoUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid ShipViaUID { get; set; }
    }


}
