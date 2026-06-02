using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// Transload 庫存列表單列（粒度 = 一個在庫 PayLoad；Type=Stock 1、Status=Active 500）。
    /// 由 ManifestRepository.GetTransloadInventoryList 客製 SQL 產出（PayLoad→Vessel→BOL→Manifest、join YAEP_Item 取 SKU）。
    /// Qty 為原生包裝單位數；OnHandPcs(最小單位件數) 由 BLL 用 PackageCacheManager 換算後填入；
    /// UOM / 客戶名 / 倉名 由 API 層經 DrKnowAll 解析；AgingDays 由 API 層用 ArrivalDate 計算。
    /// </summary>
    public interface ITransloadInventoryRowViewModel
    {
        /// <summary>列 key = WMS_PayLoad.UID。</summary>
        Guid PayLoadUID { get; set; }
        Guid ItemUID { get; set; }
        string SKU { get; set; }
        string ItemName { get; set; }
        Guid PartyUID { get; set; }
        Guid WarehouseUID { get; set; }
        Guid PackageUID { get; set; }

        /// <summary>在倉數量 = PayLoad.Quantity（原生包裝單位數，單位見 UOM）。</summary>
        int Qty { get; set; }

        /// <summary>在倉件數 = Qty 換算到最小單位(Each)；由 BLL 用 PackageCacheManager 填入。</summary>
        int OnHandPcs { get; set; }

        /// <summary>櫃號 = Vessel.RefNo。</summary>
        string ConNo { get; set; }
        string SealNo { get; set; }
        int? ContainerSize { get; set; }
        int? LoadingType { get; set; }
        int? StackableType { get; set; }
        Guid ManifestUID { get; set; }

        /// <summary>到倉日 = Vessel.ArrivalDate（user 收貨輸入；aging 來源）。</summary>
        DateTime? ArrivalDate { get; set; }
    }
}
