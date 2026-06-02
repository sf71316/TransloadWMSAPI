namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// Transload 新增 SKU 請求：以最小輸入建立產品本體並自動建立預設三層包裝 PALLET→BOX→EACH。
    /// 供 New Inbound Manifest 的「+ 馬上新增 Item」用;建立後完整刷新快取(產品/包裝/版本)讓收貨即可解析。
    /// </summary>
    public class TransloadAddProductRequestModel
    {
        /// <summary>產品代碼(= Item.ID),同時作為 Name。</summary>
        public string Sku { get; set; }

        /// <summary>客戶代碼(= Party.ID = T2 S_Client.ID);內部解析成 CustomerUID 並寫入產品屬性。</summary>
        public string CustomerPartyName { get; set; }
    }
}
