using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Results;
using YAEP.Core.Item.Constants;
using YAEP.Core.Item.DI;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Core.Item.Models;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.DI;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.Package.Constants;
using YAEP.Package.DI;
using YAEP.Package.Interfaces.Models;
using YAEP.Package.Models;
using YAEP.Utilities;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.API.Models.Response;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Model;
using YAEP.WMS.Cache;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Cache.Models;


namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// Product 相關存取資料 API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Product")]
    public class ProductController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public ProductController()
        {
            this._PackageFactory = new Lazy<PackageFactory>(() => FactoryUtils.GetPackageFactory(base.GetAuthenticationInfo()));
            this._ItemFactory = new Lazy<ItemFactory>(() => FactoryUtils.GetItemFactory(base.GetAuthenticationInfo()));
            this._PartyFactory = new Lazy<PartyFactory>(() => FactoryUtils.GetPartyFactory(base.GetAuthenticationInfo()));
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public string Test()
        {
            return $"{this.GetType().Name.Replace("Controller", " - ")}{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}";
        }

        #region Product

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword">Product ID, The length is at least 3</param>
        /// <param name="customerUID">Customer UID</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetProducts")]
        public IHttpActionResult GetProducts([FromUri] string keyword, Guid? customerUID)
        {
            if (String.IsNullOrWhiteSpace(keyword))
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} {nameof(keyword)}");
            }

            // 3 個以上的字元才進行搜尋
            if (keyword.Length >= 3)
            {
                var requestModel = new ProductSearchRequestModel();
                requestModel.ItemID = keyword;
                requestModel.CustomerUID = customerUID;

                try
                {
                    var colletion = this.getCacheProductList(requestModel);
                    var arrItemUID = colletion.Select(o => o.UID).ToArray();
                    var allCachedPackages = this.getCachePackageList(arrItemUID);

                    var list = colletion.Select(o =>
                    {
                        var model = new ProductNameResponseModel()
                        {
                            ItemID = o.ID,
                            ItemName = o.Name,
                            ItemUID = o.UID,
                        };

                        var packages = allCachedPackages.Where(p => p.ItemUID == o.UID).OrderByDescending(x=>x.CreatedOn);
                        foreach (var p in packages)
                        {
                            var package = new ProductNamePackageResponseModel
                            {
                                VersionID = p.VersionId,
                                ItemName = o.ID,
                                PackageName = p.Name,
                                PackageUID = p.UID,
                                ItemUID = p.ItemUID,
                            };
                            model.Package.Add(package);
                        }

                        return model;
                    });

                    list = list.OrderBy(o => o.ItemID);

                    //var list = colletion.OrderBy(o => o.ID);

                    var apiResult = base.GetSuccessResult(list);
                    return base.Json(apiResult);
                }
                catch (Exception ex)
                {
                    return base.GetFailureResult(-1, ex.Message);
                }
            }
            else
            {
                return base.GetFailureResult(-1, "The keyword length must larger than 3.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetManifestProductList")]
        public IHttpActionResult GetManifestProductList([FromBody] ProductSearchRequestModel requestModel)
        {
            try
            {
                var colletion = this.getCacheProductList(requestModel);

                var arrItemUID = colletion.Select(o => o.UID).ToArray();
                var allCachedPackages = this.getCachePackageList(arrItemUID);

                var list = colletion.Select(o =>
                {
                    Guid itemUID = o.UID;
                    var item = new ManifestProductResponseModel
                    {
                        ItemUID = itemUID,
                        ID = o.ID,
                        Name = o.Name,
                        Status = YAEP.Utilities.EnumerableData.GetName<ItemStatus>(o.Status),
                        ImageUID = o.ImageUID,
                        CustomerName = o.CustomerName,
                        Description = o.Description,
                    };

                    if (allCachedPackages.Any(p => p.ItemUID == itemUID))
                    {
                        var packages = allCachedPackages.Where(p => p.ItemUID == itemUID);
                        foreach (var p in packages)
                        {
                            var package = new ManifestProductPackageResponseModel
                            {
                                UID = p.UID,
                                ID = p.ID,
                                Name = p.Name,
                                VersionId = p.VersionId,
                            };
                            item.Packages.Add(package);
                        }
                    }

                    return item;
                });

                list = list.OrderBy(o => o.ID).ToList();

                var apiResult = base.GetSuccessResult(list);
                return base.Json(apiResult);

            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Create Product
        /// </summary>
        /// <param name="product">product model</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddProduct")]
        public IHttpActionResult AddProduct([FromBody] ProductRequestModel product)
        {
            if (product == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            //if (product.CustomerUID == Guid.Empty)
            //{
            //    return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(product.CustomerUID)})");
            //}
            if (product.CategoryUID == Guid.Empty)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} ({nameof(product.CategoryUID)})");
            }
            if (String.IsNullOrWhiteSpace(product.ID))
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_ITEM_ID_EMPTY);
            }
            if (String.IsNullOrWhiteSpace(product.Name))
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_ITEM_NAME_EMPTY);
            }

            if (product.GroupUID == Guid.Empty)
            {
                product.GroupUID = this.getDefaultGroupUID();
                if (product.GroupUID == Guid.Empty)
                {
                    return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(product.GroupUID)})");
                }
            }

            try
            {
                var manager = this.GetItemFactory().CreateItemManager();

                var item = new ItemModel()
                {
                    UID = (product.UID == Guid.Empty ? Guid.NewGuid() : product.UID),
                    GroupUID = product.GroupUID,
                    ID = product.ID,
                    Name = product.Name,
                    Description = product.Description,
                    Status = (int)ItemStatus.Active,
                    Type = 1,
                };

                var properties = this.parseToProductProperties(item.UID, product);

                var result = manager.Create(item, properties, product.CategoryUID);

                if (result.Success)
                {
                    // refresh product category relation
                    DrKnowAll.RefreshProductCategoryRelation(item.UID);
                    // refresh cache
                    DrKnowAll.RefreshProduct(item.UID);

                    var apiResult = base.GetSuccessResult(item);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_ADD_ITEM_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Update Product
        /// </summary>
        /// <param name="product">product model</param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("UpdateProduct")]
        public IHttpActionResult UpdateProduct(ProductRequestModel product)
        {
            if (product == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (product.UID == Guid.Empty)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} ({nameof(product.UID)})");
            }
            //if (product.CustomerUID == Guid.Empty)
            //{
            //    return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} ({nameof(product.CustomerUID)})");
            //}
            //if (product.CategoryUID == Guid.Empty)
            //{
            //    return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} ({nameof(product.CategoryUID)})");
            //}
            if (String.IsNullOrWhiteSpace(product.ID))
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_ITEM_ID_EMPTY);
            }
            if (String.IsNullOrWhiteSpace(product.Name))
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_ITEM_NAME_EMPTY);
            }

            try
            {
                var manager = this.GetItemFactory().CreateItemManager();

                var item = new ItemModel()
                {
                    UID = product.UID,
                    GroupUID = product.GroupUID,
                    ID = product.ID,
                    Name = product.Name,
                    Description = product.Description,
                    Status = (int)ItemStatus.Active,
                    Type = 1,
                };

                var properties = this.parseToProductProperties(item.UID, product);

                // 更新屬性
                var result = manager.Update(item, properties);

                if (result.Success)
                {
                    // 如果有設定 CategoryUID 才會執行更新 Category 程序
                    if (product.CategoryUID != Guid.Empty)
                    {
                        // 檢查是否已經有Category Relation
                        var checkResult = manager.CheckHasCategoryRelation(item.UID, product.CategoryUID);
                        if (checkResult.Content == ResultOfCheckBelongCategory.Yes)
                        {
                            DrKnowAll.RefreshProductCategoryRelation(item.UID);
                        }
                        else
                        {
                            // 更新 Category : 1. 刪除原有 Category 關聯, 2. 新增 Category 關聯  
                            var clearCategoryRelationResult = manager.ClearCategoryRelationByItem(item.UID);

                            var setRelationResult = manager.SetCategoryRelation(item.UID, product.CategoryUID);
                            if (setRelationResult.Success)
                            {
                                DrKnowAll.RefreshProductCategoryRelation(item.UID);
                            }
                            else
                            {
                                // 設定產品分類失敗
                                return base.GetFailureResult(-1, Resource.PRODUCT_UPDATE_ITEM_CATEGORY_FAIL);
                            }
                        }
                    }

                    // refresh cache
                    DrKnowAll.RefreshProduct(product.UID);

                    var apiResult = this.GetSuccessResult(item);
                    return this.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_UPDATE_ITEM_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("SyncItemPackage")]
        public IHttpActionResult SyncItemPackage(List<PBSCItemPackagingModel> products)
        {
            InitDIRoot();
            var manager = DIContainer.ManifestFactory.CreateManger().ManifestManager;
            manager.TracingAgent.BeginTracing("SyncItemPackage", products);
            var result = manager.ProcessPBSCItemAndPackage(products);
            manager.TracingAgent.EndTracing(result);

            if (result != null)
            {
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }

            return base.GetDataNotFoundResult();
        }

        /// <summary>
        ///  Sync Product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Sync")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult Sync([FromBody] ProductSingleSyncRequestModel product)
        {
            if (product == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (String.IsNullOrWhiteSpace(product.ID))
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_ITEM_ID_EMPTY);
            }
            if (String.IsNullOrWhiteSpace(product.Name))
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_ITEM_NAME_EMPTY);
            }
            if (String.IsNullOrWhiteSpace(product.CategoryName))
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} ({nameof(product.CategoryName)})");
            }

            if (product.GroupUID == Guid.Empty)
            {
                product.GroupUID = this.getDefaultGroupUID();
                if (product.GroupUID == Guid.Empty)
                {
                    return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(product.GroupUID)})");
                }
            }
            if (product.CustomerUID == Guid.Empty)
            {
                if (!String.IsNullOrWhiteSpace(product.CustomerID))
                {
                    product.CustomerUID = this.getCustomer(product.GroupUID, product.CustomerID)?.UID ?? Guid.Empty;
                }
            }

            var manager = this.GetItemFactory().CreateItemManager();

            // 處理 Item Category
            Guid categoryUID = Guid.Empty;
            var categoryParameters = this.GetItemFactory().CreateItemCategoryParameters();
            categoryParameters.GroupUID = product.GroupUID;
            categoryParameters.ID = product.CategoryName;
            var searchCategoryResult = manager.GetCategories(categoryParameters);
            var category = searchCategoryResult.Content?.FirstOrDefault(o => o.ID.Equals(product.CategoryName, StringComparison.OrdinalIgnoreCase));
            if (category == null)
            {
                var newCategory = new YAEP.Core.Item.Models.ItemCategoryModel()
                {
                    UID = Guid.NewGuid(),
                    GroupUID = product.GroupUID,
                    ID = product.CategoryName,
                    Name = product.CategoryName,
                    Description = product.CategoryName,
                    Status = 1,
                };

                var createCategoryResult = manager.CreateCategory(newCategory);
                if (createCategoryResult.Success)
                {
                    categoryUID = (createCategoryResult.Content?.UID ?? Guid.Empty);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_ADD_ITEM_FAIL);
                }
            }
            else
            {
                categoryUID = category.UID;
            }

            // 搜尋 Item
            var searchItemResult = manager.GetItem(product.GroupUID, product.ID);
            var foundItem = searchItemResult.Content;

            var item = new ProductRequestModel()
            {
                UID = (foundItem == null ? Guid.NewGuid() : foundItem.UID),
                GroupUID = product.GroupUID,
                CustomerUID = product.CustomerUID,
                CategoryUID = categoryUID,
                ID = product.ID,
                Name = product.Name,
                Description = product.Description,
                UPC = product.UPC,
                EAN = product.EAN,
                IsBOM = product.IsBOM,
                ImageUID = null,
                LengthInch = product.LengthInch,
                WidthInch = product.WidthInch,
                HeightInch = product.HeightInch,
                LengthCM = product.LengthCM,
                WidthCM = product.WidthCM,
                HeightCM = product.HeightCM,
                NetWeightKG = product.NetWeightKG,
                NetWeightLB = product.NetWeightLB,
                GrossWeightKG = product.GrossWeightKG,
                GrossWeightLB = product.GrossWeightLB,
                StockUOM = product.StockUOM,
                PurchaseUOM = product.PurchaseUOM,
                SellingUOM = product.SellingUOM,
                ShipUOM = product.ShipUOM,
            };

            if (foundItem == null)
            {
                // create  
                return this.AddProduct(item);
            }
            else
            {
                // update
                return this.UpdateProduct(item);
            }
        }
        /// <summary>
        ///  Batch Sync Product
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("BatchSync2")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult BatchSync2([FromBody] IEnumerable<ProductSingleSyncRequestModel> products)
        {
            if ((products?.Count() ?? 0) == 0)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(products)})");
            }

            var list = new List<BatchSyncProductResponseModel>();

            foreach (var product in products)
            {
                var model = new BatchSyncProductResponseModel();
                model.ProductId = product.ID;

                try
                {
                    var result = this.Sync(product);

                    if (result is JsonResult<APIResult<ItemModel>>)
                    {
                        var r = result as JsonResult<APIResult<ItemModel>>;
                        model.Success = true;
                        model.Data = r.Content?.Data;
                    }
                    else if (result is JsonResult<APIResult<string>>)
                    {
                        var r = result as JsonResult<APIResult<string>>;
                        model.Success = false;
                        model.Message = r.Content?.Data;
                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {
                    model.Success = false;
                    model.Message = ex.Message;
                }

                list.Add(model);
            }

            var apiResult = base.GetSuccessResult(list);
            return base.Json(apiResult);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="batchRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("BatchSync")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IHttpActionResult BatchSync([FromBody] IEnumerable<ProductBatchSyncRequestModel> batchRequest)
        {
            if ((batchRequest?.Count() ?? 0) == 0)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(batchRequest)})");
            }

            var list = new List<BatchSyncProductResponseModel>();

            foreach (var request in batchRequest)
            {
                Guid groupUID = request.GroupUID ?? Guid.Empty;
                Guid customerUID = Guid.Empty;
                if (groupUID == Guid.Empty)
                {
                    groupUID = this.getDefaultGroupUID();
                    if (groupUID == Guid.Empty)
                    {
                        foreach (var item in request.Data)
                        {
                            list.Add(new BatchSyncProductResponseModel()
                            {
                                Success = false,
                                ProductId = item.ID,
                                Message = "Account no belong any group.",
                            });
                        }
                        continue;
                    }
                }

                if (!String.IsNullOrWhiteSpace(request.CustomerID))
                {
                    customerUID = this.getCustomer(groupUID, request.CustomerID)?.UID ?? Guid.Empty;
                }

                foreach (var product in request.Data)
                {
                    var model = new BatchSyncProductResponseModel();
                    model.ProductId = product.ID;

                    try
                    {
                        var result = this.Sync(new ProductSingleSyncRequestModel()
                        {
                            GroupUID = groupUID,
                            CustomerUID = customerUID,

                            UID = product.UID,
                            ID = product.ID,
                            Name = product.Name,
                            CategoryName = product.CategoryName,
                            UPC = product.UPC,
                            EAN = product.EAN,
                            IsBOM = product.IsBOM,
                            Description = product.Description,
                            LengthInch = product.LengthInch,
                            WidthInch = product.WidthInch,
                            HeightInch = product.HeightInch,
                            LengthCM = product.LengthCM,
                            WidthCM = product.WidthCM,
                            HeightCM = product.HeightCM,
                            NetWeightKG = product.NetWeightKG,
                            NetWeightLB = product.NetWeightLB,
                            GrossWeightKG = product.GrossWeightKG,
                            GrossWeightLB = product.GrossWeightLB,
                            StockUOM = product.StockUOM,
                            PurchaseUOM = product.PurchaseUOM,
                            SellingUOM = product.SellingUOM,
                            ShipUOM = product.ShipUOM,

                        });

                        if (result is JsonResult<APIResult<ItemModel>>)
                        {
                            var r = result as JsonResult<APIResult<ItemModel>>;
                            model.Success = true;
                            model.Data = r.Content?.Data;
                        }
                        else if (result is JsonResult<APIResult<string>>)
                        {
                            var r = result as JsonResult<APIResult<string>>;
                            model.Success = false;
                            model.Message = r.Content?.Data;
                        }
                        else
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        model.Success = false;
                        model.Message = ex.Message;
                    }

                    list.Add(model);
                }
            }

            var apiResult = base.GetSuccessResult(list);
            return base.Json(apiResult);
        }

        /// <summary>
        /// Delete Product
        /// </summary>
        /// <param name="pid">unique identifier of product item</param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteProduct")]
        public IHttpActionResult DeleteProduct([FromUri] Guid pid)
        {
            if (pid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetItemFactory().CreateItemManager();

                var result = manager.Delete(pid);

                if (result.Success)
                {
                    // refresh cache
                    DrKnowAll.RefreshProductCategoryRelation(pid);
                    DrKnowAll.RefreshProduct(pid);

                    var apiResult = base.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_UPDATE_ITEM_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Get Product Information
        /// </summary>
        /// <param name="pid">unique identifier of product item</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetProductInfo")]
        public IHttpActionResult GetProductInfo([FromUri] Guid pid)
        {
            if (pid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            DrKnowAll.RefreshProduct(pid);
            try
            {
                var product = DrKnowAll.GetProduct(pid);

                var apiResult = base.GetSuccessResult(product);
                return base.Json(apiResult);

                //var manager = this.GetItemFactory().CreateItemManager();

                //var result = manager.GetItem(pid);

                //if (result.Success)
                //{
                //    var apiResult = base.GetSuccessResult(result.Content);
                //    return base.Json(apiResult);
                //}
                //else
                //{
                //    return base.GetFailureResult(-1, result.Message);
                //}
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Get product name collection
        /// </summary>
        /// <param name="cuid">unique identifier of customer</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetProductNameList")]
        public IHttpActionResult GetProductNameList([FromUri] Guid cuid)
        {
            if (cuid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var colletion = DrKnowAll.GetProductByCustomer(cuid);
                var list = colletion.Select(o => new ProductNameResponseModel()
                {
                    ItemID = o.ID,
                    ItemName = o.Name,
                    ItemUID = o.UID,
                });
                list = list.OrderBy(o => o.ItemID);

                var apiResult = base.GetSuccessResult(list);
                return base.Json(apiResult);

                //var manager = this.GetItemFactory().CreateItemManager();
                //var parameters = this.GetItemFactory().CreateItemSearchParameters();
                //parameters.ItemProperties.Add(new ItemPropertySearchModel()
                //{
                //    Name = "CustomerUID",
                //    Value = cuid.ToString(),
                //});

                //var result = manager.GetItems(parameters);

                //if (result.Success)
                //{
                //    var list = result.Content.Select(o => new ProductNameResponseModel()
                //    {
                //        ItemNo = o.ID,
                //        ItemUID = o.UID,
                //    });

                //    list = list.OrderBy(o => o.ItemNo);

                //    var apiResult = base.GetSuccessResult(list);
                //    return base.Json(apiResult);
                //}
                //else
                //{
                //    return base.GetFailureResult(-1, result.Message);
                //}
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetProductList")]
        public IHttpActionResult GetProductList([FromBody] ProductSearchRequestModel requestModel)
        {
            if (requestModel == null)
            {
                requestModel = new ProductSearchRequestModel();
            }

            try
            {
                var colletion = this.getCacheProductList(requestModel);

                var list = colletion.Select(o => new
                {
                    ItemUID = o.UID,
                    o.ID,
                    o.Name,
                    Status = YAEP.Utilities.EnumerableData.GetName<ItemStatus>(o.Status),
                    o.ImageUID,
                    o.CustomerName,
                    o.Description,
                });

                list = list.OrderBy(o => o.ID);

                var apiResult = base.GetSuccessResult(list);
                return base.Json(apiResult);
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionName("RefreshCache")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult RefreshProductCache()
        {
            try
            {
                InitDIRoot();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                DrKnowAll.Reload(DrKnowAllKeys.Product);
                DrKnowAll.Reload(DrKnowAllKeys.ProductCategory);
                DrKnowAll.Reload(DrKnowAllKeys.ProductCategoryRelation);
                DrKnowAll.Reload(DrKnowAllKeys.PackageVersion);
                DrKnowAll.Reload(DrKnowAllKeys.PackageUom);
                DrKnowAll.Reload(DrKnowAllKeys.Package);
                _instance.ReloadProductPackageCache();
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }

            var apiResult = base.GetSuccessResult(true);
            return base.Json(apiResult);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("RefreshProductCacheByItemUID")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult RefreshProductCacheByItemUID(Guid itemUID)
        {
            try
            {
                InitDIRoot();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                // Item 
                DrKnowAll.RefreshProduct(itemUID, isRefreshProductCategory: true, isRefreshProductCategoryRelation: true);

                // Package
                DrKnowAll.RefreshPackageByItem(itemUID, isRefreshPackageUOM: true, isRefreshPackageVersion: true);
                _instance.TracingAgent.BeginTracing("", itemUID);
                _instance.ReloadProductPackageCache();
                _instance.TracingAgent.EndTracing();
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }

            var apiResult = base.GetSuccessResult(true);
            return base.Json(apiResult);
        }

        private List<ItemPropertiesModel> parseToProductProperties(Guid itemUID, ProductRequestModel product)
        {
            var properties = new List<ItemPropertiesModel>();

            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.CustomerUID),
                Value = product.CustomerUID.ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.BOOLEAN,
                Name = nameof(product.IsBOM),
                Value = product.IsBOM ? "1" : "0",
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.EAN),
                Value = product.EAN,
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.UPC),
                Value = product.UPC,
            });

            if (product.ImageUID.HasValue)
            {
                properties.Add(new ItemPropertiesModel()
                {
                    ItemUID = itemUID,
                    DataType = (int)ItemDataTypes.STRING,
                    Name = nameof(product.ImageUID),
                    Value = product.ImageUID.Value.ToString(),
                });
            }

            // 2019-09-09 新增擴充欄位 
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.LengthInch),
                Value = (product.LengthInch ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.WidthInch),
                Value = (product.WidthInch ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.HeightInch),
                Value = (product.HeightInch ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.LengthCM),
                Value = (product.LengthCM ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.WidthCM),
                Value = (product.WidthCM ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.HeightCM),
                Value = (product.HeightCM ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.NetWeightKG),
                Value = (product.NetWeightKG ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.NetWeightLB),
                Value = (product.NetWeightLB ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.GrossWeightKG),
                Value = (product.GrossWeightKG ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.DECIMAL,
                Name = nameof(product.GrossWeightLB),
                Value = (product.GrossWeightLB ?? 0m).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.StockUOM),
                Value = (product.StockUOM ?? String.Empty).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.PurchaseUOM),
                Value = (product.PurchaseUOM ?? String.Empty).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.SellingUOM),
                Value = (product.SellingUOM ?? String.Empty).ToString(),
            });
            properties.Add(new ItemPropertiesModel()
            {
                ItemUID = itemUID,
                DataType = (int)ItemDataTypes.STRING,
                Name = nameof(product.ShipUOM),
                Value = (product.ShipUOM ?? String.Empty).ToString(),
            });


            return properties;
        }

        private IEnumerable<ProductCacheModel> getCacheProductList(ProductSearchRequestModel requestModel)
        {

            var colletion = DrKnowAll.GetProduct().Where(o =>
            {
                bool b = true;

                var itemUIDs = (requestModel?.ItemUID ?? new Guid[] { }).Union((requestModel?.PHierarchy ?? new Guid[] { }));
                if (itemUIDs?.Count() > 0)
                {
                    b = itemUIDs.Any(itemUID => itemUID == o.UID);
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace((requestModel.ItemID ?? String.Empty).Trim()))
                    {
                        b = o.ID.StartsWith(requestModel.ItemID.Trim(), StringComparison.OrdinalIgnoreCase);
                    }
                    if (b)
                    {
                        if (requestModel.CustomerUID.HasValue)
                        {
                            b = o.CustomerUID == requestModel.CustomerUID.Value;
                        }
                    }
                    if (b)
                    {
                        if (requestModel?.CHierarchy?.Count() > 0)
                        {
                            b = requestModel.CHierarchy.Any(ch => ch == o.GroupUID);
                        }
                    }
                }

                return b;
            });

            if (requestModel.PageSize.HasValue && requestModel.PageNumber.HasValue)
            {
                int start = requestModel.PageSize.Value * (requestModel.PageNumber.Value - 1);
                if (start <= colletion.Count())
                {
                    colletion = colletion.Skip(start).Take(requestModel.PageSize.Value);
                }
            }

            return colletion.ToArray();
        }
        private IEnumerable<ProductCacheModel> getCacheProductListByDB(ProductSearchRequestModel requestModel)
        {
            if (requestModel == null)
            {
                return new ProductCacheModel[] { };
            }

            var factory = GetItemFactory();
            var manager = factory.CreateItemManager();
            var parameters = factory.CreateItemSearchParameters();

            if (!String.IsNullOrWhiteSpace((requestModel.ItemID ?? String.Empty).Trim()))
            {
                parameters.ID = requestModel.ItemID.Trim();
            }

            if (requestModel.CustomerUID.HasValue)
            {
                var customerUID = requestModel.CustomerUID.Value;
                var propertySearchParameters = factory.CreateItemPropertiesSearchParameters();
                propertySearchParameters.Name = "CustomerUID";
                propertySearchParameters.Value = customerUID.ToString();
                parameters.ItemProperties.Add(propertySearchParameters);
            }

            if ((requestModel.ItemUID?.Count() ?? 0) > 0)
            {
                parameters.ListOfItemUID.AddRange(requestModel.ItemUID);
            }
            if ((requestModel.PHierarchy?.Count() ?? 0) > 0)
            {
                parameters.ListOfItemUID.AddRange(requestModel.PHierarchy);
            }
            if ((requestModel.CHierarchy?.Count() ?? 0) > 0)
            {
                parameters.ListOfGroupUID.AddRange(requestModel.CHierarchy);
            }

            var dataResult = manager.GetItems<ProductCacheModel>(parameters);

            var colletion = dataResult.Content;

            if ((colletion?.Count() ?? 0) > 0)
            {
                if (requestModel.PageSize.HasValue && requestModel.PageNumber.HasValue)
                {
                    int start = requestModel.PageSize.Value * (requestModel.PageNumber.Value - 1);
                    if (start <= colletion.Count())
                    {
                        colletion = colletion.Skip(start).Take(requestModel.PageSize.Value);
                    }
                }
            }

            return colletion?.ToArray() ?? new ProductCacheModel[] { };
        }
        private IEnumerable<PackageCacheModel> getCachePackageList(IEnumerable<Guid> itemUID)
        {
            var colletion = DrKnowAll.GetPackage().Where(p => itemUID.Any(uid => uid == p.ItemUID)).ToList();

            return colletion.ToArray();
        }
        private IEnumerable<PackageCacheModel> getCachePackageListByDB(IEnumerable<Guid> itemUID)
        {
            if ((itemUID?.Count() ?? 0) > 0)
            {
                var packageFactory = GetPackageFactory();
                var packageManager = packageFactory.CreatePackageManager();
                var searchPackageResult = packageManager.GetPackagesByItem(itemUID.ToArray());
                if (searchPackageResult.Success)
                {
                    return this.parsePackageCacheList(searchPackageResult.Content);
                }
            }

            return new PackageCacheModel[] { };
        }
        private IEnumerable<PackageCacheModel> parsePackageCacheList(IEnumerable<IPackageViewModel> source)
        {
            var result = source.Select(o =>
            {
                var cacheModel = new PackageCacheModel();
                cacheModel.UID = o.UID;
                cacheModel.ID = o.ID;
                cacheModel.Name = o.Name;
                cacheModel.Length = o.Length;
                cacheModel.Width = o.Width;
                cacheModel.Height = o.Height;
                cacheModel.GrossWeight = o.GrossWeight;
                cacheModel.SCC14 = o.SCC14;
                cacheModel.Status = o.Status;
                cacheModel.Type = o.Type;
                cacheModel.ItemUID = o.ItemUID;
                cacheModel.VersionUID = o.VersionUID;
                cacheModel.UOM = o.UOM;
                cacheModel.ParentUID = o.ParentUID;
                cacheModel.CreatedBy = o.CreatedBy;
                cacheModel.CreatedOn = o.CreatedOn;
                cacheModel.ModifiedBy = o.ModifiedBy;
                cacheModel.ModifiedOn = o.ModifiedOn;

                cacheModel.VersionId = o.VersionId;
                cacheModel.UomName = o.UomName;
                cacheModel.ParentUOM = o.ParentUOM;
                cacheModel.ParentUomName = o.ParentUomName;

                return cacheModel;
            }).ToArray();

            return result;
        }

        private Guid getDefaultGroupUID()
        {
            var groups = this.getGroupsByUser();
            return groups?.FirstOrDefault() ?? Guid.Empty;
        }
        private IEnumerable<Guid> getGroupsByUser()
        {
            var authInfo = base.GetAuthenticationInfo();
            var manager = this.GetIdentityFactory().CreateGroupManager();
            var result = manager.GetGroupKeysByUser(authInfo.UID);
            if (result.Success)
            {
                return result.Content;
            }
            return null;
        }

        private IPartyModel getCustomer(Guid groupUID, string customerID)
        {
            var partyManager = this.GetPartyFactory().CreatePartyManager();
            var searchPartyParameters = this.GetPartyFactory().CreatePartyParameter();
            searchPartyParameters.PartyTypeCategory = PartyTypeCategories.Customer;
            if (searchPartyParameters.ListOfGroupUID == null)
            {
                searchPartyParameters.ListOfGroupUID = new List<Guid>();
            }
            searchPartyParameters.ListOfGroupUID.Add(groupUID);
            searchPartyParameters.ID = customerID;
            var searchPartyResult = partyManager.GetParties(searchPartyParameters);

            if (searchPartyResult.Success)
            {
                return searchPartyResult.Content?.FirstOrDefault();
            }

            return null;
        }

        #endregion

        #region Product Category

        /// <summary>
        /// Get all of Product category
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetProductCategory")]
        public IHttpActionResult GetProductCategory()
        {
            try
            {
                var collection = DrKnowAll.GetProductCategory();

                var apiResult = this.GetSuccessResult(collection);
                return base.Json(apiResult);
                //var manager = this.GetItemFactory().CreateItemManager();
                //var parameters = this.GetItemFactory().CreateItemCategoryParameters();
                //var result = manager.GetCategories(parameters);

                //if (result.Success)
                //{
                //    var apiResult = this.GetSuccessResult(result.Content);
                //    return base.Json(apiResult);
                //}
                //else
                //{
                //    return base.GetFailureResult(-1, result.Message);
                //}
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        #endregion

        #region Package
        [HttpGet]
        [ActionName("GetLatestPackageByItem")]
        public IHttpActionResult GetLatestPackageByItem(Guid pid)
        {
            if (pid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                InitDIRoot();
                using (var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager)
                {
                    var apiResult = _instance.GetLatestPackageByItem(pid);
                    return this.Json(apiResult);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// Create Package
        /// </summary>
        /// <param name="package">package model</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddPackage")]
        public IHttpActionResult AddPackage([FromBody] PackageRequestModel package)
        {
            if (package == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (package.UOM == Guid.Empty)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(package.UOM)})");
            }
            if (String.IsNullOrWhiteSpace(package.Name))
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(package.Name)})");
            }
            if (package.Quantity <= 0)
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_QUANTITY_BIGGER_THAN_ZERO);
            }
            if (package.Length <= 0 || package.Width <= 0 || package.Height <= 0 || package.Height <= 0)
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_SIZE_BIGGER_THAN_ZERO);
            }
            if (package.GrossWeight <= 0)
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_GW_BIGGER_THAN_ZERO);
            }

            if (package.ParentUID == Guid.Empty)
            {
                package.ParentUID = null;
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageManager();
                package = this.AntiXSSEncode(package);
                var model = new PackageModel()
                {
                    UID = Guid.NewGuid(),
                    VersionUID = package.VersionUID,
                    ItemUID = package.ItemUID,
                    UOM = package.UOM,
                    ParentUID = package.ParentUID,
                    ID = package.ID,
                    Name = package.Name,
                    Quantity = package.Quantity,
                    Length = package.Length,
                    Width = package.Width,
                    Height = package.Height,
                    GrossWeight = package.GrossWeight,
                    ImageUID = package.ImageUID,
                    Status = (int)PackageStatus.Active,
                };

                var result = manager.AddPackage(model);

                if (result.Content)
                {
                    // refresh package cache
                    DrKnowAll.RefreshPackage(model.UID);

                    var apiResult = this.GetSuccessResult(package);
                    return this.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_ADD_PACKAGE_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Update Package
        /// </summary>
        /// <param name="package">package model</param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("UpdatePackage")]
        public IHttpActionResult UpdatePackage([FromBody] PackageUpdateRequestModel package)
        {
            if (package == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (package.UID == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (package.UOM == Guid.Empty)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(package.UOM)})");
            }
            if (String.IsNullOrWhiteSpace(package.ID))
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(package.ID)})");
            }
            if (String.IsNullOrWhiteSpace(package.Name))
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(package.Name)})");
            }
            if (package.Quantity <= 0)
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_QUANTITY_BIGGER_THAN_ZERO);
            }
            if (package.Quantity <= 0 || package.Length <= 0 || package.Width <= 0 || package.Height <= 0)
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_SIZE_BIGGER_THAN_ZERO);
            }
            if (package.GrossWeight <= 0)
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_GW_BIGGER_THAN_ZERO);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageManager();

                var getModelResult = manager.GetPackage(package.UID);

                if (!getModelResult.Success && getModelResult.Content == null)
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_PACKAGE_NOT_EXIST);
                }

                package = this.AntiXSSEncode(package);
                var model = getModelResult.Content;
                model.UOM = package.UOM;
                model.ID = package.ID;
                model.Name = package.Name;
                model.Quantity = package.Quantity;
                model.Length = package.Length;
                model.Width = package.Width;
                model.Height = package.Height;
                model.GrossWeight = package.GrossWeight;
                model.ImageUID = package.ImageUID;
                model.ModifiedBy = "";
                model.ModifiedOn = null;

                var result = manager.UpdatePackage(model);

                if (result.Content)
                {
                    // refresh package cache
                    DrKnowAll.RefreshPackage(model.UID);

                    var apiResult = this.GetSuccessResult(package);
                    return this.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_UPDATE_PACKAGE_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Delete Package
        /// </summary>
        /// <param name="pid">unique identifier of product item</param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeletePackage")]
        public IHttpActionResult DeletePackage([FromUri] Guid pid)
        {
            if (pid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageManager();

                var result = manager.DeletePackage(pid);

                if (result.Content)
                {
                    // refresh package cache
                    DrKnowAll.RefreshPackage(pid);

                    var apiResult = this.GetSuccessResult();
                    return this.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_DELETE_PACKAGE_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Get Receive UOM Quantity
        /// </summary>
        /// <param name="pid">unique identifier of package</param>
        /// <param name="rpid">unique identifier of receive package</param>
        /// <param name="qty">receive quantity</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetReceivePackageUomQty")]
        public IHttpActionResult GetReceivePackageUomQty(Guid pid, Guid rpid, int qty)
        {
            if (pid == Guid.Empty || rpid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (qty <= 0)
            {
                return base.GetFailureResult(-1, Resource.PRODUCT_RECEIVE_QTY_BIGGER_THAN_ZERO);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageManager();

                var result = manager.GetReceivePackageUomQuantity(pid, rpid, qty);

                if (result.Success)
                {
                    var apiResult = new YAEP.WMS.Api.Models.APIResult<int>();
                    apiResult.IsComplete = true;
                    apiResult.ResponseTime = DateTime.Now;
                    apiResult.Data = result.Content;
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, result.Message);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Get package list current version
        /// </summary>
        /// <param name="vid">unique identifier of package version</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetPackageList")]
        public IHttpActionResult GetPackageList([FromUri] Guid vid)
        {
            if (vid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageManager();

                var result = manager.GetPackagesByVersion(vid);

                if (result.Success)
                {
                    // 是否可異動資料
                    base.InitDIRoot();
                    var warehouseManger = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;

                    if (result.Content?.Count() > 0)
                    {
                        foreach (var package in result.Content)
                        {
                            var getAssignedQtyResult = warehouseManger.GetAssignedPackageQty(package.UID);
                            if (getAssignedQtyResult.Success)
                            {
                                package.CanEdit = getAssignedQtyResult.Content == 0;
                                package.CanDelete = getAssignedQtyResult.Content == 0;
                                package.AssignedCount = getAssignedQtyResult.Content;
                            }
                        }
                    }

                    var apiResult = this.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetDataNotFoundResult();
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Get all packages of product
        /// </summary>
        /// <param name="pid">unique identifier of product</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetAllPackageList")]
        public IHttpActionResult GetAllPackageList(Guid pid)
        {

            if (pid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageManager();

                var result = manager.GetPackagesByItem(pid);

                if (result.Success)
                {
                    var apiResult = this.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetDataNotFoundResult();
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }


        /// <summary>
        /// Get package info
        /// </summary>
        /// <param name="pgid">unique identifier of package</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetPackageInfo")]
        public IHttpActionResult GetPackageInfo(Guid pgid)
        {

            if (pgid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageManager();

                var result = manager.GetPackage(pgid);

                if (result.Success)
                {
                    var apiResult = this.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetDataNotFoundResult();
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Sync Package
        /// </summary>
        /// <param name="package">package model</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SyncPackage")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public IHttpActionResult SyncPackage([FromBody] PackageSyncRequestModel package)
        {
            InitDIRoot();
            if (package == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (package.ItemUID == Guid.Empty)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(package.ItemUID)})");
            }
            if (package.Package == null)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(package.Package)})");
            }

            // 檢查是否皆有設定 UOM
            bool allHasUom = this.checkHasSetUom(package.Package);
            if (!allHasUom)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}(Package UOM)");
            }

            try
            {
                var item = this.checkProductExists(package.ItemUID);
                if (item == null)
                {
                    return base.GetFailureResult(-1, "Item not found.");
                }

                string uom = package.Package.UOM;
                var uomUID = this.handlePackageUOM(uom);
                // 找不到 UOM 也無法建立 UOM
                if (uomUID == Guid.Empty)
                {
                    return base.GetFailureResult(-1, "Fail to sync Package UOM.");
                }

                var versionUID = this.handlePackageVersion(item.UID);
                // 找不到 UOM 也無法建立 UOM
                if (versionUID == Guid.Empty)
                {
                    return base.GetFailureResult(-1, "Fail to sync Package Version.");
                }

                bool success = this.handlePackage(package.Package, package.ItemUID, uomUID, versionUID);

                if (success)
                {
                    // refresh item
                    (new RefreshDrKnowAll()).RefreshProduct(item.UID);
                    var _instance = this.DIContainer.ManifestFactory.CreateManger().OrderManager;
                    _instance.ReloadProductPackageCache();
                    var apiResult = base.GetSuccessResult();
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, "Fail to sync Package.");
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        private Guid handlePackageVersion(Guid itemUID)
        {
            string versionId = DrKnowAll.GetProduct(itemUID)?.ID;

            var manager = this.GetPackageFactory().CreatePackageVersionManager(new VersionSerialNumberGenerator(this.GetSequenceAgent()));
            var result = manager.AddPackageVersion(itemUID, versionId);

            if (result.Success)
            {
                // refresh package version cache
                DrKnowAll.RefreshPackageVersion(result.Content);
            }

            return result?.Content ?? Guid.Empty;
        }

        private Guid handlePackageUOM(string uom)
        {
            bool uomExists = DrKnowAll.GetPackageUom()?.Any(o => o.ID.Equals(uom, StringComparison.OrdinalIgnoreCase)) ?? false;
            // UOM 不存在則建立 UOM 資料
            if (!uomExists)
            {
                var factory = this.GetPackageFactory();
                var uomManager = factory.CreatePackageUomManager();

                var createUomResult = uomManager.CreateUom(uom, uom);
                if (createUomResult.Success)
                {
                    // refresh Package UOM cache
                    DrKnowAll.RefreshPackageUom(createUomResult.Content);
                }
            }

            var model = DrKnowAll.GetPackageUom()?.FirstOrDefault(o => o.ID.Equals(uom, StringComparison.OrdinalIgnoreCase));

            return model?.UID ?? Guid.Empty;
        }

        private bool checkHasSetUom(PackageSyncRequestEntity package)
        {
            if (String.IsNullOrWhiteSpace(package.UOM))
            {
                return false;
            }

            if ((package.Children?.Count() ?? 0) > 0)
            {
                foreach (var childPackage in package.Children)
                {
                    bool hasUom = this.checkHasSetUom(childPackage);
                    if (!hasUom)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IItemModel checkProductExists(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return null;
            }

            var manager = this.GetItemFactory().CreateItemManager();

            var searchItemResult = manager.GetItem(itemUID);

            if (searchItemResult.Success)
            {
                var foundProduct = searchItemResult.Content;
                if (foundProduct?.GroupUID != this.getDefaultGroupUID())
                {
                    return null;
                }

                return foundProduct;
            }

            return null;
        }
        private IItemModel checkProductExistsByCache(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return null;
            }

            var foundProduct = DrKnowAll.GetProduct(itemUID);
            if (foundProduct == null)
            {
                DrKnowAll.RefreshProduct(itemUID);
            }
            foundProduct = DrKnowAll.GetProduct(itemUID);

            if (foundProduct.GroupUID != this.getDefaultGroupUID())
            {
                return null;
            }

            return foundProduct;
        }

        private bool handlePackage(PackageSyncRequestEntity package, Guid itemUID, Guid uomUID, Guid versionUID)
        {
            var rootPackage = new PackageModel()
            {
                UID = Guid.NewGuid(),
                VersionUID = versionUID,
                ItemUID = itemUID,
                UOM = uomUID,
                ID = package.Name,
                Name = package.Name,
                Length = package.Length,
                Width = package.Width,
                Height = package.Height,
                GrossWeight = (package.GrossWeight ?? 0m),
                Quantity = package.Quantity,
                SCC14 = package.SCC14,
                PUOM = package.PUOM,
                Status = (int)PackageStatus.Active,
                Type = 1,
            };

            if (package.Children.Count() == 0)
            {
                rootPackage.CreatedBy = "Lst-Pkg";
            }

            var packageManager = this.GetPackageFactory().CreatePackageManager();
            var addRootPackageResult = packageManager.AddPackage(rootPackage);
            if (addRootPackageResult.Success)
            {
                bool successToCreateChildren = this.createChildren(package.Children, itemUID, versionUID, rootPackage.UID);
                if (successToCreateChildren)
                {
                    return true;
                }
            }

            return false;
        }
        private bool createChildren(IEnumerable<PackageSyncRequestEntity> children, Guid itemUID, Guid versionUID, Guid parentPackageUID)
        {
            if ((children?.Count() ?? 0) == 0)
            {
                return true;
            }

            var factory = this.GetPackageFactory();
            var uomManager = factory.CreatePackageUomManager();
            var packageManager = factory.CreatePackageManager();

            foreach (var childPackageEntity in children)
            {
                string uom = childPackageEntity.UOM;
                var uomUID = this.handlePackageUOM(uom);
                // 找不到 UOM 也無法建立 UOM
                if (uomUID == Guid.Empty)
                {
                    return false;
                }

                var childPackage = new PackageModel()
                {
                    UID = Guid.NewGuid(),
                    ParentUID = parentPackageUID,
                    VersionUID = versionUID,
                    ItemUID = itemUID,
                    UOM = uomUID,
                    ID = childPackageEntity.Name,
                    Name = childPackageEntity.Name,
                    Length = childPackageEntity.Length,
                    Width = childPackageEntity.Width,
                    Height = childPackageEntity.Height,
                    GrossWeight = (childPackageEntity.GrossWeight ?? 0m),
                    Quantity = childPackageEntity.Quantity,
                    SCC14 = childPackageEntity.SCC14,
                    PUOM = childPackageEntity.PUOM,
                    Status = (int)PackageStatus.Active,
                    Type = 1,
                };

                if (childPackageEntity.Children.Count() == 0)
                {
                    childPackage.CreatedBy = "Lst-Pkg";
                }

                var addChildPackageResult = packageManager.AddPackage(childPackage);

                if (!addChildPackageResult.Success)
                {
                    return false;
                }

                bool successToCreateChildren = this.createChildren(childPackageEntity.Children, itemUID, versionUID, childPackage.UID);
                if (!successToCreateChildren)
                {
                    return false;
                }
            }

            return true;
        }


        #endregion

        #region Package Version

        /// <summary>
        /// add a default package version (include a default package)
        /// </summary>
        /// <param name="pid">unique identifier of product item</param>
        /// <param name="uuid">unique identifier of package unit of measure</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddPackageVersion")]
        public IHttpActionResult AddPackageVersion([FromUri] Guid pid, [FromUri] Guid uuid)
        {
            if (pid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                string versionId = DrKnowAll.GetProduct(pid)?.ID;

                var manager = this.GetPackageFactory().CreatePackageVersionManager(new VersionSerialNumberGenerator(this.GetSequenceAgent()));

                var result = manager.AddPackageVersion(pid, uuid, versionId);

                if (result.Success)
                {
                    // refresh package version cache
                    DrKnowAll.RefreshPackageVersion(result.Content);

                    var apiResult = base.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_ADD_PACKAGE_VERSION_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Copy Package Version
        /// </summary>
        /// <param name="vid">unique identifier of package version</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ClonePackageVersion")]
        public IHttpActionResult ClonePackageVersion([FromUri] Guid vid)
        {
            if (vid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageVersionManager(new VersionSerialNumberGenerator(this.GetSequenceAgent()));

                var result = manager.ClonePackageVersion(vid);

                if (result.Success)
                {
                    // refresh package version cache
                    DrKnowAll.RefreshPackageVersion(result.Content);

                    var apiResult = this.GetSuccessResult();
                    return this.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_CLONE_PACKAGE_VERSION_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Delete Package Version
        /// </summary>
        /// <param name="vid">unique identifier of package version</param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeletePackageVersion")]
        public IHttpActionResult DeletePackageVersion([FromUri] Guid vid)
        {
            if (vid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageVersionManager(new VersionSerialNumberGenerator(this.GetSequenceAgent()));

                var result = manager.DeletePackageVersion(vid);

                if (result.Content)
                {
                    var apiResult = this.GetSuccessResult();
                    return this.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.PRODUCT_DELETE_PACKAGE_VERSION_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// Get Package Version List 
        /// </summary>
        /// <param name="pid">unique identifier of product item</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetPackageVersionList")]
        public IHttpActionResult GetPackageVersionList([FromUri] Guid pid)
        {
            if (pid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPackageFactory().CreatePackageVersionManager(new VersionSerialNumberGenerator(this.GetSequenceAgent()));

                var result = manager.GetPackageVersionList(pid);

                if (result.Success)
                {
                    var apiResult = base.GetSuccessResult(result.Content.OrderByDescending(o => o.CreatedOn).ToList());
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetDataNotFoundResult();
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        #endregion

        #region Package UOM

        /// <summary>
        /// Get Package UOM List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetPackageUomList")]
        public IHttpActionResult GetPackageUomList()
        {
            try
            {
                var collection = DrKnowAll.GetPackageUom();

                var apiResult = this.GetSuccessResult(collection);
                return base.Json(apiResult);

                //var manager = this.GetPackageFactory().CreatePackageUomManager();

                //var result = manager.GetPackageUomList();

                //if (result.Success)
                //{
                //    var apiResult = base.GetSuccessResult(result.Content);
                //    return base.Json(apiResult);
                //}
                //else
                //{
                //    return base.GetFailureResult(-1, result.Message);
                //}
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        #endregion

        #region Factories

        private readonly Lazy<PackageFactory> _PackageFactory;
        private readonly Lazy<ItemFactory> _ItemFactory;
        private readonly Lazy<PartyFactory> _PartyFactory;
        private PackageFactory GetPackageFactory()
        {
            return this._PackageFactory.Value;
        }
        private ItemFactory GetItemFactory()
        {
            return this._ItemFactory.Value;
        }
        private PartyFactory GetPartyFactory()
        {
            return this._PartyFactory.Value;
        }

        #endregion

        #region Testing

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actualProductID"></param>
        /// <param name="customerUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetVirtualItems")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult GetVirtualItems([FromUri] string actualProductID, Guid customerUID)
        {
            if (customerUID == Guid.Empty)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} {nameof(customerUID)}");
            }
            if (String.IsNullOrWhiteSpace(actualProductID))
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} {nameof(actualProductID)}");
            }

            try
            {
                var result = this.getVirtualItems(actualProductID, customerUID);

                if (result.Success && (result.Content?.Count() ?? 0) > 0)
                {
                    var apiResult = base.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, result.Message);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetActualItem")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult GetActualItem([FromBody] IEnumerable<VirtualItemInfo> requestModel)
        {
            if ((requestModel?.Count() ?? 0) == 0)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS} {nameof(requestModel)}");
            }

            if (requestModel.GroupBy(o => o.ActualProduct).Count() > 1)
            {
                return base.GetFailureResult(-1, "The property 'ActualProduct' cannot be difference.");
            }
            if (requestModel.GroupBy(o => o.CustomerUID).Count() > 1)
            {
                return base.GetFailureResult(-1, "The property 'CustomerUID' cannot be difference.");
            }

            var customerUID = requestModel.FirstOrDefault()?.CustomerUID ?? Guid.Empty;
            string actualProduct = requestModel.FirstOrDefault()?.ActualProduct;

            if (String.IsNullOrWhiteSpace(actualProduct))
            {
                return base.GetFailureResult(-1, "The property 'ActualProduct' cannot be empty.");
            }

            if (customerUID == Guid.Empty)
            {
                return base.GetFailureResult(-1, "The property 'CustomerUID' cannot be empty.");
            }

            try
            {
                var result = this.CombineToActualItem(requestModel);

                if (result.Success && result.Content != null)
                {
                    var apiResult = base.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, result.Message);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        private IActionResult<IEnumerable<ProductCacheModel>> getVirtualItems(string actualProductID, Guid customerUID)
        {
            var factory = this.GetItemFactory();
            var manager = factory.CreateItemManager();

            var parameters = factory.CreateItemSearchParameters();
            parameters.ItemProperties.Add(new ItemPropertySearchModel()
            {
                Name = "CustomerUID",
                Value = customerUID.ToString()
            });
            parameters.ItemProperties.Add(new ItemPropertySearchModel()
            {
                Name = "ActualProduct",
                Value = actualProductID
            });

            var result = manager.GetItems<ProductCacheModel>(parameters);

            return result;
        }
        private IActionResult<ProductCacheModel> GetActualItem(string productId, Guid customerUID)
        {
            if (String.IsNullOrWhiteSpace(productId))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductCacheModel>(nameof(productId));
            }

            if (customerUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductCacheModel>(nameof(customerUID));
            }

            var factory = this.GetItemFactory();
            var manager = factory.CreateItemManager();
            var parameters = factory.CreateItemSearchParameters();
            parameters.ID = productId;
            parameters.ItemProperties.Add(new ItemPropertySearchModel()
            {
                Name = "CustomerUID",
                Value = customerUID.ToString()
            });

            var result = manager.GetItems<ProductCacheModel>(parameters);
            if (result.Success && (result.Content?.Count() ?? 0) > 0)
            {
                var r = ActionResultTemplates.Result<ProductCacheModel>();
                r.Success = true;
                r.Content = result.Content.FirstOrDefault();
                return r;
            }
            else
            {
                return ActionResultTemplates.Error<ProductCacheModel>(result.Message, innerException: result.InnerException);
            }
        }
        private IActionResult<ProductCacheModel> CombineToActualItem(IEnumerable<VirtualItemInfo> virtualItems)
        {
            if ((virtualItems?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductCacheModel>(nameof(virtualItems));
            }
            if (virtualItems.GroupBy(o => o.ActualProduct).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductCacheModel>("The property 'ActualProduct' cannot be difference.");
            }
            if (virtualItems.GroupBy(o => o.CustomerUID).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductCacheModel>("The property 'CustomerUID' cannot be difference.");
            }

            var customerUID = virtualItems.FirstOrDefault()?.CustomerUID ?? Guid.Empty;
            string actualProduct = virtualItems.FirstOrDefault()?.ActualProduct;

            if (String.IsNullOrWhiteSpace(actualProduct))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductCacheModel>("ActualProduct");
            }

            if (customerUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductCacheModel>("CustomerUID");
            }

            var getCurrentVirtualItemsResult = this.getVirtualItems(actualProduct, customerUID);
            if (!getCurrentVirtualItemsResult.Success)
            {
                return ActionResultTemplates.Error<ProductCacheModel>(getCurrentVirtualItemsResult.Message);
            }
            if ((getCurrentVirtualItemsResult.Content?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.Error<ProductCacheModel>("This item no setting virtual items.");
            }

            var result = ActionResultTemplates.Result<ProductCacheModel>();

            var currentVirtualItems = getCurrentVirtualItemsResult.Content;
            int combinedQuantity = -1;
            foreach (var currentVirtualItem in currentVirtualItems)
            {
                var virtualItem = virtualItems.FirstOrDefault(o => o.ProductUID == currentVirtualItem.UID);
                // 參數缺少虛擬 Item
                if (virtualItem == null)
                {
                    result.Message = $"missing item - {currentVirtualItem.ID}";
                    break;
                }
                // 數量無法整除
                if ((virtualItem.Quantity % currentVirtualItem.BoxQuantity) > 0)
                {
                    result.Message = $"Indivisible quantity ({virtualItem.Quantity} / {currentVirtualItem.BoxQuantity}) - {currentVirtualItem.ID}";
                    break;
                }

                // 檢查組合後的數量是否一致
                int currentCombinedQuantity = (virtualItem.Quantity / currentVirtualItem.BoxQuantity);
                if (combinedQuantity > 0 && combinedQuantity != currentCombinedQuantity)
                {
                    result.Message = $"The quantity dissmatch - {currentVirtualItem.ID}";
                    break;
                }

                combinedQuantity = currentCombinedQuantity;
            }

            if (String.IsNullOrWhiteSpace(result.Message))
            {
                var getActualItemResult = this.GetActualItem(actualProduct, customerUID);

                if (getActualItemResult.Success)
                {
                    getActualItemResult.Content.CombinedQuantity = combinedQuantity;
                    result = getActualItemResult;
                }
                else
                {
                    return getActualItemResult;
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        public class VirtualItemInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public string ProductId { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Guid ProductUID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Guid CustomerUID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ActualProduct { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int Quantity { get; set; }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CheckItem")]
        public IHttpActionResult CheckItem([FromBody] CheckItemRequestModel requestModel)
        {
            InitDIRoot();
            var items = DrKnowAll.GetProductByCustomer(requestModel.customerUID);
            var finditems = items.Where(p => requestModel.itemNames.Any(x => x == p.Name));
            var apiResult = this.GetSuccessResult<dynamic>(finditems);
            return this.Json(apiResult);
        }

        private ISequenceAgent GetSequenceAgent()
        {
            base.InitDIRoot();

            return base.DIContainer.GetSequenceAgent();
        }

    }
}
