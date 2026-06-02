using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    /// 庫存相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Inventory")]
    public class InventoryController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public InventoryController()
        {
            this._PackageFactory = new Lazy<PackageFactory>(() => FactoryUtils.GetPackageFactory(base.GetAuthenticationInfo()));

        }
        /// <summary>
        /// get inventory list
        /// </summary>
        /// <param name="parameters">search parameter</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetInventory")]
        public IHttpActionResult GetInventory(InventorySearchParameters parameters)
        {
            InitDIRoot();
            var manager = this.DIContainer.InventoryFactory.CreateInventoryManager();

            var result = manager.GetInventory(parameters);

            if (result.Success)
            {
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

                    return new
                    {

                        UID = Guid.NewGuid(),
                        i.PackageUID,
                        i.WarehouseUID,
                        i.WarehouseName,
                        i.InboundQty,
                        // Item
                        i.ItemUID,
                        i.ItemName,
                        // Customer Info
                        i.CustomerUID,
                        i.CustomerID,
                        i.CustomerName,
                        // Package name
                        PackageName = packageName,
                        // Package Tree expand text
                        PackageTree = packageTree,
                        // UOM 
                        UOM = uomName,
                        OutboundQty = allocateReceiveQty,
                        Onhand = i.InboundQty - allocateReceiveQty,
                        // Type name
                        TypeName = YAEP.Utilities.EnumerableData.GetName<InventoryType>(i.Type),
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
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// get inventory detail list
        /// </summary>
        /// <param name="itemUID">
        /// unique identifier of product item
        /// <para /><see cref="IInventoryModel.ItemUID"/> or <see cref="IItemModel.UID"/>
        /// </param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetInventoryDetail")]
        public IHttpActionResult GetInventoryDetail([FromUri] Guid itemUID)
        {
            InitDIRoot();
            var manager = this.DIContainer.InventoryFactory.CreateInventoryManager();

            var result = manager.GetInventoryDetail(itemUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        [HttpGet]
        [ActionName("GetTranascationTypeList")]
        public IHttpActionResult GetTranascationTypeList()
        {
            InitDIRoot();
            var _instance = this.DIContainer.InventoryFactory.CreateInventoryManager();
            var rs = _instance.GetTranascationTypeList();
            var result = this.GetSuccessResult<List<IEnumFieldInfo>>(rs.Content.ToList());
            return this.Json<APIResult<List<IEnumFieldInfo>>>(result);
        }
        /// <summary>
        /// get inventory list
        /// </summary>
        /// <param name="parameters">search parameter</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetTranascationList")]
        public IHttpActionResult GetTranascationList(PayloadTransactionLogParameters parameters)
        {
            InitDIRoot();
            using (var manager = this.DIContainer.InventoryFactory.CreateInventoryManager())
            {

                var Uoms = DrKnowAll.GetPackageUom();
                var result = manager.GetTranascationList(parameters);
                if (result.Success)
                {
                    var products = DrKnowAll.GetProduct();
                    var customers = DrKnowAll.GetCustomer();
                    var resultObject = result.Content.Join(products, i => i.ItemUID, p => p.UID, (i, p) =>
                    {
                        return new
                        {
                            i.RefNo,
                            i.UniqueKey,
                            i.UID,
                            i.WarehouseUID,
                            i.WarehouseID,
                            i.WarehouseName,
                            i.TargetAreaName,
                            i.TargetBinName,
                            i.TargetSlotName,
                            i.OriginalAreaName,
                            i.OriginalBinName,
                            i.OriginalSlotName,
                            i.CreatedBy,
                            CreatedOn = (i.CreatedOn.HasValue) ? i.CreatedOn : (i.PayloadModifiedOn.HasValue) ? i.PayloadModifiedOn.Value : DateTime.MinValue,
                            i.ItemUID,
                            i.PackageName,
                            ItemNo = p.ID,
                            BeforeQty = i.QtyBeforeTX,
                            AfterQty = i.QtyAfterTX,
                            PackageUID = i.TargetPackage,
                            i.TicketID,
                            i.VesselNo,
                            i.BolNo,
                            i.UOMUID,
                            // Type name
                            ActionTypeName = YAEP.Utilities.EnumerableData.GetName<PayloadTransactionLogTypes>(i.Type),
                            p.CustomerUID,
                            i.PayloadTypeName
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
                        //var getPackageTreeResult = this.GetPackageFactory().CreatePackageManager().GetPackageTree(i.PackageUID);
                        //var packageTree = getPackageTreeResult.Content;
                        //var package = packageTree?.Find(i.PackageUID);
                        return new
                        {
                            i.UniqueKey,
                            i.UID,
                            i.RefNo,
                            i.WarehouseUID,
                            i.PackageUID,
                            i.ItemUID,
                            i.WarehouseName,
                            i.TargetAreaName,
                            i.TargetBinName,
                            i.TargetSlotName,
                            i.OriginalAreaName,
                            i.OriginalBinName,
                            i.OriginalSlotName,
                            i.ItemNo,
                            i.ActionTypeName,
                            i.CreatedBy,
                            i.CreatedOn,
                            // Customer Info
                            i.CustomerUID,
                            CustomerID = c.ID,
                            CustomerName = c.Name,
                            i.PackageName,
                            i.BeforeQty,
                            i.AfterQty,
                            i.TicketID,
                            i.VesselNo,
                            i.BolNo,
                            // UOM 
                            UOM = Uoms.FirstOrDefault(x => x.UID == i.UOMUID)?.Name,
                            i.PayloadTypeName
                        };
                    }).ToList().OrderByDescending(o => o.CreatedOn);

                    if (resultObject.Count() > 0)
                    {
                        var actionResult = this.GetSuccessResult(resultObject);
                        return this.Json(actionResult);
                    }
                    else
                    {
                        return base.GetDataNotFoundResult();
                    }
                }
                else
                {
                    return base.GetDataNotFoundResult();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddInventoryQty")]
        public IHttpActionResult AddInventoryQty(AddInventoryParameters parameters)
        {
            InitDIRoot();
            var manager = this.DIContainer.InventoryFactory.CreateInventoryManager();
            parameters = this.AntiXSSEncode(parameters);
            parameters.Type = InventoryType.Stock;
            var result = manager.AddInventory(parameters, true);

            if (result.Success)
            {
                var apiResult = this.GetSuccessResult();
                return this.Json(apiResult);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.INVENTORY_ADD_QUANTITY_FAIL);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetModifyPayloadList")]
        public IHttpActionResult GetModifyPayloadList(ModifyPayloadListParameters parameters)
        {
            InitDIRoot();

            var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            manager.TracingAgent.BeginTracing();
            parameters = this.AntiXSSEncode(parameters);
            var result = manager.GetModifyPayloadList(parameters);
            var apiResult = this.GetSuccessResult<dynamic>(result.Content);
            if (!result.Success)
            {
                apiResult.IsComplete = result.Success;
                apiResult.Message = result.Message;
            }
            manager.TracingAgent.EndTracing();
            return this.Json(apiResult);
        }
        [HttpGet]
        [ActionName("GetPayloadStatusList")]
        public IHttpActionResult GetPayloadStatusList()
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;

            var result = manager.GetPayloadStatusList();
            var apiResult = this.GetSuccessResult<dynamic>(result);

            return this.Json(apiResult);
        }
        [HttpGet]
        [ActionName("GetPayloadTypeList")]
        public IHttpActionResult GetPayloadTypeList()
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;

            var result = manager.GetPayloadTypeList();
            var apiResult = this.GetSuccessResult<dynamic>(result);

            return this.Json(apiResult);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CreateAdjustmentTicket")]
        public IHttpActionResult CreateAdjustmentTicket(List<CretaeAdjustmentTicket> parameters)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            parameters = this.AntiXSSEncode(parameters);
            manager.TracingAgent.BeginTracing("", parameters);
            manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.Web;
            manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.Adjust;
            //驗証payloadtype 是否非為1 (onhand)
            var checkresult = manager.CheckAdjustmentTicket(parameters);
            if (checkresult.Content)
            {
                var result = manager.CreateAdjustmentTicket(parameters);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }
            else
            {
                var apiResult = this.GetSuccessResult<dynamic>(checkresult.Content);
                apiResult.Message = checkresult.Message;
                return this.Json(apiResult);
            }
        }
        [HttpPost]
        [ActionName("CreateSakanaAdjustmentTicket")]
        public IHttpActionResult CreateSakanaAdjustmentTicket(List<CretaeAdjustmentTicket> parameters)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            parameters = this.AntiXSSEncode(parameters);
            manager.TracingAgent.BeginTracing("", parameters);
            manager.TracingAgent.TransactionInfo.Externalfunction = TransactionlogExternalfunction.Web;
            manager.TracingAgent.TransactionInfo.Subfunction = TransactionlogSubfunction.Adjust;
            var checkresult = manager.CheckSakanaAdjustmentTicket(parameters);
            if (checkresult.Content)
            {
                var result = manager.CreateSakanaAdjustmentTicket(parameters);
                manager.TracingAgent.EndTracing(result);
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }
            else
            {
                var apiResult = this.GetSuccessResult<dynamic>(checkresult.Content);
                apiResult.Message = Resource.WAREHOUSE_MODIFIED_ONHAND_PAYLOAD_TYPE_ILLEGAL;
                return this.Json(apiResult);
            }


        }
        #region Factories

        private readonly Lazy<PackageFactory> _PackageFactory;

        private PackageFactory GetPackageFactory()
        {
            return this._PackageFactory.Value;
        }

        #endregion
        [HttpPost]
        [ActionName("CheckSlot")]
        public IHttpActionResult CheckSlot([FromBody] CheckSlotRequestModel requestModel)
        {
            InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger();
            var result = manager.WarehouseManager.CheckSlot(requestModel.warehouseUID, requestModel.slotNames);
            var apiResult = this.GetSuccessResult<dynamic>(result.Content);
            return this.Json(apiResult);
        }
        [HttpPost]
        [ActionName("ImportTotalSolutionReceivingData")]
        public IHttpActionResult ImportTotalSolutionReceivingData(ImportTotalSolutionReceivingDataRequest data)
        {
            InitDIRoot();
            //var manager = this.DIContainer.InventoryFactory.CreateInventoryManager();
            //IActionResult<IEnumerable<IImportTSReceivingDataResponseModel>> result = manager.ImportTotalSolutionReceivingData(data.data);
            //var apiResult = this.GetSuccessResult<dynamic>(result.Content);
            //apiResult.Message = result.Message;
            //return this.Json(apiResult);
            return Ok();
        }
        [HttpGet]
        [ActionName("CheckItemUsageStatus")]
        public IHttpActionResult CheckItemUsageStatus(Guid itemUID)
        {
            InitDIRoot();
            var manager = this.DIContainer.InventoryFactory.CreateInventoryManager();

            var result = manager.GetItemUsageStatus(itemUID);

            if (result.Success)
            {
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }

            return base.GetDataNotFoundResult();
        }

    }
}