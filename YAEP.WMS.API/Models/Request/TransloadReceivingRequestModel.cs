using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// Transload 建立收貨請求（對應主文件 §C3）。實作 <see cref="ITransloadReceivingInput"/> 直接餵 BLL。
    /// 1 批次 → 1 Manifest(Inbound)；每個 Container → 1 Vessel（RefNo=ConNo，容器屬性掛 Vessel）。
    /// Step 1 只生成到 Vessel 為止（Manifest + Manifest_Item_List + BOL + 每櫃 Vessel），不建庫存/ticket。
    /// </summary>
    public class TransloadReceivingRequestModel : ITransloadReceivingInput
    {
        /// <summary>批次參考號（PO Booking# / Order#）；與 WarehouseUID 一起判重複。</summary>
        public string RefNo { get; set; }

        /// <summary>入庫倉庫 UID（前端由 GetWarehouseList 選）。</summary>
        public Guid WarehouseUID { get; set; }

        /// <summary>客戶代碼（= Party.ID）；API/BLL 內解析成 PartyUID。</summary>
        public string CustomerPartyName { get; set; }

        /// <summary>櫃清單（每櫃對應 1 Vessel）。</summary>
        public List<TransloadReceivingContainerModel> Containers { get; set; }

        IEnumerable<ITransloadReceivingContainer> ITransloadReceivingInput.Containers => this.Containers;
    }

    /// <summary>單一櫃（→ 1 Vessel）；6 個容器實體屬性掛 Vessel。</summary>
    public class TransloadReceivingContainerModel : ITransloadReceivingContainer
    {
        /// <summary>櫃號 → Vessel.RefNo。</summary>
        public string ConNo { get; set; }
        /// <summary>封條號 → Vessel.SealNo。</summary>
        public string SealNo { get; set; }
        /// <summary>櫃型 enum 碼 → Vessel.ContainerSize（10=20GP/20=40GP/30=40HQ/40=45HQ/50=LooseCargo/60=LCL/90=Other）。</summary>
        public int? ContainerSize { get; set; }
        /// <summary>裝卸型 enum 碼 → Vessel.LoadingType（10=F2F/20=F2P/30=P2P/40=Full Container）。</summary>
        public int? LoadingType { get; set; }
        /// <summary>可堆疊 enum 碼 → Vessel.StackableType（0=Stackable/1=Non-Stackable）。</summary>
        public int? StackableType { get; set; }
        /// <summary>到倉日（user 輸入）→ Vessel.ArrivalDate；aging 來源。</summary>
        public DateTime? ArrivalDate { get; set; }
        /// <summary>櫃毛重（user 輸入）→ Vessel.Weight。</summary>
        public decimal? Weight { get; set; }
        /// <summary>櫃材積/CBM（user 輸入）→ Vessel.Volume。</summary>
        public decimal? Volume { get; set; }

        /// <summary>該櫃的 SKU 明細。</summary>
        public List<TransloadReceivingItemModel> Items { get; set; }

        IEnumerable<ITransloadReceivingItem> ITransloadReceivingContainer.Items => this.Items;
    }

    /// <summary>櫃內單一 SKU 行。</summary>
    public class TransloadReceivingItemModel : ITransloadReceivingItem
    {
        /// <summary>SKU（= Item.ID）；驗證用，ItemUID 由 PackageUID 反查。</summary>
        public string Sku { get; set; }
        /// <summary>輸入數量（該 PackageUID 包裝單位數）→ Manifest_Item_List.PackageQty。</summary>
        public int EnterQty { get; set; }
        /// <summary>包裝層 UID → Manifest_Item_List.PackageUID；亦用來反查 ItemUID。</summary>
        public Guid PackageUID { get; set; }
    }
}
