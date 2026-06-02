# Transload WMS API 確認清單（對外整合用）

逐一確認過的對外整合 API，全部收斂到 **`/api/Transload/{Action}`**。每支記錄端點、認證、Request、Response、對應既有邏輯、說明。

- 來源程式碼：`YAEP.WMS.API`（.NET Framework 4.6.2）
- 相關規格：`docs/TransloadWMS_API_UI_Spec.md`
- 路由總表：`docs/Transload_API_路由清單.md`
- 確認日期起：2026-05-29

## 通用事項

- **route root**：所有對外整合 API 一律 `/api/Transload/{Action}`（內部實作包既有邏輯或新寫，逐支註明）。
- **認證**：API 皆需 JWT。呼叫端須在 HTTP Header 帶 `Authorization: Bearer <token>`；需等 DrKnowAll 快取初始化完成（未完成時回 401 / code `-100`）。
- **回應統一用標準 `APIResult<T>`**：`{ IsComplete, Code, Message, ResponseTime, Data }`，資料一律放 `Data`。
  - 注意：部分**既有來源端點**原本用 BLL `IActionResult<T>`（資料在 `Content`）。Transload 版一律改包成 `APIResult<T>`。
- JSON 屬性為 **PascalCase**（沿用 C# 屬性名，無 camelCase 轉換）。

---

## ✅ #1　`GET /api/Transload/GetCustomerList`（客戶下拉）

| 項目 | 內容 |
|---|---|
| **端點** | `GET /api/Transload/GetCustomerList` |
| **spec 對應** | §5.1.6（Receiving 矩陣→客戶下拉）、§6.3（Customers 頁） |
| **認證** | 需 JWT token。**依 token 使用者的群組過濾**；該使用者無任何群組 → 回查無資料 |
| **用途** | 客戶（Party，類別 = Customer）清單 / 下拉；可加條件過濾 |
| **對應既有邏輯** | `CustomerController.GetCustomerList`（`CustomerController.cs:57`）；BLL `PartyManager.GetParties(...)`，`PartyTypeCategory=Customer`。新建 Transload 端點，內部複製此邏輯 |

### Request
Query string（`CustomerSearchRequestModel`，`[FromUri]`，全部選填）：

| 參數 | 型別 | 說明 |
|---|---|---|
| `UID` | Guid? | 指定客戶 UID |
| `ID` | string | 客戶代碼（對應 `YAEP_Party.ID`；= spec 的 `S_Client.ID` 字串，§10.8） |
| `Company` | string | 客戶名稱（對應查詢的 `Name`） |

- 不帶任何條件 → 回該 token 使用者群組內的全部 Customer。
- Header：`Authorization: Bearer <token>`（必要）。

### Response
標準 **`APIResult<T>`**（資料在 `Data`）：

```jsonc
{
  "IsComplete": true,
  "Code": 0,
  "Message": "",
  "ResponseTime": "datetime",
  "Data": [
    {
      "UID": "guid",            // 客戶(Party) UID
      "GroupUID": "guid",       // 所屬群組
      "ID": "string",           // 客戶代碼 nvarchar(50)，與 T2 S_Client.ID 同字串
      "Name": "string",         // 客戶名稱 nvarchar(100)
      "Status": 1,
      "Description": "string|null",
      "Country": "string|null", "State": "string|null", "City": "string|null",
      "Zip": "string|null", "Address": "string|null",
      "Phone": "string|null", "PhoneExtension": "string|null", "Fax": "string|null",
      "Email": "string|null",
      "CreatedBy": "string|null", "CreatedOn": "datetime|null",
      "ModifiedBy": "string|null", "ModifiedOn": "datetime|null"
    }
  ]
}
```
> 欄位依 staging DB `YAEP_Party` 表結構查證（`IPartyModel` 在外部 DLL）。注意欄位名是 `Email`（非 `Mail`）、且有 `PhoneExtension`。

### 說明 / 備註
- 只回 `PartyTypeCategory = Customer` 的 party（不含 vendor 等）。
- 群組過濾來源：`getGroupsByUser()` → `GroupManager.GetGroupKeysByUser(使用者UID)`。
- `ID` 即 WMS↔T2 客戶對應 key（`YAEP_Party.ID = S_Client.ID` 同字串，spec §10.8）。
- 此支是客戶查詢本身，輸入為搜尋條件（ID/Company），不涉及 CustomerPartyName 解析。

---

## ✅ #2　`GET /api/Transload/GetWarehouseList`（倉庫下拉）

| 項目 | 內容 |
|---|---|
| **端點** | `GET /api/Transload/GetWarehouseList` |
| **spec 對應** | §5.1.6（Receiving 矩陣→倉庫下拉；spec 原寫 `GET /api/Warehouse/*`） |
| **認證** | 需 JWT token。**依 token 使用者的群組（GroupUID）過濾**，只回 `Status > Inactive` 的倉庫 |
| **用途** | 倉庫下拉清單 |
| **對應既有邏輯** | `ThirdPartyController.GetWarehouseList` → BLL `WarehouseManager.GetThirdPartyWarehouseNameList()`（`WarehouseManager.cs:587`）。新建 Transload 端點，內部呼叫同一 BLL；**回應改包成 `APIResult<T>`**（既有 ThirdParty 版原本回 BLL `Content`） |

### Request
`GET /api/Transload/GetWarehouseList`，Header `Authorization: Bearer <token>`。

| 參數 | 型別 | 說明 |
|---|---|---|
| `CustomerPartyName` | string（選填） | **預留參數**：依客戶過濾倉庫。⚠️ 過濾邏輯**待討論後實作**（見下方備註），目前帶入不影響結果 |

- 不帶參數 → 回該 token 使用者群組內、`Status > Inactive` 的全部倉庫。

### Response
標準 **`APIResult<T>`**（資料在 `Data`）：

```jsonc
{
  "IsComplete": true, "Code": 0, "Message": "", "ResponseTime": "datetime",
  "Data": [
    {
      "UID": "guid",            // 倉庫 UID
      "GroupUID": "guid",       // 所屬群組（公司組織層級，非客戶）
      "ID": "string",           // 倉庫代碼
      "Name": "string",         // 倉庫名稱
      "Phone": "string", "Fax": "string",
      "Country": "string", "State": "string", "City": "string",
      "Zip": "string", "Address": "string",
      "Volume": 0,
      "Status": 1,              // WarehouseStatus
      "Description": "string",
      "Mail": "string", "Contact": "string",
      "PhotoUID": "guid|null",
      "CreatedBy": "string", "CreatedOn": "datetime|null",
      "ModifiedBy": "string", "ModifiedOn": "datetime|null"
    }
  ]
}
```
> 型別來源 `IEnumerable<IWarehouseModel>`；注意此 model 欄位名是 `Mail`，與客戶的 `Email` 不同。

### 說明 / 備註
- 既有過濾來源：`GetGroupUserViewByUser()` 取使用者群組 → `groups.Contains(WarehouseInnerModel.GroupUID) && Status>Inactive`（`WarehouseManager.cs:589-592`）。
- ⚠️ **客戶↔倉庫關聯待討論（TODO）**：`GroupUID` 是**公司組織層級**，一個組織底下有多個客戶（Party）。因此「客戶的 GroupUID → 倉庫」**不成立**，無法直接用 GroupUID 由客戶過濾倉庫。`CustomerPartyName` 經 `AbstractManager.GetCustomer(使用者群組, CustomerPartyName)` 只能解析出客戶 Party，**不含倉庫關聯**。客戶與倉庫的實際關聯方式尚未定義，需與使用者討論後再實作此過濾。
- 另有 `GET /api/Warehouse/GetWarehouseList`：回**全部倉庫**、無群組過濾、標準 `APIResult<T>`。本整合不採用。

---

## ✅ #3　`GET /api/Transload/GetProducts`（SKU 查詢 / autocomplete）

| 項目 | 內容 |
|---|---|
| **端點** | `GET /api/Transload/GetProducts` |
| **spec 對應** | §5.1.6、§7.1.1 |
| **認證** | 需 JWT token |
| **用途** | 依關鍵字（產品 ID）查產品，附包裝清單；供 SKU 查詢 / autocomplete |
| **對應既有邏輯** | `ProductController.GetProducts(keyword, customerUID)`（`ProductController.cs:74`）→ `getCacheProductList` + `getCachePackageList`（讀 Redis 快取 DrKnowAll）。新建 Transload 端點，內部複製此查詢，回應已是 `APIResult<T>` |

### Request
`GET /api/Transload/GetProducts`，Header `Authorization: Bearer <token>`：

| 參數 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `keyword` | string | ✅ | 產品 ID（Product ID）；**長度須 ≥ 3** 才查，否則回失敗 `The keyword length must larger than 3.`（空字串回 `COMMON_INCORRECT_PARAMETERS`） |
| `CustomerPartyName` | string | 選填 | **客戶代碼**（= `YAEP_Party.ID` = T2 `S_Client.ID`，方案 1）。API 內部以 `GetCustomer(使用者群組, CustomerPartyName)` 解析成客戶 UID 再過濾產品 |

### 內部解析流程（CustomerPartyName）
`CustomerPartyName`（客戶代碼）→ `AbstractManager.GetCustomer(使用者群組, CustomerPartyName)`（以 `Party.ID` 比對，`AbstractManager.cs:530/546`）→ 客戶 UID → 當作 `customerUID` 往下查產品快取。
- 找不到客戶 → **回失敗（查無客戶）**。
- 找到多筆 → 取第一筆（同群組內客戶代碼應唯一）。
- 不帶 `CustomerPartyName` → 不限客戶。

### Response
標準 **`APIResult<T>`**（資料在 `Data`）：

```jsonc
{
  "IsComplete": true, "Code": 0, "Message": "", "ResponseTime": "datetime",
  "Data": [
    {
      "ItemID": "string",     // 產品 ID（= Item.ID）
      "ItemName": "string",   // 產品名稱（= Item.Name）
      "ItemUID": "guid",      // 產品 UID
      "Package": [
        {
          "VersionID": "string",
          "ItemName": "string",   // ⚠️ 既有行為實際填的是 Item.ID（非名稱）
          "PackageName": "string",
          "PackageUID": "guid",
          "ItemUID": "guid"
        }
      ]
    }
  ]
}
```

### 說明 / 備註
- 結果依 `ItemID` 排序；每個產品帶其包裝清單（包裝依 `CreatedOn` 新到舊）。
- 客戶識別採方案 1（`CustomerPartyName` = 客戶代碼 `Party.ID`），與既有 `GetCustomer`、T2 對應 key（§10.8）一致。

---

## ✅ #4　`POST /api/Transload/AddProduct`（新增 SKU ＋預設包裝）

| 項目 | 內容 |
|---|---|
| **端點** | `POST /api/Transload/AddProduct` |
| **spec 對應** | §5.1.6、§7.1.2 |
| **認證** | 需 JWT token |
| **用途** | 以最小輸入（Sku + 客戶）新增產品，並自動建立預設三層包裝，讓後續 Receiving 可收貨 |
| **對應既有邏輯** | 產品本體：`ProductController.AddProduct`（`:207`）；分類查詢：`ItemManager.GetCategories(ItemCategoryParameters{GroupUID})`（`:446-449`）；客戶解析：`getCustomer(groupUID, Party.ID)`（`:1257`）；包裝：`handlePackageVersion`（`:1801`）+ `handlePackageUOM`（`:1817`，find-or-create UOM）+ `PackageManager.AddPackage`（`:1934/1992`，`ParentUID` 串巢狀） |

### Request（body）
| 參數 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `Sku` | string | ✅ | 產品代碼，對應 `Item.ID`；同時作為 `Name` |
| `CustomerPartyName` | string | ✅ | 客戶代碼（= `Party.ID`，方案 1） |

### 內部流程
1. `GroupUID` = `getDefaultGroupUID()`（**token 使用者第一個群組**）；空 → 失敗。
2. 解析客戶：`getCustomer(GroupUID, CustomerPartyName)`（用 `Party.ID` 比對）。**找不到或多筆 → 失敗**。取得 `customer.UID`。
3. 取分類：`GetCategories(ItemCategoryParameters{ GroupUID })` → **取第一筆**；查無分類 → 失敗。
4. 重複檢查：同 `GroupUID` + 同 `Sku`(ID) + 同客戶(`CustomerUID`) 已存在 → **失敗，訊息「產品已存在」**。
5. 建立 `ItemModel { UID=新, GroupUID, ID=Sku, Name=Sku, Status=Active, Type=1 }`，properties 寫入 `CustomerUID = customer.UID`（產品↔客戶關聯）；`manager.Create(item, properties, CategoryUID)`。
6. 建立 PackageVersion：`handlePackageVersion(item.UID)`（versionId = 產品 ID）。
7. 建立**三層巢狀包裝（PALLET → BOX → EACH）**：
   - UOM 字串一律**大寫**：`PALLET` / `BOX` / `EACH`（經 `handlePackageUOM` find-or-create UOM 主檔）。
   - 結構：`PALLET`(root, `ParentUID=null`) → `BOX`(`ParentUID=PALLET.UID`) → `EACH`(`ParentUID=BOX.UID`)，同一 `VersionUID`。
   - 各層 `Quantity = 1`；`Length/Width/Height/GrossWeight = 0`；`Type = 1`；`Status = Active`；`ID = Name = UOM 字串`。
8. 刷新快取（`RefreshProductCategoryRelation` + `RefreshProduct` + `ReloadProductPackageCache`）。
9. **交易**：步驟 5~7 包在同一交易；**任一失敗整批 rollback**（產品 + 版本 + 包裝同成功同失敗），避免半套資料。

### Response（`APIResult<T>`，資料在 `Data`）
```jsonc
{
  "IsComplete": true, "Code": 0, "Message": "", "ResponseTime": "datetime",
  "Data": {
    "UID": "guid", "GroupUID": "guid",
    "ID": "string",      // = Sku
    "Name": "string",    // = Sku
    "Description": null,
    "Status": 1,         // Active
    "Type": 1
  }
}
```

### 說明 / 備註
- 客戶識別採方案 1（`CustomerPartyName` = 客戶代碼 `Party.ID`）。
- 產品↔客戶關聯靠 ItemProperty `CustomerUID`（`parseToProductProperties`，`:936-942`），故必須成功解析客戶。
- 已含預設三層包裝，新增後即可被 `Receiving` 以 `PALLET`/`BOX`/`EACH` 任一 UOM 收貨；換算量預設 1，之後可再調整。

---

## ✅ #5　`GET /api/Transload/GetShipMethodList`（運送方式 / Carrier 下拉）

| 項目 | 內容 |
|---|---|
| **端點** | `GET /api/Transload/GetShipMethodList` |
| **spec 對應** | §5.1.8 |
| **認證** | 需 JWT token |
| **用途** | 運送方式 / Carrier（Ship via）下拉清單，可依客戶限縮 |
| **對應既有邏輯** | `ManifestController.GetShipMethodList(Guid? partyuid)`（`ManifestController.cs:474`）→ `ManifestManager.GetShipMethodList(partyuid)`，回 `APIResult<IEnumerable<IShipMethodModel>>`。新建 Transload 端點，內部把 `CustomerPartyName` 解析成 `partyuid` 後呼叫既有 BLL |

### Request
`GET /api/Transload/GetShipMethodList`，Header `Authorization: Bearer <token>`：

| 參數 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `CustomerPartyName` | string | 選填 | 客戶代碼（= `Party.ID`，方案 1）。內部 `getCustomer` 解析成 `PartyUID` 再查。**不帶 → 回全部**（`partyuid=null`）；帶了但解析失敗 → 回失敗 |

### Response（`APIResult<T>`，資料在 `Data`）
```jsonc
{
  "IsComplete": true, "Code": 0, "Message": "", "ResponseTime": "datetime",
  "Data": [
    {
      "UID": "guid",
      "PartyUID": "guid",       // 所屬客戶 Party
      "Type": 1,                // 運送形式（spec §10 規劃補 LTL/FTL）
      "MethodName": "string",
      "MethodValue": "string",
      "IsSignature": false,
      "Status": 1,
      "CreatedBy": "string", "ModifiedBy": "string",
      "CreatedOn": "datetime|null", "ModifiedOn": "datetime|null"
    }
  ]
}
```

### 說明 / 備註
- `CustomerPartyName` 選填；不帶回全部運送方式。
- 既有 BLL 參數為 `partyuid`(Guid?)，Transload 版改收 `CustomerPartyName`（跨支規則②）。
- spec §17 #10 待定案：`ShipMethod.PartyUID` 是否 NOT NULL（影響「全部」查詢語意）。

---

## ✅ #7　`POST /api/Transload/GetInventory`（庫存查詢 ＋ aging）

| 項目 | 內容 |
|---|---|
| **端點** | `POST /api/Transload/GetInventory` |
| **spec 對應** | §5.1.7、§6.6、§7.3.1 |
| **認證** | 需 JWT token |
| **用途** | 庫存查詢，含貨櫃資訊與 aging（在庫天數）；供庫存頁與 outbound By-SKU 預檢 |
| **對應既有邏輯** | `InventoryController.GetInventory(InventorySearchParameters)`（`InventoryController.cs:51`）→ `InventoryManager.GetInventory` + 產品/客戶快取 join + 包裝樹 + 配貨換算。**Transload 版為擴充**：改以「來源貨櫃」為粒度，並 join 至 Vessel/Manifest 取容器與 aging |

### 資料粒度（重要）
- 一列 = 一筆**在庫的入庫 PayLoad**（`WMS_PayLoad.Type=1` 且 `Status>0`）。
- 列 key（穩定）= **`WMS_PayLoad.UID`**（取代既有回應用 `Guid.NewGuid()` 的不穩定值）。
- 這比既有 `GetInventory`（依 倉庫/包裝/品項 彙總）**更細**，才能逐櫃顯示 ConNo / ArrivalDate / Aging。

### 關聯路徑（取容器與 aging）
`WMS_PayLoad.VesselUID` → `WMS_Vessel.UID`（取容器欄位）→ `WMS_Vessel.BolUID` → `WMS_BOL.ManifestUID` → `WMS_Manifest`（取 ManifestUID）。

### Request（body，皆選填）
| 參數 | 型別 | 說明 |
|---|---|---|
| `CustomerPartyName` | string | 客戶代碼（= `Party.ID`）→ 內部解析成 `CustomerUID` 過濾；帶了解析失敗 → 回失敗 |
| `Status` | int? | 在庫狀態 `StorageStatus`（IN_STORAGE / PARTIAL_OUT / FULLY_OUT） |
| `Key` | string? | 關鍵字，**同時比對 SKU（`Item.ID`）與 Container#（`WMS_Vessel.RefNo`）** |

> 2026-05-29 調整：request 改為 `CustomerPartyName / Status / Key` 三項；先前的 `WarehouseUID/PHierarchy/LoadingType/ContainerNo/AgingFrom-To` 過濾移除。

### Response（`APIResult<T>`，`Data` = 陣列；查無 → DataNotFound）
> 欄位命名對齊主文件 §D1。Qty 與 UOM 分兩欄。
```jsonc
{
  "IsComplete": true, "Code": 0, "Message": "", "ResponseTime": "datetime",
  "Data": [
    {
      "PayLoadUID": "guid",     // 列 key（穩定）= WMS_PayLoad.UID
      // 產品
      "SKU": "string",          // = Item.ID
      "ItemUID": "guid", "ItemName": "string", "Description": "string|null",
      // 客戶
      "CustomerUID": "guid", "CustomerID": "string", "CustomerName": "string",
      // 倉庫
      "WarehouseUID": "guid", "WarehouseName": "string",
      // 包裝 / 數量
      "PackageUID": "guid", "PackageName": "string", "PackageTree": "string",
      "UOM": "string",          // Each/Box/Pallet
      "OnHand": 0,              // 該 UOM 顯示量
      "OnHandPcs": 0,           // 件數（最小單位 Each，真值）
      "AllocatedQty": 0,        // 已配貨（件數）
      // 貨櫃（WMS_Vessel；ConNo=既有 RefNo，其餘為新欄）
      "ConNo": "string|null",       // = WMS_Vessel.RefNo
      "SealNo": "string|null",
      "ContainerSize": 0,           // int enum（20GP/40GP/40HQ/45HQ/LooseCargo/LCL/Other）
      "LoadingType": 0,             // int enum（F2F=10/F2P=20/P2P=30/Full=40）
      "StackableType": 0,           // int（0=Stackable/1=Non-Stackable）
      // 來源（outbound 用；UI 可不顯示）
      "ManifestUID": "guid", "SourceManifestItemUID": "guid",
      // 到倉 / aging
      "ArrivalDateActual": "datetime|null",  // = WMS_Vessel.ArrivalDate（收貨時 user 輸入）
      "AgingDays": 0,                        // (今天 − ArrivalDateActual).Days；無值→留空
      "FreeDays": 0,                         // T2 合約免費天數（WMS 無，本版先留空）
      "BillableStorageDays": 0,              // max(0, AgingDays − FreeDays)
      "StorageStatus": "string"              // IN_STORAGE/PARTIAL_OUT/FULLY_OUT
    }
  ]
}
```

### 實作注意 / 相依
- **DB 變更（依主文件 §3.5，容器欄位掛 Vessel）**：`WMS_Vessel` 新增 `SealNo / ContainerSize(int) / LoadingType(int) / StackableType(int) / Weight(decimal) / Volume(decimal) / ArrivalDate(datetime)`；**ConNo 用既有 `RefNo`**（不新增 ContainerNo）。需 Receiving 寫入後才有值；舊資料無值。
- **aging 日期源（2026-05-29 改用）= `WMS_Vessel.ArrivalDate`**（收貨時 user 輸入），不再用 `PayLoad.ReceivedDate`（實機幾乎全空，主文件 P2）。計算邏輯不變：經 `PayLoad(Type=1,Status>0).VesselUID → Vessel` 取 ArrivalDate，`AgingDays = (今天 − ArrivalDate).Days`，無值→留空。
- `OnHandPcs` 用既有 package tree `GetMinNode` + `GetReceivePackageUomQuantity` 換算；`OnHand` = OnHandPcs ÷ QtyPerPackage（該 UOM）。
- `StorageStatus` 由 allocated vs onhand 推。
- 列粒度 per 入庫 PayLoad，是對既有 `GetInventory` 的擴充（非薄包裝）。
- `FreeDays` / `BillableStorageDays` 需 T2 合約免費天數，本版先留空，之後接合約再補。

---

## ✅ #C1　`POST /api/Transload/GetManifestList`（入倉列表）

| 項目 | 內容 |
|---|---|
| **端點** | `POST /api/Transload/GetManifestList` |
| **spec 對應** | 主文件 §C1 |
| **認證** | 需 JWT token |
| **用途** | 入倉（Inbound）批次列表；含貨櫃數、總量、到倉日與 aging |
| **對應既有邏輯** | `ManifestController.GetManifestList(ManifestSearchParameters)`（`ManifestController.cs:62`）→ `ManifestManager.GetManifestList<ManifestListViewModel>`（標準 `APIResult<T>`）。**Transload 版需擴充**：固定 `Type=Inbound`、join `WMS_Vessel` 取 ConNo/SealNo/ArrivalDate、聚合 ContainerCount/Totals、解析 CustomerPartyName |

### Request（body，皆選填）
| 參數 | 型別 | 說明 |
|---|---|---|
| `CustomerPartyName` | string | 客戶代碼（= `Party.ID`）→ 解析 `CustomerUID` 過濾 `Manifest.PartyUID` |
| `Status` | int? | `ManifestStatus` 過濾（在固定 `Status>0` 範圍內再細分）|
| `Key` | string? | 關鍵字，**同時比對** Manifest 名稱(`Manifest.Name`)＋Container#(`Vessel.RefNo`)＋封條(`Vessel.SealNo`🆕) |

> **固定過濾（API 內部，呼叫端不可改）：`WMS_Manifest.Type = 1`（Inbound，`ManifestType.Inbound=1`）且 `Status > 0`**（排除作廢/刪除）。既有 `ManifestSearchParameters` 已有 `Customer/Type/manifestname/manifestref`，擴充 keyword 至 Vessel 欄位。

### Response（`APIResult<T>`，`Data` = 陣列）
```jsonc
{
  "IsComplete": true, "Code": 0, "Message": "", "ResponseTime": "datetime",
  "Data": [
    {
      "ManifestUID": "guid",
      "ManifestName": "string",            // = Manifest.Name
      "CustomerUID": "guid", "CustomerID": "string", "CustomerName": "string",
      "WarehouseUID": "guid", "WarehouseName": "string",
      "Description": "string|null",
      "Status": 1, "StatusName": "string",
      "CreatedOn": "datetime",
      "ArrivalDate": "datetime|null",       // = MIN(該 manifest 各 Vessel.ArrivalDate🆕)
      "AgingDays": 0,                       // today − ArrivalDate；無值→留空
      "ContainerCount": 0,                  // COUNT(該 manifest 的 WMS_Vessel)
      "TotalQty": 0,                        // 件數彙總
      "TotalCartons": 0, "TotalPallets": 0  // BLL 用 YAEP_Package 換算
    }
  ]
}
```

### 說明 / 備註
- 客戶識別用 `CustomerPartyName`（規則②）。
- `TotalCartons / TotalPallets` 由 **API BLL 層用 `YAEP_Package` 換算**（DB 無此欄）。
- `ArrivalDate / AgingDays` 依賴 `WMS_Vessel.ArrivalDate`（Task #3，Receiving 寫入）；舊資料無值 → 留空。
- 一列 = 一個入倉 Manifest（1 Manifest 下 N 個 Vessel=櫃）。
- **資料範圍固定 `Type=1`(Inbound) 且 `Status>0`**；作廢(VoidManifest 軟刪改 Status≤0)後不會出現在此列表。

---

## 🕗 #C4　`POST /api/Transload/VoidManifest`（入倉作廢）— 設計已定，待實作

| 項目 | 內容 |
|---|---|
| **端點** | `POST /api/Transload/VoidManifest` |
| **spec 對應** | 主文件 §1（Inbound 作廢，D4 軟刪）|
| **認證** | 需 JWT token |
| **用途** | 作廢一筆入倉 Manifest（軟刪）並**反向扣除該批收貨建立的在庫** |
| **狀態** | **設計已確認，先記錄、之後再實作**（2026-05-29 使用者決定）|

### Request（body）
| 參數 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `ManifestUID` | Guid | ✅ | 要作廢的入倉 Manifest UID |

### 行為（方案 A：軟刪 + inbound 庫存反扣 + 已用擋刪）
1. 驗證 Manifest 存在、`Type=1`(Inbound)、`Status>0`。
2. **已用擋刪（重要）**：若該批收貨的在庫**已被使用**（已配貨/已出貨/有下游 outbound 引用）→ **回失敗，不可作廢**（訊息如「庫存已被使用，無法作廢」）。
3. 若可作廢：
   - 軟刪 Manifest header（`Status→Void=0`）及連動 BOL / Manifest_Item / Receiver（可重用既有 `DeleteManifest`）。
   - **反向扣除 inbound 庫存（新工程，既有沒有）**：把該 Manifest 各 `WMS_Vessel` 底下 `Type=1` 的 `WMS_PayLoad` 軟刪/沖銷，並視需要軟刪 `WMS_Vessel`，使在庫歸零、不留幽靈庫存。
4. 全程交易；任一步失敗整批 rollback。

### Response
標準 `APIResult<T>`；成功 `Data=null`（或回作廢結果）；已被使用 → 失敗。

### 說明 / 備註（為何需新工程）
- 既有 `DeleteManifest` / `CancelReceiving`（`CancelReceiving→DeleteManifestByOrder→DeleteManifest`）**只軟刪 BOL/Manifest/Item/Receiver，不扣 `WMS_PayLoad` 庫存**；inbound 沒有「還原收貨庫存」邏輯（只有 outbound 的 `Deallocated` 還庫存）。直接重用會留幽靈庫存。
- 故 VoidManifest 必須**自建 inbound 庫存反扣**，並在「庫存已被使用」時擋刪。
- 此項先記錄、待 Receiving 一併規劃實作。

---

## 🕗 #C3　`POST /api/Transload/Receiving`（建立收貨）— 設計已定，待實作

| 項目 | 內容 |
|---|---|
| **端點** | `POST /api/Transload/Receiving` |
| **spec 對應** | 主文件 §C3、§6.5、§7.2.1 |
| **認證** | 需 JWT token |
| **用途** | 建立一批入倉：1 Manifest + 每櫃 1 Vessel + 上架建庫存 |
| **對應既有邏輯** | `ManifestManager.Receiving(IReceivingRequest)`（`ManifestManager.Order.Inbound.cs:32`）。既有**一次只建 1 Vessel**，Transload 需擴充成**每櫃建 1 Vessel** |
| **狀態** | **設計已確認，待實作**（2026-05-29）|

### Request（body）
```jsonc
{
  "RefNo": "string",             // 批次/Booking#（→ Manifest.Name/RefNo）
  "WarehouseUID": "guid",
  "CustomerPartyName": "string", // = Party.ID（規則②，既有 Receiving 已支援）
  "Containers": [
    {
      "ConNo": "string",         // → Vessel.RefNo
      "SealNo": "string",
      "ContainerSize": 0,        // int enum（10=20GP/20=40GP/30=40HQ/40=45HQ/50=LooseCargo/60=LCL/90=Other）
      "LoadingType": 0,          // int enum（F2F=10/F2P=20/P2P=30/Full=40）
      "StackableType": 0,        // int（0=Stackable/1=Non-Stackable）
      "ArrivalDate": "datetime", // user 輸入到倉日（→ Vessel.ArrivalDate）
      // 註：Container 不收 Weight/Volume；Vessel 的 Weight/Volume 由各 Item 加總得出
      "Items": [
        { "SKU": "string", "EnterQty": 0, "PackageUID": "guid" }
      ]
    }
  ]
}
```
- `Items[].PackageUID`：直接指定包裝（綁定 ItemUID 與包裝層）；件數 = `EnterQty × 該 PackageUID 每包裝件數`（後端查 package tree 換算）。

### 流程（facade，全程同一交易）
1. **併發防重**：`RequestManager.IsRequestProcessing(RECEIVING, RefNo)`。
2. **重複批次擋下**：同 `RefNo`+`WarehouseUID` 已有 manifest(Status>0) → 回「已存在」失敗。
3. 解析客戶：`CustomerPartyName → GetCustomer → CustomerUID`。
4. 比對品項/包裝：每個 Item 用 `SKU`(=Item.ID) + `PackageUID`；找不到 SKU / 包裝不符 → 收集清單。
5. **比對失敗 → 整批失敗**，回 `notFinditem / nothavepkg / notmatchpkg` 清單（#4 預設包裝已讓新 SKU 少見 nothavepkg）。
6. 建立：**1 `WMS_Manifest`**(Type=Inbound, Status=Open, PartyUID=CustomerUID, Name=RefNo) + **1 `WMS_BOL`**(ManifestUID=該 manifest，N 個 Vessel 共用) + **每個 Container 1 `WMS_Vessel`**(RefNo=ConNo，寫入 **5 新欄** SealNo/ContainerSize/LoadingType/StackableType/ArrivalDate，BolUID=該 BOL) + 每櫃每 SKU 1 `WMS_Vessel_Manifest`(其 Volume/Weight 由 package×qty 算，既有 `CalculateCUFT`/`CaculateTTLWeight`) + 上架建 `WMS_PayLoad`(VesselUID=該櫃, Type=1, 件數=換算結果)。
   - **Vessel 的 Weight/Volume 不存欄位**，需要時動態 `SUM(WMS_Vessel_Manifest.Volume/Weight WHERE VesselUID=… AND Status>0)`。
7. 任一步失敗 → 整批 rollback。

### Response（`APIResult<T>`）
```jsonc
{
  "IsComplete": true, "Code": 0, "Message": "", "ResponseTime": "datetime",
  "Data": {
    "ManifestUID": "guid",
    "Vessels": [ { "VesselUID": "guid", "ConNo": "string", "Status": 1 } ]
  }
}
```

### 待實作時的細節（記錄）
- **BOL 數量**：採 **1 BOL / manifest，N 個 Vessel 共用其 BolUID**（保留 PayLoad→Vessel→BOL→Manifest 鏈）。
- **實作途徑**：既有 `Receiving` 一次只建 1 Vessel；二選一—(i) 擴充核心 `ManifestManager.Receiving` 成迴圈建 Vessel、或 (ii) 新 `TransloadManager.Receiving` 重用底層 repo 自組 1-Manifest-N-Vessel。依 facade 零風險原則傾向 (ii)，實作時定。
- 與 `VoidManifest`(#C4) 對稱：作廢需反扣本次建立的 Type=1 PayLoad。
- 前端選 `PackageUID`：直接用 **`GetProducts`(#3) autocomplete 回的內嵌 `Package[]`**（含 PackageUID/PackageName）；**不另開 GetPackage**（前端不需 QtyPerPackage，件數換算由後端依 PackageUID 算）。
