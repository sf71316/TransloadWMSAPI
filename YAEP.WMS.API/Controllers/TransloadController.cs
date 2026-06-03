using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Package.DI;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;
using YAEP.Package.BLL.Extensions;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Response;
using YAEP.WMS.Language.Resources;
using YAEP.Identities.Constants;
using YAEP.Utilities;
using YAEP.WMS.Api.Models;
using YAEP.Interfaces;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// Transload 倉儲對外 API（進 / 存 / 出）。
    /// 扁平路由 /api/Transload/{Action}；薄薄包既有 WMS manager，僅加 transload 專用投影。
    /// 主文件：docs/Transload_WMS_API_主文件.md
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Transload")]
    public class TransloadController : AbstractApiController
    {
        public TransloadController()
        {
            this._PackageFactory = new Lazy<PackageFactory>(() => FactoryUtils.GetPackageFactory(base.GetAuthenticationInfo()));
        }

        /// <summary>
        /// 庫存查詢（在庫）。沿用既有 InventoryManager.GetInventory + controller enrichment，
        /// 額外加 transload 投影：StorageStatus（依 Onhand/InboundQty 推算）、ConNo / AgingDays（下一刀補：需 Vessel join / ArrivalDate）。
        /// UI 規格：§6.6 / §7.3.1。⚠️實務必帶 CustomerUID 或 WarehouseUID 過濾（全量約 57k 筆）。
        /// </summary>
        [HttpPost]
        [ActionName("GetInventory")]
        public IHttpActionResult GetInventory(InventorySearchParameters parameters)
        {
            InitDIRoot();
            var manager = this.DIContainer.InventoryFactory.CreateInventoryManager();

            var result = manager.GetInventory(parameters);

            if (!result.Success)
            {
                return base.GetDataNotFoundResult();
            }

            var products = DrKnowAll.GetProduct();
            var customers = DrKnowAll.GetCustomer();
            var resultObject = result.Content.Join(products, i => i.ItemUID, p => p.UID, (i, p) =>
            {
                return new
                {
                    i.WarehouseUID,
                    i.PackageUID,
                    i.WarehouseName,
                    i.InboundQty,
                    i.Onhand,
                    // Item
                    i.ItemUID,
                    ItemName = p.Name,
                    SKU = p.ID,
                    i.Type,
                    p.CustomerUID,
                };
            })
            // 過濾 Customer
            .Where(i =>
            {
                if (parameters == null)
                {
                    return true;
                }
                else
                {
                    return (!parameters.CustomerUID.HasValue || i.CustomerUID == parameters.CustomerUID.Value);
                }
            })
            .Join(customers, i => i.CustomerUID, c => c.UID, (i, c) =>
            {
                return new
                {
                    i.WarehouseUID,
                    i.PackageUID,
                    i.WarehouseName,
                    i.InboundQty,
                    i.Onhand,
                    // Item
                    i.ItemUID,
                    i.ItemName,
                    i.SKU,
                    // Customer Info
                    i.CustomerUID,
                    CustomerID = c.ID,
                    CustomerName = c.Name,
                    i.Type,
                };
            });

            // Allocate Qty / Outbound Qty
            var packageManager = this.GetPackageFactory().CreatePackageManager();
            var packageUIDs = resultObject.GroupBy(o => o.PackageUID).Select(g => g.Key).ToArray();
            var warehouseItems = resultObject.GroupBy(o => new { o.WarehouseUID, o.ItemUID }).Select(g => g.Key).ToArray();
            var treeList = packageManager.GetPackageTrees(packageUIDs);
            var allocateDataResult = manager.GetAllocatedData(
                                                                warehouseItems.Select(w => w.WarehouseUID).ToArray(),
                                                                warehouseItems.Select(w => w.ItemUID).ToArray()
                                                            );

            // [2026-06-02 主線移轉] Loading/Stackable 改讀 Vessel(容器層,ReceivingByTransload 寫入),
            // 連同 Weight/Volume 一併在下方「在庫櫃」查詢中以「最早到倉的櫃」為代表取得(取代原讀 WMS_PayLoad.Description JSON)。

            // per-item 在庫資訊(該料在該倉 on-hand)：最早收貨日(→Received/Aging) + 所在櫃號清單(→ConNo)
            //   + 容器屬性 Loading/Stackable/Weight/Volume(讀 Vessel,以「最早到倉的櫃」為代表)。
            // 收貨未寫 ReceivedDate → 退 Vessel.ArrivalDate → 退 p.CreatedOn。逐列聚合,只讀既有欄位,不動 schema。
            var arrivalByItemWh = new Dictionary<string, DateTime>();
            var conNosByItemWh = new Dictionary<string, SortedSet<string>>();
            var ltByKey = new Dictionary<string, int?>();
            var stByKey = new Dictionary<string, int?>();
            var wtByKey = new Dictionary<string, decimal?>();
            var volByKey = new Dictionary<string, decimal?>();
            var repArrByKey = new Dictionary<string, DateTime>();   // 代表櫃的到倉日(null 視為 MaxValue,真值優先)
            {
                var csA = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
                using (var cnA = new SqlConnection(csA))
                using (var cmdA = new SqlCommand(@"SELECT p.ItemUID, sl.WarehouseUID, v.RefNo,
       COALESCE(p.ReceivedDate, v.ArrivalDate, p.CreatedOn) AS Arr,
       v.LoadingType, v.StackableType, v.Weight, v.Volume
FROM WMS_PayLoad p
LEFT JOIN WMS_Vessel v ON v.UID = p.VesselUID
LEFT JOIN WMS_Slot sl ON sl.UID = p.SlotUID
WHERE p.Type = 1 AND p.Status = 500", cnA))
                {
                    cnA.Open();
                    using (var rdA = cmdA.ExecuteReader())
                    {
                        while (rdA.Read())
                        {
                            if (rdA.IsDBNull(0) || rdA.IsDBNull(1)) continue;
                            var key = rdA.GetGuid(0).ToString() + "|" + rdA.GetGuid(1).ToString();
                            DateTime? dt = rdA.IsDBNull(3) ? (DateTime?)null : rdA.GetDateTime(3);
                            if (dt.HasValue)
                            {
                                if (!arrivalByItemWh.TryGetValue(key, out var cur) || dt.Value < cur) arrivalByItemWh[key] = dt.Value;
                            }
                            var refNo = rdA.IsDBNull(2) ? null : rdA.GetString(2);
                            if (!string.IsNullOrWhiteSpace(refNo))
                            {
                                if (!conNosByItemWh.TryGetValue(key, out var set)) { set = new SortedSet<string>(); conNosByItemWh[key] = set; }
                                set.Add(refNo);
                            }
                            // 容器屬性：以「最早到倉的櫃」為代表(null 到倉日排最後,真值優先)
                            var arrRep = dt ?? DateTime.MaxValue;
                            if (!repArrByKey.TryGetValue(key, out var curRep) || arrRep < curRep)
                            {
                                repArrByKey[key] = arrRep;
                                ltByKey[key] = rdA.IsDBNull(4) ? (int?)null : rdA.GetInt32(4);
                                stByKey[key] = rdA.IsDBNull(5) ? (int?)null : rdA.GetInt32(5);
                                wtByKey[key] = rdA.IsDBNull(6) ? (decimal?)null : rdA.GetDecimal(6);
                                volByKey[key] = rdA.IsDBNull(7) ? (decimal?)null : rdA.GetDecimal(7);
                            }
                        }
                    }
                }
            }

            var r = resultObject.Select(i =>
            {
                var tree = treeList.Content?.FirstOrDefault(t => t.Find(i.PackageUID) != null);
                var package = tree?.Find(i.PackageUID);
                string packageName = package?.Name;
                string packageTree = tree?.GetTreeString(n => DrKnowAll.GetPackageUom(n.UOM)?.Name);
                string uomName = DrKnowAll.GetPackageUom(package?.UOM ?? Guid.Empty)?.Name;
                int allocateReceiveQty = 0;

                if (allocateDataResult.Success && package != null)
                {
                    var minNode = tree.GetMinNode();
                    var allocate = allocateDataResult.Content?.FirstOrDefault(o => o.ItemUID == i.ItemUID
                    && o.WarehouseUID == i.WarehouseUID && i.Type == o.OriginalPayloadType);
                    if (allocate != null)
                    {
                        var receiveQtyResult = packageManager.GetReceivePackageUomQuantity(tree, allocate.PackageUID, minNode.UID, allocate.Quantity);
                        if (receiveQtyResult.Success)
                        {
                            allocateReceiveQty = receiveQtyResult.Content;
                        }
                    }
                }

                int onHandPcs = i.InboundQty - allocateReceiveQty;          // 件數(Each)=真值
                // 換算成「收貨 UOM 單位數」(pieces → package level)；失敗則退回件數
                int onHandUom = onHandPcs;
                if (allocateDataResult != null && package != null && tree != null)
                {
                    try
                    {
                        var minNode2 = tree.GetMinNode();
                        var uomConv = packageManager.GetReceivePackageUomQuantity(tree, minNode2.UID, package.UID, onHandPcs);
                        if (uomConv.Success) { onHandUom = uomConv.Content; }
                    }
                    catch { /* 保底用件數 */ }
                }
                // transload 投影：依在庫/收貨量推算庫存狀態（無需額外資料源）
                string storageStatus = onHandPcs <= 0 ? "FULLY_OUT" : (onHandPcs < i.InboundQty ? "PARTIAL_OUT" : "IN_STORAGE");
                string itemWhKey = i.ItemUID.ToString() + "|" + i.WarehouseUID.ToString();
                DateTime? arrival = arrivalByItemWh.TryGetValue(itemWhKey, out var _arrDt) ? _arrDt : (DateTime?)null;
                string conNos = conNosByItemWh.TryGetValue(itemWhKey, out var _cset) && _cset.Count > 0 ? string.Join(", ", _cset) : null;

                // === 欄位名稱對齊「Inventory 頁(權威結構)」===
                return new
                {
                    id = Guid.NewGuid().ToString(),         // DataTable 列 key
                    UID = i.PackageUID,
                    i.PackageUID,
                    i.WarehouseUID,
                    i.WarehouseName,
                    // Item
                    i.ItemUID,
                    SKU = i.SKU,                            // YAEP_Item.ID（料號碼）
                    i.ItemName,                             // 頁面 Description
                    // Customer
                    i.CustomerUID,
                    i.CustomerID,
                    i.CustomerName,
                    // 包裝/UOM（Qty 與 UOM 分兩欄）
                    PackageName = packageName,
                    PackageTree = packageTree,
                    UOM = uomName,
                    OnHand = onHandUom,                     // 收貨 UOM 單位數
                    OnHandPcs = onHandPcs,                  // 件數(Each) 真值
                    AllocatedQty = allocateReceiveQty,
                    AvailableQty = onHandPcs,
                    TypeName = YAEP.Utilities.EnumerableData.GetName<InventoryType>(i.Type),
                    StorageStatus = storageStatus,
                    // === 以下待 P6 加欄位 / T2 後補（先回 null/0 讓欄位存在、頁面不報錯）===
                    ConNo = conNos,                         // 該料在該倉所在櫃號(Vessel.RefNo,多櫃逗號分隔)
                    LoadingType = (ltByKey.TryGetValue(itemWhKey, out var _lt) ? _lt : (int?)null),    // 容器層(Vessel.LoadingType,最早到倉櫃)
                    StackableType = (stByKey.TryGetValue(itemWhKey, out var _st) ? _st : (int?)null),  // 容器層(Vessel.StackableType,最早到倉櫃)
                    Weight = (wtByKey.TryGetValue(itemWhKey, out var _wt) ? _wt : (decimal?)null),     // 容器層(Vessel.Weight,最早到倉櫃)
                    Volume = (volByKey.TryGetValue(itemWhKey, out var _vol) ? _vol : (decimal?)null),  // 容器層(Vessel.Volume,最早到倉櫃)
                    ArrivalDateActual = arrival?.ToString("yyyy-MM-dd"),   // 該料在該倉最早收貨日(Vessel.ArrivalDate/CreatedOn 回推)
                    AgingDays = arrival.HasValue ? (int?)(DateTime.Now.Date - arrival.Value.Date).Days : (int?)null,
                    FreeDays = (int?)null,                  // TODO: T2 S_Contract_Demurrage/S_Client.Free_Days
                    Cartons = (int?)null,                   // TODO: 由 package tree 推 Box 層
                    Pallets = (int?)null,                   // TODO: 由 package tree 推 Pallet 層
                };
            });

            if (r.Count() > 0)
            {
                var actionResult = this.GetSuccessResult(r);
                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }

        // 列表聚合用(每張 manifest 的 倉庫名/櫃數/總量/到倉日 + 稽核欄)
        private class AggRow { public string WarehouseName; public int Cnt; public int Qty; public DateTime? Arr; public string ConNos;
            public string CreatedBy; public DateTime? CreatedOn; public string ModifiedBy; public DateTime? ModifiedOn; }

        /// <summary>
        /// Inbound 列表（查詢 Manifest）。沿用既有 ManifestManager.GetManifestList。
        /// UI 規格：§6.5 / Inbound 列表。TODO(下一刀): 投影 LoadingType/SealNo（P6 加欄位後）。
        /// </summary>
        [HttpPost]
        [ActionName("GetManifestList")]
        public IHttpActionResult GetManifestList(ManifestSearchParameters parameters)
        {
            InitDIRoot();
            parameters = this.AntiXSSEncode(parameters);
            parameters.Type = ManifestType.Inbound; // Inbound 列表只回進貨單(避免混入出貨/Move manifest)
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var rs = _instance.GetManifestList<ManifestListViewModel>(parameters);
            if (rs.Success)
            {
                // 名稱解析一次建字典(避免逐列重抓快取造成 N×全量)
                var custMap = BuildCustomerNameMap();
                var whMap = BuildWarehouseNameMap();
                // 一條 SQL 聚合每張 manifest 的 倉庫名/櫃數/總量/到倉日(補列表欄位,取代原本一律 null)
                var aggMap = new Dictionary<Guid, AggRow>();
                var muids = rs.Content.Select(m => m.UID).Distinct().ToList();
                if (muids.Count > 0)
                {
                    var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
                    var inClause = string.Join(",", muids.Select((_, i) => "@u" + i));
                    using (var cn = new SqlConnection(cs))
                    using (var cmd = new SqlCommand($@"
SELECT m.UID, w.Name AS WarehouseName, COUNT(DISTINCT v.UID) AS Cnt,
       SUM(ISNULL(pl.Quantity,0)) AS Qty, MIN(v.ArrivalDate) AS Arr,
       STUFF((SELECT ', ' + v2.RefNo
              FROM WMS_BOL b2
              JOIN WMS_Vessel v2 ON v2.BolUID = b2.UID
              WHERE b2.ManifestUID = m.UID AND v2.RefNo IS NOT NULL AND v2.RefNo <> ''
              GROUP BY v2.RefNo
              FOR XML PATH('')), 1, 2, '') AS ConNos,
       m.CreatedBy, m.CreatedOn, m.ModifiedBy, m.ModifiedOn
FROM WMS_Manifest m
LEFT JOIN WMS_Warehouse w ON w.UID = m.WarehouseUID
LEFT JOIN WMS_BOL b ON b.ManifestUID = m.UID
LEFT JOIN WMS_Vessel v ON v.BolUID = b.UID
LEFT JOIN WMS_PayLoad pl ON pl.VesselUID = v.UID AND pl.Status > 0 AND pl.Type = 1
WHERE m.UID IN ({inClause})
GROUP BY m.UID, w.Name, m.CreatedBy, m.CreatedOn, m.ModifiedBy, m.ModifiedOn", cn))
                    {
                        for (int i = 0; i < muids.Count; i++) cmd.Parameters.AddWithValue("@u" + i, muids[i]);
                        cn.Open();
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                aggMap[rd.GetGuid(0)] = new AggRow
                                {
                                    WarehouseName = rd.IsDBNull(1) ? null : rd.GetString(1),
                                    Cnt = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd.GetValue(2)),
                                    Qty = rd.IsDBNull(3) ? 0 : Convert.ToInt32(rd.GetValue(3)),
                                    Arr = rd.IsDBNull(4) ? (DateTime?)null : rd.GetDateTime(4),
                                    ConNos = rd.IsDBNull(5) ? null : rd.GetString(5),
                                    CreatedBy = rd.IsDBNull(6) ? null : rd.GetString(6),
                                    CreatedOn = rd.IsDBNull(7) ? (DateTime?)null : rd.GetDateTime(7),
                                    ModifiedBy = rd.IsDBNull(8) ? null : rd.GetString(8),
                                    ModifiedOn = rd.IsDBNull(9) ? (DateTime?)null : rd.GetDateTime(9),
                                };
                            }
                        }
                    }
                }
                // 欄位對齊「Inbound 頁(權威結構)」
                var today = DateTime.Now.Date;
                var data = rs.Content.Select(m =>
                {
                    aggMap.TryGetValue(m.UID, out var a);
                    string whName = a?.WarehouseName
                        ?? (whMap.TryGetValue(m.WarehouseUID, out var _wn) ? _wn : null);
                    return new
                    {
                        ManifestUID = m.UID,
                        UID = m.UID,
                        m.ManifestNo,
                        m.ManifestName,
                        CustomerUID = m.PartyUID,
                        CustomerName = custMap.TryGetValue(m.PartyUID, out var _cn) ? _cn : null,
                        m.WarehouseUID,
                        WarehouseName = whName,
                        ConNo = m.RefNo,
                        ConNos = a?.ConNos,                     // 真實櫃號清單(Vessel.RefNo 彙總)，供 Inbound 頁櫃號搜尋
                        Status = (int)m.Status,
                        m.StatusName,
                        m.TypeName,
                        containerCount = (int?)(a?.Cnt ?? 0),
                        totalQty = (int?)(a?.Qty ?? 0),
                        totalCartons = (int?)null,
                        totalPallets = (int?)null,
                        ArrivalDate = (a != null && a.Arr.HasValue) ? a.Arr.Value.ToString("yyyy-MM-dd") : null,
                        AgingDays = (a != null && a.Arr.HasValue) ? (int?)(today - a.Arr.Value.Date).Days : null,
                        CreatedBy = a?.CreatedBy,
                        CreatedOn = a?.CreatedOn?.ToString("yyyy-MM-dd HH:mm"),
                        ModifiedBy = a?.ModifiedBy,
                        ModifiedOn = a?.ModifiedOn?.ToString("yyyy-MM-dd HH:mm"),
                    };
                }).ToList();
                var result = this.GetSuccessResult(data);
                return this.Json(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }

        /// <summary>
        /// 收貨單唯讀明細(供 Inbound 頁「檢視明細」用,不可改)：依櫃(Vessel.RefNo)分組,列每櫃的 SKU/數量/Loading/Stackable。
        /// 只用正式機保證存在的欄位(RefNo/Quantity/Description JSON),不依賴 P6 新欄位。
        /// </summary>
        [HttpGet]
        [ActionName("GetManifestDetail")]
        public IHttpActionResult GetManifestDetail([FromUri] string refNo)
        {
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            // 每櫃資訊(Seal/到港日/櫃型/重量/材積) + 每項 SKU/數量/UOM(YAEP_Package.Name,收貨存最小包裝)/Loading/Stackable。
            var raw = new List<(string ConNo, string Seal, DateTime? Arr, int? CType, double? Wt, double? Vol, string SKU, string Name, int Qty, string Descr, string UOM)>();
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
SELECT v.RefNo AS ConNo, v.SealNo, v.ArrivalDate, v.ContainerSize, v.Weight, v.Volume,
       it.ID AS SKU, it.Name AS ItemName, pl.Quantity AS Qty, pl.Description AS Descr, pk.Name AS UOM
FROM WMS_Manifest m
JOIN WMS_BOL b ON b.ManifestUID = m.UID
JOIN WMS_Vessel v ON v.BolUID = b.UID
JOIN WMS_PayLoad pl ON pl.VesselUID = v.UID AND pl.Type = 1 AND pl.Status > 0
JOIN YAEP_Item it ON it.UID = pl.ItemUID
LEFT JOIN YAEP_Package pk ON pk.UID = pl.PackageUID
WHERE m.RefNo = @ref AND m.Type = 1
ORDER BY v.RefNo, it.ID", cn))
            {
                cmd.Parameters.AddWithValue("@ref", refNo);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        raw.Add((
                            rd.IsDBNull(0) ? null : rd.GetString(0),
                            rd.IsDBNull(1) ? null : rd.GetString(1),
                            rd.IsDBNull(2) ? (DateTime?)null : rd.GetDateTime(2),
                            rd.IsDBNull(3) ? (int?)null : Convert.ToInt32(rd.GetValue(3)),
                            rd.IsDBNull(4) ? (double?)null : Convert.ToDouble(rd.GetValue(4)),
                            rd.IsDBNull(5) ? (double?)null : Convert.ToDouble(rd.GetValue(5)),
                            rd.IsDBNull(6) ? null : rd.GetString(6),
                            rd.IsDBNull(7) ? null : rd.GetString(7),
                            rd.IsDBNull(8) ? 0 : Convert.ToInt32(rd.GetValue(8)),
                            rd.IsDBNull(9) ? null : rd.GetString(9),
                            rd.IsDBNull(10) ? null : rd.GetString(10)));
                }
            }
            Func<string, string, int?> ji = (descr, key) =>
            {
                if (string.IsNullOrWhiteSpace(descr)) return null;
                try { var j = Newtonsoft.Json.Linq.JObject.Parse(descr); var t = j[key]; return (t == null || t.Type == Newtonsoft.Json.Linq.JTokenType.Null) ? (int?)null : (int)t; }
                catch { return null; }
            };
            var containers = raw.GroupBy(r => r.ConNo).Select(g =>
            {
                var h = g.First();
                return new
                {
                    ConNo = g.Key,
                    SealNo = h.Seal,
                    ArrivalDate = h.Arr?.ToString("yyyy-MM-dd"),
                    ContainerSize = h.CType,   // 對外欄位名 ContainerSize(櫃尺寸 enum);實體欄位即 WMS_Vessel.ContainerSize
                    Weight = h.Wt,
                    Volume = h.Vol,
                    Items = g.Select(r => new { SKU = r.SKU, ItemName = r.Name, Qty = r.Qty, UOM = string.IsNullOrEmpty(r.UOM) ? "Each" : r.UOM, LoadingType = ji(r.Descr, "lt"), StackableType = ji(r.Descr, "st") }).ToList(),
                };
            }).ToList();
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, Containers = containers }));
        }

        /// <summary>
        /// 出貨單唯讀明細(供 Outbound 頁「檢視明細」用,不可改)：列該出貨實際揀出的 SKU/數量。
        /// 來源 WMS_WorkOrder_Payload(揀貨明細,Status>0=有效),依 Manifest.RefNo(Type=2 出貨)。只讀,不動 schema。
        /// </summary>
        [HttpGet]
        [ActionName("GetShipmentDetail")]
        public IHttpActionResult GetShipmentDetail([FromUri] string refNo)
        {
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            var items = new List<object>();
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
SELECT it.ID AS SKU, it.Name AS ItemName, SUM(wpl.Qty) AS Qty
FROM WMS_Manifest m
JOIN WMS_WorkOrder wo ON wo.ManifestUID = m.UID
JOIN WMS_WorkOrder_Payload wpl ON wpl.WorkOrderUID = wo.UID AND wpl.Status > 0
JOIN YAEP_Item it ON it.UID = wpl.ItemUID
WHERE m.RefNo = @ref AND m.Type = 2
GROUP BY it.ID, it.Name
ORDER BY it.ID", cn))
            {
                cmd.Parameters.AddWithValue("@ref", refNo);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        items.Add(new
                        {
                            SKU = rd.IsDBNull(0) ? null : rd.GetString(0),
                            ItemName = rd.IsDBNull(1) ? null : rd.GetString(1),
                            Qty = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd.GetValue(2)),
                        });
                }
            }
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, Items = items }));
        }

        /// <summary>
        /// Outbound 列表（查詢某 Manifest 的 BOL）。沿用既有 BolManager.GetBolList。
        /// UI 規格：§7.4.2 / Outbound 列表。TODO(下一刀): 投影 TrackingNumber/PODUrl（P6 加欄位後）、跨 manifest 依倉庫/客戶/日期過濾。
        /// </summary>
        [HttpGet]
        [ActionName("GetBOLList")]
        public IHttpActionResult GetBOLList(Guid muid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var _parameters = this.DIContainer.ManifestFactory.GenerateModel<IBolSearchParameters>();
            _parameters.ManifestUID = muid;
            var rs = _instance.GetBolList(_parameters);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<IBolModel>>(rs.Content.ToList());
                return this.Json<APIResult<IEnumerable<IBolModel>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }

        /// <summary>
        /// 客戶下拉（取 DrKnowAll 客戶快取，依 token 群組過濾）。UI 規格：§6.3。回 { UID, ID, Name }。
        /// </summary>
        [HttpGet]
        [ActionName("GetCustomerList")]
        public IHttpActionResult GetCustomerList()
        {
            // [2026-06-03] 依 token 所屬群組過濾(原本回全部客戶會跨公司外洩)。客戶 GroupUID=公司層。
            var groupUIDs = this.tlGetGroupUIDsByUser();
            if (groupUIDs.Count == 0) return base.GetDataNotFoundResult();
            var groupSet = new HashSet<Guid>(groupUIDs);
            var data = DrKnowAll.GetCustomer()
                .Where(c => c.Status > 0)                 // 只回啟用中客戶(Status>0)
                .Where(c => groupSet.Contains(c.GroupUID)) // 只回 token 群組可見的客戶(避免跨公司外洩)
                .Select(c => new { c.UID, c.ID, c.Name })
                .OrderBy(c => c.Name)
                .ToList();
            var result = this.GetSuccessResult(data);
            return this.Json(result);
        }

        /// <summary>
        /// 倉庫下拉（依 token 群組過濾；比照主線）。回 IWarehouseModel 清單(含 UID/ID/Name)。
        /// BLL GetThirdPartyWarehouseNameList 內已依 GetGroupUserViewByUser 的群組過濾。
        /// </summary>
        [HttpGet]
        [ActionName("GetWarehouseList")]
        public IHttpActionResult GetWarehouseList([FromUri] string customerPartyName = null)
        {
            try
            {
                InitDIRoot();
                using (var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager)
                {
                    var result = _instance.GetThirdPartyWarehouseNameList();
                    if (result.Success) return this.Json(this.GetSuccessResult(result.Content));
                    return this.GetFailureResult(-1, result.Message);
                }
            }
            catch (Exception ex) { return this.GetFailureResult(-1, ex.Message); }
        }

        /// <summary>
        /// Outbound 列表（出貨/BOL）。單一 SQL：WMS_BOL JOIN WMS_Manifest 依 PartyUID 過濾客戶，
        /// 避免 manifest→BOL 逐筆 N+1。UI 規格：§7.4.2。欄位對齊 Outbound 頁。
        /// TODO: ShippedQty/Pallets(BOL item 聚合)、Carrier/ShipMethod 名稱解析。
        /// </summary>
        [HttpPost]
        [ActionName("GetShipmentList")]
        public IHttpActionResult GetShipmentList(InventorySearchParameters parameters)
        {
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            var custMap = BuildCustomerNameMap();
            var list = new List<object>();
            // TOP 500 最近出貨(客戶 BOL 量大，如 CAP001 有 1.3 萬筆)；實務應分頁/加日期過濾
            var sql = @"SELECT TOP 500 b.UID, b.ID, b.RefNo, b.ManifestUID, b.ShipMethodUID,
                               b.Contact, b.ShipToCity, b.ShipToState, b.ShipToZip, b.ShipToAddress,
                               b.TrackingNumber, b.DeliveryDate, b.ETA, b.Status, b.CreatedOn,
                               m.PartyUID, m.WarehouseUID, m.RefNo AS ManifestRefNo, m.Name AS ManifestName,
                               m.ID AS ManifestNo, m.CreatedBy, m.ModifiedBy, m.ModifiedOn,
                               (SELECT SUM(wpl.Qty) FROM WMS_WorkOrder_Payload wpl
                                JOIN WMS_WorkOrder wo ON wo.UID = wpl.WorkOrderUID
                                WHERE wo.ManifestUID = m.UID AND wpl.Status > 0) AS ShippedQty
                        FROM WMS_BOL b INNER JOIN WMS_Manifest m ON m.UID = b.ManifestUID
                        WHERE m.Type = @type
                          AND (@cust IS NULL OR m.PartyUID = @cust)
                          AND (@wh IS NULL OR m.WarehouseUID = @wh)
                        ORDER BY b.CreatedOn DESC";
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@type", (int)ManifestType.Outbound); // 只回出貨單,排除收貨(Inbound)BOL 混入
                cmd.Parameters.AddWithValue("@cust", (object)(parameters?.CustomerUID) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@wh", (object)(parameters?.WarehouseUID) ?? DBNull.Value);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        Func<string, object> g = name => { var i = rd.GetOrdinal(name); return rd.IsDBNull(i) ? null : rd.GetValue(i); };
                        var partyUID = (Guid?)g("PartyUID");
                        var shipDate = (DateTime?)g("DeliveryDate") ?? (DateTime?)g("CreatedOn");
                        var city = (string)g("ShipToCity"); var st = (string)g("ShipToState"); var contact = (string)g("Contact");
                        list.Add(new
                        {
                            VesselUID = (Guid)g("UID"),                 // 頁面 getRowId
                            BolUID = (Guid)g("UID"),
                            ShipOrderNo = (string)g("ID") ?? (string)g("RefNo"),
                            CustomerUID = partyUID,
                            CustomerName = (partyUID.HasValue && custMap.TryGetValue(partyUID.Value, out var _cn)) ? _cn : null,
                            SourceCons = (string)g("ManifestRefNo"),    // ConNo
                            ShipMethod = (string)null,                  // TODO 解析 ShipMethodUID
                            ShipDate = shipDate?.ToString("yyyy-MM-dd"),
                            Carrier = (string)null,                     // WMS BOL header 無
                            TrackingNumber = (string)g("TrackingNumber"),
                            ShipTo = string.Join(", ", new[] { contact, city, st }.Where(x => !string.IsNullOrEmpty(x))),
                            WarehouseUID = (Guid?)g("WarehouseUID"),
                            ShippedQty = (g("ShippedQty") == null ? (int?)null : Convert.ToInt32(g("ShippedQty"))),  // 已揀出量(WO_Payload.Qty)
                            ShippedPallets = (int?)null,
                            Status = (int)(g("Status") ?? 0),
                            ManifestName = (string)g("ManifestName"),
                            ManifestNo = (string)g("ManifestNo"),       // WMS M 流水號(有意義編號)
                            CreatedOn = ((DateTime?)g("CreatedOn"))?.ToString("yyyy-MM-dd HH:mm"),
                            CreatedBy = (string)g("CreatedBy"),
                            ModifiedOn = ((DateTime?)g("ModifiedOn"))?.ToString("yyyy-MM-dd HH:mm"),
                            ModifiedBy = (string)g("ModifiedBy"),
                        });
                    }
                }
            }
            return this.Json(this.GetSuccessResult(list));
        }

        /// <summary>
        /// 建立收貨。沿用既有 Order/Receiving(舊端點,可建庫存,進存出回歸測試走這條)。UI 規格：§6.5/§7.2.1。
        /// ⚠️既有 Receiving 一次建 1 Vessel；多櫃(每櫃 1 Vessel)擴充為 TODO。
        /// SSCC pallet barcode 版本另見 ReceivingByTransload(不取代此端點)。
        /// </summary>
        [HttpPost]
        [ActionName("Receiving")]
        public IHttpActionResult Receiving(ReceivingRequest request)
        {
            InitDIRoot();
            if (request == null || request.WarehouseUID == Guid.Empty)
                return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {
                _instance.TracingAgent.BeginTracing("", request);
                var rs = _instance.Receiving(request);
                _instance.TracingAgent.EndTracing(rs);
                if (rs.Success) return this.Json(this.GetSuccessResult<IReceivingResponse>(rs.Content));
                return this.GetFailureResult<IReceivingResponse>(-1, rs.Content?.Message, rs.Content);
            }
        }

        /// <summary>
        /// 建立收貨(Transload SSCC 版,獨立端點,不取代舊 Receiving)。BLL ReceivingByTransload：生成 Manifest/BOL/每櫃Vessel
        /// + 系統產 SSCC pallet barcode(WarehouseManager.GenerateBarcode → 落 WMS_WorkOrder_Pod) → auto-assign。
        /// 每行帶 PackageUID(精確包裝版本)。完成另呼叫 CompleteReceiving。
        /// </summary>
        [HttpPost]
        [ActionName("ReceivingByTransload")]
        public IHttpActionResult ReceivingByTransload([FromBody] TransloadReceivingRequestModel request)
        {
            try
            {
                InitDIRoot();
                if (request == null) return this.GetFailureResult(-1, "Request is empty.");
                if (string.IsNullOrWhiteSpace(request.CustomerPartyName)) return this.GetFailureResult(-1, "Incorrect parameters: CustomerPartyName");
                if (request.Containers == null || request.Containers.Count == 0) return this.GetFailureResult(-1, "Incorrect parameters: Containers");
                var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                var result = _instance.ReceivingByTransload(request);
                if (!result.Success) return this.GetFailureResult(-1, result.Message);
                var content = result.Content;
                var response = new TransloadReceivingResponseModel
                {
                    ManifestUID = content?.ManifestUID ?? Guid.Empty,
                    Vessels = new List<TransloadReceivingVesselResult>(),
                };
                if (content?.Vessels != null)
                    foreach (var v in content.Vessels)
                        response.Vessels.Add(new TransloadReceivingVesselResult { VesselUID = v.UID, ConNo = v.RefNo, Status = v.Status });
                return this.Json(this.GetSuccessResult(response));
            }
            catch (Exception ex) { return this.GetFailureResult(-1, ex.Message); }
        }

        /// <summary>收貨完成(Transload)：對 Manifest 觸發系統完成 → WMS_PayLoad(Stock)+Inventory。ReceivingByTransload 已內含,此為獨立觸發備用。</summary>
        [HttpPost]
        [ActionName("CompleteReceiving")]
        public IHttpActionResult CompleteReceiving([FromBody] TransloadCompleteReceivingRequestModel request)
        {
            try
            {
                InitDIRoot();
                if (request == null || request.ManifestUID == Guid.Empty) return this.GetFailureResult(-1, "Incorrect parameters: ManifestUID");
                var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                var result = _instance.CompleteReceivingByTransload(request.ManifestUID);
                if (!result.Success) return this.GetFailureResult(-1, result.Message);
                return this.Json(this.GetSuccessResult(new { ManifestUID = request.ManifestUID, IsComplete = true }));
            }
            catch (Exception ex) { return this.GetFailureResult(-1, ex.Message); }
        }

        /// <summary>
        /// 建立出貨配置（auto-assign）。沿用既有 Order/Allocated。UI 規格：§6.7/§7.4.1。
        /// </summary>
        [HttpPost]
        [ActionName("Allocated")]
        public IHttpActionResult Allocated(AllocatedRequest request)
        {
            InitDIRoot();
            if (request == null || (string.IsNullOrEmpty(request.CustomerPartyName) && request.CustomerUID == Guid.Empty) || request.WarehouseUID == Guid.Empty)
                return this.BadRequest(Resource.MANIFEST_COMMON_REQUEST_NULL);
            if (string.IsNullOrEmpty(request.CustomerPartyName) && request.CustomerUID != Guid.Empty)
            {
                // facade 放寬：前端只給 CustomerUID 時，補 PartyName 以滿足下游(manager 實際以 CustomerUID 配貨)
                request.CustomerPartyName = DrKnowAll.GetCustomer(request.CustomerUID)?.ID ?? "-";
            }
            using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
            {
                _instance.TracingAgent.BeginTracing("", request);
                var rs = _instance.Allocated(request);
                _instance.TracingAgent.EndTracing(rs);
                // 傳遞實際成敗(原本一律回 IsComplete=true 會吞掉配貨失敗,讓前端誤判成功)
                var _msg = rs.Message;
                if (!rs.Success && rs.InnerException != null)
                    _msg = (rs.Message ?? "") + " | EX: " + rs.InnerException.Message + " @@ " + rs.InnerException.StackTrace;
                return this.Json(this.GetSuccessResult<IAllocatedResponse>(rs.Content, _msg, rs.Success));
            }
        }

        /// <summary>
        /// SKU 查詢（取 DrKnowAll 產品快取，依客戶 + keyword 過濾）。UI 規格：§7.1.1。
        /// 回 { UID, ID, Name, CustomerUID, Packages:[{PackageUID, UOM_ID, UOM_Name, QtyPerPackage}] }。
        /// </summary>
        [HttpGet]
        [ActionName("GetProducts")]
        public IHttpActionResult GetProducts([FromUri] Guid? customerUID, [FromUri] string keyword = null)
        {
            var kw = (keyword ?? "").Trim().ToLower();
            var products = DrKnowAll.GetProduct()
                .Where(p => p.Status > 0)                 // 只回啟用中品項(Status>0)
                .Where(p => !customerUID.HasValue || p.CustomerUID == customerUID.Value)
                .Where(p => kw == "" || (p.ID ?? "").ToLower().Contains(kw) || (p.Name ?? "").ToLower().Contains(kw))
                .OrderBy(p => p.ID)
                //.Take(200)
                .ToList();

            // 真實 UOM 包裝(取代寫死 Each stub):從 package cache 依 ItemUID 取各層。
            // YAEP_Package.Quantity = 該單位「每 1 個父單位內含幾個」(SDR-050: Pallet→Box Q=40→Each Q=1)。
            // 某層的 base(Each) 換算量 = 沿『子節點』鏈累乘 child.Quantity。
            var pkgByItem = DrKnowAll.GetPackage()
                .Where(g => g != null && g.Status > 0)    // 只取啟用中的包裝定義(P Item)
                .GroupBy(g => g.ItemUID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var data = products.Select(p =>
            {
                var nodes = pkgByItem.ContainsKey(p.UID) ? pkgByItem[p.UID] : null;
                var packages =
                    (nodes != null && nodes.Count > 0)
                    ? nodes.Select(n =>
                        {
                            int qtyPer = 1;
                            var cur = n;
                            // 沿子鏈往 base(Each)累乘
                            while (true)
                            {
                                var child = nodes.FirstOrDefault(x => x.ParentUID == cur.UID);
                                if (child == null) break;
                                qtyPer *= (child.Quantity <= 0 ? 1 : child.Quantity);
                                cur = child;
                            }
                            var uomName = !string.IsNullOrEmpty(n.UomName) ? n.UomName : (DrKnowAll.GetPackageUom(n.UOM)?.Name ?? "Each");
                            return new { PackageUID = (Guid?)n.UID, UOM_ID = uomName, UOM_Name = uomName, QtyPerPackage = qtyPer };
                        })
                        .GroupBy(x => x.UOM_ID).Select(g => g.First())   // 同名 UOM 去重
                        .OrderByDescending(x => x.QtyPerPackage)
                        .ToList()
                    // 無包裝定義時不再假造 Each;回空清單,UOM 下拉只反映 P Item 真實設定(請至 P Item 設定包裝)。
                    : new[] { new { PackageUID = (Guid?)null, UOM_ID = "Each", UOM_Name = "Each", QtyPerPackage = 1 } }.Take(0).ToList();
                return new { p.UID, p.ID, p.Name, p.CustomerUID, Packages = packages };
            }).ToList();
            return this.Json(this.GetSuccessResult(data));
        }

        public class CreateProductRequest
        {
            public string ItemNo { get; set; }
            public string Name { get; set; }
            public Guid? CustomerUID { get; set; }
            public List<CreateProductPkg> Packages { get; set; }
        }
        public class CreateProductPkg { public string UOM { get; set; } public int Qty { get; set; } }

        /// <summary>
        /// 建立品項 + 包裝樹(供 Inbound 建單「+ New Item」用)。預設 Pallet/Box/Each = 1/1/1
        /// (滿足 WMS 收貨驗證:最小=Each + 最大在 {pallet,box...})。沿用既有 ProcessPBSCItemAndPackage。
        /// ⚠️ BLL SyncPackage 內把 group/customer 寫死 CAP001 → 目前僅適用 CAP001(demo)。
        /// </summary>
        [HttpPost]
        [ActionName("CreateProduct")]
        public IHttpActionResult CreateProduct(CreateProductRequest req)
        {
            InitDIRoot();
            if (req == null || string.IsNullOrWhiteSpace(req.ItemNo))
                return this.BadRequest("ItemNo required");
            var levels = (req.Packages != null && req.Packages.Count > 0)
                ? req.Packages
                : new List<CreateProductPkg> {
                    new CreateProductPkg { UOM = "PALLET", Qty = 1 },
                    new CreateProductPkg { UOM = "BOX", Qty = 1 },
                    new CreateProductPkg { UOM = "EACH", Qty = 1 },
                  };
            var pkgs = new List<YAEP.WMS.Interfaces.IPBSCPackagingModel>();
            for (int i = 0; i < levels.Count; i++)
            {
                pkgs.Add(new YAEP.WMS.Model.PBSCPackagingModel
                {
                    PROD_ID = req.ItemNo,
                    PARENT_MEASURE = i == 0 ? "" : (levels[i - 1].UOM ?? "").ToUpper(),
                    MEASURE = (levels[i].UOM ?? "").ToUpper(),
                    QTY = levels[i].Qty <= 0 ? 1 : levels[i].Qty,
                });
            }
            var pkgItem = new YAEP.WMS.Model.PBSCItemPackagingModel
            {
                Item = new YAEP.WMS.Model.PBSCItemModel
                {
                    ID = req.ItemNo,
                    Name = string.IsNullOrWhiteSpace(req.Name) ? req.ItemNo : req.Name,
                    CategoryName = "Transload",
                    CustomerUID = req.CustomerUID ?? Guid.Empty,
                },
                Packages = pkgs,
            };
            var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var rs = manager.ProcessPBSCItemAndPackage(new List<YAEP.WMS.Model.PBSCItemPackagingModel> { pkgItem });
            if (rs == null || !rs.Success)
                return this.GetFailureResult(-1, rs?.Message ?? "Create product failed.");
            // 刷新 API 端產品快取,讓 GetProducts 立即看到(查 DB 取新 UID 後 RefreshProduct)
            try
            {
                var grp = new Guid("C69136EF-9141-4ED2-AA24-9C23B6A14CF2"); // BLL SyncPackage 寫死的 group
                var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT TOP 1 UID FROM YAEP_Item WHERE ID=@id AND GroupUID=@g AND Status>0 ORDER BY CreatedOn DESC", cn))
                {
                    cmd.Parameters.AddWithValue("@id", req.ItemNo);
                    cmd.Parameters.AddWithValue("@g", grp);
                    cn.Open();
                    var o = cmd.ExecuteScalar();
                    if (o != null && o != DBNull.Value) DrKnowAll.RefreshProduct((Guid)o);
                }
            }
            catch { /* 快取刷新非致命,GetProducts 重載亦可 */ }
            return this.Json(this.GetSuccessResult(new { req.ItemNo, Name = pkgItem.Item.Name }));
        }

        /// <summary>
        /// 新增 SKU(供 New Inbound Manifest「+ 馬上新增 Item」)。最小輸入 Sku + CustomerPartyName:
        /// 建立產品本體(綁客戶屬性) + 包裝版本 + 預設三層包裝 PALLET→BOX→EACH,
        /// 然後**完整刷新快取**(Redis 產品/分類/包裝/版本 + 重載 MemoryCache 的 product-package)。
        /// ⚠️ 快取機制:新增 item/改包裝後必須觸發刷新,否則 process 內 _PackageCache 取不到新 package、
        ///    收貨完成(CompleteTicketData→GetMinPackage)會 NRE。此端點把刷新內建,建完即可收貨。
        /// </summary>
        /// <param name="request">Sku(產品代碼=Item.ID) / CustomerPartyName(客戶代碼=Party.ID)</param>
        [HttpPost]
        [ActionName("AddProduct")]
        public IHttpActionResult AddProduct([FromBody] TransloadAddProductRequestModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Sku))
                return this.GetFailureResult(-1, "Incorrect parameters: Sku");
            if (string.IsNullOrWhiteSpace(request.CustomerPartyName))
                return this.GetFailureResult(-1, "Incorrect parameters: CustomerPartyName");
            try
            {
                InitDIRoot();
                var sku = request.Sku.Trim();
                var customerPartyName = request.CustomerPartyName.Trim();

                // 1. group = token 使用者預設群組
                var groupUID = this.tlGetDefaultGroupUID();
                if (groupUID == Guid.Empty)
                    return this.GetFailureResult(-1, "No group for current user.");

                // 2. 客戶解析(CustomerPartyName = Party.ID),用快取客戶清單
                var customer = DrKnowAll.GetCustomer()?.FirstOrDefault(c =>
                    !string.IsNullOrEmpty(c.ID) && c.ID.Equals(customerPartyName, StringComparison.OrdinalIgnoreCase));
                if (customer == null)
                    return this.GetFailureResult(-1, "Customer not found: " + customerPartyName);
                var customerUID = customer.UID;

                var itemManager = FactoryUtils.GetItemFactory(base.GetAuthenticationInfo()).CreateItemManager();

                // 3. 重複檢查(同群組 + 同 SKU + 同客戶)
                bool exists = DrKnowAll.GetProduct().Any(o =>
                    o.GroupUID == groupUID
                    && !string.IsNullOrEmpty(o.ID) && o.ID.Equals(sku, StringComparison.OrdinalIgnoreCase)
                    && o.CustomerUID == customerUID);
                if (exists)
                    return this.GetFailureResult(-1, "產品已存在: " + sku);

                // 4. 取/建分類(Transload)
                var categoryUID = this.tlEnsureCategory(groupUID, "Transload");
                if (categoryUID == Guid.Empty)
                    return this.GetFailureResult(-1, "No category for group.");

                // 5~7. 建產品 + 版本 + 三層包裝(同交易;本機無 MSDTC 不開交易,提早 return 走 finally Dispose 未 Complete → rollback)
                var itemUID = Guid.NewGuid();
                bool isLocal = bool.TryParse(ConfigurationManager.AppSettings["YAEP.WMS.API.IsLocal"], out var parsedLocal) && parsedLocal;
                TransactionScope scope = !isLocal ? new TransactionScope() : null;
                try
                {
                    var item = new YAEP.Core.Item.Models.ItemModel
                    {
                        UID = itemUID,
                        GroupUID = groupUID,
                        ID = sku,
                        Name = sku,
                        Status = 1, // Active
                        Type = 1,
                    };
                    var properties = new List<YAEP.Core.Item.Models.ItemPropertiesModel>
                    {
                        new YAEP.Core.Item.Models.ItemPropertiesModel
                        {
                            ItemUID = itemUID,
                            DataType = (int)YAEP.Core.Item.Constants.ItemDataTypes.STRING,
                            Name = "CustomerUID",
                            Value = customerUID.ToString(),
                        },
                    };
                    var createResult = itemManager.Create(item, properties, categoryUID);
                    if (createResult == null || !createResult.Success)
                        return this.GetFailureResult(-1, "Create product failed: " + (createResult?.Message ?? "(null)") + " [cat=" + categoryUID + "]");

                    // 包裝版本(放進版本快取,避免後續包裝取不到版本)
                    var versionUID = this.tlHandlePackageVersion(itemUID, sku);
                    if (versionUID == Guid.Empty)
                        return this.GetFailureResult(-1, "Create package version failed.");

                    // 預設三層 PALLET→BOX→EACH
                    if (!this.tlCreateDefaultPackages(itemUID, versionUID))
                        return this.GetFailureResult(-1, "Create default packages failed.");

                    scope?.Complete();
                }
                finally { scope?.Dispose(); }

                // 8. 完整刷新快取(best-effort):Redis 產品/分類/包裝 + 重載 MemoryCache 的 product-package
                try
                {
                    DrKnowAll.RefreshProductCategoryRelation(itemUID);
                    DrKnowAll.RefreshProduct(itemUID, isRefreshProductCategory: true, isRefreshProductCategoryRelation: true);
                    DrKnowAll.RefreshPackageByItem(itemUID, isRefreshPackageUOM: true, isRefreshPackageVersion: true);
                    this.DIContainer.ManifestFactory.CreateManger().OrderManager.ReloadProductPackageCache();
                }
                catch { /* 刷新失敗忽略,下次重載會補 */ }

                return this.Json(this.GetSuccessResult(new { ItemUID = itemUID, Sku = sku, CustomerPartyName = customerPartyName }));
            }
            catch (Exception ex)
            {
                return this.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>取得 token 使用者所屬的全部群組 UID（供清單依群組過濾用）。</summary>
        private List<Guid> tlGetGroupUIDsByUser()
        {
            var authInfo = base.GetAuthenticationInfo();
            var mgr = this.GetIdentityFactory().CreateGroupManager();
            var r = mgr.GetGroupKeysByUser(authInfo.UID);
            return (r != null && r.Success ? r.Content : null)?.ToList() ?? new List<Guid>();
        }

        private Guid tlGetDefaultGroupUID()
        {
            var authInfo = base.GetAuthenticationInfo();
            var mgr = this.GetIdentityFactory().CreateGroupManager();
            var r = mgr.GetGroupKeysByUser(authInfo.UID);
            var groupUIDs = (r != null && r.Success ? r.Content : null)?.ToList();
            if (groupUIDs == null || groupUIDs.Count == 0) return Guid.Empty;

            // 產品依慣例掛「公司層」(YAEP_Group.Type=200，如 Trucking2000)。
            // GetGroupKeysByUser 第一個 group 可能是 team(800)/warehouse(400)，直接取會把產品掛錯層級。
            // → 先看使用者群組是否已含公司層(200)；否則從第一個群組沿 ParentUID 往上走到 200。
            const int CompanyGroupType = 200;
            var direct = groupUIDs.Select(uid => DrKnowAll.GetGroup(uid))
                                  .FirstOrDefault(g => g != null && g.Type == CompanyGroupType);
            if (direct != null) return direct.UID;

            var cur = DrKnowAll.GetGroup(groupUIDs[0]);
            int guard = 0;
            while (cur != null && cur.Type != CompanyGroupType && guard++ < 20)
            {
                cur = (!cur.ParentUID.HasValue || cur.ParentUID.Value == Guid.Empty) ? null : DrKnowAll.GetGroup(cur.ParentUID.Value);
            }
            if (cur != null && cur.Type == CompanyGroupType) return cur.UID;

            // 找不到公司層 → 退回原行為(第一個群組)，至少不擋流程
            return groupUIDs[0];
        }

        private Guid tlEnsureCategory(Guid groupUID, string categoryName)
        {
            var manager = FactoryUtils.GetItemFactory(base.GetAuthenticationInfo()).CreateItemManager();
            var p = FactoryUtils.GetItemFactory(base.GetAuthenticationInfo()).CreateItemCategoryParameters();
            p.GroupUID = groupUID;
            p.ID = categoryName;
            var res = manager.GetCategories(p);
            var cat = (res != null && res.Success ? res.Content : null)?.FirstOrDefault(o => o.ID.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (cat != null) return cat.UID;
            var newCat = new YAEP.Core.Item.Models.ItemCategoryModel
            {
                UID = Guid.NewGuid(),
                GroupUID = groupUID,
                ID = categoryName,
                Name = categoryName,
                Description = categoryName,
                Status = 1,
            };
            var cr = manager.CreateCategory(newCat);
            return (cr != null && cr.Success) ? (cr.Content?.UID ?? Guid.Empty) : Guid.Empty;
        }

        private Guid tlHandlePackageVersion(Guid itemUID, string versionId)
        {
            var mgr = this.GetPackageFactory().CreatePackageVersionManager(new VersionSerialNumberGenerator(base.DIContainer.GetSequenceAgent()));
            var r = mgr.AddPackageVersion(itemUID, versionId);
            if (r != null && r.Success) DrKnowAll.RefreshPackageVersion(r.Content);
            return r?.Content ?? Guid.Empty;
        }

        private Guid tlHandlePackageUOM(string uom)
        {
            bool uomExists = DrKnowAll.GetPackageUom()?.Any(o => o.ID.Equals(uom, StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!uomExists)
            {
                var uomManager = this.GetPackageFactory().CreatePackageUomManager();
                var createUomResult = uomManager.CreateUom(uom, uom);
                if (createUomResult != null && createUomResult.Success)
                    DrKnowAll.RefreshPackageUom(createUomResult.Content);
            }
            var model = DrKnowAll.GetPackageUom()?.FirstOrDefault(o => o.ID.Equals(uom, StringComparison.OrdinalIgnoreCase));
            return model?.UID ?? Guid.Empty;
        }

        /// <summary>建立預設三層巢狀包裝 PALLET → BOX → EACH(Quantity=1、尺寸 0)。任一步失敗回 false。</summary>
        private bool tlCreateDefaultPackages(Guid itemUID, Guid versionUID)
        {
            var packageManager = this.GetPackageFactory().CreatePackageManager();
            var levels = new[] { "PALLET", "BOX", "EACH" };
            Guid? parentUID = null; // root(PALLET) 的 ParentUID 須為 null
            foreach (var uom in levels)
            {
                var uomUID = this.tlHandlePackageUOM(uom);
                if (uomUID == Guid.Empty) return false;
                var package = new YAEP.Package.Models.PackageModel
                {
                    UID = Guid.NewGuid(),
                    ParentUID = parentUID,
                    VersionUID = versionUID,
                    ItemUID = itemUID,
                    UOM = uomUID,
                    ID = uom,
                    Name = uom,
                    Length = 0,
                    Width = 0,
                    Height = 0,
                    GrossWeight = 0,
                    Quantity = 1,
                    Status = 1, // Active
                    Type = 1,
                };
                var addResult = packageManager.AddPackage(package);
                if (addResult == null || !addResult.Success) return false;
                parentUID = package.UID;
            }
            return true;
        }

        /// <summary>
        /// 出貨完成(揀貨)：查該 outbound 單的 WorkOrder_Payload → Order/PickAll → manifest 達 500、onhand 移 OutboundTemp(從可用扣除)。
        /// 完整出貨 = 先 Allocated(建單配貨) 再 CompleteOutbound(揀貨完成)。
        /// </summary>
        [HttpPost]
        [ActionName("CompleteOutbound")]
        public IHttpActionResult CompleteOutbound([FromUri] string refNo)
        {
            InitDIRoot();
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            var wpls = new List<Guid>();
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"SELECT wpl.UID FROM WMS_WorkOrder_Payload wpl
                    JOIN WMS_WorkOrder wo ON wo.UID = wpl.WorkOrderUID
                    JOIN WMS_Manifest m ON m.UID = wo.ManifestUID
                    WHERE m.RefNo = @ref AND m.Type = 2", cn))
            {
                cmd.Parameters.AddWithValue("@ref", refNo);
                cn.Open();
                using (var rd = cmd.ExecuteReader()) { while (rd.Read()) { wpls.Add(rd.GetGuid(0)); } }
            }
            if (wpls.Count == 0) return this.GetFailureResult(-1, "找不到出貨工作單(請先 Allocated)");
            Func<int> getStatus = () =>
            {
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT Status FROM WMS_Manifest WHERE RefNo = @ref AND Type = 2", cn))
                {
                    cmd.Parameters.AddWithValue("@ref", refNo);
                    cn.Open();
                    var o = cmd.ExecuteScalar();
                    return (o != null && o != DBNull.Value) ? Convert.ToInt32(o) : -1;
                }
            };
            int status = getStatus();
            // PickAll 內 BatchAddLog→GetTransactionLogType() 需要 TransactionInfo 三段都設妥,否則拖垮交易;比照正規流程設定。重試到 manifest=500。
            for (int attempt = 0; attempt < 4 && status != 500; attempt++)
            {
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
                {
                    var req = new PickAllRequest { RefNo = refNo, RequestFunction = 2, ChangeStatus = 0, ItemRefUID = wpls, RequestBy = "transload" };
                    try
                    {
                        _instance.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.Web;
                        _instance.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                        _instance.TracingAgent.TransactionInfo.Action = TransactionlogAction.PickAll;
                        _instance.TracingAgent.BeginTracing("", req); // 對齊 OrderController.PickAll(初始化 transaction log context)
                        var pr = _instance.PickAll(req);
                        _instance.TracingAgent.EndTracing(pr);
                    }
                    catch { /* 以 manifest 狀態為準 */ }
                }
                status = getStatus();
            }
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, ManifestStatus = status, Completed = (status == 500) }));
        }

        /// <summary>
        /// 作廢進貨(Void,非實刪)：把 manifest+BOL 設 Void(Status=0),並退掉這批收貨在 home slot 的 onhand
        /// (依 WMS_PayLoad 收貨量,經 WMS_HomeAddressRelation 精準定位該料 home 儲位扣回,扣到 0 為止)。交易保護。
        /// 註:假設一料一 home 儲位(transload 測試倉模型);多 home 儲位需再細分。
        /// </summary>
        [HttpPost]
        [ActionName("VoidInbound")]
        public IHttpActionResult VoidInbound([FromUri] string refNo)
        {
            InitDIRoot();
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                Guid mid;
                using (var c0 = new SqlCommand("SELECT TOP 1 UID FROM WMS_Manifest WHERE RefNo=@ref AND Type=1", cn))
                {
                    c0.Parameters.AddWithValue("@ref", refNo);
                    var o = c0.ExecuteScalar();
                    if (o == null || o == DBNull.Value) return this.GetFailureResult(-1, "找不到進貨單(或已作廢)");
                    mid = (Guid)o;
                }
                using (var cmd = new SqlCommand(@"
SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;
BEGIN TRAN;
;WITH recv AS (
  SELECT pl.ItemUID, SUM(pl.Quantity) AS Qty
  FROM WMS_PayLoad pl JOIN WMS_Vessel v ON v.UID=pl.VesselUID
  JOIN WMS_BOL b ON b.UID=v.BolUID
  WHERE b.ManifestUID=@mid AND pl.Type=1 AND pl.Status>0
  GROUP BY pl.ItemUID
),
tgt AS (
  SELECT inv.UID, ROW_NUMBER() OVER (PARTITION BY inv.ItemUID ORDER BY inv.Qty DESC) AS rn
  FROM WMS_Inventory inv
  JOIN recv r ON r.ItemUID = inv.ItemUID
  JOIN WMS_HomeAddressRelation hr ON hr.ItemUID = inv.ItemUID AND hr.SlotUID = inv.SlotUID
  WHERE inv.Status > 0
)
UPDATE inv SET inv.Qty = CASE WHEN inv.Qty >= r.Qty THEN inv.Qty - r.Qty ELSE 0 END
FROM WMS_Inventory inv
JOIN tgt t ON t.UID = inv.UID AND t.rn = 1
JOIN recv r ON r.ItemUID = inv.ItemUID;
UPDATE WMS_BOL SET Status=0 WHERE ManifestUID=@mid;
UPDATE WMS_Manifest SET Status=0 WHERE UID=@mid;
COMMIT;", cn))
                {
                    cmd.Parameters.AddWithValue("@mid", mid);
                    cmd.ExecuteNonQuery();
                }
            }
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, Voided = true }));
        }

        /// <summary>
        /// 作廢出貨(Void,非實刪)：把出貨 manifest+BOL 設 Void(Status=0),並把已揀出的量加回 home slot onhand
        /// (依 WMS_WorkOrder_Payload.Qty,經 WMS_HomeAddressRelation 定位該料 home 儲位)。交易保護。
        /// </summary>
        [HttpPost]
        [ActionName("VoidOutbound")]
        public IHttpActionResult VoidOutbound([FromUri] string refNo)
        {
            InitDIRoot();
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                Guid mid;
                using (var c0 = new SqlCommand("SELECT TOP 1 UID FROM WMS_Manifest WHERE RefNo=@ref AND Type=2", cn))
                {
                    c0.Parameters.AddWithValue("@ref", refNo);
                    var o = c0.ExecuteScalar();
                    if (o == null || o == DBNull.Value) return this.GetFailureResult(-1, "找不到出貨單(或已作廢)");
                    mid = (Guid)o;
                }
                using (var cmd = new SqlCommand(@"
SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;
BEGIN TRAN;
;WITH shp AS (
  SELECT wpl.ItemUID, SUM(wpl.Qty) AS Qty
  FROM WMS_WorkOrder_Payload wpl
  JOIN WMS_WorkOrder wo ON wo.UID=wpl.WorkOrderUID
  WHERE wo.ManifestUID=@mid AND wpl.Status > 0
  GROUP BY wpl.ItemUID
),
tgt AS (
  SELECT inv.UID, ROW_NUMBER() OVER (PARTITION BY inv.ItemUID ORDER BY inv.Qty DESC) AS rn
  FROM WMS_Inventory inv
  JOIN shp s ON s.ItemUID = inv.ItemUID
  JOIN WMS_HomeAddressRelation hr ON hr.ItemUID = inv.ItemUID AND hr.SlotUID = inv.SlotUID
  WHERE inv.Status > 0
)
UPDATE inv SET inv.Qty = inv.Qty + s.Qty
FROM WMS_Inventory inv
JOIN tgt t ON t.UID = inv.UID AND t.rn = 1
JOIN shp s ON s.ItemUID = inv.ItemUID;
UPDATE WMS_BOL SET Status=0 WHERE ManifestUID=@mid;
UPDATE WMS_Manifest SET Status=0 WHERE UID=@mid;
COMMIT;", cn))
                {
                    cmd.Parameters.AddWithValue("@mid", mid);
                    cmd.ExecuteNonQuery();
                }
            }
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, Voided = true }));
        }

        /// <summary>
        /// 進貨完成(上架 put-away)：收貨(UploadTicketDataByPod)後,貨在 InboundTemp 暫存。本端點把 Type300 上架 ticket 收尾:
        /// 指派作業員(AddWorker,否則 CompleteTicketData 的 INNER JOIN Assignee_Relation 撈不到) → ChangeToSlot(指定正式儲位)
        /// → CompleteTicket(以 WMS_Ticket.ID 字串比對) → 貨從 InboundTemp 移到正式 slot、manifest 達 500、Type300 ticket=600。
        /// 完整進貨 = Receiving → UploadTicketDataByPod(逐 barcode) → CompleteInbound。
        /// </summary>
        [HttpPost]
        [ActionName("CompleteInbound")]
        public IHttpActionResult CompleteInbound([FromUri] string refNo, [FromUri] string slot = null, [FromUri] Guid? groupUID = null)
        {
            InitDIRoot();
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            // 預設作業員群組:沿用既有完成進貨(5581357)的 CAP/21WPTX 群組;前端授權方案定案後改傳真實群組
            var group = groupUID ?? new Guid("b88631c1-84f5-46de-9294-56c2e694e0b4");

            var ticketIds = new List<string>();
            var ticketInfoUIDs = new List<Guid>();
            var warehouseUID = Guid.Empty;
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"SELECT t.UID AS TUID, t.ID AS TID, ti.UID AS TIUID, m.WarehouseUID
                    FROM WMS_Ticket t
                    JOIN WMS_TicketInfo ti ON ti.TicketUID = t.UID
                    JOIN WMS_WorkOrder wo ON wo.UID = t.WorkOrderUID
                    JOIN WMS_Manifest m ON m.UID = wo.ManifestUID
                    WHERE m.RefNo = @ref AND m.Type = 1 AND t.Type = 300 AND t.Status < 600 AND ti.Status < 600", cn))
            {
                cmd.Parameters.AddWithValue("@ref", refNo);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var tid = rd.GetString(1);
                        if (!ticketIds.Contains(tid)) ticketIds.Add(tid);
                        ticketInfoUIDs.Add(rd.GetGuid(2));
                        if (warehouseUID == Guid.Empty) warehouseUID = rd.GetGuid(3);
                    }
                }
            }

            Func<int> getStatus = () =>
            {
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT Status FROM WMS_Manifest WHERE RefNo = @ref AND Type = 1", cn))
                {
                    cmd.Parameters.AddWithValue("@ref", refNo);
                    cn.Open();
                    var o = cmd.ExecuteScalar();
                    return (o != null && o != DBNull.Value) ? Convert.ToInt32(o) : -1;
                }
            };

            if (ticketInfoUIDs.Count == 0)
            {
                // 沒有待上架的 Type300 ticket → 可能已完成或尚未收貨
                int st0 = getStatus();
                return this.Json(this.GetSuccessResult(new { RefNo = refNo, ManifestStatus = st0, Completed = (st0 == 500), Message = "無待上架 ticket(請先 Receiving+UploadTicketDataByPod,或已完成)" }));
            }

            // 目標儲位:指定 slot 或取該倉「可揀」正式儲位。配貨引擎只從 Status=100 的可揀 slot 取貨,
            // 故優先取 Status=100(如 ZZ);避免取到 SP/SV(Status=400 暫存區,配不到)。
            string targetSlot = slot;
            if (string.IsNullOrEmpty(targetSlot))
            {
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT TOP 1 Name FROM WMS_Slot WHERE WarehouseUID = @w AND Type = 300 AND Status > 0 ORDER BY (CASE WHEN Status = 100 THEN 0 ELSE 1 END), Name", cn))
                {
                    cmd.Parameters.AddWithValue("@w", warehouseUID);
                    cn.Open();
                    targetSlot = (cmd.ExecuteScalar() as string) ?? "A1";
                }
            }

            // 1) 指派作業員(讓 CompleteTicketData 撈得到 ticketinfo)
            using (var _t = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                _t.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                _t.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                _t.AddWorkder(new MaintainWorkderParameters { TicketInfoUID = ticketInfoUIDs.ToArray(), GroupUID = new[] { group } });
            }
            // 2) 指定每個 ticketinfo 的目的正式儲位
            foreach (var tiu in ticketInfoUIDs)
            {
                using (var _t = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
                {
                    _t.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                    _t.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                    _t.TracingAgent.BeginTracing("", tiu);
                    _t.ChangeToSlot(new ChangeToSlotParameter { TicketInfoUID = tiu, SlotName = targetSlot });
                }
            }
            // 3) 完成上架 ticket(以 WMS_Ticket.ID 字串)
            string completeMsg = null; bool completeSuccess = false;
            using (var _t = this.DIContainer.ManifestFactory.CreateManger().TicketManager)
            {
                _t.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.APP;
                _t.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.General;
                _t.TracingAgent.TransactionInfo.Action = TransactionlogAction.Move;
                var cr = _t.CompleteTicketData(ticketIds.ToArray());
                completeSuccess = cr != null && cr.Success;
                completeMsg = cr == null ? "(null)" : cr.Message;
            }

            // 4) 自動建 home address(收到的每個品項 → 上架 slot)。配貨規劃器(TransloadAllocatePlanner)
            //    只從「有 home address 對照的可揀 slot」取 onhand;transload 新收的品項若無 home address 會配不到、出不了貨。
            //    這步讓收的貨立即可被出貨配貨(WMS 本機自足,無需外部設定)。
            try
            {
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand(@"
DECLARE @slotUID uniqueidentifier = (SELECT TOP 1 UID FROM WMS_Slot WHERE WarehouseUID=@w AND Name=@slot);
IF @slotUID IS NOT NULL
INSERT INTO WMS_HomeAddressRelation(UID, ItemCategoryUID, ItemUID, SlotUID, Type, OutboundType, Sequence, Status, CreatedBy, CreatedOn)
SELECT NEWID(), 0x0, x.ItemUID, @slotUID, 1, 0, 1, 100, 'transload', GETDATE()
FROM (SELECT DISTINCT pl.ItemUID FROM WMS_PayLoad pl
      JOIN WMS_Vessel v ON v.UID = pl.VesselUID
      JOIN WMS_BOL b ON b.UID = v.BolUID
      JOIN WMS_Manifest m ON m.UID = b.ManifestUID
      WHERE m.RefNo = @ref AND m.Type = 1 AND pl.ItemUID IS NOT NULL) x
WHERE NOT EXISTS (SELECT 1 FROM WMS_HomeAddressRelation h WHERE h.ItemUID = x.ItemUID AND h.SlotUID = @slotUID AND h.Status > 0);", cn))
                {
                    cmd.Parameters.AddWithValue("@w", warehouseUID);
                    cmd.Parameters.AddWithValue("@slot", targetSlot);
                    cmd.Parameters.AddWithValue("@ref", refNo);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* home address 補建非致命,不擋收貨完成 */ }

            int status = getStatus();
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, ManifestStatus = status, Completed = (status == 500), Slot = targetSlot, CompleteTicketSuccess = completeSuccess, CompleteTicketMessage = completeMsg }));
        }

        /// <summary>
        /// 寫入容器(櫃)屬性到該收貨 manifest 的 Vessel:ConNo(→Vessel.RefNo)/SealNo/ContainerSize/LoadingType/StackableType/ArrivalDate/Weight/Volume。
        /// (對外欄位名 ContainerSize=櫃尺寸;相容舊 request key ContainerType;實體欄位即 WMS_Vessel.ContainerSize。)
        /// Transload 專用路徑(不動核心 Receiving)。核心 Receiving 每 manifest 建 1 個 Vessel → 單櫃直接寫;多櫃目前寫主櫃(N-Vessel 複製為後續)。
        /// 寫入後 ArrivalDate 可供 aging、Vessel.RefNo=ConNo 讓庫存櫃明細(GetInventoryContainers)顯示真 ConNo。
        /// </summary>
        [HttpPost]
        [ActionName("SetContainerInfo")]
        public IHttpActionResult SetContainerInfo([FromUri] string refNo, [FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            Func<string, object> S = k => { var t = body?[k]; return (t == null || t.Type == Newtonsoft.Json.Linq.JTokenType.Null) ? null : (object)(string)t; };
            Func<string, object> I = k => { var t = body?[k]; return (t == null || t.Type == Newtonsoft.Json.Linq.JTokenType.Null) ? (object)DBNull.Value : (object)(int)t; };
            Func<string, object> D = k => { var t = body?[k]; return (t == null || t.Type == Newtonsoft.Json.Linq.JTokenType.Null) ? (object)DBNull.Value : (object)(decimal)t; };
            object conNo = S("ConNo");
            object arrival; { var t = body?["ArrivalDate"]; arrival = (t == null || t.Type == Newtonsoft.Json.Linq.JTokenType.Null || string.IsNullOrEmpty((string)t)) ? (object)DBNull.Value : (object)(DateTime)t; }
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            int rows;
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"UPDATE v SET
                        v.RefNo = CASE WHEN @conNo IS NULL THEN v.RefNo ELSE @conNo END,
                        v.SealNo = @sealNo, v.ContainerSize = @ct, v.LoadingType = @lt, v.StackableType = @st,
                        v.ArrivalDate = @arrival, v.Weight = @weight, v.Volume = @volume, v.ModifiedOn = GETUTCDATE()
                    FROM WMS_Vessel v
                    JOIN WMS_BOL b ON b.UID = v.BolUID
                    JOIN WMS_Manifest m ON m.UID = b.ManifestUID
                    WHERE m.RefNo = @ref AND m.Type = 1", cn))
            {
                cmd.Parameters.AddWithValue("@ref", refNo);
                cmd.Parameters.AddWithValue("@conNo", conNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@sealNo", S("SealNo") ?? DBNull.Value);
                // 對外欄位名為 ContainerSize(櫃尺寸);相容舊 request key ContainerType。實體欄位即寫 WMS_Vessel.ContainerSize。
                var ctVal = I("ContainerSize"); if (ctVal is DBNull) ctVal = I("ContainerType");
                cmd.Parameters.AddWithValue("@ct", ctVal);
                cmd.Parameters.AddWithValue("@lt", I("LoadingType"));
                cmd.Parameters.AddWithValue("@st", I("StackableType"));
                cmd.Parameters.AddWithValue("@arrival", arrival);
                cmd.Parameters.AddWithValue("@weight", D("Weight"));
                cmd.Parameters.AddWithValue("@volume", D("Volume"));
                cn.Open();
                rows = cmd.ExecuteNonQuery();
            }
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, VesselsUpdated = rows }));
        }

        /// <summary>
        /// 寫入每個 SKU(品項) 的 Loading Type / Stackable。**不動正式機 schema**:WMS_PayLoad 無 LoadingType/StackableType 欄,
        /// 故存進既有 `WMS_PayLoad.Description` 的 JSON(鍵 lt/st),read-merge-write(既有 JSON 合併;非 JSON 內容則以新 JSON 取代)。
        /// body: { Items: [ { ItemNo, LoadingType, StackableType }, ... ] }。per-SKU 屬性(非櫃層)。
        /// </summary>
        [HttpPost]
        [ActionName("SetItemLoadingType")]
        public IHttpActionResult SetItemLoadingType([FromUri] string refNo, [FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            if (string.IsNullOrEmpty(refNo)) return this.BadRequest("refNo required");
            var items = body?["Items"] as Newtonsoft.Json.Linq.JArray;
            if (items == null || items.Count == 0) return this.Json(this.GetSuccessResult(new { RefNo = refNo, ItemsUpdated = 0 }));
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            int updated = 0;
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                foreach (var it in items)
                {
                    var itemNo = (string)it["ItemNo"];
                    if (string.IsNullOrEmpty(itemNo)) continue;
                    var ltTok = it["LoadingType"];
                    var stTok = it["StackableType"];
                    int? lt = (ltTok == null || ltTok.Type == Newtonsoft.Json.Linq.JTokenType.Null) ? (int?)null : (int)ltTok;
                    int? st = (stTok == null || stTok.Type == Newtonsoft.Json.Linq.JTokenType.Null) ? (int?)null : (int)stTok;
                    if (!lt.HasValue && !st.HasValue) continue;

                    var rows = new List<KeyValuePair<Guid, string>>();
                    using (var sel = new SqlCommand(@"
SELECT pl.UID, pl.Description
FROM WMS_PayLoad pl
JOIN WMS_Vessel v ON v.UID = pl.VesselUID
JOIN WMS_BOL b ON b.UID = v.BolUID
JOIN WMS_Manifest m ON m.UID = b.ManifestUID
JOIN YAEP_Item it ON it.UID = pl.ItemUID
WHERE m.RefNo = @ref AND m.Type = 1 AND pl.Type = 1 AND it.ID = @itemNo", cn))
                    {
                        sel.Parameters.AddWithValue("@ref", refNo);
                        sel.Parameters.AddWithValue("@itemNo", itemNo);
                        using (var rd = sel.ExecuteReader())
                        {
                            while (rd.Read())
                                rows.Add(new KeyValuePair<Guid, string>(rd.GetGuid(0), rd.IsDBNull(1) ? null : rd.GetString(1)));
                        }
                    }
                    foreach (var row in rows)
                    {
                        Newtonsoft.Json.Linq.JObject j;
                        try { j = string.IsNullOrWhiteSpace(row.Value) ? new Newtonsoft.Json.Linq.JObject() : Newtonsoft.Json.Linq.JObject.Parse(row.Value); }
                        catch { j = new Newtonsoft.Json.Linq.JObject(); }
                        if (lt.HasValue) j["lt"] = lt.Value;
                        if (st.HasValue) j["st"] = st.Value;
                        using (var upd = new SqlCommand("UPDATE WMS_PayLoad SET Description = @d WHERE UID = @u", cn))
                        {
                            upd.Parameters.AddWithValue("@d", j.ToString(Newtonsoft.Json.Formatting.None));
                            upd.Parameters.AddWithValue("@u", row.Key);
                            updated += upd.ExecuteNonQuery();
                        }
                    }
                }
            }
            return this.Json(this.GetSuccessResult(new { RefNo = refNo, ItemsUpdated = updated }));
        }

        /// <summary>
        /// 庫存「各櫃明細」(主從展開用)。payload 層:該料在各 ConNo(櫃)/儲位的 onhand。UI 規格：§7.3.2。
        /// </summary>
        [HttpGet]
        [ActionName("GetInventoryContainers")]
        public IHttpActionResult GetInventoryContainers([FromUri] Guid itemUID, [FromUri] Guid? warehouseUID = null)
        {
            var cs = ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            var list = new List<object>();
            // Received：收貨流程未寫 WMS_PayLoad.ReceivedDate(常 null) → 退而求其次取 Vessel.ArrivalDate(SetContainerInfo 寫的到倉日),
            //           再退到 p.CreatedOn(收貨建單時間,一定有值)。純讀既有欄位,不動 schema。
            var sql = @"SELECT v.RefNo AS Vessel, sl.Name AS Slot, sl.WarehouseUID,
                               SUM(p.Quantity) AS Qty,
                               COALESCE(MIN(p.ReceivedDate), MIN(v.ArrivalDate), MIN(p.CreatedOn)) AS Received,
                               MIN(p.CreatedOn) AS CreatedOn
                        FROM WMS_PayLoad p
                        LEFT JOIN WMS_Vessel v ON v.UID = p.VesselUID
                        LEFT JOIN WMS_Slot sl ON sl.UID = p.SlotUID
                        WHERE p.ItemUID = @item AND p.Type = 1 AND p.Status = 500
                          AND (@wh IS NULL OR sl.WarehouseUID = @wh)
                        GROUP BY v.RefNo, sl.Name, sl.WarehouseUID
                        HAVING SUM(p.Quantity) > 0
                        ORDER BY v.RefNo";
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@item", itemUID);
                cmd.Parameters.AddWithValue("@wh", (object)warehouseUID ?? DBNull.Value);
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        Func<string, object> g = name => { var i = rd.GetOrdinal(name); return rd.IsDBNull(i) ? null : rd.GetValue(i); };
                        var vessel = (string)g("Vessel");
                        var conNo = string.IsNullOrEmpty(vessel) ? null : vessel.Replace("Vessel ", "");
                        var received = (DateTime?)g("Received");
                        list.Add(new
                        {
                            ConNo = conNo,
                            Slot = (string)g("Slot"),
                            OnHandPcs = Convert.ToInt32(g("Qty") ?? 0),
                            ReceivedDate = received?.ToString("yyyy-MM-dd"),
                        });
                    }
                }
            }
            return this.Json(this.GetSuccessResult(list));
        }

        private static Dictionary<Guid, string> BuildCustomerNameMap()
        {
            var d = new Dictionary<Guid, string>();
            foreach (var c in DrKnowAll.GetCustomer()) { d[c.UID] = c.Name; }
            return d;
        }
        private static Dictionary<Guid, string> BuildWarehouseNameMap()
        {
            var d = new Dictionary<Guid, string>();
            foreach (var w in DrKnowAll.GetWarehouse()) { d[w.UID] = w.Name; }
            return d;
        }

        private readonly Lazy<PackageFactory> _PackageFactory;
        private PackageFactory GetPackageFactory()
        {
            return this._PackageFactory.Value;
        }
    }
}
