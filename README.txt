WMS API — Transload 變更檔交接包（v3，最新）
產生日期：2026-06-02
來源 working copy：E:\Project\DC\WMS\API（SVN：…/svn/DEV/WMS/WMSv2，base r71654）

說明
====
本包為 WMS API 上「未提交（uncommitted）」的本機變更檔，共 20 個，保留相對路徑。
請以 WMS API 根目錄（…\WMS\API）為基準，用 SVN/Beyond Compare 比對後覆蓋到你的 checkout。
覆蓋前務必比對，避免蓋掉你自己的環境設定。標 [新檔] 者在來源 WC 尚未 svn add，請記得納入版控。

檔案清單
========
[Transload API / 控制器]
- YAEP.WMS.API\Controllers\TransloadController.cs            [新檔] 主控制器：dc-list/inbound/outbound/CompleteInbound/Allocated/SetContainerInfo/GetShipmentList/AddProduct
- YAEP.WMS.API\YAEP.WMS.API.csproj                                  納入上述新檔編譯

[Transload Receiving / AddProduct 的 Model（本批新增）]
- YAEP.WMS.API\Models\Request\TransloadReceivingRequestModel.cs          [新檔]
- YAEP.WMS.API\Models\Request\TransloadCompleteReceivingRequestModel.cs  [新檔]
- YAEP.WMS.API\Models\Request\TransloadAddProductRequestModel.cs         [新檔]
- YAEP.WMS.API\Models\Response\TransloadReceivingResponseModel.cs        [新檔]
- YAEP.WMS.BLL\Model\InnerModel\TransloadReceivingInnerRequest.cs        [新檔]
- YAEP.WMS.BLL\Model\InnerModel\TransloadReceivingResult.cs              [新檔]
- YAEP.WMS.Interfaces\Model\Order\ITransloadReceiving.cs                 [新檔]
- YAEP.WMS.BLL\YAEP.WMS.BLL.csproj / YAEP.WMS.Interfaces\YAEP.WMS.Interfaces.csproj  納入上述新檔編譯

[BLL 業務邏輯]
- YAEP.WMS.BLL\Manager\ManifestManager.Order.Inbound.cs       收貨流程（櫃資訊改由 SetContainerInfo 補寫，避免動 Vessel schema）
- YAEP.WMS.BLL\Manager\ManifestManager.Order.Outbound.cs      出貨 NullReference 修復（manifestInfo.Content → manifestModel）＋失敗訊息上拋
- YAEP.WMS.BLL\Module\WorkOrder\Planner\Receiving\DefaultReceivingPlanner.cs  單櫃多 SKU 收貨 NRE 修復；收貨 UOM 須＝最小包裝
- YAEP.WMS.BLL\Module\WorkOrder\AutoAssign\OrderAutoAssign\ExternalOutboundFullAllocatedAutoAssignAgent.cs  配貨失敗訊息寫進 Response＋Trace 診斷
- YAEP.WMS.BLL\Module\Common\TracingAgent.cs                  log 類別算不出時 TryParse 退預設，避免 log 例外把交易 rollback
- YAEP.WMS.Interfaces\Manager\IOrderManager.cs                GetAllItem() 回傳型別對齊（IEnumerable<IProductExtendModel>）

[疑似非 transload，一併附上供比對]
- YAEP.WMS.BLL\Manager\ManifestManager.Ticket.Mobile.cs       POD/ticket 行動端 null 防護

[設定／發佈檔 — 環境相關，請勿直接覆蓋！]
- YAEP.WMS.API\Web.config                                     ※ 目前指向副本 WMS_TL（dev sandbox）。正式環境用你自己的連線，勿覆蓋
- YAEP.WMS.API\Properties\PublishProfiles\FolderProfile.pubxml

⚠️ DB schema 注意（切回正式 WMS 前必看）
============================================
transload dev 在副本 WMS_TL 上加了 5 個欄位，正式 WMS 沒有，程式會用到：
  - WMS_BOL.TrackingNumber（GetShipmentList 會 SELECT）、WMS_BOL.PODUrl（暫未用）
  - WMS_Vessel.ContainerType / Volume / Weight（SetContainerInfo 會 UPDATE）
正式 WMS 另有 WMS_Vessel.ContainerSize（與 ContainerType 命名相撞）。
→ 連回正式 WMS 前，這 5 欄要先 ALTER 補上（repo 無腳本），否則收貨寫櫃 / 出貨清單會「Invalid column name」。
※ 對外契約/UI 已把「櫃尺寸」正名為 ContainerSize，DB 實體欄位仍叫 ContainerType，由 API 內部映射（不動 schema）。

建置／部署備忘
==============
- YAEP.WMS.API 專案「沒有」參考 YAEP.WMS.BLL（DI 執行期從 bin 載入）。
  改 BLL 後必須：
    1) MSBuild YAEP.WMS.BLL.csproj /t:Rebuild（普通 build 會被增量跳過）
    2) 手動 Copy YAEP.WMS.BLL\bin\Debug\YAEP.WMS.BLL.dll(+pdb) → YAEP.WMS.API\bin\
  只改 API 端則 build API csproj 即可。
- VS2022 Professional MSBuild。重編前先停 WMS 的 iisexpress（port 8081）。
