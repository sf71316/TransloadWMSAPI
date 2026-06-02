# Transload WMS API — 已完成端點規格（RD 版）

> 對象：前端 / 整合 RD。本文件只列**目前已完成、可串接**的端點，含完整 Request / Response 與真實範例。
> 規劃全貌與待辦見 `Transload_WMS_API_主文件.md`（SSOT）；本文件以實作程式碼為準。
> 最後更新：2026-06-01

---

## 0. 通用慣例

| 項目 | 說明 |
|---|---|
| **Base URL（伺服器）** | **`http://192.168.1.19:8080/`** ← RD 串接用此位址 |
| Base URL（本機開發） | `http://localhost:8081`（IIS Express；SSL 埠 44364） |
| 路由模板 | `/api/{Controller}/{Action}`；Transload 端點皆為 `/api/Transload/{Action}` |
| 完整 URL 範例 | `http://192.168.1.19:8080/api/Transload/GetProducts?keyword=TES` |
| 認證 | 先呼叫 `POST /api/auth/applogin` 取 JWT，後續請求帶 Header `Authorization: Bearer <token>` |
| 內容型別 | `Content-Type: application/json`（GET 用 query string） |
| 客戶識別 | 一律用 **`CustomerPartyName`**（= Party.ID = T2 S_Client.ID），後端解析成 PartyUID；依登入者所屬群組過濾，看不到的客戶回 `Customer not found.` |

### 0.1 回應包裝 `APIResult<T>`

所有端點回應統一格式，資料放在 `Data`：

```jsonc
{
  "IsComplete": true,      // 成功 true / 失敗 false
  "Code": 0,               // 成功 0；一般失敗 -1；系統初始化中 -100
  "Message": "",           // 失敗時的訊息
  "ResponseTime": "2026-06-01T11:42:03.0090934+08:00",
  "Data": { /* T，依端點而定 */ }
}
```

### 0.2 錯誤慣例

| 情況 | HTTP | Code | 說明 |
|---|---|---|---|
| 成功 | 200 | 0 | `Data` 有值 |
| 一般失敗 | 400 | -1 | 參數錯誤 / 查無 / 商業邏輯失敗，`Message` 有原因，`Data=null` |
| 系統初始化中 | 401 | -100 | 站台啟動後快取（DrKnowAll）尚未載入完成，稍候重試即可 |

> PowerShell `Invoke-RestMethod` 對 HTTP 400 會丟例外，失敗 body 在 `$_.ErrorDetails.Message`。

---

## 1. 端點總覽

| # | 功能 | Method | 路由 | 狀態 |
|---|---|---|---|---|
| 1 | 取得 Token | POST | `/api/auth/applogin` | ✅ |
| 2 | 客戶下拉 | GET | `/api/Transload/GetCustomerList` | ✅ |
| 3 | 倉庫下拉 | GET | `/api/Transload/GetWarehouseList` | ✅（客戶過濾未啟用） |
| 4 | 產品查詢(autocomplete) | GET | `/api/Transload/GetProducts` | ✅ |
| 5 | 新增 SKU | POST | `/api/Transload/AddProduct` | ✅ |
| 6 | 入庫列表 | POST | `/api/Transload/GetManifestList` | ✅（彙總欄暫回 0） |
| 7 | 建立收貨（含完成入庫） | POST | `/api/Transload/Receiving` | ✅ |
| 8 | 收貨完成（手動重試） | POST | `/api/Transload/CompleteReceiving` | ✅ |
| 9 | 庫存列表 | POST | `/api/Transload/GetInventory` | ✅ |
| 附 | 收貨刪除（清測試資料） | POST | `/api/Order/CancelReceiving` | ✅（既有） |

---

## 2. 端點明細

### 2.1 取得 Token — `POST /api/auth/applogin`

**Request**（不需 token）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| Account | string | ✓ | 帳號 |
| Password | string | ✓ | 密碼 |

```json
{ "Account": "T2user", "Password": "T2user" }
```

**Response 欄位**

| 欄位 | 型別 | 說明 |
|---|---|---|
| Data | string | JWT token，後續請求帶 `Authorization: Bearer <Data>` |

```json
{ "IsComplete": true, "Code": 0, "Message": "", "Data": "eyJhbGciOi...<JWT>" }
```

---

### 2.2 客戶下拉 — `GET /api/Transload/GetCustomerList`

依 token 使用者所屬群組過濾，只回類別為 Customer 的 Party。

**Request**（query string，皆選填）

| 欄位 | 型別 | 說明 |
|---|---|---|
| UID | guid | 指定客戶 UID |
| ID | string | 客戶代碼（精確/前綴依底層） |
| Company | string | 客戶名稱 |

`GET /api/Transload/GetCustomerList?ID=COSCO`

**Response** — `Data` = 客戶(Party)陣列；主要欄位：

| 欄位 | 型別 | 說明 |
|---|---|---|
| UID | guid | 客戶 UID（= PartyUID） |
| ID | string | 客戶代碼（= CustomerPartyName） |
| Name | string | 客戶名稱 |

```jsonc
{
  "IsComplete": true, "Code": 0,
  "Data": [
    { "UID": "1959665e-b004-4118-8b82-9f81eca03582", "ID": "COSCO", "Name": "COSCO", "...": "其餘標準 Party 欄位" }
  ]
}
```

> 無所屬群組或查無資料 → 回查無資料（`Data` 空 / 失敗）。

---

### 2.3 倉庫下拉 — `GET /api/Transload/GetWarehouseList`

回該 token 使用者群組內、狀態 > Inactive 的倉庫。

**Request**（query string）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| customerPartyName | string | — | **預留參數，目前不參與過濾**（客戶↔倉庫關聯定義後才實作，Task #2） |

`GET /api/Transload/GetWarehouseList`

**Response** — `Data` = 倉庫陣列；主要欄位：

| 欄位 | 型別 | 說明 |
|---|---|---|
| UID | guid | 倉庫 UID（建立 Receiving 用此值） |
| ID | string | 倉庫代碼 |
| Name | string | 倉庫名稱 |
| GroupUID | guid | 所屬群組 |
| Country/State/City/Zip/Address | string | 地址 |
| Status | int | 狀態 |

**真實回應範例**（伺服器實測）

```json
{
  "IsComplete": true,
  "Code": 0,
  "Message": "",
  "ResponseTime": "2026-06-01T12:43:42.6645361+08:00",
  "Data": [
    {
      "UID": "4598f8e2-4287-4841-b232-c4c710e4fbcf",
      "GroupUID": "2bb00716-83d5-4df1-9bc8-fa0d1a92a116",
      "ID": "T2 Warhouse",
      "Name": "T2 Warhouse",
      "Phone": "",
      "Fax": null,
      "Country": "USA",
      "State": null,
      "City": null,
      "Zip": null,
      "Address": null,
      "Volume": 999999.0,
      "Status": 100,
      "Description": null,
      "Mail": null,
      "Contact": "",
      "PhotoUID": "00000000-0000-0000-0000-000000000000",
      "CreatedBy": null,
      "CreatedOn": "2021-06-22T00:00:00",
      "ModifiedBy": null,
      "ModifiedOn": "2021-06-22T00:00:00"
    }
  ]
}
```

---

### 2.4 產品查詢（autocomplete） — `GET /api/Transload/GetProducts`

依產品 ID 前綴比對，選填客戶限縮。讀 DrKnowAll 快取。

**Request**（query string）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| keyword | string | ✓ | 產品 ID 關鍵字，**長度須 ≥ 3** |
| customerPartyName | string | — | 客戶代碼；帶了但查無客戶 → `Customer not found.` |

`GET /api/Transload/GetProducts?keyword=TES&customerPartyName=COSCO`

**Response** — `Data` = 產品陣列，每筆含 `Package[]`：

| 欄位 | 型別 | 說明 |
|---|---|---|
| ItemID | string | 產品代碼（= SKU） |
| ItemName | string | 產品名稱 |
| ItemUID | guid | 產品 UID |
| CustomerUID | guid | 客戶 UID |
| CustomerName | string | 客戶名稱 |
| Package[] | array | 該產品的包裝清單（見下） |
| Package[].VersionID | string | 包裝版本 |
| Package[].PackageName | string | 包裝名稱（如 PALLET/BOX/EACH） |
| Package[].PackageUID | guid | **包裝層 UID（Receiving 帶入此值）** |
| Package[].ItemUID | guid | 所屬產品 UID |

**真實回應範例**（伺服器實測；`keyword=TES&customerPartyName=COSCO` 回 4 筆，以下節錄 1 筆，其餘結構相同）

```jsonc
{
  "IsComplete": true,
  "Code": 0,
  "Message": "",
  "ResponseTime": "2026-06-01T12:43:47.4567269+08:00",
  "Data": [
    {
      "ItemID": "TESTSKU002",
      "ItemName": "TESTSKU002",
      "ItemUID": "eeeed7ff-43f7-48fb-82ec-3c1bfa76e06d",
      "CustomerUID": "1959665e-b004-4118-8b82-9f81eca03582",
      "CustomerName": "COSCO",
      "Package": [
        { "VersionID": "TESTSKU002 ver.1", "ItemName": "TESTSKU002", "PackageName": "EACH",   "PackageUID": "b280ec5d-415a-49e2-a65b-388e96b4a47c", "ItemUID": "eeeed7ff-43f7-48fb-82ec-3c1bfa76e06d" },
        { "VersionID": "TESTSKU002 ver.1", "ItemName": "TESTSKU002", "PackageName": "BOX",    "PackageUID": "b4cc5adc-87ff-4d06-b084-2f01d2c5d897", "ItemUID": "eeeed7ff-43f7-48fb-82ec-3c1bfa76e06d" },
        { "VersionID": "TESTSKU002 ver.1", "ItemName": "TESTSKU002", "PackageName": "PALLET", "PackageUID": "8546858f-81ba-4025-842b-fac758d27d09", "ItemUID": "eeeed7ff-43f7-48fb-82ec-3c1bfa76e06d" }
      ]
    }
    // ... 其餘 TESTSKU001 / TESTSKU004 / TESTSKU005 結構相同
  ]
}
```

> 收貨(Receiving)通常帶 **PALLET 層** 的 `PackageUID`（如上例 `8546858f-...`）。
> `Package[]` 順序為 EACH → BOX → PALLET（依建立時間新→舊）。
> keyword 不帶 customerPartyName 時回**全域**符合前綴的產品；帶 customerPartyName 才限縮該客戶。

---

### 2.5 新增 SKU — `POST /api/Transload/AddProduct`

以最小輸入建立產品本體（綁客戶），並自動建立預設三層巢狀包裝 **PALLET → BOX → EACH**（Quantity=1、尺寸 0）。

**Request**（body）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| Sku | string | ✓ | 產品代碼（同時作為 Name） |
| CustomerPartyName | string | ✓ | 客戶代碼 |

```json
{ "Sku": "TESTSKU010", "CustomerPartyName": "COSCO" }
```

**Response** — `Data` = 新建產品

| 欄位 | 型別 | 說明 |
|---|---|---|
| UID | guid | 新產品 UID |
| ID | string | = Sku |
| Name | string | = Sku |
| GroupUID | guid | 所屬群組 |
| Status | int | 1 = Active |

```json
{ "IsComplete": true, "Code": 0,
  "Data": { "UID": "....", "ID": "TESTSKU010", "Name": "TESTSKU010", "GroupUID": "...", "Status": 1 } }
```

> 失敗訊息：`Customer not found.`（查無客戶）/ `Customer code is not unique.`（客戶重複）/ `產品已存在`（同群組+SKU+客戶重複）。

---

### 2.6 入庫列表 — `POST /api/Transload/GetManifestList`

固定 Type=Inbound 且 Status>0。客戶 / Status / Key 過濾皆在 SQL 內。

**Request**（body，皆選填）

| 欄位 | 型別 | 說明 |
|---|---|---|
| CustomerPartyName | string | 客戶代碼 → 過濾 Manifest.PartyUID |
| Status | int? | ManifestStatus 細分過濾（在 Status>0 範圍內） |
| Key | string | 關鍵字，比對 Manifest.Name 或該 Manifest 任一 Vessel.RefNo(ConNo) |

```json
{ "CustomerPartyName": "COSCO", "Status": null, "Key": "TLD-TEST" }
```

**Response** — `Data` = 列表陣列：

| 欄位 | 型別 | 說明 |
|---|---|---|
| ManifestUID | guid | Manifest UID |
| ManifestName | string | Manifest 名稱 |
| CustomerUID / CustomerID / CustomerName | guid/string | 客戶 |
| WarehouseUID / WarehouseName | guid/string | 倉庫 |
| Description | string | 備註 |
| Status / StatusName | int/string | 狀態 |
| CreatedOn | datetime? | 建立時間 |
| ContainerCount | int | 該 manifest 的 Vessel(櫃) 數 |
| ArrivalDate | datetime? | 到倉日 = MIN(Vessel.ArrivalDate)；無資料為 null |
| AgingDays | int? | 在倉天數 = today − ArrivalDate；ArrivalDate 為 null 時為 null |
| **Qty** | int | **在倉數量** = SUM(PayLoad.Quantity, Type=Stock)；原生包裝單位數，單位見 UOM |
| **UOM** | string | **在倉包裝單位名稱**（如 `Pallet`）；同 manifest 多種單位時以 `/` 串接 |
| **ReceivedQty** | int | **實到數量** = SUM(收貨 Ticket.ActQty)；原生包裝單位數，單位見 ReceivedUOM |
| **ReceivedUOM** | string | **實到包裝單位名稱**（如 `Pallet`）；多種單位時以 `/` 串接 |

**真實回應範例**（伺服器實測）

```json
{
  "IsComplete": true,
  "Code": 0,
  "Message": "",
  "ResponseTime": "2026-06-01T14:29:01.4275374+08:00",
  "Data": [
    {
      "ManifestUID": "0fefed2e-e744-40a7-b2a7-b809fbcbb3e9",
      "ManifestName": "TLD-TEST-003",
      "CustomerUID": "1959665e-b004-4118-8b82-9f81eca03582",
      "CustomerID": "COSCO",
      "CustomerName": "COSCO",
      "WarehouseUID": "4598f8e2-4287-4841-b232-c4c710e4fbcf",
      "WarehouseName": "T2 Warhouse",
      "Description": null,
      "Status": 500,
      "StatusName": "Complete",
      "CreatedOn": "2026-06-01T03:42:01.157",
      "ArrivalDate": "2026-06-01T00:00:00",
      "AgingDays": 0,
      "ContainerCount": 1,
      "Qty": 5,
      "UOM": "Pallet",
      "ReceivedQty": 5,
      "ReceivedUOM": "Pallet"
    }
  ]
}
```

> **數量說明**：`Qty`/`ReceivedQty` 為**原生包裝單位數**（非件數換算），搭配 `UOM`/`ReceivedUOM` 顯示單位名稱。例如「5 Pallet」。
> 在倉(`Qty`)＝目前實際在儲位的庫存；實到(`ReceivedQty`)＝收貨當下的數量。出貨後在倉會減少、實到不變。

---

### 2.7 建立收貨 — `POST /api/Transload/Receiving`  ⭐

**一次呼叫完成整條入庫流程**：建立 Manifest/BOL/Vessel(每櫃 1 個)/明細 → 自動規畫工單(WorkOrder/Pod/Payload)、生成 SSCC 棧板條碼 → 指派 team → **完成收貨**(建在庫 PayLoad 落暫存區) → **移動上架**(搬到實際儲位，成為可用庫存)。

生成前會驗證每行的**產品**（客戶/群組可見性 + 重複）與**包裝 PackageUID** 是否存在；任一缺整批失敗。`RefNo + WarehouseUID` 重複亦擋下。

**Request**（body）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| RefNo | string | ✓ | 批次參考號（PO/Order#）；與 WarehouseUID 一起判重複 |
| WarehouseUID | guid | ✓ | 入庫倉庫（由 GetWarehouseList 選） |
| CustomerPartyName | string | ✓ | 客戶代碼 |
| Containers[] | array | ✓ | 櫃清單，每櫃 → 1 Vessel |

**Containers[]（每櫃）**

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| ConNo | string | ✓ | 櫃號 → Vessel.RefNo |
| SealNo | string | — | 封條號 → Vessel.SealNo |
| ContainerSize | int? | — | 櫃型碼：10=20GP / 20=40GP / 30=40HQ / 40=45HQ / 50=LooseCargo / 60=LCL / 90=Other |
| LoadingType | int? | — | 裝卸型碼：10=F2F / 20=F2P / 30=P2P / 40=Full Container |
| StackableType | int? | — | 可堆疊：0=Stackable / 1=Non-Stackable |
| ArrivalDate | datetime? | — | 到倉日 → Vessel.ArrivalDate（aging 來源） |
| Items[] | array | ✓ | 該櫃 SKU 明細 |

**Items[]（櫃內每行）**

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| Sku | string | ✓ | SKU（= Item.ID），驗證用 |
| EnterQty | int | ✓ | 數量（該 PackageUID 的包裝單位數，如「5 板」） |
| PackageUID | guid | ✓ | 包裝層 UID（由 GetProducts 取，通常用 Pallet 層）；亦用來反查 ItemUID |

```json
{
  "RefNo": "TLD-TEST-003",
  "WarehouseUID": "4598f8e2-4287-4841-b232-c4c710e4fbcf",
  "CustomerPartyName": "COSCO",
  "Containers": [
    {
      "ConNo": "TCON-003", "SealNo": "SEAL-003",
      "ContainerSize": 40, "LoadingType": 30, "StackableType": 0,
      "ArrivalDate": "2026-06-01",
      "Items": [
        { "Sku": "TESTSKU002", "EnterQty": 5, "PackageUID": "8546858f-81ba-4025-842b-fac758d27d09" }
      ]
    }
  ]
}
```

**Response** — `Data`

| 欄位 | 型別 | 說明 |
|---|---|---|
| ManifestUID | guid | 建立的 Manifest UID |
| Vessels[] | array | 每櫃建立結果 |
| Vessels[].VesselUID | guid | Vessel UID |
| Vessels[].ConNo | string | 櫃號（= Vessel.RefNo） |
| Vessels[].Status | int | 200 = 已生成 |

```json
{
  "IsComplete": true, "Code": 0, "Message": "",
  "Data": {
    "ManifestUID": "0fefed2e-e744-40a7-b2a7-b809fbcbb3e9",
    "Vessels": [
      { "VesselUID": "7d534cce-d93c-4ded-8589-157b863cded3", "ConNo": "TCON-003", "Status": 200 }
    ]
  }
}
```

> 回 `IsComplete: true` 即代表**生成 + 收貨 + 上架全部完成**，貨已成為儲位上的可用庫存。
> 常見失敗：`Customer not found.` / 產品或包裝不存在 / `RefNo + Warehouse 重複`。

---

### 2.8 收貨完成（手動重試） — `POST /api/Transload/CompleteReceiving`

對指定 Manifest 觸發完成（系統 IsAllPass）。**正常情況 Receiving 已自動呼叫此流程，本端點供異常重試**。

內部為**兩段式**：先完成收貨票（建 PayLoad 落暫存區）→ 再完成移動票（搬到實際儲位）。

**Request**（body）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| ManifestUID | guid | ✓ | 由 Receiving 回應取得 |

```json
{ "ManifestUID": "0fefed2e-e744-40a7-b2a7-b809fbcbb3e9" }
```

**Response 欄位** — `Data`

| 欄位 | 型別 | 說明 |
|---|---|---|
| ManifestUID | guid | 完成的 Manifest UID |
| IsComplete | bool | true = 收貨+上架已完成 |

```json
{ "IsComplete": true, "Code": 0,
  "Data": { "ManifestUID": "0fefed2e-e744-40a7-b2a7-b809fbcbb3e9", "IsComplete": true } }
```

> 若該 Manifest 的票已完成，會回失敗 `Ticket has been completed.`（屬正常防呆，非錯誤）。

---

### 2.9 庫存列表 — `POST /api/Transload/GetInventory`

在庫清單，**粒度 = 一個在庫 PayLoad**（固定 `Type=Stock` 且 `Status=Active 500`，即已上架可用的庫存）。

**Request 參數**（body，皆選填）

| 欄位 | 型別 | 說明 |
|---|---|---|
| CustomerPartyName | string | 客戶代碼 → 過濾 Manifest.PartyUID |
| Key | string | 關鍵字，同時比對 SKU(=Item.ID) 與 ConNo(=Vessel.RefNo) |

```json
{ "CustomerPartyName": "COSCO", "Key": "TESTSKU002" }
```

**Response 欄位** — `Data` = 庫存陣列（每列一個在庫 PayLoad）

| 欄位 | 型別 | 說明 |
|---|---|---|
| PayLoadUID | guid | 列 key（= WMS_PayLoad.UID） |
| ItemUID / SKU / ItemName | guid/string | 產品 |
| CustomerUID / CustomerName | guid/string | 客戶 |
| WarehouseUID / WarehouseName | guid/string | 倉庫 |
| **Qty** | int | 在倉數量（原生包裝單位數） |
| **UOM** | string | 在倉包裝單位名稱（如 `Pallet`） |
| **OnHandPcs** | int | 在倉件數（換算到最小單位 Each） |
| ConNo / SealNo / ContainerSize | string/int | 容器（來自 Vessel） |
| LoadingType / **LoadingTypeName** | int / string | 裝卸型碼 + 顯示名稱（10=F2F/20=F2P/30=P2P/40=Full Container；無對應碼時 Name=null） |
| StackableType / **StackableTypeName** | int / string | 可堆疊碼 + 顯示名稱（0=Stackable/1=Non-Stackable；無對應碼時 Name=null） |
| ManifestUID | guid | 來源 Manifest |
| ArrivalDate | datetime? | 到倉日（Vessel.ArrivalDate） |
| AgingDays | int? | 在倉天數 = today − ArrivalDate |

**真實回應範例**（伺服器實測）

```json
{
  "IsComplete": true,
  "Code": 0,
  "Message": "",
  "ResponseTime": "2026-06-01T16:03:03.2862976+08:00",
  "Data": [
    {
      "PayLoadUID": "80123cea-973c-4b3e-af36-ee7ab60cde11",
      "ItemUID": "eeeed7ff-43f7-48fb-82ec-3c1bfa76e06d",
      "SKU": "TESTSKU002",
      "ItemName": "TESTSKU002",
      "CustomerUID": "1959665e-b004-4118-8b82-9f81eca03582",
      "CustomerName": "COSCO",
      "WarehouseUID": "4598f8e2-4287-4841-b232-c4c710e4fbcf",
      "WarehouseName": "T2 Warhouse",
      "Qty": 3,
      "UOM": "Pallet",
      "OnHandPcs": 3,
      "ConNo": "TCON-004",
      "SealNo": "SEAL-004",
      "ContainerSize": 20,
      "LoadingType": 30,
      "LoadingTypeName": "P2P",
      "StackableType": 0,
      "StackableTypeName": "Stackable",
      "ManifestUID": "5e719335-42e7-4f22-b16f-d70c477db4ae",
      "ArrivalDate": "2026-06-01T00:00:00",
      "AgingDays": 0
    }
  ]
}
```

> **數量說明**：`Qty`+`UOM` = 原生包裝單位（如「5 Pallet」）；`OnHandPcs` = 換算到最小單位的件數。
> **範圍**：只列已上架可用庫存（Active）。計費欄（FreeDays/BillableStorageDays）屬計費端、WMS 不提供；配貨量（AllocatedQty）待出貨功能完成後再加。

---

### 附. 收貨刪除（清測試資料） — `POST /api/Order/CancelReceiving`

> 既有端點，非 Transload 命名空間。乾淨清掉一整套尚未完成的生成資料；**已完成（已上架）的 Manifest 無法 Cancel**（入倉作廢請待 VoidManifest，Task #4）。

**Request**（body）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| CustomerPartyName | string | ✓ | 客戶代碼 |
| WarehouseUID | guid | ✓ | 倉庫 |
| RefNo | string | ✓ | 批次參考號 |

```json
{ "CustomerPartyName": "COSCO", "WarehouseUID": "4598f8e2-4287-4841-b232-c4c710e4fbcf", "RefNo": "TLD-TEST-003" }
```

**Response 欄位** — `Data`

| 欄位 | 型別 | 說明 |
|---|---|---|
| IsComplete | bool | true = 已清除 |
| Message | string | 失敗原因（如 `not find manifest info.` 查無可清的 manifest） |

```json
{ "IsComplete": true, "Code": 0, "Data": { "IsComplete": true, "Message": "" } }
```

---

## 3. 串接流程範例（Inbound）

```
1. POST /api/auth/applogin                 → 取 token
2. GET  /api/Transload/GetWarehouseList    → 選倉庫 WarehouseUID
3. GET  /api/Transload/GetProducts?keyword=TES&customerPartyName=COSCO
                                           → 取 SKU 與 PackageUID（Pallet 層）
4. POST /api/Transload/Receiving           → 一次完成入庫（回 ManifestUID）
5. POST /api/Transload/GetManifestList     → 查入庫列表確認
```

---

## 4. 狀態碼對照（供 RD 對資料）

| enum | 值 | 說明 |
|---|---|---|
| TicketType | Receiving=100, Move=300, Outbound=200 | 票別 |
| TicketInfoStatus | Open=300, Complete=600 | 票完成 = 600 |
| PayloadStatus | WaitingForProcessing=100, Active=500 | 在庫可用 = 500 |
| PayloadType | Stock=1 | 在庫 |
| Vessel(回應) Status | 200 | 已生成 |
