using System;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// Transload 入庫列表單列（頭欄位 + DB 端彙總）。
    /// 由 ManifestRepository.GetTransloadInboundList 客製 SQL 產出，每個 Manifest 一列。
    /// 數量以「原生包裝單位數」呈現，UOM 名稱由 <see cref="ITransloadInboundPackageViewModel"/> 明細在 API 層解析。
    /// </summary>
    public interface ITransloadInboundRowViewModel
    {
        Guid UID { get; set; }
        string ManifestName { get; set; }
        string ManifestNo { get; set; }
        Guid PartyUID { get; set; }
        Guid WarehouseUID { get; set; }
        string Description { get; set; }
        ManifestStatus Status { get; set; }
        DateTime? CreatedOn { get; set; }

        /// <summary>櫃數 = COUNT(DISTINCT Vessel.UID)（BOL→Vessel，Status&gt;0）。</summary>
        int ContainerCount { get; set; }

        /// <summary>實到數量 = SUM(TicketInfo.ActQty)（WorkOrder→Ticket→TicketInfo，Type=Receiving 100、Status&gt;0）；原生包裝單位數，單位見 ReceivedPackages。</summary>
        int ReceivedQty { get; set; }

        /// <summary>在倉數量 = SUM(PayLoad.Quantity)（BOL→Vessel→PayLoad，Type=Stock 1、Status&gt;0）；原生包裝單位數，單位見 OnhandPackages。</summary>
        int OnhandQty { get; set; }

        /// <summary>到倉日 = MIN(Vessel.ArrivalDate)（該 manifest 最早到倉的櫃）；無資料為 null。</summary>
        DateTime? ArrivalDate { get; set; }
    }
}
