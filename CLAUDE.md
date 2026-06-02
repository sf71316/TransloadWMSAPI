# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> 這是單一倉庫 (monorepo) 的**後端**子專案。倉庫整體結構見上層 `../CLAUDE.md`；對應的前端（Transload UI）在 `../Frontend/`（見 `../Frontend/CLAUDE.md`）。前端透過 `POST /api/Transload/*` 呼叫本專案的 `YAEP.WMS.API/Controllers/TransloadController.cs`；前後端對接的事實來源在本目錄的 `docs/`。

## 專案概觀

YAEP WMS（倉儲管理系統）後端，主要產出物是 `YAEP.WMS.API`：一個以 IIS / IIS Express 裝載的 ASP.NET Web API（.NET Framework 4.6.2、ASP.NET Web API + MVC 5.2.7、C# 7.0）。資料層使用 Dapper（透過外部 `YAEP.Data.ORM` 包裝），快取使用 Redis，分散式追蹤使用 OpenTelemetry + Jaeger，記錄使用 NLog，相依性注入使用 Unity。

- **版本控制是 SVN（`.svn/`），不是 git。** 不要假設有 git 工作流程。
- 解決方案中許多 `YAEP.*` 相依性是 `ExternalDll/` 下的**預編譯 DLL**（以 `HintPath` 參考），原始碼不在本儲存庫內，無法閱讀或修改：`YAEP.Data.ORM*`、`YAEP.Identities.*`、`YAEP.Core.Party.*`、`YAEP.Core.Item.*`、`YAEP.Package.*`、`YAEP.SSO.*`、`YAEP.Cache`、`YAEP.Templates.DI.Unity`、`YAEP.Utilities` 等。

## 建置、執行與測試

NuGet 使用舊式 `packages.config`，請以 **MSBuild**（非 `dotnet build`）建置。本機 MSBuild 隨 Visual Studio 18 (Enterprise) 安裝，路徑為：

```
C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe
```

（其他機器可用 `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -find "MSBuild\**\Bin\MSBuild.exe"` 找出路徑。）

`packages/` 通常已還原（約 109 個套件），可直接建置。以下為本機**實測可用**的建置指令（PowerShell）：

```powershell
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
# 完整重建整個方案（從原始碼編譯所有 16 個專案）
& $msbuild "WMS_2.sln" /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /m
# 增量建置
& $msbuild "WMS_2.sln" /t:Build /p:Configuration=Debug
```

> 若套件未還原，先執行 `nuget restore WMS_2.sln`（`packages.config` 專案不支援 `msbuild /t:restore` 或 `dotnet restore`，必須用 `nuget.exe`）。
>
> 實測 Rebuild 結果為 **0 個錯誤、約 93 個警告**；警告皆為非致命的組件版本衝突（`MSB3277`/`MSB3247`，如 `Newtonsoft.Json` 11 vs 13 及 `System.*` runtime 組件的 binding redirect），不影響產出。

- **執行 API**：用 Visual Studio 2022 開啟 `WMS_2.sln`，將 `YAEP.WMS.API` 設為啟動專案，以 IIS Express 執行（HTTP `http://localhost:8081/`、SSL 埠 44364）。API 路由模板為 `api/{controller}/{action}/{id}`；`/Help` 頁面（HelpPage Area）提供 API 文件。
- **背景服務**：`YAEP.WMS.NotificationSender.Host` 是 Console 程式，以 Timer 每 6 秒輪詢執行 `SenderAgent` 發送通知；需獨立啟動。
- **測試**：唯一測試專案為 `YAEP.WMS.Cache.Tests`（MSTest，`MSTest.TestAdapter` 1.3.2）。在 VS Test Explorer 執行，或用 `vstest.console.exe`：

```powershell
vstest.console.exe YAEP.WMS.Cache.Tests\bin\Debug\YAEP.WMS.Cache.Tests.dll
# 單一測試：
vstest.console.exe YAEP.WMS.Cache.Tests\bin\Debug\YAEP.WMS.Cache.Tests.dll /Tests:方法名稱
```

> 注意：測試會連線真實的 Redis / DB（見 `Cache.Tests/ConnectionSettings.cs`、`App.config`），並非純單元測試。

## 分層架構與請求流程

解決方案資料夾依層級分組：`0_Web`(API)、`1_interface`、`2_Model`、`3_BLL`、`4_Repository`、`5_Common`、`6_Cache`。一次請求由上而下穿過：

```
Controller (YAEP.WMS.API/Controllers)
   → DIRoot + Factory (YAEP.WMS.DI.Agent)        ← Unity 容器，組裝該次請求的相依性
   → Manager / BLL (YAEP.WMS.BLL/Manager, Module)  ← 商業邏輯、交易、快取
   → Repository / DAL (YAEP.WMS.DAL/Repository)     ← 資料存取
   → IObjectRelationalMappingLayer = DbEntities     ← Dapper 包裝 (外部 YAEP.Data.ORM)，SQL Server
```

介面定義集中在 `YAEP.WMS.Interfaces`（`Manager/`、`Repository/`、`Cache/`、`Model/`），資料模型在 `YAEP.WMS.Model`，列舉與常數在 `YAEP.WMS.Constant`。

### Controller 慣例
- 所有 API Controller 繼承 `AbstractApiController`（`API/Code/AbstractApiController.cs`）。
- 每個 action 通常先呼叫 `InitDIRoot()`，再透過 `this.DIContainer.{XxxFactory}.Create{Xxx}Manager()` 取得 Manager。
- 類別標註 `[Authentication]`（JWT 驗證）、`[ConnectionLog]`、`[EnableCors]`；可用 `[SkipAuthentication]` 略過驗證。
- 回應一律包成 `APIResult<T>`：成功用 `GetSuccessResult(...)`、失敗用 `GetFailureResult(...)`、查無資料用 `GetDataNotFoundResult()`。
- 字串輸入用 `AntiXSSEncode` / `GetFilterXSSstring()` 做 XSS 過濾。

### DI 的關鍵：DIRoot 是 T4 產生的
- `YAEP.WMS.DI.Agent/DIRoot.cs` 由 **T4 範本 `DIRootGenerator.tt` 產生**：範本會掃描 `Factory/` 資料夾中所有 `*Factory.cs`，自動建立對應的 Lazy 屬性。**不要手改 `DIRoot.cs`**；要新增 Factory，請新增 `Factory/XxxFactory.cs` 後重新執行 T4（在 VS 中對 .tt 按右鍵 → Run Custom Tool）。
- 各 Factory 繼承 `AbstractMiddleLayerFactory`（`DI.Agent/Code/AbstractMiddleLayerFactory.cs`），在 `CallPublicDIPool()` 內以 Unity 註冊三類對應：`IModelDescriptor<TModel>` → `GenericModelDescriptor`、`IRepositoryHandler<TModel>` → `GenericRepositoryHandler`、各 `IXxxRepository` → 具體 Repository、各 `IXxxManager` → 具體 Manager。新增一個資料表/Model 的完整鏈路時，這四處都要註冊。
- 注意：多個 Manager 介面可能對應到**同一個實作類別**（例如 `IManifestManger`、`IBolManager`、`IVesselManager`、`IWorkOrderManager` 全部對應 `ManifestManager`）。

### BLL / DAL 基底
- Manager 繼承 `AbstractManager`（`BLL/Manager/AbstractManager.cs`）：提供交易控制（`BeginTranaction`/`CommitTransaction`/`RollbackTransaction`，搭配 `TransactionScopeAgent`）、`TracingAgent`、各式快取清除/載入（Product / Package），以及 `Log(...)`。回傳型別為 `IActionResult<T>` / `IExtensionActionResult<R>`，呼叫端以 `.Success` 與 `.Content` 取值。
- Repository 繼承 `AbstractRepository<T>`（`DAL/Repository/AbstractRepository.cs`）：提供 `Create`/`Update`/`Delete`、`SqlBulkCopy` 式批次（`BatchInsert/Update/DeleteTable`）、`ToDataTable`，例外經 `Tracehandler`（`YAEP.WMS.UniversalModule` 的 `ExceptionTraceHandler`）處理。

## 快取：DrKnowAll（重要）

主檔資料（產品、包裝、客戶/Party、倉庫、Identities、共用）以 Redis 快取，入口是 `DrKnowAll`（`YAEP.WMS.Cache/2_redis/DrKnowAll*.cs`，依領域切成多個 partial 檔）；底層 Redis 存取在 `2_redis/controllers/`。

- App 啟動時（`Global.asax.cs` `Application_Start`）會背景執行 `DrKnowAll.Reload(DrKnowAllKeys.ALL)` 載入快取，完成後設 `DrKnowAll.IsInitialized = true`。
- **`[Authentication]` 會在 `DrKnowAll.IsInitialized` 為 false 時，以 401 + code `-100` 擋下所有請求**（系統準備中）。除錯啟動慢時這是預期行為。
- 另有記憶體快取 `CacheManager`（`Cache/0_basic/CacheManager.cs`）與 `RefreshDrKnowAll`（更新單筆主檔後刷新快取）。

## 慣例與陷阱

- **多檔 partial class**：大型 Manager / Controller 切成多個檔，以功能命名，例如 `ManifestManager.Order.Inbound.cs`、`OrderController.Outbound.cs`、`MobileController.Attachment.cs`、`DrKnowAll.Product.cs`。修改某功能前先找齊同名 partial 檔。
- **檔名含零寬空格（U+200B）**：部分 `DrKnowAll​.*.cs` 檔名在 `DrKnowAll` 與 `.` 之間夾了零寬空格（API 與 Cache 專案皆有）。用萬用字元或複製既有檔名，不要手打，否則會找不到檔案。
- **設定與密鑰在 `Web.config`**：連線字串 `YAEP.WMS.ConnectString`（SQL Server）、Redis 主機/密碼、Jaeger 位址、各式 `YAEP.WMS.API.*` 旗標。`Web.config.stagging`、`Web.Debug/Release.config` 為各環境版本。**檔內含明文正式環境密碼，切勿外洩或寫入版本控管之外的地方。** RDLC 標籤路徑等也在此（目前為絕對路徑）。
- **語系**：每次請求由 `Global.asax.cs` 依 `Accept-Language` 標頭設定 `Resource.Culture`；訊息字串放在 `YAEP.WMS.Language/Resources`（`.resx`）。
- **資料庫專案**：`DB/WMS.sqlproj` 為 SSDT 專案，含 `dbo/Tables`、`Stored Procedures`、`Functions`、`Views`、`User Defined Types`，可作為 schema 與 SP 的事實來源。
