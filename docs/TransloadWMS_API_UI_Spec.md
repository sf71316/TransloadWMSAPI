# Transload WMS 入倉 / 出倉 / 報價 / 計費系統規格書

版本：**v1.3（最終版）**
日期：2026-05-28
文件位置：`E:\Project\TransloadWMS\docs\TransloadWMS_API_UI_Spec.md`

---

## 📚 線上瀏覽（給 dev / QA / 同事）

整份 `docs/` 已內嵌到 SPA，**直接在瀏覽器看、不必 git checkout**：

| 入口 | URL |
|---|---|
| **SPA Docs 頁** | http://192.168.88.185:5173/docs |
| 直接開到本文件 | http://192.168.88.185:5173/docs?file=TransloadWMS_API_UI_Spec.md |
| 設計文件（舊版備份） | http://192.168.88.185:5173/docs?file=Transload-WMS-設計文件.md |
| 詳細規格（舊版備份） | http://192.168.88.185:5173/docs?file=Transload-WMS-詳細規格.md |
| 原型素材 prototype/ | http://192.168.88.185:5173/docs（左欄最後一項） |

> SPA 頂列右側「**Debug ON**」按鈕（或 URL 加 `?debug=1`）會把每個畫面對應的 spec 章節 + API endpoint 視覺化覆蓋出來（紫色 = spec 章節、綠色 = API），方便對照本文件 / Network 面板。

## 🗂 開發文件清單

| 檔案 | 角色 | 路徑 |
|---|---|---|
| **本文件 v1.3** | 唯一規格 (SSOT) | `docs/TransloadWMS_API_UI_Spec.md` |
| 設計文件 v1.0（備份） | 概覽，已被本文件取代 | `docs/Transload-WMS-設計文件.md` |
| 詳細規格 v1.0（備份） | 明細，已被本文件取代 | `docs/Transload-WMS-詳細規格.md` |
| 原型 HTML | UI 參考（部分採用） | `prototype/TransloadWMS (6) 1.html` |
| 截圖 9 張 | UI 參考 | `prototype/Image (3-7).jfif` / `prototype/Image (62-65).png` |
| SPA 原始碼 | demo + mock 實作 | `spa/src/`（每個 page 檔頂有 spec/API 註解區塊） |

開發環境：
- WMS API repo `E:\Project\DC\WMS\API`（YAEP.WMS .NET 4.6.2）— Phase 0/1 後端
- T2 API/UI `E:\Project\Trucking\Trucking2000`（HeptarunWebAPI / Heptarun / HRTWebSite）
- WMS DB `192.168.1.21 / WMS / sa`
- T2  DB `192.168.88.16 / Heptarun / sa`

## 💬 線上討論

目前**未開正式 issue 平台**。團隊請：
- 直接在 SPA Docs 頁讀完 spec 後，把問題寫進 PR description 或 email Jason
- 待定案項目集中列在本文件 **§17 待定案**
- 後續若開 GitHub Issues / Notion，本段會更新連結

---

## 0. v1.3 累積變更摘要

「能延用就延用，不確定再加」原則套到實際 schema 後的最終定案。詳見 memory `db-schema-verified-2026-05-28.md` + `prototype-audit-2026-05-28.md`。

### 0.1 v1.2 → v1.3 變更（最新一輪定案，T2 SO 即建）

| 議題 | v1.2 | **v1.3 最終** | 理由 |
|---|---|---|---|
| T2 S_Order 建立時機 | 延遲到 `billing-events/sync` 時 `orders/ensure` 才建 | **Receiving/Outbound 儲存時即雙寫 T2 SO** | SPA 看得到「此 receiving/出貨對應的 SO」；計費路徑提前明確；不必延遲 ensure |
| SO 顆粒度（原 §17.4 待定）| 每客戶每期 vs 每櫃（待定） | **1 receiving = 1 inbound SO；1 outbound = 1 outbound SO**；Outbound SO `Refer_To_Order` 指向 source inbound SO | 1:1 對應，清楚；用既有 `S_Order.Refer_To_Order` 欄位連結 |
| Invoice 顆粒度 | 不明 | **1 SO = 1 invoice**（Inbound SO 出 Receiving+Transload+Storage 帳；Outbound SO 出 Outbound 帳） | 簡單，無需合併多 SO 機制 |
| Phase 0 範圍 | 只動 WMS（計費 Phase 1）| **擴大**：含 T2 端 `orders/inbound` + `orders/outbound` API + SPA 雙寫 | 即建 SO 需要 Phase 0 就接 T2 |
| T2 新 API | quotes / billing-events / orders/ensure / invoices | **加 2 個**：`POST /api/transload/orders/inbound`、`POST /api/transload/orders/outbound`（`orders/ensure` 退為 fallback） | 雙寫需要明確 endpoint |
| 雙寫策略 | n/a | **SPA 端串接呼叫**：先 WMS Receiving/Outbound API → 成功後接呼 T2 orders/inbound 或 outbound API；T2 失敗 → SPA warning 並依賴 sync 的 fallback ensure 補建 | 不在後端做 cross-system transaction；失敗有 fallback |

### 0.2 v1.1 → v1.2 變更

| 議題 | v1.1 | **v1.2 最終** | 理由 |
|---|---|---|---|
| Quote header | 新表 `S_Transload_Rate_Card` | **延用既有 `S_Rate_Extra_Contract`**（2460 筆既在用） | 結構相容 (Client_UID + Start/End + Apply/Pricing_Type) |
| Quote item | 寫 `S_Rate_Service` | **開新表 `S_Quote_Item`**（命名通用，未來 T2 其他模塊可用） | S_Rate_Service 不夠表達 quote 結構；Extra_Item 缺 Service_UID |
| Shipment Request | 規劃兩張新表 | **延用 `S_Order` + `S_Order.Type=transload_request`** | 既有 order workflow 可表達 |
| WMS_Manifest 加欄位 | 7 個 (+SealNo/Container/Loading/Stackable/Arrival/TotalPallets/TotalCartons) | **3 個** (SealNo/ContainerType/LoadingType) | TotalPallets/Cartons 動態 SUM；ArrivalDate 用 MIN(PayLoad.ReceivedDate)；Stackable 移到 Vessel |
| WMS_BOL 加欄位 | 3 個 (+Tracking/ShipmentMode/POD) | **2 個** (TrackingNumber/PODUrl) | ShipmentMode 用 WMS_ShipMethod 既有表 |
| WMS_Inventory 加欄位 | 1 個 (SourceManifestItemUID) | **0 個** | 走完整 receiving，靠 WMS_PayLoad 反查 |
| WMS_Vessel 加欄位 | 0 個 | **1 個** (StackableType) | Stackable 是出倉那層的疊放屬性 |
| WMS_ShipMethod | 不動 | **新增 2 筆 system-wide**：LTL / FTL | 取代 BOL.ShipmentMode 欄位 |
| Phase 0 receiving 流程 | cross-dock 簡化直入庫（待定） | **走完整 WorkOrder→Ticket→PayLoad→Inventory** | 為了用 `WMS_PayLoad.ReceivedDate` 與 per-payload 反查 |
| 出貨單 / Invoice 分工 | 不明 | **BOL by WMS（含 SKU+地址）, Invoice by T2 per container**（T2 不存 SKU） | 各自最自然出對應文件；客戶要 SKU 看 BOL |

### 0.2 v1.0 → v1.1 變更（前一輪，仍有效）

實地查證 WMS@192.168.1.21 與 T2@192.168.88.16 兩台 DB 後收斂。詳見 memory `db-schema-verified-2026-05-28.md`。

| 議題 | v1.0 (ChatGPT) | **v1.1 最終** | 理由 |
|---|---|---|---|
| T2 新表數量 | 5 張：Customer_Map / Quote / Quote_Item / Source_Map / Billing_Event | **只 1 張**：`S_Transload_Rate_Card` 報價單表頭 | 既有 `S_Rate_Service`/`S_Order_Container_Leg_Journal`/`InvoiceContext.CreateInvoice` 已具備所有功能，不重造輪子 |
| 報價單明細 | `S_Transload_Quote_Item` 新表 | **直接寫 `S_Rate_Service`** | Client_UID+Service_UID+Price+StartDate/EndDate 即為一筆 quote line |
| 計費 charge | `S_Transload_Billing_Event` 快照表 | **直接寫 `S_Order_Container_Leg_Journal`** | 既有表已含 Remain_Amount/Remain_Qty 判定未開票，CreateInvoice 直接吃 |
| WMS↔T2 客戶映射 | T2 新表 `S_Transload_Customer_Map` | **不加任何欄位 — WMS `YAEP_Party.ID` 直接寫入 T2 `S_Client.ID` 同字串**（使用者拍板：建一樣 ID 就好，WMS 配合 T2） | 客戶代碼字串對齊即可 |
| WMS↔T2 訂單貨櫃 | `S_Transload_Source_Map` 新表 | **不加任何欄位 — `S_Order_Container.ID = WMS_Manifest.RefNo = ConNo` 自然對應**（使用者拍板，跟客戶映射同樣道理） | ConNo 字串對齊；既有欄位夠 |
| Storage 階梯計費 | 自定 | **既有 `S_Rate_Extra_Contract/Item.Duration_Start/End/Days_Type`** | 既有機制完整 |
| Free_Days 來源 | rate card / customer | **`S_Contract_Demurrage.Free_Days` 第一順位**，備援 `S_Client.Free_Days` | 既有 per Client/SSL/State 設定 |
| 測試帳號 | 新建 Marie/john/sara/pacific | **既有 `mariew` 員工 + 任一含 PACIFIC 的 S_Client（如 `Client-5465 ALUMAFOLD PACIFIC INC`）** | 使用者明確指示用 T2 現有 customer |
| 客戶登入機制 | 隱含 | **`Core_User.Account = S_Client.Account_ID` 是 T2 既成 link**（2008 筆 client 使用）；SPA 登入後以 JWT.Account 反查 `S_Client.Account_ID` 取 `Client_UID` | 不另造對應表 |
| Container Type | 隱含 int | **WMS 寫 int enum；T2 sync 時轉成 `S_Order_Container.Type` 的 uniqueidentifier**（T2 端是 GUID） | T2 既有 schema 限制 |
| 金額精度 | 混用 decimal(10,4) / (18,4) | **一律 `decimal(10,4)`** 對齊 T2 既有 Leg_Journal | 對齊既有 |
| Carrier 表 | 寫到 `WMS_ShipVia` | **改用 `WMS_ShipMethod`**（`WMS_ShipVia` **不存在**） | 修正 v1.0 錯誤 |
| Portal ShipmentRequest 表 | 開在 WMS DB | **改開在 T2 DB**（如要保留為 row）`S_Transload_Shipment_Request` + `_Item` | 對齊「新表開 T2」規則 |
| **Outbound 來源選擇** | 強制先選 Container（ContainerUID 必填） | **兩種模式：By Container（單櫃出貨/Full Container Release）+ By SKU（直接挑 SKU，跨櫃自動 FIFO 配貨）** ContainerUID 改為**選填**；Items[] 改為 per-line 帶 SourceManifestUID/SourceManifestItemUID | 使用者拍板：客戶常常只說「我要 500 件 SKU X」、不知道也不關心在哪個櫃；同 SKU 散在多櫃時系統自動依 aging 老→新配貨可降低 storage 帳 |
| **實作優先順序** | T2 DB → WMS → 計費 → UI | **Phase 0：進存出貨優先（下週到貨）** → Phase 1 計費 → Phase 2 SPA 其餘 → Phase 3 整合 | 使用者明確指示 |

---

## 1. 目的與系統邊界

本系統處理 Transload 倉儲作業：入倉 Receiving、SKU 建檔、Inventory 與 Aging、Outbound、Customer Portal 出貨申請、Quote Sheet 報價、T2 Order 對應、可計費事件、開立 T2 Invoice。

設計重點：

- UI 是獨立新平台位於 `E:\Project\TransloadWMS`
- SKU / 入倉 / 出倉 / 庫存資料存 **WMS**
- 報價 / 計費 / T2 Order / Invoice 存 **T2**
- 新平台需要新資料表，**建立在 T2 DB**（且最大化延用既有，本版只建 1 張）
- WMS 加欄位命名比照 WMS；T2 加欄位/新表命名比照 T2
- 登入用 T2 `Core_User` / Auth API；客戶身分以 `S_Client.Account_ID = Core_User.Account` 對應

## 2. 既有專案位置

```text
E:\Project\TransloadWMS                              <- 本 SPA + 文件
E:\Project\Trucking\Trucking2000                     <- T2
E:\Project\Trucking\Trucking2000\HeptarunWebAPI      <- T2 API
E:\Project\Trucking\Trucking2000\Heptarun            <- T2 後端邏輯
E:\Project\Trucking\Trucking2000\HRTWebSite          <- T2 前端 UI (WebForms+ExtNet)
E:\Project\DC\WMS\API                                <- WMS API (YAEP.WMS, .NET 4.6.2, Dapper)
```

DB：

- WMS：SQL Server 2022 @192.168.1.21 / DB=`WMS` / sa
- T2 ：SQL Server 2022 @192.168.88.16 / DB=`Heptarun` / sa

## 3. 命名規則

### 3.1 T2 命名（既有 + 新表/欄位皆照此）

| 類型 | 命名 |
|---|---|
| PK | `UID uniqueidentifier DEFAULT(newid())` |
| 業務代碼 | `ID varchar(50)` 或 `nvarchar(100)` |
| FK | 底線，如 `Customer_UID`、`SO_UID`、`Container_UID`、`Service_UID` |
| 日期 | `Invoice_Date`、`Effective_Date` 等 |
| 金額 | `decimal(10,4)` |
| audit | `CreatedBy nvarchar(50) / CreatedOn datetime / ModifiedBy / ModifiedOn` |
| 名稱前綴 | `S_Transload_*`（屬 Sales/billing 域） |

### 3.2 WMS 命名（既有 + 新欄位皆照此）

| 類型 | 命名 |
|---|---|
| PK | `UID uniqueidentifier DEFAULT(newid())` |
| FK | **無底線** PascalCase，如 `WarehouseUID`、`PartyUID`、`ItemUID` |
| audit | `CreatedBy varchar(50) / CreatedOn datetime DEFAULT(getutcdate()) / ModifiedBy / ModifiedOn` |

## 4. 使用者角色與登入

### 4.1 測試帳號（使用 T2 既有資料，不新建）

| 角色 | T2 Core_User Account | 對應 S_Client | 備註 |
|---|---|---|---|
| 管理員 | `mariew`（既有） | n/a | UserType = 內部員工 |
| 客戶 | 任一既有 S_Client.Account_ID（如含 `PACIFIC` 的客戶） | 例如 `Client-5465 ALUMAFOLD PACIFIC INC` | UserType = External_Customer |

> **取消**：v1.0 表 4.1 列的 Marie / john / sara / pacific 帳號**不新建**。使用者明確指示「用 T2 現有 customer」。

### 4.2 登入 API

| 角色 | API |
|---|---|
| 員工 | `POST /api/auth/internal/login` |
| 客戶 | `POST /api/auth/ext/login` |

T2 Auth：`E:\Project\Trucking\Trucking2000\HeptarunWebAPI\Controllers\AuthController.cs`

回應 `AuthenticationInfo`：`UID / Account / MemberName / Identification / LoginedTime / ExpirationTime / Email / Skype / FirstName / LastName / Country / State / UserType / ID / Token / Phone / Alt_Phone`。

### 4.3 客戶身分對應（**重要：T2 既成機制**）

- T2 既有：`Core_User.Account` 字串等於 `S_Client.Account_ID` 字串（**2008 筆 client 用此 link**）。
- `Core_User` **沒有 Client_UID 欄位**；ext/login JWT 只夾 user UID/Account/Type。
- **SPA 登入後**：拿 JWT.Account → 呼叫 T2 `/api/transload/me`（新增；見 §8.1）→ 反查 `S_Client.Account_ID = JWT.Account` → 取得 `S_Client.UID`（= Client_UID）。
- 後續所有 API 帶 `Client_UID`（T2 `S_Client.UID`）；WMS 端透過 **`YAEP_Party.ID = S_Client.ID`** 字串對應反查 `YAEP_Party.UID`（= `WMS_Party_UID`）。兩個 UID 各自保留，**只透過 ID 字串 join**。

### 4.4 權限

| 角色 | 可用頁面 |
|---|---|
| Admin / Employee | 全部（Dashboard / Receiving / Inventory / Outbound / Quote / Billing / Settings） |
| Customer | Portal、自己的 Inventory / Shipment / Invoice |

## 4.5 UI 慣例（從原型截圖確認，HTML 沒明說的版面細節）

- **品牌欄（左上）**：「**TRANSLOAD WMS**」橘字 + 副標「3PL MANAGEMENT」灰字
- **使用者區（右上）**：`姓名 + 角色標籤(ADMIN/STAFF/CUSTOMER) + Sign Out`，例「Marie Administrator | ADMIN | Sign Out」
- **Nav 編號順序**：`Dashboard | 1. Customer Mgmt | 2. Receiving | 3. Inventory | 4. Outbound | 5. Billing | Customer Portal | Warehouses | (Admin: Users)`（編號隨工作流）
- **每個列表頁通用 filter bar**：`All Customers` 下拉 + 中央 `Search XXX...` 框
- **欄位標籤雙語**（英 + 中括號）：CONTAINER NO. (柜号) / SEAL NO. (封条号) / ARRIVAL DATE (到仓日期) / CUSTOMER (客户) / LOADING TYPE (装柜类型) / TOTAL QTY (总数量) / TOTAL CARTONS (总箱数) / TOTAL PALLETS (总托数) / WEIGHT (重量) / CBM (体积) / SKU DETAILS (产品) / AGING (库龄) / 等
- **必填以紅色 `*` 標示**
- **彩色 badge 規範**：
  - Loading Type：F2F=紅 / F2P=紫 / P2P=青 / Full Container=灰
  - Container Type：藍灰系
  - Status (Container)：In Storage=綠 / Partially Out=橘 / Fully Out=灰
  - Aging：≤ FreeDays=綠 / FreeDays+1..30d=橘 / >30d=紅（同時也是 "SKUs AGING > 30 DAYS" KPI 門檻）
- **KPI 卡片**：每頁頂端 4-card 區（不只 Dashboard，Inventory 頁也有）
- **主 CTA 用橘色**（Save Receipt / Save Quote Sheet / Generate Invoice）；次要按鈕灰白

---

## 5. UI 頁面總表

獨立 SPA（建議 React + Vite + TypeScript），路徑：`E:\Project\TransloadWMS\src\`。

| 頁面 | 路徑 | 說明 |
|---|---|---|
| Login | `/login` | T2 帳號登入 |
| Dashboard | `/dashboard` | 作業總覽 |
| Customers | `/customers` | 客戶資料檢視（讀 T2） |
| Quotes | `/quotes` | Quote Sheet / 報價 |
| Receiving | `/receiving` | 入倉 |
| Inventory | `/inventory` | 庫存 |
| Outbound | `/outbound` | 出倉 |
| Billing | `/billing` | 可計費 / Invoice |
| Customer Portal | `/portal` | 客戶入口 |
| Settings | `/settings` | 系統設定 |

## 5.1 SPA 頁面 ↔ API 對照矩陣（implementation checklist）

> 標記：♻️ 既有直接重用 / ✏️ 既有擴充欄位 / 🆕 新增端點。WMS=YAEP.WMS API；T2=HeptarunWebAPI。S2S=server-to-server，SPA 不直接呼叫。

### 5.1.1 Auth & 共用
| 動作 | 端點 | 標記 | 備註 |
|---|---|---|---|
| 員工登入 | T2 `POST /api/auth/internal/login` | ♻️ | 既有 AuthController；UserType 員工 |
| 客戶登入 | T2 `POST /api/auth/ext/login` | ♻️ | 既有；UserType=External_Customer |
| 取自己客戶身分 | T2 `GET /api/transload/me` | 🆕 | JWT.Account → `S_Client.Account_ID` → 回 `{ Client_UID, Client_ID, Client_Name, IsEmployee }` |
| WMS 服務認證 | WMS JWT（Phase 0 暫用 WMS 既有 auth；Phase 3 接 T2 SSO） | ♻️/待定 | §17.1 |

### 5.1.2 Login `/login`
| 動作 | 端點 | 標記 |
|---|---|---|
| 員工/客戶登入 | T2 `auth/internal/login` 或 `auth/ext/login` | ♻️ |
| 解析身分 | T2 `GET /api/transload/me` | 🆕 |

### 5.1.3 Dashboard `/dashboard`
| KPI / 區塊 | 端點 | 標記 |
|---|---|---|
| 在庫貨櫃/板/箱/件、Aging > free_days 數 | WMS `POST /api/Inventory/GetInventory`（擴充回應） | ✏️ |
| 今日入倉/出倉筆數 | WMS `GET /api/Transload/Summary?from=&to=`（**選用新端點**，或前端對 Manifest by date 自己彙總） | 🆕（選用） |
| 未出帳金額 | T2 `GET /api/transload/billing-events?billingStatus=unbilled&from=&to=` | 🆕 |
| 各客戶彙總 | WMS Inventory + T2 billing-events 兩邊 join（SPA 端聚合） | ♻️+🆕 |

### 5.1.4 Customers `/customers`（唯讀）
| 動作 | 端點 | 標記 |
|---|---|---|
| 客戶清單（讀 T2 + 對應 WMS Party） | T2 `GET /api/transload/customers` | 🆕 |
| 該客戶的有效 quote | T2 `GET /api/transload/quotes/active?customerUID=` | 🆕 |
| 該客戶當前在庫 | WMS `POST /api/Inventory/GetInventory`（傳 CustomerUID） | ✏️ |
| 該客戶 invoice 摘要 | T2 `GET /api/transload/invoices?customerUID=` | 🆕 |

### 5.1.5 Quotes `/quotes`
| 動作 | 端點 | 標記 |
|---|---|---|
| 報價單清單 | T2 `GET /api/transload/quotes?customerUID=` | 🆕 |
| 單筆報價單 | T2 `GET /api/transload/quotes/{uid}` | 🆕 |
| 取生效中報價單 | T2 `GET /api/transload/quotes/active?customerUID=&operationDate=` | 🆕 |
| Service 項主檔下拉 | T2 `GET /api/transload/service-items?chargeType=&keyword=` | 🆕 |
| 查現有 PC/PV 價 | T2 `GET /api/transload/contract-prices?customerUID=&serviceUID=&operationDate=` | 🆕 |
| 建立報價單 | T2 `POST /api/transload/quotes` | 🆕 |
| 修改報價單 | T2 `PUT /api/transload/quotes/{uid}` | 🆕 |
| 停用報價單 | T2 `POST /api/transload/quotes/{uid}/deactivate` | 🆕 |

### 5.1.6 Receiving `/receiving`
| 動作 | 端點 | 標記 |
|---|---|---|
| 客戶下拉（WMS YAEP_Party） | WMS `GET /api/Customer/GetCustomerList` | ♻️ |
| 倉庫下拉 | WMS `GET /api/Warehouse/*` | ♻️ |
| SKU 查詢 | WMS `GET /api/Product/GetProducts?keyword=&customerUID=` | ♻️ |
| 新增 SKU | WMS `POST /api/Product/AddProduct` | ♻️ |
| 收貨清單 | WMS `POST /api/Inventory/GetInventory`（依 ManifestRefNo / LoadingType / ArrivalDate 篩） | ✏️ |
| 建立收貨（WMS 端） | WMS `POST /api/Order/Receiving`（**擴充 3 欄**：SealNo/ContainerType/LoadingType；ArrivalDate 走 PayLoad；TotalPallets/Cartons 動態 SUM；StackableType 移到 Outbound Vessel） | ✏️ |
| **雙寫：T2 即建 Inbound SO** | T2 `POST /api/transload/orders/inbound`（v1.3 新增；WMS 收貨成功後 SPA 接呼）| 🆕 |
| 取生效中 rate card（顯示套用哪張） | T2 `GET /api/transload/quotes/active?customerUID=` | 🆕 |

### 5.1.7 Inventory `/inventory`
| 動作 | 端點 | 標記 |
|---|---|---|
| 庫存查詢 + aging | WMS `POST /api/Inventory/GetInventory`（**擴充 query**：ItemKeyword/ManifestRefNo/LoadingType/AgingDays 範圍；**擴充 response**：ConNo/SourceManifestItemUID/LoadingType/ArrivalDateActual/AgingDays/FreeDays/BillableStorageDays） | ✏️ |
| 取免費天數（顯示 BillableStorageDays） | T2 `GET /api/transload/quotes/active?customerUID=` 或 `GET /api/transload/contract-prices?serviceUID=STORAGE` | 🆕 |
| Per-container 在庫明細 | WMS `GET /api/Inventory/GetInventoryDetail?manifestUID=` | ♻️/✏️ |

### 5.1.8 Outbound `/outbound`
| 動作 | 端點 | 標記 |
|---|---|---|
| 出貨清單 | WMS `GET /api/Bol/*`（含 TrackingNumber/ShipmentMode/PODUrl） | ✏️ |
| Carrier / ShipMethod 下拉 | WMS `GET /api/ShipMethod/*` 或既有對應端點 | ♻️（需確認端點名） |
| 來源 container + 可出 SKU | WMS `POST /api/Inventory/GetInventory`（filter by ContainerUID） | ✏️ |
| 建立出貨（WMS 端） | WMS `POST /api/Order/Allocated`（**擴充**：SourceManifestUID/ShipMethodUID/TrackingNumber/PODUrl/ServiceLines[]） | ✏️ |
| 寫 BOL | WMS `POST /api/Bol/Create` 或 `Update`（擴充欄位） | ✏️ |
| **雙寫：T2 即建 Outbound SO** | T2 `POST /api/transload/orders/outbound`（v1.3 新增；WMS 出貨成功後 SPA 接呼，含 Inbound_SO_UID）| 🆕 |
| 上傳 POD 檔案 | WMS `POST /api/Bol/UploadPod?bolUID=` | 🆕（如需檔案 upload） |

### 5.1.9 Billing `/billing`（會計使用；HRTWebSite 不另開模組）
| 動作 | 端點 | 標記 |
|---|---|---|
| 同步 WMS 可計費事件 | T2 `POST /api/transload/billing-events/sync` | 🆕 |
| 未開票清單（依 Remain_Amount） | T2 `GET /api/transload/billing-events?customerUID=&from=&to=&billingStatus=unbilled` | 🆕 |
| 已開票清單 | T2 同上端點 `billingStatus=invoiced` | 🆕 |
| 確認 charges | T2 `POST /api/transload/billing-events/confirm` | 🆕 |
| 確保 SO 存在 | T2 `POST /api/transload/orders/ensure` | 🆕 |
| 開 Invoice | T2 `POST /api/transload/invoices/generate` | 🆕 |
| Invoice 清單 | T2 `GET /api/transload/invoices?customerUID=&from=&to=` | 🆕 |
| 下載 PDF | T2 `GET /api/transload/invoices/{uid}/pdf` 或既有 `Invoice/Download` | ♻️/🆕 |

### 5.1.10 Customer Portal `/portal`（客戶限用）
| 動作 | 端點 | 標記 |
|---|---|---|
| 我的在庫 | WMS `POST /api/Inventory/GetInventory`（後端依 JWT.Account → S_Client.ID → YAEP_Party.UID 強制過濾 CustomerUID） | ✏️ |
| 我的出貨單 | WMS `GET /api/Bol/*?customerUID=` | ✏️ |
| 我的 invoice | T2 `GET /api/transload/invoices?customerUID=`（依 JWT 強制過濾） | 🆕 |
| 提出出貨申請 | T2 `POST /api/transload/shipment-requests` | 🆕 |
| 我的申請紀錄 | T2 `GET /api/transload/shipment-requests?status=` | 🆕 |
| 員工 process/reject 申請 | T2 `POST /api/transload/shipment-requests/{uid}/process \| reject` | 🆕 |

### 5.1.11 Server-to-Server（SPA 不呼叫）
| 動作 | 端點 | 標記 |
|---|---|---|
| T2 撈 WMS 可計費事件 | WMS `GET /api/Transload/BillableEvents?customerUID=&from=&to=` | 🆕 S2S |
| T2 確保 WMS Party 存在（migration） | WMS `POST /api/Party/Upsert`（建議端點，避免靠 ID 直接 INSERT） | 🆕 S2S（選用） |

### 5.1.12 統計（v1.3 更新）

| 區段 | ♻️ 既有 | ✏️ 擴充 | 🆕 新增 |
|---|---|---|---|
| WMS | 6 個（Login/Customer/Warehouse/Product×2/ShipMethod） | 5 個（Receiving/Inventory/Allocated/BOL×2） | 2 個（Transload/BillableEvents S2S、選用的 UploadPod / Party/Upsert / Summary） |
| T2 | 2 個（auth internal/ext） | 0 | **約 20 個**：me, customers, service-items, contract-prices, quotes(×5), billing-events(×3), **orders/inbound, orders/outbound** (v1.3 新增), orders/ensure (fallback), invoices(×3), shipment-requests(×3) |

**結論**：SPA 端開發優先順序
- Phase 0：WMS ✏️ 5 個 + ♻️ 6 個 + **T2 🆕 orders/inbound + orders/outbound + me + customers**（v1.3 即建 SO 需）
- Phase 1：T2 🆕 計費相關約 14 個（quotes / billing-events / invoices / contract-prices / service-items）
- Phase 3：補上 Portal shipment-requests / SSO

## 6. UI 詳細規格

### 6.1 Login

| 欄位 | 型別 | 必填 |
|---|---|---|
| `account` | string | Y |
| `password` | string | Y |
| `loginType` | enum `internal` / `customer` | Y |

成功流程：
1. 呼叫 T2 internal 或 ext login → 取得 JWT。
2. 若 customer：呼叫 T2 `GET /api/transload/me` 反查 `Client_UID`。
3. 保存 JWT + Profile + (Client_UID for customer)。
4. Admin/Employee → Dashboard；Customer → Portal。

### 6.2 Dashboard

KPI：`inboundTodayCount / outboundTodayCount / activeContainerCount / onHandPallets / onHandCartons / agingOverFreeDaysCount / uninvoicedAmount`

各客戶彙總欄：`CustomerID / CustomerName / ContainerCount / Pallets / Cartons / Pieces / AgingOverFreeDays / UninvoicedAmount`

來源：WMS Inventory summary + T2 uninvoiced charge summary。

### 6.3 Customers（唯讀）

| 欄位 | 來源 |
|---|---|
| `CustomerCode` | `S_Client.ID` 或 `Account_ID` |
| `CustomerName` | `S_Client.Name` |
| `T2_Client_UID` | `S_Client.UID` |
| `WMS_Party_UID` | `YAEP_Party.UID`（透過 `YAEP_Party.ID = S_Client.ID` 反查） |
| `Contact_Name` / `Contact_Email` / `Contact_Phone` | `S_Client.*` |
| `Billing_Term_UID` | `S_Client.Payment_Term_ID` |
| `Status` | `S_Client.Status` |

API：
- T2 `GET /api/transload/customers`（讀既有 S_Client + 加上 WMS Party 對應）
- WMS `GET /api/Customer/GetCustomerList`（既有 WMS Party 清單）

### 6.4 Quote Sheet / Contract Pricing（v1.2 最終版）

**v1.2 原則**：報價單表頭**延用既有 `S_Rate_Extra_Contract`**（不開新表）；費率明細**寫進新表 `S_Quote_Item`**（通用命名，未來其他模塊可用）。

#### 表頭欄位（寫入既有 `S_Rate_Extra_Contract`）

| UI 欄位 | T2 既有欄位 | 必填 | 說明 |
|---|---|---|---|
| `ID` | `ID` (int auto) | — | 既有 ID 是 int auto |
| `Name` | `Description` | Y | 顯示名（如 "PACIFIC 2026 Q2 Transload Quote"）|
| `Client_UID` | `Client_UID` | Y | T2 S_Client.UID |
| `Effective_Date` | `StartDate` | Y | 既有 |
| `Expire_Date` | `EndDate` | Y | 既有（v1.2 改必填，便於 active quote 判定） |
| — | `Apply_Type = 99` | — | **transload quote 識別碼**（待 §17.9 與既有 enum 對齊）|
| — | `Pricing_Type = 99` | — | 同上 |
| — | `From_Type / Destination_Type / *_UID / *_Zip_UID` | — | transload 不用路線，填 0 / NULL |

#### 費率明細（寫新表 `S_Quote_Item`，DDL 見 §9.2）

每一項費率 = 1 列 `S_Quote_Item`：

| 欄位 | 來源/值 |
|---|---|
| `Quote_Header_UID` | `S_Rate_Extra_Contract.UID` |
| `Quote_Header_Type` | `'S_Rate_Extra_Contract'` |
| `Service_UID` | 對應 `P_Item_Service.UID`（例 `F2F_UNLOAD` 對應的 service，§9.6） |
| `Display_Group` | F2F / F2P / P2P / Storage / Outbound（**持久化**，UI 自動分組） |
| `Loading_Type` | 10/20/30/40（**持久化**，計費 sync 時對應 Unload_Type） |
| `Calculation_Basis` | `per_pallet` / `per_container` / `per_carton` / `per_piece` / `per_day` / `fixed` |
| `Price` | decimal(10,4) |
| `Currency` | varchar(10) DEFAULT USD |
| `Sort_Order` | int |

#### Storage 階梯費率（複雜情況：用既有 `S_Rate_Extra_Item`）

如客戶需要分段 storage（例 Tier 1 免費 0–7 天 / Tier 2 8–14 天每板 $2.50 / Tier 3 15+ 天 $5.00），用既有 `S_Rate_Extra_Item` 接在同一張 `S_Rate_Extra_Contract` 下：
- `S_Rate_Extra_Item.Contract_UID = S_Rate_Extra_Contract.UID`（quote header）
- `S_Rate_Extra_Item.Duration_Start / Duration_End / Days_Type` 存階梯範圍
- `S_Rate_Extra_Item.Rate` 存該 tier 單價

> 也就是說 quote header (`S_Rate_Extra_Contract`) 可同時掛兩種 children：
> - `S_Quote_Item`（一般 transload 服務 19 項）
> - `S_Rate_Extra_Item`（storage 階梯，如果有）

#### 收費項目主檔（用既有 `P_Item_Service`，補 15 筆資料）

見 §9.6。Charge_Type 用 10/20/30/40（既有只有 1/2，transload 自行新增分類值）。

### 6.5 Receiving 入倉頁

查詢欄位：`Client_UID / WarehouseUID / ConNo / LoadingType / ArrivalDateFrom/To / Status`

列表欄：`ManifestUID / ConNo / SealNo / CustomerName / WarehouseName / ContainerType / LoadingType / ArrivalDateActual* / TotalPallets* / TotalCartons* / TotalQty* / AgingDays* / BillingStatus / Status`
（*=動態算或聚合，非 manifest row 上的欄位；query 須 join 計算，見 §10.5~10.7）

表單欄位（POST → WMS `/api/Order/Receiving`，擴充）：

| 欄位 | 型別 | 必填 | 寫入 |
|---|---|---|---|
| `RefNo` | string | Y | `WMS_Manifest.RefNo` (=ConNo) |
| `WarehouseUID` | Guid | Y | `WMS_Manifest.WarehouseUID` |
| `CustomerUID` | Guid | Y | `WMS_Manifest.PartyUID`（WMS Party UID；用 `S_Client.ID` 反查 YAEP_Party.ID） |
| `CustomerPartyName` | string | N | 解析後填 CustomerUID |
| `ReceivingType` | int | Y | 既有 |
| `IsTransferOrder` | bool | N | 既有 |
| `Volume` | decimal | N | `WMS_Manifest.Volume` |
| `Weight` | decimal | N | `WMS_Manifest.Weight` |
| `Description` | string | N | `WMS_Manifest.Description` |
| `SealNo` | string | N | **新增 WMS 欄位** |
| `ContainerType` | int | Y | **新增 WMS 欄位**（int enum；T2 sync 時轉 `S_Order_Container.Type` UID） |
| `LoadingType` | int | Y | **新增 WMS 欄位**（10/20/30/40） |
| `ArrivalDate` | DateTime | Y | **不存 Manifest**；寫到每筆 `WMS_PayLoad.ReceivedDate`（receiving 流程走完整 WorkOrder/Ticket/PayLoad；§10.7 用 MIN 聚合） |
| `Container[]` | array | Y | Receiving Container |
| `ImportItems[]` | array | N | 自動建 SKU |

> v1.2 移除的欄位：~~StackableType~~（移到 §6.7 Outbound 寫進 `WMS_Vessel.StackableType`）、~~TotalPallets/TotalCartons~~（動態 SUM via Manifest_Item_List join YAEP_Package，§10.6）、~~ArrivalDateActual on Manifest~~（用 PayLoad.ReceivedDate MIN 聚合，§10.7）。

#### v1.3 Receiving 儲存後的雙寫流程（**新增**）

```
1. SPA 按 [Save Receipt] → POST WMS /api/Order/Receiving
   → 取得 { ManifestUID, RefNo (= ConNo), Status }
2. 成功則 → SPA 接呼 POST T2 /api/transload/orders/inbound
   → 取得 { SO_UID, SO_ID, Container_UID }
3. SPA 把 SO_UID 暫存在 form state；Outbound 頁建立此 container 的出貨時帶入作 Inbound_SO_UID
4. 失敗處理：
   - WMS 失敗 → 整個 receiving 取消，提示錯誤
   - T2 失敗 → WMS 已寫入，提示 warning「T2 SO 未建，將於 Billing sync 時自動補建」
     （Phase 1 Billing sync 透過 §8.6.3 orders/ensure fallback 補）
```

> v1.3 拍板：T2 SO 在 receiving 儲存時即建（不延後到 Billing sync）。理由：SPA 之後出貨頁能立刻看到該 container 對應的 inbound SO，計費路徑提前明確。

`Container[].Items[]`：

| 欄位 | 型別 |
|---|---|
| `ItemUID` (既有 SKU) / `Name` (新 SKU) | Guid / string |
| `Barcode` | string |
| `PackageUOM` / `PackageQty` | string / int |
| `Cartons` / `Pallets` | int / int |

### 6.6 Inventory（庫存管理）

**頂部 KPI 卡（4 張，依 filter 範圍動態算）**：
- `TOTAL UNITS ON HAND`（在庫件數）
- `TOTAL CARTONS`（在庫箱數）
- `TOTAL PALLETS`（在庫板數）
- `SKUs AGING > 30 DAYS`（固定 30 天門檻，補 Dashboard 用客戶 FreeDays 動態門檻的另一 KPI）

**Filter bar**：`All Customers` 下拉 + `Search SKU or description...`

查詢欄位：`Client_UID / WarehouseUID / ItemKeyword / ManifestRefNo / LoadingType / AgingDaysFrom/To`

**列表欄**（對齊原型 Img 7）：`SKU / DESCRIPTION / CUSTOMER / CONTAINER NO. / LOADING TYPE(彩色badge) / ON HAND / CARTONS / PALLETS / WAREHOUSE / RECEIVED DATE / AGING (库龄, 彩色badge)`

完整 API 回應欄位：`InventoryUID / ItemUID / SKU / ItemName / CustomerUID / CustomerID / CustomerName / WarehouseUID / WarehouseName / PackageUID / PackageName / Onhand / AllocatedQty / AvailableQty / ConNo / SourceManifestItemUID / ArrivalDateActual / AgingDays / FreeDays / BillableStorageDays`

> 注意：WMS_Inventory **無 PartyUID**，客戶資訊透過 `SourceManifestItemUID → WMS_Manifest_Item_List → WMS_Manifest.PartyUID` 帶出。

API：WMS `POST /api/Inventory/GetInventory`（擴充）

### 6.7 Outbound 出倉頁

查詢：`Client_UID / ConNo / ShipOrderNo / ShipDateFrom/To / Status`

列表欄：`VesselUID / BolUID / ShipOrderNo / SourceCons (聚合) / CustomerName / ShipmentMode / ShipDate / Carrier / TrackingNumber / BolNo / ShipTo / ShippedPallets / ShippedCartons / ShippedQty / BillingStatus / Status`

> 因一張出貨單可能來自多個 container（By-SKU 模式），列表的 `SourceCons` 欄位顯示所有來源 ConNo（單櫃顯示 1 個，跨櫃顯示 `N1, N2, …` 或 `MSCU1234 +2`）。

> **v1.2 簡化（使用者 2026-05-28 拍板）**：Phase 0/1 **SPA 只實作 By-SKU 模式**（Outbound + Portal 兩邊邏輯一致），By-Container UI 暫時隱藏（型別/後端流程保留以後可開）。Items 輸入改成「**1 個數量 + 1 個 UOM 下拉**（each / box / pallet）」、不再拆 Qty/Cartons/Pallets 三格。後端依 `Product.PcsPerCarton` / `PcsPerPallet` 換算成 pieces 再做 FIFO 拆單。

#### 6.7.1 表單兩種建單模式（**使用者拍板**）

| 模式 | 觸發時機 | UX |
|---|---|---|
| **By Container**（單櫃出貨） | 客戶要求把某櫃整出 / Full Container Storage release / cross-dock 一次出完 | 先選 Container → 自動帶出該櫃所有 on-hand SKU → 勾選並填 QtyToShip |
| **By SKU**（依產品出貨）**【預設】** | 客戶說「我要 N 件 SKU X」、不關心在哪個櫃 | 先選 Customer → SKU 搜尋（顯示 across-containers 總 on-hand）→ 填 QtyToShip → 後端依 **FIFO（aging 最久優先）** 自動分配各 container 出庫量 |

> 兩種模式共用同一個 POST endpoint；差別只在前端組 Items[] 時是否帶 `SourceManifestUID`（By Container 帶單一值；By SKU 由後端自動拆分後寫入）。

#### 6.7.2 表單欄位（POST → WMS `/api/Order/Allocated` + WMS BOL API）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `CustomerUID` | Guid | Y | WMS Party UID |
| `WarehouseUID` | Guid | Y |  |
| `ContainerUID` (=來源 ManifestUID) | Guid | **N**（v1.1 改選填） | By Container 模式填；By SKU 模式留空 |
| `Inbound_SO_UID` | Guid | **Y (by-container)** / **N (by-sku 多櫃)** | v1.3：對應 Inbound SO；By Container 必填（即來源 container 的 inbound SO_UID）；By SKU 模式可空，由後端 FIFO 拆分後**每個 source container 各建 1 張 outbound SO**（refer to 對應 inbound SO） |
| `Mode` | string | Y | `'by-container'` \| `'by-sku'`（v1.1 新增；後端依此驗證） |
| `ShipOrderNo` | string | Y |  |
| `ShipDate` | DateTime | Y |  |
| `ShipToName / Address / City / State / Zip / Country` | string | N |  |
| `Carrier` | string | N | 解析至 `WMS_ShipMethod` |
| `TrackingNumber` | string | N |  |
| `BolNo` | string | N |  |
| `ShipMethodUID` | Guid | Y | v1.2：對應 `WMS_ShipMethod` LTL/FTL 兩筆 system-wide 資料（取代原 `ShipmentMode int`） |
| `StackableType` | int | N | v1.2 新增 → 寫進 `WMS_Vessel.StackableType` (0 Stackable / 1 Non-Stackable) |
| `PODUrl` | string | N | `WMS_BOL.PODUrl` |
| `Description` | string | N |  |
| `Items[]` | array | Y | 見下 |
| `ServiceLines[]` | array | N |  |

> **修正**：v1.0 寫 `WMS_ShipVia`，實際 WMS DB **沒有 ShipVia 表**，只有 `WMS_ShipMethod`。Carrier 資訊掛 `WMS_ShipMethod`。
> v1.2：`ShipmentMode int` 取消 → 改成 `ShipMethodUID` 指向 `WMS_ShipMethod` LTL/FTL 兩筆既有資料。

`Items[]` 兩種模式對應欄位（**v1.2 新增 `RequestedQty` + `Unit` 欄位給 By-SKU UOM 輸入用**）：

> **By-SKU 欄位細節**：
> - `RequestedQty` (decimal)：使用者輸入的原始數量
> - `Unit` (string)：`'each' | 'box' | 'pallet'`
> - `QtyToShip` (int)：後端依 `RequestedQty × Product.PcsPerCarton/PcsPerPallet` 自動換算成 pieces 後寫入；用於 FIFO 配貨 + 庫存扣減


| 欄位 | By Container | By SKU（送入） | By SKU（後端拆完寫回 WMS 的最終 row） |
|---|---|---|---|
| `ItemUID` | Y | Y | Y |
| `SKU` | Y | Y | Y |
| `QtyToShip` | Y | Y（總量） | Y（單櫃分配量） |
| `CartonsToShip / PalletsToShip` | N | N | Y（後端從來源 inventory row 同比例算） |
| `SourceManifestUID` | 從 header 帶（單一值） | **空**（後端 FIFO 配） | **必填**（每行對應 1 個來源櫃） |
| `SourceManifestItemUID` | 後端 join 找 | **空** | 後端帶 `WMS_Manifest_Item_List.UID` |
| `PackageUID / PayloadUID` | N | N | 後端帶 |

**FIFO 配貨規則（By SKU）**：

1. 撈該 customer 該 SKU 所有 on-hand `WMS_Inventory` row
2. 依該 manifest 的 ArrivalDate（**取 `MIN(WMS_PayLoad.ReceivedDate)` 動態聚合**，§10.7）ASC（aging 老→新）排序
3. 依序扣減：`take = min(QtyToShip 剩餘, row.Onhand)`，拆成一個 outbound item line
4. 若總 on-hand 不足 → API 回 400（前端可在送出前用 `/api/Inventory/GetInventory` 預檢）

> **計費影響**：FIFO 老→新讓 aging 久（已過 free days、開始計 storage 費）的庫存優先出，可降低總 storage 帳 — 對客戶有利、也是業界慣例。
>
> **跨櫃出貨對應 WMS schema**：一張 outbound shipment（一個 `WMS_BOL` / `WMS_Vessel`）可包多個來源櫃，透過 `WMS_Vessel_Manifest` (M:N) 連到多個 outbound `WMS_Manifest`，每個 outbound manifest 連 1 個 source inbound manifest。By SKU 模式下後端可能建立 N 個 outbound manifest（1 per source container），全綁到同一個 vessel/BOL。

#### 6.7.3 v1.3 Outbound 儲存後的雙寫流程（**新增**）

```
1. SPA 按 [Save Shipment] → POST WMS /api/Order/Allocated + /api/Bol/Create
   → 取得 { VesselUID, BolUID, ShipOrderNo }
2. 成功則 → SPA 接呼 POST T2 /api/transload/orders/outbound
   → Request 含 Inbound_SO_UID（By Container 模式直接帶；By SKU 模式每個 source container 各呼一次）
   → 取得 { SO_UID, SO_ID, Container_UID }
3. 失敗處理：
   - WMS 失敗 → 整個 outbound 取消，提示錯誤
   - T2 失敗 → WMS 已寫入 vessel/BOL，提示 warning「T2 SO 未建，將於 Billing sync 時補建」
4. By SKU 跨櫃情況：1 個 WMS Vessel/BOL 對應 N 個 T2 Outbound SO（每 source container 1 張）
```

> v1.3 拍板：每次 Outbound 一張獨立 SO，連結回對應的 Inbound SO 透過 `S_Order.Refer_To_Order`。

`ServiceLines[]`：`Name / Qty / Unit / UnitPrice / Amount / Description`

`Unit` 列舉：`per pallet` / `per carton` / `per piece` / `per container` / `per day` / **`per shipment`**（原型實際使用 `per shipment` for Shipping Fee）

> Carrier 細部（原型 INIT_CARRIERS）含 `name / driver / truckNo / trailerNo`。SPA 顯示 carrier 下拉時用 `name`；driver/truck/trailer 屬 T2 卡車域的既有 entity，不在 WMS 重複建。WMS_ShipMethod 只存 carrier name。

### 6.8 Billing 計費頁（會計使用）

查詢：`Client_UID / From / To / Billing_Status / Invoice_UID / ConNo`

列表欄（**直接讀 `S_Order_Container_Leg_Journal`**，不另存 Billing_Event）：

`Leg_Journal_UID / SO_UID / Container_UID / Invoice_UID / CustomerName / Reference_No / Container_No / Service_ID / Service_Name / Charge_Type / Qty / Rate / Amount / Currency / Service_Date / Billing_Status / Remain_Amount`

**顯示分組（依原型圖 65）**：列表依 `Charge_Type` 折疊成四區塊，順序固定：
1. **RECEIVING**（10）：unload fee
2. **TRANSLOAD**（20）：sorting / handling / reload / palletizing / pallet / wrap / label / p2p loading / p2p storage
3. **STORAGE**（30）：demurrage by day-range；line 上**顯示 free-period 副資訊**，例如 `Storage Fee (4d billable of 18d aging, 14d free)`，免費期內顯示 `within free period (4d of 14d free) — $0.00`
4. **OUTBOUND**（40）：shipping fee / handling fee / 手動 service lines

每客戶一張 collapsable 表格 header；表格內依四類分區累加 sub-total + grand total。

按鈕：`[Sync WMS Events]` / `[Create/Update T2 Order]` / `[Confirm Charges]` / `[Generate Invoice]` / `[View Invoice]` / `[Export]`

API：T2 `/api/transload/billing-events/sync`、`/billing-events`、`/orders/ensure`、`/billing-events/confirm`、`/invoices/generate`。

### 6.9 Customer Portal

頁面：My Inventory / Shipment Request / My Shipments / My Invoices。

> **v1.2 簡化**：與 §6.7 一致，SPA Phase 0/1 **只實作 By-SKU 模式**；Items 用「數量 + UOM 下拉 (each/box/pallet)」一格輸入，倉庫端核准時依 Product 規格 + FIFO 配貨。

#### 6.9.1 Shipment Request 兩種建單模式（**對齊 §6.7 Outbound**）

| 模式 | UX |
|---|---|
| **By Container** | 客戶在 My Inventory 找到某櫃 → 點「申請此櫃出貨」→ 帶出該櫃所有 on-hand SKU |
| **By SKU**（預設） | 客戶輸入 SKU + 數量；不必知道在哪個櫃；倉庫端核准時系統 FIFO 配貨 |

Shipment Request 表單欄位：

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `Client_UID` | Guid | Y | 由登入身分自動帶 |
| `Mode` | string | Y | `'by-container'` \| `'by-sku'` |
| `ContainerUID` (=ManifestUID 來源) | Guid | **N**（v1.1 改選填） | By Container 模式填 |
| `RequestedDate` | DateTime | Y |  |
| `ShipTo` | string | Y |  |
| `Notes` | string | N |  |
| `Items[]` | array | Y |  |

Items：`ItemUID / SKU / QtyRequested`（By SKU 模式不帶 SourceManifestUID；倉庫核准時才由 §6.7 FIFO 規則配）

API：
- T2 `POST /api/transload/shipment-requests`（**改開在 T2**；見 §8.7）
- T2 `GET /api/transload/shipment-requests?customerUID=`
- T2 `GET /api/transload/customer/invoices`

> **修正**：v1.0 把 ShipmentRequest 放在 WMS，違背「新表開 T2」規則；v1.1 移到 T2。
> **v1.1 新增**：兩種模式 + ContainerUID 改選填。對應 §9.3 表 schema：`Container_UID uniqueidentifier NULL`、`Mode varchar(20) NOT NULL`。

---

## 7. WMS API 規格

專案：`E:\Project\DC\WMS\API\YAEP.WMS.API`。回傳 `APIResult<T>`。

### 7.1 Product / SKU API（既有，直接重用）

#### 7.1.1 `GET /api/Product/GetProducts` （既有，line 75）

Query：`keyword?` / `customerUID?`

Response：`UID / ID(SKU) / Name / CustomerUID / Description`

#### 7.1.2 `POST /api/Product/AddProduct` （既有，line 206）

Request `ProductRequestModel`（已存在於 WMS）：`UID? / ID(SKU) / Name / CustomerUID / CategoryUID? / GroupUID? / UPC / EAN / IsBOM / Description / ImageUID / LengthInch/CM / WidthInch/CM / HeightInch/CM / NetWeightKG/LB / GrossWeightKG/LB / StockUOM / PurchaseUOM / SellingUOM / ShipUOM`

### 7.2 Receiving API

#### 7.2.1 `POST /api/Order/Receiving` （**擴充既有 DTO**）

請求欄位見 §6.5 表單。**v1.2 新增 3 個欄位**（WMS 命名無底線）：`SealNo / ContainerType / LoadingType`。其餘原規劃 4 欄已取消：`StackableType` 移到 Outbound `WMS_Vessel`、`ArrivalDate` 走 `WMS_PayLoad.ReceivedDate`、`TotalPallets / TotalCartons` 動態 SUM。

Response：`ManifestUID / RefNo / Status`。

### 7.3 Inventory API

#### 7.3.1 `POST /api/Inventory/GetInventory` （**擴充既有 DTO 與回應**）

Request `InventorySearchParameters` 既有：`PHierarchy / CHierarchy / CustomerUID / WarehouseUID / AreaUID / BinUID / SlotUID`。新增：`ItemKeyword / ManifestRefNo / LoadingType? / AgingDaysFrom? / AgingDaysTo?`。

Response 既有欄位 + 新增：`ConNo / SourceManifestItemUID / LoadingType / ArrivalDateActual / AgingDays / FreeDays / BillableStorageDays`。

> 客戶資訊透過 `SourceManifestItemUID → WMS_Manifest_Item_List.ManifestUID → WMS_Manifest.PartyUID` 帶出（WMS_Inventory 本身無 PartyUID）。

#### 7.3.2 `GET /api/Inventory/GetInventoryDetail?itemUID=` （既有，line 198）

回單一 SKU 的詳細。

### 7.4 Outbound API

#### 7.4.1 `POST /api/Order/Allocated` （**擴充既有**）

請求欄位見 §6.7.2 表單。**v1.1 行為差異**：

- `Mode='by-container'`：原有行為，`ContainerUID` 必填，Items[] 全部對應同一個 source manifest。
- `Mode='by-sku'`（v1.1 新增）：`ContainerUID` 留空；Items[] 帶 `ItemUID + QtyToShip`；BLL 進入 **FIFO 拆單流程**：
  1. 對每個 `ItemUID`，撈 `WMS_Inventory` join `WMS_Manifest` where `PartyUID=CustomerUID AND Onhand>0`，依 `ArrivalDateActual ASC` 排序
  2. 依序扣減直到滿足 `QtyToShip`；產生 N 個內部 outbound item line（每行帶 `SourceManifestUID / SourceManifestItemUID / 該行 QtyToShip / 比例算出 CartonsToShip / PalletsToShip`）
  3. 為涉及到的每個 source manifest 建立 outbound `WMS_Manifest`（type=outbound），全部串到同一個 `WMS_Vessel` + `WMS_BOL`（透過 `WMS_Vessel_Manifest`）
  4. 庫存扣減：對每個 inventory row 用 `WorkOrder/SaveOutboundAssignedItem`（既有 §7.4.3）做 allocate + ship

BOL 寫入欄位：`ShipmentMode / TrackingNumber / PODUrl / ServiceLines`。

Response：`ManifestUIDs[] / VesselUID / BolUID / ShipOrderNo / AllocationDetail[]` —
`AllocationDetail[]: { ItemUID, SKU, RequestedQty, Allocations[]: { SourceManifestUID, ConNo, AgingDays, QtyAllocated } }` 讓前端可顯示拆單結果（用於審核 / debug）。

#### 7.4.2 BOL API（`WMS_BOL` 加 3 欄）

寫入 `WMS_BOL.TrackingNumber / ShipmentMode / PODUrl`。

#### 7.4.3 `POST /api/WorkOrder/SaveOutboundAssignedItem` （既有，line 176）

維持既有用法。Allocated by-sku 模式拆單後對每行呼叫。

#### 7.4.4 `GET /api/Inventory/GetItemAvailability?customerUID=&itemUID=` （**v1.1 新增，By-SKU 預檢用**）

回該 customer 該 SKU 在所有 container 的 on-hand 總計 + 分櫃明細：
```
{
  ItemUID, SKU, ItemName,
  TotalOnhand, TotalCartons, TotalPallets,
  Allocations[]: {
    ManifestUID, ConNo, WarehouseName, LoadingType,
    Onhand, Cartons, Pallets, ArrivalDateActual, AgingDays
  }   // 依 ArrivalDateActual ASC 排序（即 FIFO 順序）
}
```
SPA Outbound 表單（By SKU 模式）即時呼叫此 API 顯示「總可出量 250 件 = 老櫃 80 + 中櫃 70 + 新櫃 100」，讓客服跟客戶確認。

### 7.5 WMS Transload API（新增 Controller，只 1 個端點）

新檔：`YAEP.WMS.API\Controllers\TransloadController.cs`

#### 7.5.1 `GET /api/Transload/BillableEvents`

給 T2 server-to-server。WMS **不算錢**，只提供作業事實。

Query：`customerUID?` / `from` / `to` / `warehouseUID?` / `includeInvoiced=false`

Response：

```
Containers[]: { ManifestUID, ConNo, CustomerUID, WarehouseUID, LoadingType, ContainerType,
                ArrivalDateActual, TotalPallets, TotalCartons, TotalQty,
                OnHandPallets, OnHandCartons, OnHandQty, AgingDays }
Outbounds[]:  { VesselUID, ManifestUID, SourceManifestUID, ShipOrderNo, ConNo, CustomerUID,
                ShipDate, Mode, ShippedPallets, ShippedCartons, ShippedQty,
                ServiceLines[] (Name, Qty, Unit, UnitPrice, Amount) }
```

> ShipmentRequest API **不在 WMS**，改開在 T2（§8.7）。

---

## 8. T2 API 規格

專案：`HeptarunWebAPI`。回傳 `ResultModel<T>`。新增 Controller：`Controllers\TransloadBillingController.cs`。

### 8.1 Me（登入後反查客戶身分）

#### `GET /api/transload/me`

Header 帶 JWT。回應：

```
{
  UID, Account, UserType,
  Client_UID?,        // 若 UserType=External_Customer，從 S_Client.Account_ID 反查
  Client_ID?, Client_Name?
}
```

### 8.2 Quote API（v1.2：寫 `S_Rate_Extra_Contract` 表頭 + `S_Quote_Item` 明細）

#### 8.2.1 `GET /api/transload/quotes?customerUID=&status=&effectiveDate=`

回 `S_Rate_Extra_Contract` 表頭清單（**filter `Apply_Type=99` 或 `Pricing_Type=99` 識別 transload quote**，避免列出 trucking PC contract）。

#### 8.2.2 `GET /api/transload/quotes/{uid}`

回表頭 + 從 `S_Quote_Item` 撈該 `Quote_Header_UID = {uid}` 的全部 line items。

#### 8.2.3 `GET /api/transload/quotes/active?customerUID=&operationDate=`

回 active quote（`Client_UID` 符合 + `StartDate <= operationDate <= EndDate`）。計費用。

#### 8.2.4 `POST /api/transload/quotes`

Request：
```
{
  ID,                           // quote 業務代碼
  Name,                         // 寫到 S_Rate_Extra_Contract.Description
  Client_UID,
  Effective_Date, Expire_Date?,
  Currency,
  Status,
  Items[]: { Service_UID, Price, Display_Group, Loading_Type, Calculation_Basis, Sort_Order, Description? }
}
```

寫入：
- 表頭 → `S_Rate_Extra_Contract`：`Client_UID`、`StartDate=Effective_Date`、`EndDate=Expire_Date`、`Apply_Type=99` (transload)、`Pricing_Type=99` (transload quote)、`Description=Name`
- 每個 Item → `S_Quote_Item`：`Quote_Header_UID = S_Rate_Extra_Contract.UID`、`Quote_Header_Type='S_Rate_Extra_Contract'`、`Service_UID`、`Price`、`Display_Group`、`Loading_Type`、`Calculation_Basis`、`Currency`

Response：`{ Quote_UID (= S_Rate_Extra_Contract.UID), Item_UIDs[] }`

#### 8.2.5 `PUT /api/transload/quotes/{uid}`

更新表頭 + 同步 items（delete missing / insert new / update existing）。

#### 8.2.6 `POST /api/transload/quotes/{uid}/deactivate`

`S_Rate_Extra_Contract.EndDate = today` 或加 `Status=0`（看既有 Extra_Contract 是用哪個欄位表示停用）。連動 `S_Quote_Item.Status=0`。

### 8.3 Service Item / Contract Price 查詢 API

#### 8.3.1 `GET /api/transload/service-items?keyword=&chargeType=&status=`

查既有 `P_Item_Service`。回：`UID / ID / Name / Charge_Type / P_Class_UID / Charge_FSC / Status`。

#### 8.3.2 `GET /api/transload/contract-prices?customerUID=&serviceUID=&operationDate=&containerType=&containerSize=&rateType=`

查 PC 售價（`S_Rate_Service` / `S_Rate_Extra_Contract` / `S_Rate_Extra_Item`）與 PV 成本（`B_Cost_Service` / `B_Cost_Extra_Contract` / `B_Cost_Extra_Item`）。

回：
```
{ Service_UID, PC_Rate_UID?, PC_Rate_Source, Price?, 
                PV_Cost_UID?, PV_Cost_Source, Cost?, Currency }
```

### 8.4 Customer API（**單純讀 T2，WMS 端 Party 由 ID 字串對應**）

#### 8.4.1 `GET /api/transload/customers`

回既有 S_Client 清單。`WMS_Party_UID` 由 T2 端反查 WMS `YAEP_Party.ID = S_Client.ID` 取得（若 WMS 無對應 row，由 sync 機制自動補建）。

```
{ UID(=S_Client.UID), ID, Name, Account_ID, WMS_Party_UID?,
  Payment_Term_ID, Free_Days, Status, Email, Phone, ... }
```

> v1.0 / v1.0.5 曾規劃 `link-wms-party` 端點與 `T2ClientUID` 欄位 — **v1.1 取消**。WMS Party 由 ID 同字串自動對齊。如需 migration：對既有 S_Client 一次性 upsert 進 WMS YAEP_Party（`YAEP_Party.ID = S_Client.ID`）。

### 8.5 Billing Event API（**直接讀寫既有 leg journal，不另存中介表**）

#### 8.5.1 `POST /api/transload/billing-events/sync`

Request：`Client_UID / From / To / Warehouse_UID? / Create_T2_Order / Recalculate_Existing`（WMS Party 由 T2 端用 `S_Client.ID` 反查，不需 SPA 傳）

動作：
1. 呼叫 WMS `GET /api/Transload/BillableEvents`。
2. **v1.3：SO 通常已存在**（Receiving/Outbound 儲存時已透過 `orders/inbound` 或 `orders/outbound` 即建）。若 SPA 雙寫失敗導致缺 SO，呼叫 `orders/ensure` (§8.6.3) fallback 補建。
3. `S_Order_Container` 通常也已存在（同上）；缺則同步補。
4. 依 `S_Quote_Item.Price`（v1.2，§11.3.2）/ `S_Contract_Demurrage` 算四類費用 → insert `S_Order_Container_Leg_Journal`：
   ```
   SO_UID, Container_UID, Service_UID, Service_Date,
   Qty, Rate, Amount, Remain_Amount=Amount, Remain_Qty=Qty,
   Bill_To_UID=Client_UID, Currency, Status=1
   ```
5. Container_Type int → uniqueidentifier：T2 端有 container size/type 主檔 UID，sync 時透過對照表轉換（mapping 寫在 `TransloadBillingContext`）。

Response：`SO_UID / Created_Order / Created_Event_Count / Updated_Event_Count / Skipped_Event_Count / Total_Amount / Leg_Journal_UIDs`

#### 8.5.2 `GET /api/transload/billing-events?customerUID=&from=&to=&billingStatus=&invoiceUID=&containerNo=`

查 `S_Order_Container_Leg_Journal` join container join order。`Billing_Status` 由 `Remain_Amount` 判定（>0=未開票；=0=已開）。

#### 8.5.3 `POST /api/transload/billing-events/confirm`

Request：`Leg_Journal_UIDs[] / SO_UID / Confirm_By`

更新 leg journal `Status`（或自定 confirmed flag）。Response：`Confirmed_Count`。

### 8.6 Order / Invoice API（v1.3：T2 SO 即建）

#### 8.6.1 `POST /api/transload/orders/inbound` ⭐ v1.3 新增

**用途**：SPA Receiving 儲存時呼叫，在 T2 即時建立 inbound SO + container。SPA 雙寫流程：先 WMS `POST /api/Order/Receiving` → 成功取得 ManifestUID → 接呼此 endpoint 建 T2 SO。

Request：
```
{
  Client_UID,                      // T2 S_Client.UID
  ConNo,                           // = WMS_Manifest.RefNo (SPA 從 Receiving 回應取)
  SealNo?,
  ContainerType (int),             // §11.2 enum，後端轉成 S_Order_Container.Type UID
  LoadingType (int),               // §11.1 enum → S_Order_Container.Unload_Type
  ArrivalDate,                     // DateTime → S_Order.Order_Date + S_Order_Container.Ingate_Date
  Warehouse_Reference?,            // 文字描述
  Description?
}
```

動作：
1. 建立 `S_Order`：Type=transload_inbound, Customer_UID=Client_UID, Order_Date=ArrivalDate, Ref_No=ConNo, Status=1 (Active)
2. 建立 `S_Order_Container`：SO_UID, ID=ConNo, IsTransloader=1, Unload_Type=LoadingType, Seal_No=SealNo, Type=container_type_uid, Ingate_Date=ArrivalDate
3. 寫 charge 預留（Receiving + Transload 一次性費用可此時 sync 進 leg_journal，視策略）

Response：
```
{ SO_UID, SO_ID, Container_UID }
```

> **冪等**：若已存在 `S_Order.Ref_No = ConNo && Type = transload_inbound && Customer_UID = Client_UID` 的 row，直接 return 既有 UID。

#### 8.6.2 `POST /api/transload/orders/outbound` ⭐ v1.3 新增

**用途**：SPA Outbound 儲存時呼叫，在 T2 即時建立 outbound SO + container，並連結 source inbound SO。

Request：
```
{
  Client_UID,
  Inbound_SO_UID,                  // 從 SPA Outbound 表單「選 source container」帶來
  ConNo,                           // source container ConNo（顯示用）
  ShipOrderNo,                     // = WMS_Vessel.RefNo
  ShipDate,                        // DateTime
  Carrier?,
  TrackingNumber?,
  ShipMethodUID?,                  // WMS_ShipMethod UID（LTL/FTL，§10.4）
  ShipTo?,                         // 完整地址 string (顯示用)
  Description?
}
```

動作：
1. 建立 `S_Order`：Type=transload_outbound, Customer_UID=Client_UID, Order_Date=ShipDate, Ref_No=ShipOrderNo, **Refer_To_Order=Inbound_SO_UID**, Status=1
2. 建立 `S_Order_Container`：SO_UID, ID=`{ConNo}-OUT-{ShipOrderNo}` (避免撞 key), IsTransloader=1, **Refer_To_Container=<inbound S_Order_Container UID>**, Outgate_Date=ShipDate
3. Outbound charge（service lines）可此時 sync 進 leg_journal，或延後在 Billing 頁 sync

Response：
```
{ SO_UID, SO_ID, Container_UID }
```

> **冪等**：若已存在 `S_Order.Ref_No = ShipOrderNo && Type = transload_outbound` 的 row，直接 return 既有。

#### 8.6.3 `POST /api/transload/orders/ensure` （**v1.3 後退為 fallback**）

Request：`Client_UID / ConNo / Type ('inbound' | 'outbound') / Reference_No?`

**用途**：Phase 1 Billing sync 時的 fallback — 若 SPA 雙寫失敗導致 T2 沒對應 SO，sync 時補建。正常流程不會用到（SPA 雙寫成功的話 SO 已存在，sync 直接取）。

Response：`SO_UID / SO_ID / Created (bool)`

#### 8.6.4 `POST /api/transload/invoices/generate`

Request：`SO_UID / Bill_To_UID / Bill_To_Type / Leg_Journal_UIDs[] / Invoice_Date? / Send_Email`

動作：呼叫既有 `InvoiceContext.CreateInvoice(...)`：
```csharp
CreateInvoice(InvoiceLineItemModelCollection, SO_UID, Bill_To_UID,
              Bill_To_Type, InvoiceType.General, InvoiceDate, false, null, CreateBy)
```
（內部 `UpdateInvoicedRemainAmout` 自動把 leg journal `Remain_Amount` 歸 0）

如 `Send_Email=true` → `InvoiceGenerator.Action(...)` 出 RDLC PDF + `PostOffice` 寄信。

Response：`Invoice_UID / Invoice_ID / Amount / Overdue_Date / Email_Sent`

> v1.3 顆粒度：**1 SO = 1 invoice**。Inbound SO 的 invoice 含 Receiving + Transload + Storage charges；每張 Outbound SO 的 invoice 只含 Outbound charges。

#### 8.6.5 `GET /api/transload/invoices?customerUID=&from=&to=&invoiceUID=`

查既有 `S_Invoice`。

### 8.7 Shipment Request API（**移到 T2**）

#### 8.7.1 `POST /api/transload/shipment-requests`

Request：
```
{
  Client_UID,
  Mode: 'by-container' | 'by-sku',       // v1.1 新增
  ConNo?,                                 // by-container 必填；by-sku 留空
  Requested_Date, Ship_To, Notes,
  Items[]: { Item_UID, SKU, Qty_Requested }
}
```
（用 ConNo 字串對應，不存 WMS Manifest UID）

寫既有 `S_Order`（Type=200 transload_request，見 §9.4）；申請項目 SKU 摘要寫進 `S_Order.Description`。

#### 8.7.2 `GET /api/transload/shipment-requests?customerUID=&status=`

#### 8.7.3 `POST /api/transload/shipment-requests/{uid}/approve`

核准後觸發 WMS `POST /api/Order/Allocated`（沿用 §7.4.1 的 Mode 欄位；by-sku 模式由 WMS 端做 FIFO 拆單）。

---

## 9. T2 DB 新增 / 修改（v1.2 最終版）

**T2 端只新增 1 張表 (`S_Quote_Item`) + 15 筆 `P_Item_Service` 資料列**，**T2 既表不加任何欄位**。

### 9.1 Quote header — **延用既有 `S_Rate_Extra_Contract`**（不開新表）

v1.2 拍板：報價單表頭直接用既有 `S_Rate_Extra_Contract`（既在用 **2460 筆** PC contract），不開 `S_Transload_Rate_Card`。

對應方式：
- `S_Rate_Extra_Contract.Client_UID` = T2 客戶 UID
- `S_Rate_Extra_Contract.StartDate / EndDate` = quote 生效 / 失效日
- `S_Rate_Extra_Contract.Apply_Type / Pricing_Type` = 標記為 transload 用（建議新增 enum 值，例如 `Apply_Type=99 / Pricing_Type=99` 代表 Transload Quote）
- transload 不用 `From_Type / Destination_Type` 等路線欄位 → 設 default 0 / NULL
- `Description` 寫 quote 顯示名（例 `"PACIFIC 2026 Q2 Transload Quote"`）

**理由**：trucking 與 transload 都需要「per-customer + 生效期間 + 階梯結構」的合約 header；既有結構完備，命名也已通用。

### 9.2 `S_Quote_Item` — **唯一新表**（命名通用，未來 T2 其他模塊可重用）

```sql
CREATE TABLE S_Quote_Item (
  UID                uniqueidentifier NOT NULL CONSTRAINT DF_SQI_UID DEFAULT(newid()) PRIMARY KEY,
  ID                 varchar(50)      NOT NULL,
  Name               nvarchar(200)    NULL,
  Quote_Header_UID   uniqueidentifier NOT NULL,                                          -- 通常指向 S_Rate_Extra_Contract.UID
  Quote_Header_Type  varchar(50)      NOT NULL CONSTRAINT DF_SQI_HT DEFAULT('S_Rate_Extra_Contract'),
  Service_UID        uniqueidentifier NOT NULL,                                          -- FK P_Item_Service.UID
  Display_Group      varchar(50)      NULL,                                              -- 'Storage' / 'F2F' / 'F2P' / 'P2P' / 'Outbound' / 其他
  Loading_Type       int              NULL,                                              -- 10/20/30/40，對應 §11.1 enum
  Calculation_Basis  varchar(50)      NULL,                                              -- 'per_pallet' / 'per_container' / 'per_carton' / 'per_piece' / 'per_day' / 'fixed'
  Price              decimal(10,4)    NOT NULL,
  Currency           varchar(10)      NOT NULL CONSTRAINT DF_SQI_CUR DEFAULT('USD'),
  Sort_Order         int              NULL,
  Description        nvarchar(1000)   NULL,
  Status             int              NOT NULL CONSTRAINT DF_SQI_ST DEFAULT(1),
  CreatedBy nvarchar(50) NULL, CreatedOn datetime NULL,
  ModifiedBy nvarchar(50) NULL, ModifiedOn datetime NULL
);
CREATE INDEX IX_SQI_Header ON S_Quote_Item(Quote_Header_UID);
CREATE INDEX IX_SQI_Service ON S_Quote_Item(Service_UID);
```

設計要點：
- **polymorphic header**：`Quote_Header_UID + Quote_Header_Type` 可指向任何 quote header 表（v1.2 用 `S_Rate_Extra_Contract`；未來其他模塊可用其他 header 表）
- transload 的 19 個費率（freeDays / storagePerPallet / f2fUnload / f2pPalletize / …） → **一個 service 對一筆 row**
- `Display_Group + Loading_Type + Calculation_Basis` 三欄組合，讓 UI 自動分組顯示 + sync 時知道用什麼基數算
- 計費時：**charge 寫入時 Rate 來源 = `S_Quote_Item.Price`**（不寫 `S_Rate_Service`，避免兩套合約價系統混淆）

### 9.3 T2 既表 — **不加任何欄位**（v1.1+v1.2 持續拍板）

對應方式參考：
- WMS↔T2 客戶：`YAEP_Party.ID = S_Client.ID` 字串對應
- WMS↔T2 貨櫃：`WMS_Manifest.RefNo = S_Order_Container.ID = ConNo` 字串對應
- `S_Order_Container.Seal_No / Unload_Type / Ingate_Date / Outgate_Date / Due_Date / Size / Size_UID / Rate / Remain_Rate / IsTransloader` 全是既有欄位
- Container Type int → UID 對照寫在 `TransloadBillingContext.cs` hard-code（§11.2）

### 9.4 Shipment Request — **延用 `S_Order`**（不開新表）

v1.2 拍板：Customer Portal 出貨申請寫進既有 `S_Order` 並用 `S_Order.Type` 區分。

- `S_Order.Type` 新增枚舉值：`200 = transload_request`（既有 type enum 加項，不改 schema）
- `S_Order.Customer_UID` = 申請客戶
- `S_Order.Order_Date` = 申請日
- `S_Order.Ref_No` = 可放 source container ConNo（如 by-container 模式）
- `S_Order.PO_No` = 留空或客戶 PO
- `S_Order.Status` = 1 Pending → 2 Processing → 4 Done（或 9 Rejected）
- 申請項目（SKU + qty）暫用 `S_Order.Description` 文字摘要（如 `"FURN-001×100, FURN-002×50; Ship to Dallas"`）
- 核准 → 改 Type=normal SO + 觸發 WMS Allocated

> 原 v1.0 / v1.1 規劃 `S_Transload_Shipment_Request + _Item` 兩張表 → **v1.2 取消**。
> 申請項目 SKU/Qty 詳細結構若需要結構化儲存，Phase 3 實作時再決定（可考慮加 `S_Order_Request_Line` 子表，但目前用 Description 文字即可）。

### 9.4 既表延用清單（不新建鏡像）

| 用途 | 既有表 / Entity | Context |
|---|---|---|
| 收費項目主檔 | `P_Item_Service`（補 transload 服務列即可） | `Heptarun\P\Item\ItemServiceContext.cs` |
| 客戶售價（PC） | `S_Rate_Service` | `Heptarun\S\Rate\RateServiceContext.cs` |
| PC 階梯/Extra | `S_Rate_Extra_Contract` + `S_Rate_Extra_Item`（Duration_Start/End/Days_Type） | `Heptarun\S\Rate\RateExtraContractContext.cs` + `RateExtraItemContext.cs` |
| 成本（PV） | `B_Cost_Service` / `B_Cost_Extra_Contract` / `B_Cost_Extra_Item` | `Heptarun\B\Cost\CostServiceContext.cs` 等 |
| Storage 免費天數 | `S_Contract_Demurrage.Free_Days`（per Client/SSL/State） | |
| 付款條件 | `S_Payment_Term` | |
| 客戶 | `S_Client`（Account_ID = Core_User.Account 對應 ext customer） | |
| 訂單 | `S_Order` | |
| 訂單貨櫃 | `S_Order_Container`（IsTransloader/Unload_Type/Seal_No/Size/Ingate_Date/Rate/Remain_Rate 都已有） | |
| Charge journal | `S_Order_Container_Leg_Journal` | `OrderContainerLegJournalContext.cs` |
| 開立發票 | `S_Invoice` + `S_Invoice_Line_Item` via `InvoiceContext.CreateInvoice` | `Heptarun\S\Invoice\_context\InvoiceContext.cs` |
| 出單 PDF + 寄信 | `InvoiceGenerator.Action` | `Heptarun\S\Invoice\_context\InvoiceGenerator.cs` |

### 9.5 P_Item_Service 資料補列（不建表，建資料）

於 `P_Item_Service` insert 各 transload service（Service_UID 由插入時產生）：

| 建議 ID / Code | Name | Charge_Type |
|---|---|---|
| `F2F_UNLOAD` | F2F Floor Unloading | 10 Receiving |
| `F2F_SORT` | F2F Sorting | 20 Transload |
| `F2F_HANDLING` | F2F Handling | 20 Transload |
| `F2F_RELOAD` | F2F Reloading | 20 Transload |
| `F2P_UNLOAD` | F2P Floor Unloading | 10 Receiving |
| `F2P_PALLETIZE` | F2P Palletizing | 20 Transload |
| `F2P_PALLET` | F2P Pallet | 20 Transload |
| `F2P_WRAP` | F2P Wrapping | 20 Transload |
| `F2P_LABEL` | F2P Labeling | 20 Transload |
| `P2P_FORKLIFT` | P2P Forklift Unloading | 10 Receiving |
| `P2P_STORAGE` | P2P Storage | 30 Storage |
| `P2P_LOADING` | P2P Loading | 40 Outbound |
| `STORAGE_PER_PALLET_DAY` | Storage per Pallet Day | 30 Storage |
| `HANDLING_FEE` | Handling Fee | 40 Outbound |
| `SHIPPING_FEE` | Shipping Fee | 40 Outbound |

---

## 10. WMS DB 新增 / 修改（v1.2 最終版）

**WMS 不開任何新表。** 只在既有表加 **6 個欄位** + `WMS_ShipMethod` 補 2 筆資料（WMS 命名 PascalCase 無底線）。

v1.1 原規劃 11 個欄位，v1.2 精簡到 6 個 + 既表資料：

| 欄位 | v1.1 規劃 | v1.2 處理 | 理由 |
|---|---|---|---|
| `WMS_Manifest.SealNo / ContainerType / LoadingType` | 加 | ✅ 加 | 沒有既有欄位可放 |
| `WMS_Manifest.StackableType` | 加 | ❌ **移到 `WMS_Vessel.StackableType`** | 疊放是出倉那層屬性 |
| `WMS_Manifest.ArrivalDateActual` | 加 | ❌ **不加，取 `MIN(WMS_PayLoad.ReceivedDate)`** | 走完整 receiving，PayLoad 已有日期 |
| `WMS_Manifest.TotalPallets / TotalCartons` | 加 | ❌ **不加，動態 SUM** | 透過 `WMS_Manifest_Item_List` join `YAEP_Package` 階層算 |
| `WMS_BOL.TrackingNumber / PODUrl` | 加 | ✅ 加 | 沒有既有欄位可放 |
| `WMS_BOL.ShipmentMode` | 加 | ❌ **用既有 `WMS_ShipMethod`** | LTL/FTL 是 ship method 本質 |
| `WMS_Inventory.SourceManifestItemUID` | 加 | ❌ **不加，走完整 receiving 靠 `WMS_PayLoad` 反查** | PayLoad.SlotUID + ItemUID 可推 Manifest |
| `WMS_Vessel.StackableType` | — | ✅ **新增**（從 Manifest 搬來） | 出倉那層屬性 |

### 10.1 `WMS_Manifest` 加 3 欄

```sql
ALTER TABLE WMS_Manifest ADD
  SealNo        NVARCHAR(100) NULL,
  ContainerType INT           NULL,    -- §11.2 enum
  LoadingType   INT           NULL;    -- §11.1 enum
```

### 10.2 `WMS_Vessel` 加 1 欄

```sql
ALTER TABLE WMS_Vessel ADD
  StackableType INT NULL;   -- 0 Stackable / 1 Non-Stackable，出倉時的疊放屬性
```

### 10.3 `WMS_BOL` 加 2 欄

```sql
ALTER TABLE WMS_BOL ADD
  TrackingNumber NVARCHAR(100) NULL,
  PODUrl         NVARCHAR(500) NULL;
-- ShipmentMode 不加；改用 WMS_BOL.ShipMethodUID 指向 §10.4 的 LTL/FTL row
```

### 10.4 `WMS_ShipMethod` 補 2 筆資料（**取代 ShipmentMode 欄位**）

```sql
-- system-wide LTL/FTL（PartyUID = NULL 或預設 system party）
INSERT INTO WMS_ShipMethod (UID, PartyUID, Type, MethodName, MethodValue, IsSignature, Status, CreatedBy, CreatedOn)
VALUES
  (newid(), <system_party_uid_or_null>, 0, 'LTL', 'LTL', 0, 1, 'system', getutcdate()),
  (newid(), <system_party_uid_or_null>, 0, 'FTL', 'FTL', 0, 1, 'system', getutcdate());
```

> `WMS_ShipMethod.PartyUID` 既有為 NOT NULL — 需檢查能否傳 NULL，否則建一個 system party 作為 holder（或 sentinel UID `00000000-0000-0000-0000-000000000000`）。**Phase 0 實作時定案**。

### 10.5 `WMS_Inventory` — **不加欄位**（v1.2 取消）

原 v1.1 規劃加 `SourceManifestItemUID` → **v1.2 取消**。改用 Phase 0 走完整 receiving 流程：

```
Receiving 流程：
  WMS_Manifest (含 SKU 申報)
  → 建 WMS_WorkOrder (上架單)
  → 建 WMS_Ticket (作業單)
  → 建 WMS_PayLoad (per SKU per slot: SlotUID + ItemUID + Quantity + ReceivedDate)
  → 建 WMS_Inventory (per slot: SlotUID + ItemUID + Qty)
```

per-container 在庫反查路徑：
```sql
-- 某 container (manifest) 目前在庫 SKU + 數量：
SELECT i.ItemUID, SUM(i.Qty) AS OnHand
FROM WMS_Inventory i
JOIN WMS_PayLoad p ON p.SlotUID = i.SlotUID AND p.ItemUID = i.ItemUID
JOIN WMS_WorkOrder_Payload wp ON wp.PayLoadUID = p.UID
JOIN WMS_WorkOrder wo ON wo.UID = wp.WorkOrderUID
JOIN WMS_Manifest m ON m.UID = wo.ManifestUID   -- 假設 WorkOrder 有 ManifestUID
WHERE m.RefNo = @ConNo
GROUP BY i.ItemUID
```

> 實際 join 路徑要在 Phase 0 開發時確認（依 WMS_WorkOrder schema）；可包成 view `vw_TransloadInventoryByContainer`。

### 10.6 `WMS_Manifest` TotalPallets / TotalCartons — **動態算，不存**

不存 manifest 級的 TotalPallets / TotalCartons。查詢時透過 `WMS_Manifest_Item_List` join `YAEP_Package` 階層算：

```sql
-- 某 manifest 的總板數 / 箱數（依 YAEP_Package 階層判定哪層是 pallet / carton）
SELECT
  m.UID, m.RefNo,
  SUM(CASE WHEN pkg.Type = <pallet_type_value> THEN mil.PackageQty ELSE 0 END) AS TotalPallets,
  SUM(CASE WHEN pkg.Type = <carton_type_value> THEN mil.PackageQty ELSE 0 END) AS TotalCartons,
  SUM(mil.PackageQty * pkg.Quantity) AS TotalPieces  -- 依各 SKU PackageQty × YAEP_Package.Quantity 算總件數
FROM WMS_Manifest m
JOIN WMS_Manifest_Item_List mil ON mil.ManifestUID = m.UID
JOIN YAEP_Package pkg ON pkg.UID = mil.PackageUID
WHERE m.UID = @manifestUID
GROUP BY m.UID, m.RefNo
```

> YAEP_Package 階層的 `Type` 值代表 pallet/carton/piece 哪層 — Phase 0 開發時查 WMS 既有資料確認 enum value，可寫成 view `vw_TransloadManifestSummary`。

### 10.7 `WMS_Manifest` ArrivalDateActual — **聚合 PayLoad，不存**

不存 manifest 級的 ArrivalDateActual。查詢時：

```sql
-- 某 manifest 實際到貨日（取最早一筆 PayLoad 收貨日）
SELECT m.UID, m.RefNo, MIN(p.ReceivedDate) AS ArrivalDateActual
FROM WMS_Manifest m
JOIN WMS_PayLoad p ON p.UID IN (
  -- 該 manifest 下所有 PayLoad（依 WorkOrder/Ticket join）
  SELECT wp.PayLoadUID FROM WMS_WorkOrder_Payload wp
  JOIN WMS_WorkOrder wo ON wo.UID = wp.WorkOrderUID
  WHERE wo.ManifestUID = @manifestUID
)
GROUP BY m.UID, m.RefNo
```

Aging：`DATEDIFF(day, ArrivalDateActual, GETUTCDATE())`。

> Phase 0 收貨流程要確保 PayLoad 真的有寫 ReceivedDate（既有欄位但要看 WMS 既有 receiving 邏輯）。如果 ReceivedDate 一律是 NULL → fallback 用 `WMS_Manifest.CreatedOn`。

### 10.8 `YAEP_Party` — **不加欄位**，用相同 ID 字串對應

**v1.1 最終定案**：WMS 不為「客戶映射」加任何欄位。WMS `YAEP_Party.ID`（nvarchar(50)）**直接寫入跟 T2 `S_Client.ID` 完全相同的字串**（例：`Client-5465`、`0079CA`、`0204TX`），兩邊靠這個業務代碼字串對應。

```text
T2 S_Client                          WMS YAEP_Party
─────────────────                    ───────────────────
UID         (Guid)                   UID         (Guid)        ← 兩邊各自獨立
ID          "Client-5465"   ───┐ ↔ ┌─ID         "Client-5465"  ← 同字串，這是對應 key
Account_ID  "Client-5465"      │     Name       (同步)
Name        "ALUMAFOLD..."     │     ...
```

> v1.0 將映射放 T2（`S_Transload_Customer_Map` 表）；v1.0.5 改放 WMS 加欄位 `T2ClientUID`；**v1.1 最終取消加欄位**，用同 ID 字串對應 — 使用者拍板「WMS 配合 T2，建一樣的 id 就好」。

**含意**：
- WMS 端不主動維護客戶主檔；新客戶從 T2 同步進來時，**用 T2 的 `S_Client.ID` 當 `YAEP_Party.ID`** 建一筆 YAEP_Party
- 查 `WMS_Party_UID`：給 T2 `S_Client.ID` → `SELECT UID FROM YAEP_Party WHERE ID = @clientID`（不存在則建立）
- 查 T2 `Client_UID`：給 WMS `YAEP_Party.ID` → `SELECT UID FROM S_Client WHERE ID = @partyID`
- 兩個 UID 都還是各自 DB 的 PK，**只透過 ID 字串 join**，不互存彼此的 UID
- migration：一次性把 T2 既有 S_Client 的 `ID + Name + 基本聯絡資訊` upsert 進 WMS `YAEP_Party`（給定相同 ID），之後新增/改名走同步機制（待定 — 手動 / 排程 / 即時）

---

## 11. 計費規則

### 11.1 Loading Type（int enum，存 `WMS_Manifest.LoadingType`，sync 時對到 `S_Order_Container.Unload_Type`）

| 值 | 名稱 |
|---|---|
| 10 | F2F - Floor to Floor |
| 20 | F2P - Floor to Pallet |
| 30 | P2P - Pallet to Pallet |
| 40 | Full Container Storage |

### 11.2 Container Type（int enum，存 `WMS_Manifest.ContainerType`；T2 sync 時透過對照表轉成 `S_Order_Container.Type` 的 uniqueidentifier）

| 值 | 名稱 |
|---|---|
| 10 | 20GP |
| 20 | 40GP |
| 30 | 40HQ |
| 40 | 45HQ |
| 50 | Loose Cargo |
| 60 | LCL |
| 90 | Other |

> 對照表寫在 `Heptarun\S\Transload\_context\TransloadBillingContext.cs`（hard-code 或讀 T2 既有 container type 主檔）。

### 11.3 Receiving / Transload / Storage / Outbound 計費

每筆 charge insert 一列 `S_Order_Container_Leg_Journal`：
- `Service_UID` = 上述 §9.6 對應的 `P_Item_Service.UID`
- `Rate` = **`S_Quote_Item.Price`**（v1.2：依 active quote 的 `Quote_Header_UID + Service_UID` 反查；**不再讀 `S_Rate_Service`**）
- `Qty` = 板/箱/件/天
- `Amount` = Qty × Rate
- `Remain_Amount = Amount, Remain_Qty = Qty`
- `Customer_Note` = 自動補 SKU 摘要（例：`FURN-001×100, FURN-002×50`）— **v1.2 新增**，給 invoice line description 用

#### 11.3.1 完整 if/else 演算法（從原型 `TransloadWMS (6) 1.html` 計費引擎抄出）

`sync-charges` 對每個 container 依 `Unload_Type` 走以下分支；每個 charge type **每 container 只開一次**（用 `Remain_Amount > 0` 判定）。

```
let lt = container.LoadingType              // 10 F2F / 20 F2P / 30 P2P / 40 Full
let pallets = totalPalletsByManifest(container.UID)   // §10.6 動態 SUM via Manifest_Item_List join YAEP_Package
let arrival = arrivalDateActual(container.UID)        // §10.7 MIN(WMS_PayLoad.ReceivedDate)
let rate = activeQuoteItems(container.Client_UID, arrival)  // dict by Service_ID → S_Quote_Item.Price

// === 1. RECEIVING（unloading）===
if (!isCharged(container, "Receiving")) {
  if (lt == F2F && rate.F2F_UNLOAD    > 0) charge("F2F Floor Unloading",     qty=1,       rate.F2F_UNLOAD)
  if (lt == F2P && rate.F2P_UNLOAD    > 0) charge("F2P Floor Unloading",     qty=1,       rate.F2P_UNLOAD)
  if (lt == P2P && rate.P2P_FORKLIFT  > 0) charge("P2P Forklift Unloading",  qty=1,       rate.P2P_FORKLIFT)
}

// === 2. TRANSLOAD（per pallet work）===
if (!isCharged(container, "Transload")) {
  if (lt == F2F) {
    if (rate.F2F_SORT     > 0) charge("F2F Sorting Fee",   qty=1,       rate.F2F_SORT)
    if (rate.F2F_HANDLING > 0) charge("F2F Handling Fee",  qty=1,       rate.F2F_HANDLING)
    if (rate.F2F_RELOAD   > 0) charge("F2F Reloading Fee", qty=1,       rate.F2F_RELOAD)
  }
  else if (lt == F2P) {
    if (rate.F2P_PALLETIZE > 0) charge("F2P Palletizing Fee", qty=pallets, rate.F2P_PALLETIZE)
    if (rate.F2P_PALLET    > 0) charge("F2P Pallet Fee",       qty=pallets, rate.F2P_PALLET)
    if (rate.F2P_WRAP      > 0) charge("F2P Wrapping Fee",     qty=pallets, rate.F2P_WRAP)
    if (rate.F2P_LABEL     > 0) charge("F2P Label Fee",        qty=pallets, rate.F2P_LABEL)
  }
  else if (lt == P2P) {
    if (rate.P2P_LOADING > 0) charge("P2P Loading Fee", qty=1,       rate.P2P_LOADING)
    if (rate.P2P_STORAGE > 0) charge("P2P Storage Fee", qty=pallets, rate.P2P_STORAGE)  // 一次性處理費
  }
}

// === 3. STORAGE（demurrage，per day-range）===
if (!isCharged(container, "Storage")) {
  let ag = today - arrival                       // arrival = MIN(WMS_PayLoad.ReceivedDate)
  let freeDays = lookupFreeDays(container.Client_UID)  // §11.4：S_Contract_Demurrage 優先，備援 S_Client.Free_Days
  let billableDays = max(0, ag - freeDays)
  if (billableDays > 0) {
    let onHandPallets = onHandPalletsByManifest(container.UID)  // §10.5 join Inventory → PayLoad → WorkOrder → Manifest
    charge("Storage Fee", qty=onHandPallets * billableDays, rate.STORAGE_PER_PALLET_DAY)
  }
  // billableDays=0 → UI 顯示 "within free period (Xd of Yd free)" 副資訊（不寫 leg journal row）
}

// === 4. OUTBOUND（per outbound shipment）===
for ob in container.outbounds {
  if (!isCharged(ob, "Outbound")) {
    for line in ob.serviceLines {  // 手動 UI 輸入為主
      charge(line.name, line.qty, line.unitPrice, customerNote=skuSummary(ob))
    }
    // 自動補：可依設定加 rate.HANDLING_FEE / rate.SHIPPING_FEE
  }
}
```

**設計取捨**：
- F2P / P2P 的 `pallets` 用 §10.6 **動態算的板數**（依 Manifest_Item_List join YAEP_Package 階層 SUM）— 客戶 SKU 拆解後算出
- **P2P_STORAGE** 是 transload **一次性處理費**（× pallets），跟 §3 真正 storage demurrage（per day × on-hand pallets）**完全不同概念**。兩者不要搞混
- 計費單位選擇（per pallet / container / carton / piece / day）：v1.2 預設 `STORAGE_PER_PALLET_DAY`；其他三個（per container/carton/piece）保留 `S_Quote_Item.Calculation_Basis` 但暫不實作多單位切換

#### 11.3.2 Rate 來源（v1.2 拍板）

- **Rate 來源 = `S_Quote_Item.Price`**（不再讀 `S_Rate_Service`）
- 查法：sync 時取 `S_Rate_Extra_Contract`（active quote header）→ `S_Quote_Item.Quote_Header_UID = header.UID + Service_UID = (從 P_Item_Service 取的 transload service UID)` → 取 `Price`
- 寫入 `S_Order_Container_Leg_Journal.Rate + Original_Rate` 同步寫入此 Price → **charge 一旦寫入即凍結**，後續 quote 改不影響此 charge
- 稽核需求：可透過 `Quote_Header_UID`（= `S_Rate_Extra_Contract.UID`）反查當時 quote header；明細細項可由 `Service_UID` 反查
- 原型 `linkedRateCard` 19 欄 snapshot 概念被 `S_Quote_Item` 取代（quote items 本身就是 snapshot 結構，每張 quote 互相獨立）

#### 11.3.3 Invoice line description 自動帶 SKU 摘要

開 Invoice 時（§8.6.2），SPA / `TransloadBillingContext` 將 charge 的 SKU 摘要寫進 `S_Invoice_Line_Item.Description`：

```
"F2P Palletizing Fee - Container CRXU4428037 (FURN-001×100, FURN-002×50)"
```

SKU 摘要由 `S_Order_Container_Leg_Journal.Customer_Note`（sync 時寫入）或現場 join WMS `WMS_Manifest_Item_List` 取得。客戶 invoice 上看得到 SKU；T2 schema 不存獨立 SKU 欄位（**SKU 是 WMS 的資料**）。

詳細 SKU 出貨記錄 → WMS 出的 **BOL / Packing List** PDF（見 §12）。

### 11.4 Storage 計費（含階梯與免費天數）

#### Free_Days 來源優先順序（v1.1 確定）

1. **`S_Contract_Demurrage.Free_Days`**（per Client_UID / SSL_UID / State）— 第一優先
2. `S_Client.Free_Days`（per 客戶整體）— 備援
3. 預設 0

#### 階梯計費（用既有 `S_Rate_Extra_Contract / Item`）

`S_Rate_Extra_Item.Duration_Start / Duration_End / Days_Type` 表達階梯：
- Tier A：`Duration_Start=0, Duration_End=7, Price=0`（免費）
- Tier B：`Duration_Start=8, Duration_End=14, Price=2.50/pallet/day`
- Tier C：`Duration_Start=15, Duration_End=NULL, Price=5.00/pallet/day`

計算：
```
BillableDays = max(0, AgingDays - Free_Days)
依 BillableDays 落到哪個 tier 取 Price
Amount = OnHandPallets × DaysInTier × Price  (各 tier 加總)
```

### 11.5 Outbound 計費

來源：
- 出貨表單手動 `ServiceLines[]`（UI 輸入 Name/Qty/Unit/UnitPrice/Amount）
- 或從 `P_Item_Service` 對應 service（`HANDLING_FEE / SHIPPING_FEE / P2P_LOADING`）× `S_Rate_Service` 取單價自動算

---

## 12. T2 Order / Invoice 流程 + 出貨單分工

### 12.1 BOL / Invoice 分工（v1.2 拍板）

**兩份不同文件，由不同系統出**：

| 文件 | 由誰出 | 內容 | 收件人 |
|---|---|---|---|
| **BOL / Packing List** | **WMS** | Ship From/Ship To 地址 + Carrier + Tracking + POD + **SKU + Qty + Weight** | 卡車司機（簽收）+ 收貨方 |
| **Invoice** | **T2** | Customer + Bill To + Charge lines（per container）+ Amount + Payment Term | 客戶會計（付款）|

**理由**：
- WMS 才有 SKU 與倉內出貨事實（`WMS_Vessel_Manifest` 含 ItemUID/Qty/Volume/Weight；`WMS_BOL` 含完整 ShipTo/ShipFrom 地址）
- T2 才有 AR / Payment Term / Bill_To / Invoice 流程
- **T2 端 schema 完全不存 SKU**（因為不需要 — SKU 看 BOL）
- Invoice line description 若需 SKU 摘要，由 sync 時自動補（§11.3.3）

**SPA Outbound 流程**：
1. SPA 從 T2 拉客戶 default ship-to（用 `S_Client.UID + S_Order_Container.Consignee_UID/Consignee_Ship_To_Address_UID` 預設）prefill
2. 員工可手動修改 ship-to（出特定門市/倉）
3. SPA POST WMS `/api/Order/Allocated` + `/api/Bol/Create`
   - WMS_BOL 寫入地址 snapshot（既有 ShipToZip/Address/City/State/Country 欄位）+ TrackingNumber + PODUrl
   - WMS_Vessel_Manifest 寫 SKU + Qty
4. BOL/Packing List PDF 由 WMS 端產（既有或新增 endpoint）

### 12.2 訂單建立顆粒度（v1.3 定案，原 §17.4 待定已解）

**1 receiving = 1 inbound SO；1 outbound = 1 outbound SO**

- Receiving 儲存時：T2 建 1 張 `S_Order`（type=transload_inbound）+ 1 張 `S_Order_Container`（IsTransloader=1, ID=ConNo）
- 同一 container 之後**多次** Outbound（FTL/LTL 分批）：**每次出貨各建一張新 SO**（type=transload_outbound）+ `S_Order_Container`，透過 `S_Order.Refer_To_Order` 指向**對應的 inbound SO**
- 1 inbound SO + N outbound SO 形成樹狀結構：
  ```
  Inbound SO (CRXU4428037)
    ├─ Outbound SO #1 (Ship 100 to Dallas) ── Refer_To_Order → Inbound SO
    ├─ Outbound SO #2 (Ship 200 to LA)     ── Refer_To_Order → Inbound SO
    └─ Outbound SO #3 (Ship 300 to Chicago)── Refer_To_Order → Inbound SO
  ```
- **Invoice 顆粒度**：1 SO = 1 invoice。Inbound SO 開的 invoice 含 Receiving + Transload + Storage charges；每張 Outbound SO 開的 invoice 含 Outbound charges

### 12.3 Container 對應

**Inbound SO（receiving 時建立）**：
- `S_Order.Type = transload_inbound` enum value（與既有 type 對齊；待 §17.9 確認）
- `S_Order.Customer_UID = Client_UID`
- `S_Order.Ref_No = ConNo`（方便 InvoiceSearch 查詢）
- `S_Order.Order_Date = ArrivalDateActual`
- `S_Order_Container.ID = ConNo`（**= WMS_Manifest.RefNo，自然對應**）
- `S_Order_Container.IsTransloader = 1`
- `S_Order_Container.Unload_Type = LoadingType (int)`
- `S_Order_Container.Seal_No = SealNo`
- `S_Order_Container.Type = container type UID`（對照表轉自 ContainerType int，§11.2）
- `S_Order_Container.Ingate_Date = ArrivalDateActual`（取 `MIN(WMS_PayLoad.ReceivedDate)`，§10.7；若 receiving 即建時尚未有 PayLoad → 用使用者填的 ArrivalDate）
- **不存 WMS_Manifest UID**（用 ConNo 字串對應，§9.3）

**Outbound SO（每次出貨時建立）**：
- `S_Order.Type = transload_outbound`
- `S_Order.Customer_UID = Client_UID`
- `S_Order.Refer_To_Order = <inbound SO UID>`（連結回 source inbound SO）
- `S_Order.Ref_No = ShipOrderNo`
- `S_Order.Order_Date = ShipDate`
- `S_Order_Container.ID = ConNo + '-OUT-' + ShipOrderNo`（避免跟 inbound 撞 key；或用 ShipOrderNo 作 ID）
- `S_Order_Container.Refer_To_Container = <inbound S_Order_Container UID>`
- `S_Order_Container.IsTransloader = 1`
- `S_Order_Container.Outgate_Date = ShipDate`

### 12.4 Invoice 建立

呼叫既有：

```csharp
Heptarun\S\Invoice\_context\InvoiceContext.cs

CreateInvoice(
    InvoiceLineItemModelCollection InvoiceLineItems,  // 從 Leg_Journal 轉，Description 自動帶 SKU 摘要（§11.3.3）
    Guid SO_UID,
    Guid Bill_To_UID,
    Guid Bill_To_Type,
    InvoiceType.General,
    DateTime? InvoiceDate = null,
    bool IsFullyPaid = false,
    Guid? InvoiceUID = null,
    string CreateBy = ""
)
```

InvoiceContext 內 `UpdateInvoicedRemainAmout` 自動把 leg journal `Remain_Amount/Remain_Qty/Remain_Inner_Cost` 歸 0。

PDF + 寄信：`InvoiceGenerator.Action(InvoiceGeneratorActionPara)` → RDLC + `PostOffice.Send`。Invoice PDF 可附 BOL/Packing PDF 連結（從 WMS 取）作為 supporting document。

---

## 13. 各專案新增 / 修改清單

### 13.1 Transload SPA 新平台（`E:\Project\TransloadWMS\src\`）

| 檔案 | 說明 |
|---|---|
| `src/api/t2Client.ts` | T2 API client + JWT |
| `src/api/wmsClient.ts` | WMS API client + JWT |
| `src/types/auth.ts` | Auth types |
| `src/types/transload.ts` | Transload types |
| `src/pages/Login.tsx` | 登入 |
| `src/pages/Dashboard.tsx` | 總覽 |
| `src/pages/Customers.tsx` | 客戶 |
| `src/pages/Quotes.tsx` | 報價 |
| `src/pages/Receiving.tsx` | 入倉 |
| `src/pages/Inventory.tsx` | 庫存 |
| `src/pages/Outbound.tsx` | 出倉 |
| `src/pages/Billing.tsx` | 計費 |
| `src/pages/Portal.tsx` | 客戶 Portal |

### 13.2 T2 API（`HeptarunWebAPI`）

新增：
| 檔案 | 說明 |
|---|---|
| `Controllers\TransloadBillingController.cs` | Transload 全部端點（me / quotes / customers / billing / orders / invoices / shipment-requests） |
| `Models\Transload\*.cs` | Request / Response DTOs |

### 13.3 T2 後端（`Heptarun`）

新增：
| 檔案 | 說明 |
|---|---|
| `S\Quote\entites\S_Quote_Item.cs` | Quote item entity（Yos `[YosDCS]`，**通用命名，不限 Transload**） |
| `S\Quote\_context\QuoteItemContext.cs` | Quote item context（通用） |
| `S\Transload\_context\TransloadRateCardContext.cs` | 報價單 + 寫 S_Rate_Service |
| `S\Transload\_context\TransloadBillingContext.cs` | 撈 WMS / 算四類 / 寫 leg journal / 呼 CreateInvoice |
| `S\Transload\_context\TransloadOrderContext.cs` | 確保 S_Order + S_Order_Container |
| `S\Transload\_context\TransloadShipmentRequestContext.cs` | |
| `S\Transload\_context\TransloadCustomerContext.cs` | me 反查 + Client↔Party 對應 |
| `S\Transload\enums\TransloadEnums.cs` | LoadingType / ContainerType / ChargeType / BillingStatus |

重用（不改）：
| 檔案 | 用途 |
|---|---|
| `S\Invoice\_context\InvoiceContext.cs` | CreateInvoice / getUnInvoiceDataTable / UpdateInvoicedRemainAmout |
| `S\Invoice\_context\InvoiceGenerator.cs` | PDF + 寄信 |
| `S\Order\_context\OrderContainerLegJournalContext.cs` | leg journal CRUD |
| `P\Item\ItemServiceContext.cs` | `P_Item_Service` |
| `S\Rate\RateServiceContext.cs` | `S_Rate_Service` |
| `S\Rate\RateExtraContractContext.cs` / `RateExtraItemContext.cs` | 階梯 |
| `B\Cost\CostServiceContext.cs` / `CostExtraContractContext.cs` / `CostExtraItemContext.cs` | PV 成本 |

### 13.4 T2 UI（`HRTWebSite`）— **不新增任何模組**（v1.1 拍板）

使用者拍板：**T2 端帳單畫面用原本的 SO/Invoice 就好**，HRTWebSite 端**不加 Transload 專屬 UI 模組**。

- 會計開帳走既有 `Module\Invoice\InvoiceSearch.aspx`：transload 的 charge 已寫入 `S_Order_Container_Leg_Journal`、且歸屬於對應的 `S_Order`（每客戶每期或每櫃一張），既有 InvoiceSearch 自然能查到該 SO 的未開票 charge，按既有流程開 Invoice。
- Quote Sheet 維護**只在 SPA `/quotes` 頁**操作，呼叫 T2 API（`POST /api/transload/quotes`）寫 `S_Rate_Extra_Contract`（表頭，延用既有）+ `S_Quote_Item`（明細，新表）。HRTWebSite 端**不另開 Quote 維護畫面**。
- 不改 `_MenuBuilder.cs`：選單不加項目。
- 不改 `InvoiceSearch.aspx`：transload SO 透過 `S_Order_Container.IsTransloader=1` 識別，既有畫面顯示沒問題。

**含意**：T2 端只新增 API + Entity + Context（§13.2/§13.3），**完全不動 HRTWebSite**。會計訓練成本最低 — 只是「多了一類 SO，照舊操作」。

### 13.5 WMS API（`E:\Project\DC\WMS\API`）

新增：
| 檔案 | 說明 |
|---|---|
| `YAEP.WMS.API\Controllers\TransloadController.cs` | **只** BillableEvents（ShipmentRequest 移 T2） |
| `YAEP.WMS.API\Models\Response\TransloadBillableEventsResponse.cs` | |
| `YAEP.WMS.BLL\Manager\TransloadManager.cs` | 聚合 aging / per-container onhand |
| `YAEP.WMS.DAL\Repository\TransloadRepository.cs` | SQL |
| `YAEP.WMS.Interfaces\Manager\ITransloadManager.cs` / `ITransloadRepository.cs` | |

修改：
| 檔案 | 說明 |
|---|---|
| `YAEP.WMS.API\Models\Request\ReceivingRequest.cs` | 加 3 個 transload 欄位（SealNo/ContainerType/LoadingType）|
| `YAEP.WMS.API\Controllers\InventoryController.cs` | GetInventory response 加 conNo/aging/onHand（透過 PayLoad join 算）|
| `YAEP.WMS.API\Controllers\OrderController.Inbound.cs` | 寫新欄位 + 走完整 receiving（WorkOrder/Ticket/PayLoad/Inventory）|
| `YAEP.WMS.API\Controllers\OrderController.Outbound.cs` | 寫 Vessel/BOL 新欄位 |
| `YAEP.WMS.API\Controllers\BolController.cs` | 寫 Tracking/PODUrl/ShipMethodUID |
| `YAEP.WMS.DI.Agent\DIRoot.cs` | 註冊 TransloadManager/Repository |
| `DB\dbo\Tables\WMS_Manifest.sql` | 3 欄（SealNo/ContainerType/LoadingType）|
| `DB\dbo\Tables\WMS_Vessel.sql` | 1 欄（StackableType）|
| `DB\dbo\Tables\WMS_BOL.sql` | 2 欄（TrackingNumber/PODUrl）|
| `DB\dbo\Migrations\WMS_ShipMethod_seed.sql` | 補 2 筆 LTL/FTL 資料 |
| Model 實體（`YAEP.WMS.Model\...`） | Dapper attribute 加屬性 |

> `YAEP_Party` **不加任何欄位** — 改用 `YAEP_Party.ID = S_Client.ID` 同字串對應。

---

## 14. 驗收測試案例

### 14.1 登入

- `mariew` 員工以 `/api/auth/internal/login` 登入 → 進 Dashboard
- 選定一筆既有 PACIFIC 類客戶（如 `Client-5465 ALUMAFOLD PACIFIC INC`），以該 client 的 `Account_ID` 用 `/api/auth/ext/login` 登入 → 進 Portal
- SPA `GET /api/transload/me` 回 `Client_UID = S_Client.UID`

### 14.2 Quote

- Pacific 建 Quote → 表頭 1 列 `S_Rate_Extra_Contract`（Apply_Type=99 transload）、每費率 1 列 `S_Quote_Item`
- 階梯 storage 建 `S_Rate_Extra_Contract` + `S_Rate_Extra_Item`
- Active quote 可被 Billing sync 讀到

### 14.3 Receiving

- 建 F2P container（含 SealNo/ContainerType/LoadingType；ArrivalDate 由 receiving 流程透過 PayLoad.ReceivedDate 帶；StackableType 在 outbound 填）
- 建 SKU（既有 → ItemUID；新 → AddProduct）
- WMS 產生 `WMS_Manifest` + `WMS_Manifest_Item_List`
- Inventory 可查到該 container 的 SKU、aging 從 ArrivalDateActual 算

### 14.4 Inventory

- 依 client/warehouse/SKU/container 查 → 顯示 `ConNo / OnHand / AgingDays / FreeDays / BillableStorageDays`

### 14.5 Outbound

- 從 container 選 SKU 出貨 → `POST /api/Order/Allocated`
- 扣 inventory、建 `WMS_Vessel`（含 StackableType）+ `WMS_BOL`（含 TrackingNumber/PODUrl/ShipMethodUID→LTL or FTL）

### 14.6 Billing

- T2 sync WMS billable events
- `S_Order` + `S_Order_Container`（IsTransloader=1, Unload_Type, Seal_No, ID=ConNo）建立 — 透過 ID 對應 WMS_Manifest.RefNo
- `S_Order_Container_Leg_Journal` 寫入四類 charge
- 確認 → Generate Invoice → `S_Invoice` + `S_Invoice_Line_Item`、leg journal `Remain_Amount` 歸 0
- 寄 PDF（如 SendEmail=true）

### 14.7 Customer Portal

- 客戶只看自己庫存（JWT.Account → `S_Client.Account_ID` 反查 → `S_Client.ID` → WMS `YAEP_Party.ID` 反查 → `YAEP_Party.UID` 過濾）
- Customer 送 shipment request → T2 寫 `S_Order`（Type=200 transload_request）
- 員工核准 → 觸發 WMS Allocated

---

## 15. 實作優先順序（依「下週到貨」調整）

### Phase 0：最優先（下週到貨前必須能用）— 進存出貨 + T2 SO 即建

**v1.3 範圍擴大**：除 WMS 部分外，**T2 端需要的 `orders/inbound` + `orders/outbound` API 也納入 Phase 0**（為了 SPA 雙寫即建 SO）。

1. **WMS DB 6 欄位 + 2 筆資料**：
   - `WMS_Manifest` 加 3 欄（SealNo/ContainerType/LoadingType）
   - `WMS_Vessel` 加 1 欄（StackableType）
   - `WMS_BOL` 加 2 欄（TrackingNumber/PODUrl）
   - `WMS_ShipMethod` 補 2 筆（LTL / FTL system-wide）
2. WMS Model 實體加屬性
3. WMS Receiving 擴充 + **走完整 receiving**（WorkOrder → Ticket → PayLoad → Inventory；確保 `WMS_PayLoad.ReceivedDate` 有寫入）
4. WMS Inventory `GetInventory` 回應擴充（per-container 反查走 `WMS_PayLoad` join 路徑；TotalPallets/Cartons 動態 SUM via Manifest_Item_List join YAEP_Package；ArrivalDateActual 取 MIN(WMS_PayLoad.ReceivedDate)）
5. WMS Outbound + Vessel.StackableType + BOL 寫入 TrackingNumber/PODUrl/ShipMethodUID
6. **T2 `orders/inbound` + `orders/outbound` API 上線**（v1.3 新增；§8.6.1+§8.6.2）+ 容器 type int↔UID 對照表（§11.2）；不必上 quote / billing-events / invoices API（Phase 1）
7. **SPA 雙寫流程**：Receiving Save → WMS + T2；Outbound Save → WMS + T2；T2 失敗 warning + fallback 補建（§6.5/§6.7 雙寫流程）
8. SPA Login + Receiving + Inventory + Outbound 三畫面
9. **WMS `YAEP_Party` migration**：把既有 T2 `S_Client` upsert 進 WMS YAEP_Party（`YAEP_Party.ID = S_Client.ID`），確保客戶在 WMS 端找得到
10. 登入用 WMS 既有帳號或暫用 T2 internal（員工）；客戶 portal 等 Phase 3 整合
11. **計費先不做**（資料先正確收進 WMS + T2 SO，charge 計算 Phase 1 補；SO 已建好，charge 直接寫入 leg_journal）

### Phase 1：T2 計費

1. T2 DB：**只建 1 張 `S_Quote_Item`**（通用命名）；補 `P_Item_Service` 15 筆 transload 專用代碼（不加任何 T2 既表欄位）
2. T2 entities + contexts：
   - `Heptarun\S\Quote\entites\S_Quote_Item.cs` + `QuoteItemContext.cs`（通用 quote item）
   - `Heptarun\S\Transload\_context\TransloadBillingContext.cs`（撈 WMS / 算四類 / 寫 leg journal / 呼 CreateInvoice）
   - `TransloadOrderContext.cs`（v1.3：實作 `orders/inbound` + `orders/outbound` + fallback `orders/ensure`）
3. T2 API：`/api/transload/me / quotes / customers / contract-prices / billing-events/sync / invoices/generate`
   - Quote API 寫入 `S_Rate_Extra_Contract`（既有表頭）+ `S_Quote_Item`（新表）
   - Charge rate 來源從 `S_Quote_Item.Price` 取
   - sync-charges 時 SO 通常已存在（Phase 0 即建），直接寫 leg_journal
4. **HRTWebSite 不動**（會計走既有 `Module\Invoice\InvoiceSearch.aspx`，Quote 在 SPA 維護）
5. 端到端：對 Phase 0 已收進來的 inbound/outbound SO sync 計算 charge 並開 invoice

### Phase 2：SPA 其餘

1. SPA Quotes / Billing 檢視 / Customer Portal / Dashboard

### Phase 3：整合

1. 換成 T2 ext/login 客戶登入（取代 Phase 0 的暫用 WMS 登入）
2. WMS `YAEP_Party` 同步機制定案（手動 / 排程 / 即時，§17.2）
3. Customer Portal 自助申請流程（寫入 `S_Order` Type=200，§9.4）
4. 跨系統授權方案落地（待定 §17.1）

### Phase 4：端到端驗收

依 §14 全跑一遍。

---

## 16. 明確不採用 / 不新建（v1.3 最終）

**v1.3 新增**：
- ❌ **T2 S_Order 不延遲到 Billing sync 才建**（v1.2 是延遲建；v1.3 改為 Receiving/Outbound 儲存時即建）
- ❌ **Inbound + Outbound 不共用同一張 SO**（每次出貨各建一張 outbound SO，refer_to inbound SO）
- ❌ **Invoice 不跨 SO 合併**（1 SO = 1 invoice，客戶月結若需合併要另開機制）

- ❌ 不新建 Marie / john / sara / pacific 帳號（使用既有 `mariew` + 既有 S_Client）
- ❌ T2 端不新建：`S_Transload_Customer_Map` / `S_Transload_Rate_Card` / `S_Transload_Quote` / `S_Transload_Source_Map` / `S_Transload_Billing_Event` / `S_Transload_Shipment_Request` + `_Item`（**全部用既有表代**）
- ✅ T2 只新建 1 張：**`S_Quote_Item`**（通用命名，未來其他模塊可用）
- ❌ T2 既表不加任何欄位（含 `S_Order_Container.WMS_Manifest_UID`、`S_Client.WMS_Party_UID` 都取消）
- ❌ WMS 不開任何新表（只加 3+1+2=6 個欄位 + WMS_ShipMethod 補 2 筆）
- ❌ WMS 不加 `YAEP_Party.T2ClientUID`、`WMS_Inventory.SourceManifestItemUID`、`WMS_Manifest.TotalPallets/Cartons/ArrivalDateActual/StackableType`（動態算 / 移層 / 走 PayLoad）
- ❌ 不在 SPA 保存使用者密碼（用 T2 既有 Auth）
- ❌ 不在 WMS 保存 quote / 合約價格 / 計算金額 / 開 invoice
- ❌ 不在 T2 存 SKU 主檔（SKU 留在 WMS；BOL by WMS 含 SKU，Invoice by T2 per container）
- ❌ 不讓客戶看到其他客戶資料
- ❌ 不寫 `WMS_ShipVia`（不存在；用 `WMS_ShipMethod`）
- ❌ HRTWebSite 不新增任何模組（會計用既有 InvoiceSearch；Quote 在 SPA 維護）
- ❌ 不 cross-dock 簡化收貨直入庫（v1.2 拍板走完整 WorkOrder/Ticket/PayLoad receiving）

---

## 17. 待定案（實作前需確認）

1. **跨系統授權**：WMS 直接驗 T2 JWT（共用 signing key），或 SPA 拿 T2 token 換 WMS token？Phase 0 暫用 WMS 既有 auth 規避。
2. **WMS↔T2 客戶同步機制**：WMS `YAEP_Party.ID = S_Client.ID` 字串對齊，但 T2 新增 / 改名後如何同步到 WMS？選項：(a) 一次性 migration + 手動補；(b) sync API（T2 API 推到 WMS）；(c) sync-charges 時 auto-upsert（每次撈 BillableEvents 順便確保 Party 存在）
3. ~~cross-dock 簡化收貨直入庫~~ → **v1.2 已定案：走完整 receiving (WorkOrder/Ticket/PayLoad/Inventory)，本項取消**
4. ~~Transload 訂單顆粒度~~ → **v1.3 已定案：1 receiving = 1 inbound SO；1 outbound = 1 outbound SO（Refer_To_Order 連，1 SO = 1 invoice），本項取消**
5. **Container Type int → UID 對照**：寫在 `TransloadBillingContext` 的 hard-code 對照表，或讀 T2 既有 container type 主檔？
6. **Service_UID 對照**：§9.5 的 15 個 service 補進 `P_Item_Service` 時，ID/Charge_Type/P_Class_UID 實際值需確認
7. **WMS server-to-server**：T2 撈 BillableEvents 用什麼帳號 / API key？
8. ~~Rate 快照 vs 即時查~~ → **v1.2 已定案：rate 來源 `S_Quote_Item.Price`（每張 quote 本身就是 snapshot，互不干擾）；charge 寫入時 Rate+Original_Rate 凍結。本項取消。**
9. **`S_Rate_Extra_Contract.Apply_Type / Pricing_Type` 識別碼**：transload quote header 用哪個 enum value 區隔（不要跟 trucking PC contract 撞）？v1.2 暫定 99，實作時與既有 enum 對齊。
10. **`WMS_ShipMethod.PartyUID` NOT NULL**：LTL/FTL 兩筆 system-wide 要放哪個 PartyUID？建一個 system party，或檢查能否改 NULL，或用 sentinel UID `00000000-...`？Phase 0 實作時定案。
11. **YAEP_Package 階層的 pallet/carton Type value**：§10.6 動態算 TotalPallets/Cartons 需要 YAEP_Package.Type 的 enum value 對應；Phase 0 查既有 WMS 資料確認。
12. **WMS_WorkOrder ↔ Manifest 連結欄位名**：§10.5 / §10.7 反查路徑用 `WMS_WorkOrder.ManifestUID` 假設；Phase 0 確認 WMS_WorkOrder 實際 schema。

---

## 18. 環境設定（實作時填值）

- T2 DB connection
- WMS DB connection
- T2 JWT signing key
- WMS service account / S2S key
- 既有 `mariew` 員工 UID
- 測試 PACIFIC 客戶 `S_Client.UID` + `S_Client.ID`（WMS `YAEP_Party` 用同 ID 建一筆即可，UID 由 WMS 自己生）
- 預設 Warehouse UID（receiving 預設倉）
- 預設 `Bill_To_Type` Guid
