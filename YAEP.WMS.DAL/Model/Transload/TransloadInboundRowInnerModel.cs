using System;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    /// <summary>
    /// <see cref="ITransloadInboundRowViewModel"/> 的 Dapper 對應實作（入庫列表頭+彙總）。
    /// 欄位名稱對齊 GetTransloadInboundList 主 SQL 的別名。
    /// </summary>
    internal class TransloadInboundRowInnerModel : ITransloadInboundRowViewModel
    {
        public Guid UID { get; set; }
        public string ManifestName { get; set; }
        public string ManifestNo { get; set; }
        public Guid PartyUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public string Description { get; set; }
        public ManifestStatus Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int ContainerCount { get; set; }
        public int ReceivedQty { get; set; }
        public int OnhandQty { get; set; }
        public DateTime? ArrivalDate { get; set; }
    }
}
