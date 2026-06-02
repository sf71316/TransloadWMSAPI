using System;
using System.Collections.Generic;

namespace YAEP.WMS.Interfaces
{
    /// <summary>Transload 收貨：櫃內單一 SKU 行。</summary>
    public interface ITransloadReceivingItem
    {
        /// <summary>SKU（= Item.ID）；驗證用。</summary>
        string Sku { get; }
        /// <summary>輸入數量（該 PackageUID 包裝單位數）→ Manifest_Item_List.PackageQty。</summary>
        int EnterQty { get; }
        /// <summary>包裝層 UID → Manifest_Item_List.PackageUID；亦用來反查 ItemUID。</summary>
        Guid PackageUID { get; }
    }

    /// <summary>Transload 收貨：單一櫃（→ 1 Vessel，6 個容器屬性掛 Vessel）。</summary>
    public interface ITransloadReceivingContainer
    {
        /// <summary>櫃號 → Vessel.RefNo。</summary>
        string ConNo { get; }
        string SealNo { get; }
        int? ContainerSize { get; }
        int? LoadingType { get; }
        int? StackableType { get; }
        DateTime? ArrivalDate { get; }
        IEnumerable<ITransloadReceivingItem> Items { get; }
    }

    /// <summary>Transload 收貨輸入（BLL 介面層；客戶以 CustomerPartyName 字串，BLL 內解析成 UID）。</summary>
    public interface ITransloadReceivingInput
    {
        string RefNo { get; }
        Guid WarehouseUID { get; }
        string CustomerPartyName { get; }
        IEnumerable<ITransloadReceivingContainer> Containers { get; }
    }

    /// <summary>Transload 收貨結果：建立的 Manifest 與每櫃 Vessel。</summary>
    public interface ITransloadReceivingResult
    {
        Guid ManifestUID { get; set; }
        IEnumerable<IVesselModel> Vessels { get; set; }
    }
}
