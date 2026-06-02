# Transload WMS API 主文件（進 / 存 / 出 + 計費 / 報價 / Portal）

> **單一主文件**（2026-05-29 合併）。整合三部分：① 對外路由清單(master) ② UI 欄位 ↔ DB 對應 ③ WMS 實作方案。
> 其餘舊文件（需求文件、欄位對應 md/html、實作方案、舊接口文件、整體規格書 SSOT）已移至 `docs/archive/`。
> 來源程式：UI=`t2_wms_transload_ui`、WMS=`E:\Project\DC\WMS\API`(YAEP.WMS, .NET 4.6.2)、T2=`Trucking2000`。
> DB 欄位實機撈出：WMS@192.168.1.21、T2(Heptarun)@192.168.88.16。

## 0. 通用慣例

- **路由 root 統一 `/api/Transload/{Action}`**（扁平 action 命名，不用 RESTful）。新增 API 一律先進 §1 路由清單，再回填 §2/§3。
- **WMS 回應信封**：`APIResult<T>` = `{ IsComplete, Code, Message, ResponseTime, Data }`（資料在 `Data`）。
- **T2 回應信封**：`ResultModel` = `{ Success, Message, Data }`。前端 `api/*` 接 WMS 時每支加一行 adapter 把 `IsComplete→Success`。
- **認證**：Header `Authorization: Bearer {Identification}`（Phase 0 用 WMS JWT）。
- **金額** `decimal(10,4)`；件數(pieces)為整數、是唯一真值。
- **數量單位 (UOM)**：每個 SKU 的 UOM 來自 WMS 包裝版本 `YAEP_Package`，標準三層 **Each / Box / Pallet**（Each=基本件數），使用者仍可逐行改選。**Qty 與 UOM 一律拆成兩個獨立欄位**（輸入、回應、顯示皆然，不可合併成一格）；Box/Pallet 數量由 `QtyPerPackage` 換算自件數。

**欄位來源圖例**：🟩直接欄位 / 🟦join帶出 / 🟧計算聚合(DB無) / 🟥跨系統T2(WMS無) / 🆕WMS需新增

**共用 enum**

| Enum | 值 |
|---|---|
| `LoadingType` | 10=F2F / 20=F2P / 30=P2P / 40=Full Container（彩色 badge）|
| `ContainerSize` | int enum（10=20GP/20=40GP/30=40HQ/40=45HQ/50=LooseCargo/60=LCL/90=Other）|
| `StackableType` | 0=Stackable / 1=Non-Stackable |
| `StorageStatus` | IN_STORAGE / PARTIAL_OUT / FULLY_OUT |
| `OutboundStatus` | DRAFT / SHIPPED |
| `ShipMethod` | LTL / FTL |
| `UOM`(item) | Each / Box / Pallet（來自 `YAEP_Package`，Qty 與 UOM 分兩欄）|

---

# 1. 對外路由清單（master）

route root 統一 `/api/Transload/{Action}`。內部實作（重用既有邏輯或新寫）逐支於 §3。

| 章節 | API 名稱 | WMS 路由 | 狀態 |
|---|---|---|---|
| §6.3 | 客戶下拉 | `GET /api/Transload/GetCustomerList` | 邏輯已確認，待實作 |
| §5.1.6 | 倉庫下拉 | `GET /api/Transload/GetWarehouseList` | 待實作（CustomerPartyName 過濾待議）|
| §7.1.1 | SKU 查詢 | `GET /api/Transload/GetProducts` | 待實作 |
| §7.1.2 | 新增 SKU | `POST /api/Transload/AddProduct` | 待實作（含預設三層包裝）|
| §5.1.8 | Carrier/ShipMethod 下拉 | `GET /api/Transload/GetShipMethodList` | 待實作 |
| §6.5/§7.2.1 | 建立收貨 | `POST /api/Transload/Receiving` | 設計已定(Req=RefNo/Warehouse/CustomerPartyName/Containers[7欄+ArrivalDate+Items{SKU/EnterQty/PackageUID}]；每櫃建1Vessel；重複擋下/整批失敗)，待實作 |
| §6.6/§7.3.1 | 庫存查詢 | `POST /api/Transload/GetInventory` | 邏輯已釐清(detail join)，待實作 |
| §7.3.2 | 在庫明細 | `GET /api/Transload/GetInventoryDetail` | 先跳過（既有可重用，暫不規格化；2026-05-29 使用者決定）|
| §6.7/§7.4.1 | 建立出貨 | `POST /api/Transload/Allocated` | auto-assign 既有支援，待實作 |
| §7.4.2 | 出貨單 BOL | `/api/Transload/Bol*`（Create/Update/UploadPod）| 未確認 |
| §7.4.3 | 出庫拆單 | `POST /api/Transload/SaveOutboundAssignedItem` | 未確認 |
| §5.1.3 | 今日進出筆數 | `GET /api/Transload/Summary?from=&to=` | 🆕 選用 |
| §7.4.4 | by-sku 預檢 | `GET /api/Transload/GetItemAvailability` | 🆕 待建 |
| §7.5.1 | 可計費事件(S2S) | `GET /api/Transload/BillableEvents` | 🆕 待建 |

**UI 列表/作廢額外需要（待併入上表）**

| 對應 UI | WMS 路由 | 狀態 |
|---|---|---|
| Inbound 列表 | `POST /api/Transload/GetManifestList` | 邏輯已確認，待實作（擴充既有 GetManifestList；Req=CustomerPartyName/Status/Key，固定 Type=1(Inbound) 且 Status>0）|
| Inbound 作廢 | `POST /api/Transload/VoidManifest` | 設計已定，待實作（Req=ManifestUID；軟刪 header+**自建 inbound 庫存反扣**；庫存已被使用則擋刪）|
| Outbound 列表 | `GET /api/Transload/GetBOLList` | 🆕 待補（屬 Bol*）|
| Outbound 作廢 | `POST /api/Transload/VoidShipment` | 🆕 待補（軟刪，D4）|

> Customer 頁（客戶列表）打 **T2** `POST /client/search/customer`，不在 WMS `/api/Transload` 範圍。

**跨支規則**
1. ✅ 回應統一 `APIResult<T>`。
2. ✅ 客戶識別統一用 `CustomerPartyName`（字串＝`Party.ID`=`S_Client.ID`，API 內解析成 UID）。(2026-05-29 定案；各支 Request 一律收 CustomerPartyName，不收 CustomerUID)
3. ⏳ 客戶↔倉庫關聯：`GroupUID` 是公司組織層級（非客戶），無法用客戶 GroupUID 過濾倉庫；`GetWarehouseList` 的 CustomerPartyName 過濾待此關聯定義。

---

# 2. UI 元件 ↔ API ↔ DB 欄位對應

## A. 共用下拉（Inbound/Outbound filter + 表單）

### A1. 客戶下拉 `GET /api/Transload/GetCustomerList`（WMS）
| 欄位 | 對應 DB | 類型 | 說明 |
|---|---|---|---|
| `UID` | `YAEP_Party.UID` | 🟩 | |
| `ID` | `YAEP_Party.ID` | 🟩 | =T2 S_Client.ID |
| `Name` | `YAEP_Party.Name` | 🟩 | |
| `FreeDays` | `S_Contract_Demurrage.Free_Days`/`S_Client.Free_Days` | 🟥 | T2，WMS 無 |

### A2. 倉庫下拉 `GET /api/Transload/GetWarehouseList`（WMS）
`UID`→`WMS_Warehouse.UID`🟩、`ID`→`WMS_Warehouse.ID`🟩、`Name`→`WMS_Warehouse.Name`🟩

### A3. SKU 下拉 `GET /api/Transload/GetProducts`（WMS）
Request：`CustomerPartyName`(選填,→`Party.ID` 解析成 UID)、`keyword`(→`YAEP_Item.ID`，≥3 字)
| 欄位 | 對應 DB | 類型 |
|---|---|---|
| `UID` | `YAEP_Item.UID` | 🟩 |
| `ID`(SKU) | `YAEP_Item.ID` | 🟩 |
| `Name` | `YAEP_Item.Name` | 🟩 |
| `CustomerUID` | item↔party 在 DrKnowAll 快取/Core.Item property(非 WMS DB 欄位, 見 P7) | 🟦 |
| `PcsPerCarton` | `YAEP_Package.Quantity`(carton 層) | 🟦 |
| `CartonsPerPallet` | `YAEP_Package.Quantity`(pallet÷carton) | 🟧 |
| `Packages[]` | `YAEP_Package`×`YAEP_Package_UOM` | 🟦 |

> `Packages[]` = UOM 下拉來源，每筆 `{ PackageUID, UOM_ID, UOM_Name, QtyPerPackage }`，標準三層 Each(1)/Box(=PcsPerCarton)/Pallet(=PcsPerCarton×CartonsPerPallet)。前端輸入「Qty + UOM」後以 `QtyPerPackage` 換算成件數。

## B. Customer 頁 `POST {T2}/client/search/customer`（T2，已接真 API）
Request：`UID`/`ID`/`SCAC` → filter `S_Client.UID`/`.ID`/`.SCAC`
| 欄位 | 對應 DB | 類型 |
|---|---|---|
| `UID` | `S_Client.UID` | 🟩 =Client_UID |
| `ID` | `S_Client.ID` | 🟩 |
| `Name` | `S_Client.Name` | 🟩 |
| `Phone` | `S_Client.Phone` | 🟩 |
| `Email` | `S_Client.Email` | 🟩 |
| `Status` | `S_Client.Status` | 🟩 |

## C. Inbound（進）

### C1. 入倉列表 `POST /api/Transload/GetManifestList`（WMS）
Request：`customerUID`(→`WMS_Manifest.PartyUID`)、`keyword`(→`Manifest.Name`/`Vessel.RefNo`(ConNo)/`Vessel.SealNo🆕`)、`status`(→`Status`)
| 欄位 | 對應 DB | 類型 |
|---|---|---|
| `ManifestUID` | `WMS_Manifest.UID` | 🟩 |
| `ManifestName` | `WMS_Manifest.Name` | 🟩 |
| `CustomerUID` | `WMS_Manifest.PartyUID` | 🟩 |
| `CustomerName` | `YAEP_Party.Name` | 🟦 |
| `WarehouseUID` | `WMS_Manifest.WarehouseUID` | 🟩 |
| `WarehouseName` | `WMS_Warehouse.Name` | 🟦 |
| `ArrivalDate` | `MIN(WMS_Vessel.ArrivalDate🆕)`(user 輸入) | 🟧 |
| `Description` | `WMS_Manifest.Description` | 🟩 |
| `Status` | `WMS_Manifest.Status` | 🟩 |
| `CreatedOn` | `WMS_Manifest.CreatedOn` | 🟩 |
| `AgingDays` | today−`MIN(WMS_Vessel.ArrivalDate🆕)` | 🟧 |
| `containerCount` | COUNT(`WMS_Vessel` under manifest) | 🟧 |
| `totalQty/Cartons/Pallets` | SUM / YAEP_Package 推 | 🟧 |

### C2. Container 結構（展開/編輯）— **Vessel=實體櫃**，1 Manifest → N Vessel
| 欄位 | 對應 DB | 類型 |
|---|---|---|
| `VesselUID` | `WMS_Vessel.UID`（=櫃，1 manifest 下 N 筆）| 🟩 |
| `ConNo` | `WMS_Vessel.RefNo` | 🟩 |
| `SealNo` | `WMS_Vessel.SealNo` | 🆕 |
| `ContainerSize` | `WMS_Vessel.ContainerSize` | 🆕 |
| `LoadingType` | `WMS_Vessel.LoadingType` | 🆕 |
| `StackableType` | `WMS_Vessel.StackableType` | 🆕 |
| `Weight` | `SUM(WMS_Vessel_Manifest.Weight)`（該 vessel，Status>0）| 🟧 |
| `Cbm` | `SUM(WMS_Vessel_Manifest.Volume)`（該 vessel，Status>0）| 🟧 |
| `TotalQty` | SUM(該 vessel 的 `WMS_Vessel_Manifest.Qty`) | 🟧 |
| `Cartons/Pallets` | YAEP_Package 推 | 🟧 |
| `Skus[].ItemUID` | `WMS_Vessel_Manifest.ItemUID`(連 `Manifest_Item_List`) | 🟩 |
| `Skus[].SKU/Name` | `YAEP_Item.ID/.Name` | 🟦 |
| `Skus[].EnterQty` | 使用者輸入數量(該 UOM 下) | 🟩 |
| `Skus[].UOM` | 收貨包裝版本(Each/Box/Pallet) | 🟩 |
| `Skus[].Qty` | =EnterQty×QtyPerPackage(件數,真值) | 🟧 |

> Qty 與 UOM 分兩欄；`Qty`(件數) 由 `EnterQty`+`UOM` 換算，寫進 `Manifest_Item_List.PackageQty`(對應 PackageUID 包裝層)。

### C3. 建立入倉 `POST /api/Transload/Receiving`（擴充 `Order/Receiving`）
寫入：1 批次→1 `WMS_Manifest`(PartyUID=CustomerUID 經 CustID 對應, WarehouseUID, Name=ManifestName) + **每櫃 1 `WMS_Vessel`**(RefNo=ConNo, SealNo🆕, ContainerSize🆕, LoadingType🆕, StackableType🆕, **ArrivalDate🆕=user 輸入到倉日**；Weight/Volume 不存、動態 SUM(Vessel_Manifest)) + 每櫃每 SKU 1 筆 `WMS_Vessel_Manifest`(連 `WMS_Manifest_Item_List`) → 上架建 `WMS_PayLoad`(VesselUID=該櫃)。
> ⚠️既有 `Receiving` 一次只建 1 Vessel，facade 需擴充成「每櫃建 1 Vessel」。
Response：`{ ManifestUID, Vessels:[{VesselUID,ConNo,Status}] }`。雙寫 T2 由 SPA 接呼。

## D. Inventory（存）

### D1. 庫存列表 `POST /api/Transload/GetInventory`（WMS）
Request：`CustomerPartyName`(→Party.ID 解析 UID)／`Status`(StorageStatus)／`Key`(關鍵字，同時查 SKU=`Item.ID` 與 Container#=`Vessel.RefNo`)（2026-05-29 定）
| 欄位 | 對應 DB | 類型 |
|---|---|---|
| `ItemUID` | `WMS_Inventory.ItemUID` | 🟩 |
| `SKU/ItemName` | `YAEP_Item.ID/.Name` | 🟦 |
| `CustomerUID` | `WMS_Manifest.PartyUID`(via PayLoad→Vessel→BOL→Manifest) | 🟦 |
| `CustomerName` | `YAEP_Party.Name` | 🟦 |
| `WarehouseUID` | `WMS_Inventory.WarehouseUID` | 🟩 |
| `WarehouseName` | `WMS_Warehouse.Name` | 🟦 |
| `ConNo` | `WMS_Vessel.RefNo`（via `PayLoad(Type=1).VesselUID`）| 🟦 |
| `LoadingType` | `WMS_Vessel.LoadingType` | 🆕🟦 |
| `OnHandPcs` | `WMS_Inventory.Qty` − 已配（件數,真值）| 🟧 |
| `UOM` | 收貨包裝版本(Each/Box/Pallet) | 🟩 |
| `OnHand` | =OnHandPcs÷QtyPerPackage(該 UOM 顯示量)| 🟧 |
| `AllocatedQty` | SUM(WorkOrder_Payload/配貨) | 🟧 |
| `Cartons/Pallets` | YAEP_Package 推（僅供 KPI 聚合）| 🟧 |
| `ArrivalDateActual` | `WMS_Vessel.ArrivalDate🆕`(via PayLoad→Vessel) | 🆕🟦 |
| `AgingDays` | today−ArrivalDateActual | 🟧 |
| `FreeDays` | `S_Contract_Demurrage.Free_Days` | 🟥 |
| `BillableStorageDays` | max(0,Aging−FreeDays) | 🟧 |
| `StorageStatus` | allocated vs onhand | 🟧 |

> UI 列表把 **On Hand(該 UOM 量) / UOM / Pieces(Each)** 顯示成三個獨立欄位（Qty 與 UOM 分開），不顯示 Cartons/Pallets 欄（僅後端聚合用）。

### D2. KPI（前端聚合 D1）：SUM(OnHandPcs)/SUM(Cartons)/SUM(Pallets)/COUNT(Aging>30) 🟧
### D3. 可出量 `GET /api/Transload/GetItemAvailability`：`{ SKU, TotalOnhand🟧, Allocations[]{ConNo🟦(Vessel.RefNo),Onhand🟧,ArrivalDateActual🟦(Vessel.ArrivalDate🆕),AgingDays🟧} }`（僅顯示用；實際出貨配櫃由既有引擎 Sequence 決定，見 P5）

## E. Outbound（出）

### E1. 出貨列表 `GET /api/Transload/GetBOLList`（WMS）
| 欄位 | 對應 DB | 類型 |
|---|---|---|
| `VesselUID` | `WMS_Vessel.UID` | 🟩 |
| `BolUID` | `WMS_BOL.UID` | 🟩 |
| `ShipOrderNo` | `WMS_Vessel.RefNo` | 🟩 |
| `CustomerUID` | `WMS_Vessel_Manifest.PartyUID` | 🟩 |
| `CustomerName` | `YAEP_Party.Name` | 🟦 |
| `WarehouseUID/Name` | `WMS_Manifest.WarehouseUID`→`WMS_Warehouse.Name` | 🟦 |
| `ShipDate` | `WMS_BOL.ETA`(既有出貨寫 ETA=ETD；非 DeliveryDate) | 🟩 |
| `Carrier` | `WMS_ShipMethod.MethodName`(via BOL.ShipMethodUID) | 🟦 |
| `TrackingNumber` | `WMS_BOL.TrackingNumber` | 🆕 |
| `BolNo` | `WMS_BOL.ID/.RefNo` | 🟩 |
| `ShipMethod` | `WMS_ShipMethod.MethodName`(LTL/FTL) | 🟦 |
| `StackableType` | `WMS_Vessel.StackableType` | 🆕 |
| `ShipToName` | `WMS_BOL.Contact` | 🟩 |
| `Address/City/State/Zip/Country` | `WMS_BOL.ShipToAddress/City/State/Zip/Country` | 🟩 |
| `Status` | `WMS_BOL.Status`/`WMS_Vessel.Status` | 🟩 |
| `SourceCons` | 來源櫃 `WMS_Vessel.RefNo`(via 來源 PayLoad/Vessel) | 🟧 |
| `ShippedQty/Cartons/Pallets` | SUM(`WMS_Vessel_Manifest.Qty`)/推 | 🟧 |
| `ServiceLines[]` | `S_Order_Container_Leg_Journal` | 🟥 T2 |

### E2. Items（拆單後）
`ItemUID`→`WMS_Vessel_Manifest.ItemUID`🟩、`SKU/Name`→`YAEP_Item`🟦、`SourceConNo`→來源櫃`WMS_Vessel.RefNo`🟦、`QtyToShip`→`WMS_Vessel_Manifest.Qty`🟩、`Cartons/Pallets`→推🟧

### E3. 建立出貨 `POST /api/Transload/Allocated`（+ `Bol*` + `SaveOutboundAssignedItem`）
By-SKU 送入：每行 `{ SKU, RequestedQty, Unit }`（**Qty 與 UOM 分兩欄**；使用者不選來源櫃）→ 後端用 `QtyPerPackage` 換算成件數 → **既有 auto-assign 引擎自動跨櫃配貨**(排序 `HomeAddress.Sequence`，非 FIFO) → 寫 `WMS_Vessel_Manifest` + 扣 `WMS_Inventory.Qty` + `AddBol`(+TrackingNumber🆕)。Response：`{ VesselUID, BolUID, ShipOrderNo, Items[] }`（Items 內 `QtyToShip` 為各來源櫃件數，`SourceCons` 回填）。雙寫 T2 由 SPA 接呼。

## F. Warehouse / Login
| 頁 | API → DB |
|---|---|
| Warehouse | WMS `GET /api/Transload/GetWarehouseList` → `WMS_Warehouse`（唯讀；UI 已接，mock 用實機 14 筆）|
| Login | T2 `auth/internal/login` / `auth/ext/login`（`Core_User` via CA）；Bearer=`Identification` |

---

# 2T. T2 端頁面（Dashboard / Quotes / Billing / Portal）

> 這四頁主要打 **T2**（host 不同，route `/{T2}/api/transload/*`，信封 `ResultModel{Success,Message,Data}`）。UI 已用 mock 實作（`src/api/{dashboard,quotes,billing,portal}.js` + `mock/extras.js`），下方為換真 API 的契約。計費對應既有 T2 表（不另建鏡像）。

## 2T.0 共用 T2 路由（新增）
| API 名稱 | T2 路由 | UI 頁 |
|---|---|---|
| 我的客戶身分 | `GET /api/transload/me` | Login(客戶)/Portal |
| 報價單清單/單筆/生效中 | `GET /api/transload/quotes`、`/quotes/{uid}`、`/quotes/active` | Quotes |
| 建/改/停用報價 | `POST/PUT /api/transload/quotes`、`/quotes/{uid}/deactivate` | Quotes |
| Service 項/合約價 | `GET /api/transload/service-items`、`/contract-prices` | Quotes |
| 可計費事件 | `GET /api/transload/billing-events`、`POST /billing-events/sync`、`/confirm` | Billing |
| 即建 SO（雙寫） | `POST /api/transload/orders/inbound`、`/orders/outbound`、`/orders/ensure`(fallback) | Inbound/Outbound |
| 開發票/清單 | `POST /api/transload/invoices/generate`、`GET /invoices` | Billing |
| 出貨申請 | `POST /api/transload/shipment-requests`、`GET /…?customerUID=`、`/{uid}/approve\|reject` | Portal |

## 2T.1 Dashboard（聚合，前端組）
KPI：`inboundToday`🟧 / `activeContainers`🟧 / `onHandPallets`🟧 / `onHandCartons`🟧 / `agingOverFree`(Aging>FreeDays)🟧 / `uninvoiced`(T2 未開票🟥)。
各客戶彙總列：`code/name/containers/pallets/cartons/pieces/agingOverFree/uninvoiced`。來源＝WMS GetInventory 聚合 + T2 `billing-events?billingStatus=unbilled` 加總。

## 2T.2 Quotes（報價單 / Rate Card）
表頭 `UID/QuoteNo/Name/CustomerUID(+Name/Code)/EffectiveDate/ExpiryDate/Currency/Status` → 既有 `S_Rate_Extra_Contract`(Apply_Type=99 識別 transload)🟥。
費率 **19 欄** → 新表 `S_Quote_Item`(1 service=1 列)🟥；UI 分 5 群組：
- Storage：`FreeDays/StoragePerPallet/StoragePerContainer/StoragePerCarton/StoragePerPiece`
- F2F：`F2fUnload/F2fSort/F2fHandling/F2fReload`
- F2P：`F2pUnload/F2pPalletize/F2pPallet/F2pWrap/F2pLabel`
- P2P：`P2pForklift/P2pStorage/P2pLoading`
- Outbound：`HandlingFee/ShippingFee`

## 2T.3 Billing（可計費 → 開發票）
列表（`GET billing-events`）依 **客戶 → ChargeType(10 Receiving/20 Transload/30 Storage/40 Outbound)** 分組；每列 `UID/CustomerUID/ConNo/ChargeCode/ChargeName/ChargeType/Description/Qty/Rate/Amount/Currency/BillingStatus/ServiceDate` → 既有 `S_Order_Container_Leg_Journal`🟥。`BillingStatus`：0草稿/10可開/20已推/30確認/40已開票/90取消（unbilled=≠40）。
開發票 `POST invoices/generate {CustomerUID, ChargeUIDs[], InvoiceDate, SendEmail}` → 既有 `InvoiceContext.CreateInvoice` → `S_Invoice`+`S_Invoice_Line_Item`，leg journal `Remain_Amount` 歸 0🟥。
Invoice 列：`UID/InvoiceID/CustomerUID/InvoiceDate/OverdueDate/Amount/BalanceAmount/Status(0/10/20/90)/EmailSent`。

## 2T.4 Portal（客戶自助）
身分：`me` 由 `Core_User.Account ↔ S_Client.Account_ID` 反查 Client_UID🟥；以下查詢一律以該 Client 過濾。
- 我的在庫：WMS `GetInventory`(JWT 強制過濾客戶)✏️，欄位同 §D1（On Hand/UOM/Pieces 三欄）。
- 出貨申請(by-sku)：`POST shipment-requests {Client_UID, Mode:'by-sku', RequestedDate, ShipTo, Notes, Items:[{ItemUID,SKU,QtyRequested(pcs),RequestedQty,Unit}]}` → 既有 `S_Order`(Type=200)🟥；UI 送出前用 `GetItemAvailability` 做 FIFO 配櫃預覽。
- 我的發票：`GET invoices?customerUID=`🟥。

---

# 3. WMS 實作方案

## 3.1 架構定位
- 開**獨立 `TransloadController`**（`[RoutePrefix("api/Transload")]`），沿用既有 `ThirdPartyController` 的外部+JWT+factory 姿態，但它有寫入，故為兄弟而非子集。
- 定位 **Facade/Adapter**：寫入全**重用既有 Manager**（`IOrderManager.Receiving/.Allocated`、`IBolManager.AddBol`、`IInventoryManager.GetInventory`）；Transload 層只做形狀轉接 + transload 投影(aging/FreeDays/StorageStatus/FIFO)。對核心出貨流程零風險。
- 用 partial controller 檔（`.Inbound/.Inventory/.Outbound/.Common`），比照 `OrderController.Inbound/Outbound`，**不用 Areas**。

## 3.2 既有架構（落地依據）
| 項目 | 內容 |
|---|---|
| 基底 | `AbstractApiController`（`InitDIRoot()`/`GetSuccessResult`/`DIContainer`）|
| DI | Unity，`DIRoot` 註冊 `ManifestFactory/InventoryFactory/WarehouseFactory`(Lazy)|
| 認證 | 全域 `[Authentication]` JWT + `[EnableCors *]` |
| 收貨 | `OrderController.Inbound.cs`→`IOrderManager`(`ManifestManager.Order.Inbound.cs`)→Manifest/ItemList/Payload repo |
| 出貨 | `OrderController.Outbound.cs`→`ManifestManager.Order.Outbound.cs`(已有 `AllocateType`)→WorkOrder/Ticket repo |
| 庫存 | `InventoryController.cs`→`IInventoryManager`；已算 onhand−allocated |
| BOL | `BolController.cs`→`IBolManager`(`ManifestManager.Bol.cs`)|
| 既有範式 | `ThirdPartyController`：唯讀、`InitDIRoot()`、`DrKnowAll` 快取 |

## 3.3 新增檔案
```
Controllers/TransloadController.{Common,Inbound,Inventory,Outbound}.cs  [RoutePrefix("api/Transload")]
Models/Request/Transload*.cs  Models/Response/Transload*.cs
Interfaces/Manager/ITransloadManager.cs
BLL/Manager/TransloadManager.{,Inbound,Inventory,Outbound}.cs  (注入既有 IOrderManager/IInventoryManager/IBolManager)
DAL/Repository/TransloadRepository.cs  (新查詢：庫存 detail join 取 ConNo/客戶/aging、每櫃彙總)
DI.Agent: 註冊 ITransloadManager→TransloadManager
```

## 3.4 重點落地
- **Receiving**：1 Manifest + **每櫃建 1 Vessel**(RefNo=ConNo, 容器欄位掛 Vessel) + 每item一筆 Vessel_Manifest（既有 Receiving 一次只建 1 Vessel，需擴充成迴圈）。
- **GetInventory**：用 detail-style join（`Inventory→PayLoad(Type=1)→Vessel→BOL→Manifest`）取 ConNo/客戶/LoadingType + 補 Aging/FreeDays/BillableStorageDays/StorageStatus 投影。
- **Allocated(出貨)**：**直接呼既有 auto-assign**（給 SKU+量、引擎自動跨櫃挑貨，排序用既有 `HomeAddress.Sequence`）→ `AddBol`。**不自訂 FIFO**（P5 定案）。
- aging：today − `WMS_Vessel.ArrivalDate`(user 輸入)；totals 動態 SUM。

## 3.5 DB 變更（對齊 spec §0.1 v1.3；**容器欄位掛 Vessel**）
| 表 | 加欄位 |
|---|---|
| `WMS_Vessel` | `SealNo nvarchar`,`ContainerSize int`,`LoadingType int`,`StackableType int`,**`ArrivalDate datetime`**(user 輸入到倉日, aging 用)；**ConNo 用既有 `RefNo`**（不另加 ContainerNo）；**Weight/Volume 不加欄 → 動態 `SUM(WMS_Vessel_Manifest.Weight/Volume)`**（容器=Vessel）|
| `WMS_BOL` | `TrackingNumber varchar`,`PODUrl varchar` |
| `WMS_ShipMethod` | 資料補 `LTL`/`FTL` 兩筆 |
| `WMS_Manifest` | （選）`Batch_Ref` — 若需批次名；否則用 `Name` |

> aging 日期源**已定**（P2/D6）：用 `WMS_Vessel.ArrivalDate`（收貨時 user 輸入）；aging = today − ArrivalDate。

## 3.6 待拍板決策
- ~~D1 batch vs 每櫃~~ **已決**：1 `WMS_Manifest`(批次) + N `WMS_Vessel`(櫃)，容器屬性掛 Vessel（vessel=櫃，使用者確認）。
- **D2 FreeDays 來源**（T2）：Phase 0 暫用預設、Phase 1 由 T2 帶。
- **D3 認證**：Phase 0 WMS JWT / Phase 3 T2 SSO。
- **D4 作廢**：軟刪改 Status。
- ~~D5 客戶識別~~ **已決**：用相同 CustID(`Party.ID=S_Client.ID`)，Phase 0 兩邊手動建。
- ~~D6 aging 日期源~~ **已決**：user 收貨輸入到倉日 → `Vessel.ArrivalDate🆕`。
- ~~P5 出貨排序~~ **已決**：用既有引擎 `HomeAddress.Sequence`，不自訂 FIFO（UI 文案要去掉 FIFO 字眼）。
- ~~D7 客戶匯入~~ **已決**：手動，不做自動 migration。

## 3.7 施作順序
骨架(`TransloadController`+`ITransloadManager`+DI+`GetManifestList`) → 存(`GetInventory`+`GetItemAvailability`,先驗 adapter) → 進(加3欄+`Receiving`facade) → 出(`GetBOLList`+`Allocated`+`Bol`) → 共用下拉 → 前端 `api/*` 逐支換真。

---

# 4. 風險與問題（2026-05-29 實機比對 DB+程式+UI）

| # | 嚴重度 | 問題 | 實證 | 影響 / 對策 |
|---|---|---|---|---|
| **P1** | ✅ 已定策略 | **客戶識別** | WMS `YAEP_Party` 與 T2 `S_Client` 各自獨立 | **決策(2026-05-29)**：用**相同 CustID** 識別(`YAEP_Party.ID = S_Client.ID` 字串)；Phase 0 **兩邊手動建**對應客戶（不做自動 migration）。⚠️營運注意：新客戶上線時須先在兩系統建好同 ID，否則收貨會找不到 Party |
| **P2** | ✅ 已定策略 | **入庫時間/aging 來源** | 既有收貨不寫到倉日；`ReceivedDate` 幾乎全空 | **決策(2026-05-29)**：到倉日改由 **user 收貨時自行輸入**；facade 寫入 `WMS_Vessel.ArrivalDate🆕`(每櫃)。aging = today − `Vessel.ArrivalDate`。不再依賴自動 ReceivedDate |
| **P3** | 🟡 Med（已釐清，vessel=櫃） | **收貨「每櫃一 Vessel」需擴充** | 確認 **Vessel=實體櫃**(裝箱清單分配到各 vessel)；但既有 inbound `Receiving` **一次只建 1 個 Vessel**(RefNo=`"Vessel "+BookingNo`)，非每櫃一個 | 模型 = **1 `WMS_Manifest`(批次) + N `WMS_Vessel`(櫃) + 每item一筆 `WMS_Vessel_Manifest`**。Transload 收貨 facade 要**每櫃建 1 Vessel**(RefNo=ConNo)、容器欄位掛 Vessel（D1 改判：採此模型，非每櫃一 manifest）|
| **P4** | 🟡 Med（要重寫，做得到） | **庫存→櫃號要走 detail join** | 既有**聚合** `GetInventory` GROUP BY item/package 不帶 ConNo/客戶；但 `GetInventoryDetail` 的 SQL 已能回推：`Inventory.SlotUID+ItemUID → PayLoad(Type=1).VesselUID → Vessel.RefNo → BOL → Manifest`(回應有 `VesselRef`) | ConNo **拿得到**。Transload `GetInventory` 改用 detail-style join 取 ConNo/客戶(`Manifest.PartyUID→Party`)/LoadingType(Vessel新欄)；注意 PayLoad 僅 140 distinct vessel，舊資料覆蓋可能不全 |
| **P5** | ✅ 已定策略 | **跨櫃配貨用既有引擎排序** | 既有 `FullAllocatedPlanner`/auto-assign 自動跨櫃挑貨 ✅；排序依 `HomeAddress.Sequence` | **決策(2026-05-29)**：**用既有引擎 Sequence 排序，不自訂 FIFO**。後端跨櫃配貨直接呼既有 auto-assign。⚠️UI Outbound「Items (FIFO across containers)」字樣名實不符，**前端文案要改**（去掉 FIFO 字眼，改「auto across containers」之類）|
| **P6** | 🟡 Med | **缺欄位** | `WMS_Vessel` 無 SealNo/ContainerSize/LoadingType/StackableType/Weight/Volume（容器屬性）；`WMS_BOL` 無 Tracking/PODUrl；`WMS_ShipMethod` 無 LTL/FTL 資料 | 見 §3.5 DDL，需先補（容器欄位掛 Vessel）|
| **P7** | 🟢 已釐清 | **SKU↔客戶在 App 層、非 DB 欄位** | WMS DB 無 item↔customer 欄位/表；`YAEP_Item.GroupUID` 只 2 個 group 且非 Party。**但** product↔客戶關聯存在 **DrKnowAll 快取 + 外部 `YAEP.Core.Item` property**，既有 `ThirdParty.GetProducts?customerUID=` production 已能按客戶過濾 | SKU 按客戶過濾 **runtime 可行**，Transload 重用既有 GetProducts 即可（SQL 追不到屬正常）。⚠️新 SKU 須經 `AddProduct(帶 CustomerUID)` 建立才綁客戶 |
| **P8** | 🟡 Med | 信封不一致 | WMS `APIResult{IsComplete}` vs 前端 `{Success}` | 前端 adapter 一行 |
| **P9** | 🟢 Low | BOL 欄位細節 | 無獨立 ShipToName(用 `Contact`)；出貨日既有寫 `ETA=ETD`(非 DeliveryDate) | doc 的 ShipDate 對應改 `WMS_BOL.ETA` |
| **P10** | 🟢 Low | RefNo 不唯一 | 同 RefNo 多 manifest | 查詢/作廢別用 RefNo 當唯一鍵，用 UID |

> 補充：WMS 為**運行中系統**（inbound manifest 345、outbound 3744、inventory 在庫 52k 筆），更佐證 Transload 必須走 **facade 隔離**、不可改動核心流程。
> **更新（2026-05-29 三次釐清）**：P1/P2 **已定策略**（P1 同 CustID 兩邊手動建；P2 user 輸入到倉日存 `Vessel.ArrivalDate`）。P3/P4/P5 經 vessel=櫃 釐清皆非 blocker。**已無 blocker、決策皆已定**。剩餘純實作：P6 加 Vessel/BOL 欄位、P8 前端 adapter、前端改 Outbound「FIFO」文案（P5 用既有 Sequence 排序）。（P7 SKU 按客戶過濾 runtime 可行）
