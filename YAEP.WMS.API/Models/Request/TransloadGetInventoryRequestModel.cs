namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// Transload 庫存列表查詢請求（固定 PayLoad Type=Stock 1、Status=Active 500）。
    /// </summary>
    public class TransloadGetInventoryRequestModel
    {
        /// <summary>客戶代碼（= Party.ID，選填）；解析成 CustomerUID 後過濾 Manifest.PartyUID。</summary>
        public string CustomerPartyName { get; set; }

        /// <summary>關鍵字（選填）：同時比對 SKU(=Item.ID) 與 ConNo(=Vessel.RefNo)。</summary>
        public string Key { get; set; }
    }
}
