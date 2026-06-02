using System;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// Transload 庫存列表單列回應（粒度 = 一個在庫 PayLoad，Type=Stock、Status=Active）。
    /// </summary>
    public class TransloadInventoryResponseModel
    {
        /// <summary>列 key = WMS_PayLoad.UID。</summary>
        public Guid PayLoadUID { get; set; }

        public Guid ItemUID { get; set; }
        public string SKU { get; set; }
        public string ItemName { get; set; }

        public Guid CustomerUID { get; set; }
        public string CustomerName { get; set; }

        public Guid WarehouseUID { get; set; }
        public string WarehouseName { get; set; }

        /// <summary>在倉數量 = PayLoad.Quantity（原生包裝單位數，單位見 UOM）。</summary>
        public int Qty { get; set; }

        /// <summary>在倉包裝單位名稱（如 Pallet）。</summary>
        public string UOM { get; set; }

        /// <summary>在倉件數 = Qty 換算到最小單位(Each)。</summary>
        public int OnHandPcs { get; set; }

        /// <summary>櫃號 = Vessel.RefNo。</summary>
        public string ConNo { get; set; }
        public string SealNo { get; set; }
        public int? ContainerSize { get; set; }

        /// <summary>裝卸型碼：10=F2F/20=F2P/30=P2P/40=Full Container。</summary>
        public int? LoadingType { get; set; }
        /// <summary>裝卸型顯示名稱（依 LoadingType 碼對應）；無對應碼時為 null。</summary>
        public string LoadingTypeName { get; set; }

        /// <summary>可堆疊碼：0=Stackable/1=Non-Stackable。</summary>
        public int? StackableType { get; set; }
        /// <summary>可堆疊顯示名稱（依 StackableType 碼對應）；無對應碼時為 null。</summary>
        public string StackableTypeName { get; set; }

        public Guid ManifestUID { get; set; }

        /// <summary>到倉日 = Vessel.ArrivalDate；無資料為 null。</summary>
        public DateTime? ArrivalDate { get; set; }

        /// <summary>在倉天數 = today − ArrivalDate；ArrivalDate 為 null 時為 null。</summary>
        public int? AgingDays { get; set; }
    }
}
