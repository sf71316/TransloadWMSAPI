# Transload 對外 API 路由清單

給 RD 的對外整合 API 列表。route root 統一為 **`/api/Transload/{Action}`**（2026-05-29 定案）；內部實作（包既有邏輯或新寫）後續逐支再定。

- 來源程式碼：`YAEP.WMS.API`（.NET Framework 4.6.2）
- 相關規格：`docs/TransloadWMS_API_UI_Spec.md`
- 詳細 Request/Response：見 `docs/WMS_API_確認清單.md`

| 章節代號 | 文件上 API 名稱 | WMS API（/api/Transload） | 狀態 |
|---|---|---|---|
| §5.1.6 / §6.3 | 客戶下拉（GetCustomerList） | `GET /api/Transload/GetCustomerList` | 邏輯已確認，待實作 |
| §5.1.6 | 倉庫下拉（Warehouse） | `GET /api/Transload/GetWarehouseList` | 邏輯已確認，待實作（CustomerPartyName 過濾待討論） |
| §5.1.6 / §7.1.1 | SKU 查詢（GetProducts） | `GET /api/Transload/GetProducts` | 邏輯已確認，待實作 |
| §5.1.6 / §7.1.2 | 新增 SKU（AddProduct） | `POST /api/Transload/AddProduct` | 邏輯已確認，待實作（含預設三層包裝） |
| §5.1.8 | Carrier/ShipMethod 下拉 | `GET /api/Transload/GetShipMethodList` | 邏輯已確認，待實作（CustomerPartyName 選填） |
| §C1 | 入倉列表（GetManifestList） | `POST /api/Transload/GetManifestList` | 邏輯已確認，待實作（擴充既有；Type 固定 Inbound） |
| §5.1.6 / §6.5 / §7.2.1 | 建立收貨（Receiving） | `POST /api/Transload/Receiving` | 設計已定，待實作（每櫃建 1 Vessel；重複擋下/整批失敗） |
| — | 包裝清單（GetPackage） | ~~`GET /api/Transload/GetPackage`~~ | ❌ 取消（用 GetProducts 內嵌 Package[] 選 PackageUID；前端不需 QtyPerPackage） |
| 主文件 §1 | 入倉作廢（VoidManifest） | `POST /api/Transload/VoidManifest` | 設計已定，待實作（軟刪+inbound 庫存反扣；已用擋刪） |
| §5.1.7 / §6.6 / §7.3.1 | 庫存查詢（GetInventory） | `POST /api/Transload/GetInventory` | 邏輯已確認，待實作（per-payload 粒度＋aging；依賴 WMS_Vessel 加 5 欄） |
| §5.1.7 / §7.3.2 | 在庫明細（GetInventoryDetail） | `GET /api/Transload/GetInventoryDetail` | 先跳過（既有可重用，暫不規格化） |
| §5.1.8 / §6.7 / §7.4.1 | 建立出貨（Allocated） | `POST /api/Transload/Allocated` | 未確認 |
| §5.1.8 / §7.4.2 | 出貨單（BOL API） | `/api/Transload/Bol*`（Create/Update/UploadPod） | 未確認 |
| §7.4.3 | 出庫拆單（SaveOutboundAssignedItem） | `POST /api/Transload/SaveOutboundAssignedItem` | 未確認 |
| §5.1.3 | 今日入倉/出倉筆數（Summary） | `GET /api/Transload/Summary?from=&to=` | 🆕 待建（選用） |
| §7.4.4 | by-sku 預檢（GetItemAvailability） | `GET /api/Transload/GetItemAvailability` | 🆕 待建 |
| §5.1.11 / §7.5.1 | 可計費事件（BillableEvents, S2S） | `GET /api/Transload/BillableEvents` | 🆕 待建 |

## 跨支規則
1. ✅（已定）回應統一用標準 `APIResult<T>`（`IsComplete/Code/Message/Data`，資料在 `Data`）。
2. ✅（已定）客戶識別統一用 `CustomerPartyName`＝客戶代碼 `Party.ID`（= T2 `S_Client.ID`），API 內部以 `GetCustomer` 解析成 UID（#3/#4/#5/#7 已採；查詢類選填、寫入類必填）。
3. ⏳（待討論）客戶↔倉庫關聯：`GroupUID` 為公司組織層級（非客戶），無法由客戶 GroupUID 過濾倉庫；`GetWarehouseList` 的 `CustomerPartyName` 過濾邏輯待此關聯定義後實作。

## DB 變更（Transload 需求）
1. **`WMS_Vessel` 新增 5 欄**（2026-05-29 使用者決策，**改放 Vessel，非 spec §10 的 Manifest**）：
   - `SealNo nvarchar` / `ContainerNo nvarchar` / `ContainerSize int`（20GP/40GP/40HQ/45HQ/LooseCargo/LCL/Other，沿用 §11.2 編號）/ `LoadingType int`（F2F=10/F2P=20/P2P=30/Full=40，§11.1）/ `IsStackable bit`
   - Receiving 建立 Vessel（`ManifestManager.Order.Inbound.cs:249-258`）時需寫入這 5 欄。
   - 用於 #7 GetInventory 的 ContainerNo/LoadingType 過濾與回傳。
   - 註：staging DB 實測 `WMS_PayLoad` 已有 `ReceivedDate`（SSDT `.sql` 檔過時），aging 直接用之，**不需改 PayLoad**。
