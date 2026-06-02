namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// Transload 入庫列表查詢請求（固定 Type=Inbound, Status&gt;0）。
    /// </summary>
    public class TransloadGetManifestListRequestModel
    {
        /// <summary>客戶代碼（= Party.ID，選填）；解析成 CustomerUID 後過濾 Manifest.PartyUID。</summary>
        public string CustomerPartyName { get; set; }

        /// <summary>ManifestStatus 細分過濾（選填，在 Status&gt;0 範圍內再細分）。</summary>
        public int? Status { get; set; }

        /// <summary>關鍵字（選填）：目前比對 Manifest.Name；Vessel.RefNo(ConNo) / Vessel.SealNo 比對待 Receiving 寫入後擴充。</summary>
        public string Key { get; set; }
    }
}
