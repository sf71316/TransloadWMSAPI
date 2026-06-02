using System;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// Transload 收貨完成請求：對指定 Manifest 觸發完成（系統 IsAllPass）→ 建 WMS_PayLoad(Stock)+WMS_Inventory。
    /// ManifestUID 由 Receiving 回應取得。
    /// </summary>
    public class TransloadCompleteReceivingRequestModel
    {
        public Guid ManifestUID { get; set; }
    }
}
