using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Core.Party.Constants;
using YAEP.Data.NoSql.Redis;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Cache.Redis.Controllers;

namespace YAEP.WMS.Cache.Redis
{
    /*
    *  Item 
    */
    public static partial class DrKnowAll
    {
        /// <summary>
        /// get product list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ProductCacheModel> GetProduct()
        {
            var productController = new ProductRedisController();

            var allProducts = productController.RetrieveAll();

            if ((allProducts?.Count() ?? 0) == 0)
            {
                allProducts = ReloadAllProduct();
            }
            
            return allProducts;
        }
        public static IEnumerable<ProductCacheModel> GetProducts(IEnumerable<Guid> groupUID, IEnumerable<Guid> customerUID)
        {
            if ((groupUID?.Count() ?? 0) == 0)
            {
                return null;
            }

            var productController = new ProductRedisController();
            var conditions = new List<SearchCondition>();
            conditions.Add(SearchCondition.AND("GroupUID", o =>
            {
                var guid = YAEP.Utilities.Utility.ToGuid(o?.ToString());

                return groupUID.Any(g => g == guid);
            }));

            if ((customerUID?.Count() ?? 00) > 0)
            {
                conditions.Add(SearchCondition.AND("CustomerUID", o =>
                {
                    var guid = YAEP.Utilities.Utility.ToGuid(o?.ToString());

                    return customerUID.Any(g => g == guid);
                }));
            }

            return productController.RetrieveByConditions(conditions.ToArray()); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        /// <returns></returns>
        public static ProductCacheModel GetProduct(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return null;
            }

            var productController = new ProductRedisController();

            return productController.Retrieve(itemUID).FirstOrDefault(); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupUID"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static ProductCacheModel GetProduct(Guid groupUID, string itemID)
        {
            if (groupUID == Guid.Empty)
            {
                return null;
            }
            if (String.IsNullOrWhiteSpace(itemID))
            {
                return null;
            }

            string groupUIDString = groupUID.ToString();
            var condition1 = SearchCondition.AND("GroupUID", _ =>
            {
                return (_ ?? String.Empty).ToString().Equals(groupUIDString, StringComparison.OrdinalIgnoreCase);
            });
            var condition2 = SearchCondition.AND("ID", _ =>
            {
                return (_ ?? String.Empty).ToString().Equals(itemID, StringComparison.OrdinalIgnoreCase);
            });

            var productController = new ProductRedisController(); 
            var found = productController.RetrieveByConditions(new SearchCondition[] { condition1, condition2 });

            return found.FirstOrDefault();
            //return GetProduct().FirstOrDefault(o => o.GroupUID == groupUID && o.ID.Equals(itemId, StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerUID"></param>
        /// <returns></returns>
        public static IEnumerable<ProductCacheModel> GetProductByCustomer(Guid customerUID)
        {
            if (customerUID == Guid.Empty)
            {
                return null;
            }

            string customerUIDString = customerUID.ToString();
            var condition1 = SearchCondition.AND("CustomerUID", _ =>
            {
                return (_ ?? String.Empty).ToString().Equals(customerUIDString, StringComparison.OrdinalIgnoreCase);
            });

            var productController = new ProductRedisController();
            var found = productController.RetrieveByConditions(new SearchCondition[] { condition1 });

            return found.ToArray();
            //return GetProduct().Where(o => o.CustomerUID == customerUID);
        }

        /// <summary>
        /// get product category list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ProductCategoryCacheModel> GetProductCategory()
        {
            var productCategoryController = new ProductCategoryRedisController();
            var allProductCategory = productCategoryController.RetrieveAll();

            if ((allProductCategory?.Count() ?? 0) == 0)
            {
                allProductCategory = ReloadProductCategory();
            }

            return allProductCategory;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ProductCategoryRelationCacheModel> GetProductCategoryRelation()
        {
            var productCategoryRelationController = new ProductCategoryRelationRedisController();
            var allProductCategoryRelation = productCategoryRelationController.RetrieveAll();

            if ((allProductCategoryRelation?.Count() ?? 0) == 0)
            {
                allProductCategoryRelation = ReloadProductCategoryRelation();
            }

            return allProductCategoryRelation;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        /// <returns></returns>
        public static IEnumerable<ProductCategoryRelationCacheModel> GetProductCategoryRelation(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return null;
            }

            return GetProductCategoryRelation().Where(o => o.ItemUID == itemUID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        /// <param name="isRefreshProductCategory"></param>
        /// <param name="isRefreshProductCategoryRelation"></param>
        public static void RefreshProduct(Guid itemUID, bool isRefreshProductCategory = true, bool isRefreshProductCategoryRelation = true)
        {
            if (itemUID == Guid.Empty)
            {
                return;
            }

            if (isRefreshProductCategoryRelation)
            {
                RefreshProductCategoryRelation(itemUID);
            }

            if (isRefreshProductCategory)
            {
                RefreshProductCategory(itemUID);
            }

            // load from db
            var manager = FactoryUtils.GetItemFactoryInstance().CreateItemManager();
            var dataResult = manager.GetItem(itemUID);

            var productController = new ProductRedisController();
            if (dataResult.Success)
            {
                var data = dataResult.Content;
                if (data != null)
                {
                    // load properties from db 
                    var properties = manager.GetProperties(itemUID)?.Content;
                    var inputData = GetProductCacheModel(data, properties);
                    productController.Replace(itemUID, inputData);
                }
                else
                {
                    productController.Delete(itemUID);
                }
            }
            else
            {
                productController.Delete(itemUID);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        public static void RemoveProduct(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return;
            }

            var productController = new ProductRedisController();
            productController.Delete(itemUID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        public static void RefreshProductCategory(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return;
            }

            var categoryUIDArray = GetProductCategoryRelation(itemUID).Select(o => o.CategoryUID).ToArray();

            var manager = FactoryUtils.GetItemFactoryInstance().CreateItemManager();
            var dataResult = manager.GetCategories(itemUID);
            var productCategoryController = new ProductCategoryRedisController();

            if (dataResult.Success)
            {
                var categories = Copy(dataResult.Content);
                productCategoryController.Replace(categoryUIDArray, categories);
            }
            else
            {
                productCategoryController.Remove(categoryUIDArray);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        public static void RefreshProductCategoryRelation(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return;
            }

            var manager = FactoryUtils.GetItemFactoryInstance().CreateItemManager();
            var dataResult = manager.GetRelation(itemUID);
            var productCategoryRelationController = new ProductCategoryRelationRedisController();

            if (dataResult.Success)
            {
                var relations = Copy(dataResult.Content);
                productCategoryRelationController.Replace(itemUID, relations);
            }
            else
            {
                productCategoryRelationController.Remove(itemUID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<ProductCategoryRelationCacheModel> ReloadProductCategoryRelation()
        {
            var manager = FactoryUtils.GetItemFactoryInstance().CreateItemManager();
            var dataResult = manager.GetAllCategoryRelation();

            if (dataResult.Success)
            {
                var allProductCategoryRelation = Copy(dataResult.Content);

                var productCategoryRelationController = new ProductCategoryRelationRedisController();
                productCategoryRelationController.Create(allProductCategoryRelation);

                return allProductCategoryRelation;
            }

            return new ProductCategoryRelationCacheModel[] { };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<ProductCategoryCacheModel> ReloadProductCategory()
        {
            var itemFactory = FactoryUtils.GetItemFactoryInstance();
            var manager = itemFactory.CreateItemManager();
            var parameters = itemFactory.CreateItemCategoryParameters();
            var dataResult = manager.GetCategories(parameters);

            if (dataResult.Success)
            {
                var allProductCategory = Copy(dataResult.Content);

                var productCategoryController = new ProductCategoryRedisController();
                productCategoryController.Create(allProductCategory);

                return allProductCategory;
            }

            return new ProductCategoryCacheModel[] { };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<ProductCacheModel> ReloadAllProduct()
        {
            var itemFactory = FactoryUtils.GetItemFactoryInstance();

            var itemManager = itemFactory.CreateItemManager();
            var itemParameters = itemFactory.CreateItemSearchParameters();
            var items = itemManager.GetItems<ProductCacheModel>(itemParameters).Content;
            var categoryRelations = itemManager.GetAllCategoryRelation().Content;

            var partyFactory = FactoryUtils.GetPartyFactoryInstance();
            var partyManager = partyFactory.CreatePartyManager();
            var customers = partyManager.GetParties(PartyTypeCategories.Customer).Content;

            var cacheData = new HashSet<ProductCacheModel>(items.Select(o =>
            {
                // Customer Name
                o.CustomerName = customers.FirstOrDefault(c => c.UID == o.CustomerUID)?.Name;

                // Category
                o.CategoryUID = (categoryRelations.FirstOrDefault(r => r.ItemUID == o.UID)?.CategoryUID ?? Guid.Empty);

                return o;
            }));

            var productController = new ProductRedisController();
            productController.Create(cacheData);

            return cacheData;
        }

        private static ProductCacheModel GetProductCacheModel(IItemModel o, IEnumerable<IItemPropertiesModel> properties)
        {
            var p = new ProductCacheModel()
            {
                UID = o.UID,
                GroupUID = o.GroupUID,
                ID = o.ID,
                Name = o.Name,
                Type = o.Type,
                Status = o.Status,
                Description = o.Description,
                CreatedBy = o.CreatedBy,
                CreatedOn = o.CreatedOn,
                ModifiedBy = o.ModifiedBy,
                ModifiedOn = o.ModifiedOn,
            };

            properties = properties?.Where(prop => prop.ItemUID == o.UID);

            p.CustomerUID = YAEP.Utilities.Utility.ToGuid(properties?.FirstOrDefault(prop => prop.Name == nameof(p.CustomerUID))?.Value);
            p.EAN = properties?.FirstOrDefault(prop => prop.Name == nameof(p.EAN))?.Value;
            p.UPC = properties?.FirstOrDefault(prop => prop.Name == nameof(p.UPC))?.Value;
            p.ImageUID = YAEP.Utilities.Utility.ToGuid(properties?.FirstOrDefault(prop => prop.Name == nameof(p.ImageUID))?.Value);
            p.IsBOM = properties?.FirstOrDefault(prop => prop.Name == nameof(p.IsBOM))?.Value == "1";
            p.LengthInch = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.LengthInch))?.Value ?? "0");
            p.WidthInch = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.WidthInch))?.Value ?? "0");
            p.HeightInch = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.HeightInch))?.Value ?? "0");
            p.LengthCM = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.LengthCM))?.Value ?? "0");
            p.WidthCM = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.WidthCM))?.Value ?? "0");
            p.HeightCM = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.HeightCM))?.Value ?? "0");
            p.NetWeightKG = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.NetWeightKG))?.Value ?? "0");
            p.NetWeightLB = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.NetWeightLB))?.Value ?? "0");
            p.GrossWeightKG = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.GrossWeightKG))?.Value ?? "0");
            p.GrossWeightLB = decimal.Parse(properties?.FirstOrDefault(prop => prop.Name == nameof(p.GrossWeightLB))?.Value ?? "0");
            p.StockUOM = properties?.FirstOrDefault(prop => prop.Name == nameof(p.StockUOM))?.Value;
            p.PurchaseUOM = properties?.FirstOrDefault(prop => prop.Name == nameof(p.PurchaseUOM))?.Value;
            p.SellingUOM = properties?.FirstOrDefault(prop => prop.Name == nameof(p.SellingUOM))?.Value;
            p.ShipUOM = properties?.FirstOrDefault(prop => prop.Name == nameof(p.ShipUOM))?.Value; 
            p.ActualProduct = properties?.FirstOrDefault(prop => prop.Name == nameof(p.ActualProduct))?.Value;
            p.PUOM = properties?.FirstOrDefault(prop => prop.Name == nameof(p.PUOM))?.Value;

            // Customer Name
            p.CustomerName = DrKnowAll.GetCustomer(p.CustomerUID)?.Name;

            // Category
            p.CategoryUID = (DrKnowAll.GetProductCategoryRelation(p.UID)?.FirstOrDefault()?.CategoryUID ?? Guid.Empty);

            return p;
        }
        private static IEnumerable<ProductCategoryCacheModel> Copy(IEnumerable<IItemCategoryModel> source)
        {
            return (source?.Select(o => new ProductCategoryCacheModel()
            {
                UID = o.UID,
                GroupUID = o.GroupUID,
                ID = o.ID,
                Description = o.Description,
                Name = o.Name,
                Type = o.Type,
                Status = o.Status,
                CreatedBy = o.CreatedBy,
                CreatedOn = o.CreatedOn,
                ModifiedBy = o.ModifiedBy,
                ModifiedOn = o.ModifiedOn,
            }) ?? new ProductCategoryCacheModel[] { }).ToArray();
        }
        private static IEnumerable<ProductCategoryRelationCacheModel> Copy(IEnumerable<IItemCategoryRelationModel> source)
        {
            return (source?.Select(o => new ProductCategoryRelationCacheModel()
            {
                ItemUID = o.ItemUID,
                CategoryUID = o.CategoryUID
            }) ?? new ProductCategoryRelationCacheModel[] { }).ToArray();
        }

    }

}