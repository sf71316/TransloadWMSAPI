using System;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// Transload 入庫列表單列回應。
    /// </summary>
    public class TransloadManifestListResponseModel
    {
        public Guid ManifestUID { get; set; }
        public string ManifestName { get; set; }

        public Guid CustomerUID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }

        public Guid WarehouseUID { get; set; }
        public string WarehouseName { get; set; }

        public string Description { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? CreatedOn { get; set; }

        /// <summary>到倉日（= MIN 該 manifest 的 Vessel.ArrivalDate）；Receiving 寫入 Vessel.ArrivalDate 後啟用。</summary>
        public DateTime? ArrivalDate { get; set; }

        /// <summary>在庫天數（today − ArrivalDate）；ArrivalDate 為 null 時為 null。</summary>
        public int? AgingDays { get; set; }

        /// <summary>該 manifest 之 Vessel 數（COUNT）。</summary>
        public int ContainerCount { get; set; }

        /// <summary>在倉數量 = SUM(PayLoad.Quantity)（Type=Stock 1、Status&gt;0）；原生包裝單位數，單位見 UOM。</summary>
        public int Qty { get; set; }

        /// <summary>在倉包裝單位名稱（如 Pallet）；同 manifest 多種單位時以「/」串接。</summary>
        public string UOM { get; set; }

        /// <summary>實到數量 = SUM(TicketInfo.ActQty)（Ticket.Type=Receiving 100、Status&gt;0）；原生包裝單位數，單位見 ReceivedUOM。</summary>
        public int ReceivedQty { get; set; }

        /// <summary>實到包裝單位名稱（如 Pallet）；同 manifest 多種單位時以「/」串接。</summary>
        public string ReceivedUOM { get; set; }
    }
}
