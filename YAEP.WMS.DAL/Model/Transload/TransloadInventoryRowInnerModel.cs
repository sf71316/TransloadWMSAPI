using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    /// <summary>
    /// <see cref="ITransloadInventoryRowViewModel"/> 的 Dapper 對應實作（庫存列表 per-PayLoad）。
    /// 欄位名稱對齊 GetTransloadInventoryList SQL 的別名；OnHandPcs 預設 0，由 BLL 換算後填入。
    /// </summary>
    internal class TransloadInventoryRowInnerModel : ITransloadInventoryRowViewModel
    {
        public Guid PayLoadUID { get; set; }
        public Guid ItemUID { get; set; }
        public string SKU { get; set; }
        public string ItemName { get; set; }
        public Guid PartyUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid PackageUID { get; set; }
        public int Qty { get; set; }
        public int OnHandPcs { get; set; }
        public string ConNo { get; set; }
        public string SealNo { get; set; }
        public int? ContainerSize { get; set; }
        public int? LoadingType { get; set; }
        public int? StackableType { get; set; }
        public Guid ManifestUID { get; set; }
        public DateTime? ArrivalDate { get; set; }
    }
}
