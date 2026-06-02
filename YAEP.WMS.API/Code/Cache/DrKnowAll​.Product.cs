using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.Api.Code.Cache.Models;

namespace YAEP.WMS.Api.Code.Cache
{
    /*
    *  Item 
    */
    public static partial class DrKnowAll
    {
        /// <summary>
        /// 
        /// </summary>
        public static DrKnowLoadingStatus ProductLoadingStatus { get; set; } = DrKnowLoadingStatus.Pending;
        /// <summary>
        /// 
        /// </summary>
        public static DrKnowLoadingStatus ProductCategoryLoadingStatus { get; set; } = DrKnowLoadingStatus.Pending;
        /// <summary>
        /// 
        /// </summary>
        public static DrKnowLoadingStatus ProductCategoryRelationLoadingStatus { get; set; } = DrKnowLoadingStatus.Pending;

        /// <summary>
        /// get product list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ProductCacheModel> GetProduct()
        {
            var enumType = DrKnowAllKeys.Product;

            var knowledge = Instance.Recollect<ProductCacheModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetItemFactory().CreateItemManager();

                var parameters = GetItemFactory().CreateItemSearchParameters();
                var dataResult = manager.GetItems<ProductCacheModel>(parameters);
                if (dataResult.Success && (dataResult.Content?.Count() ?? 0) > 0)
                {
                    var customers = DrKnowAll.GetCustomer();
                    var categoryRelations = DrKnowAll.GetProductCategoryRelation();

                    //foreach (var item in dataResult.Content)
                    //{
                    //    // Customer Name
                    //    item.CustomerName = customers.FirstOrDefault(o => o.UID == item.CustomerUID)?.Name;
                    //    // Category
                    //    item.CategoryUID = (categoryRelations.FirstOrDefault(o => o.ItemUID == item.UID)?.CategoryUID ?? Guid.Empty);
                    //}

                    //knowledge.SetData(dataResult.Content);

                    var data = new HashSet<ProductCacheModel>(dataResult.Content.Select(o =>
                    {
                        // Customer Name
                        o.CustomerName = customers.FirstOrDefault(c => c.UID == o.CustomerUID)?.Name;

                        // Category
                        o.CategoryUID = (categoryRelations.FirstOrDefault(r => r.ItemUID == o.UID)?.CategoryUID ?? Guid.Empty);

                        return o;
                    }));

                    knowledge.SetData(data);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
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

            return GetProduct().FirstOrDefault(o => o.UID == itemUID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupUID"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static ProductCacheModel GetProduct(Guid groupUID, string itemId)
        {
            if (groupUID == Guid.Empty)
            {
                return null;
            }

            return GetProduct().FirstOrDefault(o => o.GroupUID == groupUID && o.ID.Equals(itemId, StringComparison.OrdinalIgnoreCase));
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

            return GetProduct().Where(o => o.CustomerUID == customerUID);
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

            // Customer Name
            p.CustomerName = DrKnowAll.GetCustomer(p.CustomerUID)?.Name;

            // Category
            p.CategoryUID = (DrKnowAll.GetProductCategoryRelation(p.UID)?.FirstOrDefault()?.CategoryUID ?? Guid.Empty);

            return p;
        }

        /// <summary>
        /// get product category list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IItemCategoryModel> GetProductCategory()
        {
            var enumType = DrKnowAllKeys.ProductCategory;

            var knowledge = Instance.Recollect<IItemCategoryModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetItemFactory().CreateItemManager();
                var parameters = GetItemFactory().CreateItemCategoryParameters();
                var dataResult = manager.GetCategories(parameters);
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IItemCategoryRelationModel> GetProductCategoryRelation()
        {
            var enumType = DrKnowAllKeys.ProductCategoryRelation;

            var knowledge = Instance.Recollect<IItemCategoryRelationModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetItemFactory().CreateItemManager();
                var dataResult = manager.GetAllCategoryRelation();

                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        /// <returns></returns>
        public static IEnumerable<IItemCategoryRelationModel> GetProductCategoryRelation(Guid itemUID)
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

            var enumType = DrKnowAllKeys.Product;

            var collection = GetProduct() ?? (new ProductCacheModel[] { });
            var cacheData = new HashSet<ProductCacheModel>(collection);
            // remove
            cacheData.RemoveWhere(o => o.UID == itemUID);

            // load from db
            var manager = GetItemFactory().CreateItemManager();
            var dataResult = manager.GetItem(itemUID);
            var dataInDB = dataResult.Content;

            if (dataInDB != null)
            {
                // load properties from db
                var propertiesResult = manager.GetProperties(itemUID);
                var properties = propertiesResult.Content;
                var inputData = GetProductCacheModel(dataInDB, properties);

                // add 
                cacheData.Add(inputData);
            }

            var knowledge = Instance.Recollect<ProductCacheModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
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

            var enumType = DrKnowAllKeys.Product;

            var collection = GetProduct();

            var collectionToFilter = collection.Where(o => o.UID != itemUID);

            var knowledge = Instance.Recollect<ProductCacheModel>(enumType.ToString());
            knowledge.SetData(collectionToFilter);
            Instance.Remember(enumType.ToString(), knowledge);
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

            var enumType = DrKnowAllKeys.ProductCategory;

            // load from db
            var manager = GetItemFactory().CreateItemManager();
            var dataResult = manager.GetCategories(itemUID);
            var dataInDB = dataResult.Content;

            var collection = GetProductCategory() ?? (new IItemCategoryModel[] { });

            var cacheData = new HashSet<IItemCategoryModel>(collection);

            if ((dataInDB?.Count() ?? 0) > 0)
            {
                // remove
                cacheData.RemoveWhere(o => dataInDB.Any(g => g.UID == o.UID));

                // add
                foreach (var category in dataInDB)
                {
                    cacheData.Add(category);
                }
            }

            var knowledge = Instance.Recollect<IItemCategoryModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryUID"></param>
        public static void RemoveProductCategory(IEnumerable<Guid> categoryUID)
        {
            if ((categoryUID?.Count() ?? 0) == 0)
            {
                return;
            }

            var enumType = DrKnowAllKeys.ProductCategory;

            var collection = GetProductCategory();

            var collectionToFilter = collection.Where(o => !categoryUID.Any(g => g == o.UID));

            var knowledge = Instance.Recollect<IItemCategoryModel>(enumType.ToString());
            knowledge.SetData(collectionToFilter);
            Instance.Remember(enumType.ToString(), knowledge);
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

            var enumType = DrKnowAllKeys.ProductCategoryRelation;

            var collection = GetProductCategoryRelation() ?? (new IItemCategoryRelationModel[] { });
            var cacheData = new HashSet<IItemCategoryRelationModel>(collection);
            // remove
            cacheData.RemoveWhere(o => o.ItemUID == itemUID);

            // load from db
            var manager = GetItemFactory().CreateItemManager();
            var dataResult = manager.GetRelation(itemUID);
            var dataInDB = dataResult.Content;

            if ((dataInDB?.Count() ?? 0) > 0)
            {
                // add
                foreach (var relation in dataInDB)
                {
                    cacheData.Add(relation);
                }
            }

            var knowledge = Instance.Recollect<IItemCategoryRelationModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        public static void RemoveProductCategoryRelation(Guid itemUID)
        {
            if (itemUID == Guid.Empty)
            {
                return;
            }

            var enumType = DrKnowAllKeys.ProductCategoryRelation;

            var collection = GetProductCategoryRelation();

            var collectionToFilter = collection.Where(o => o.ItemUID != itemUID);

            var knowledge = Instance.Recollect<IItemCategoryRelationModel>(enumType.ToString());
            knowledge.SetData(collectionToFilter);
            Instance.Remember(enumType.ToString(), knowledge);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemCategoryRelations"></param>
        public static void SetProductCategoryRelation(IEnumerable<IItemCategoryRelationModel> itemCategoryRelations)
        {
            var enumType = DrKnowAllKeys.ProductCategoryRelation;

            var collection = new List<IItemCategoryRelationModel>(GetProductCategoryRelation());

            collection.AddRange(itemCategoryRelations);

            var knowledge = Instance.Recollect<IItemCategoryRelationModel>(enumType.ToString());
            knowledge.SetData(collection);
            Instance.Remember(enumType.ToString(), knowledge);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static void ReloadProduct()
        {
            var enumType = DrKnowAllKeys.Product;

            var knowledge = Instance.Recollect<ProductCacheModel>(enumType.ToString());

            var manager = GetItemFactory().CreateItemManager();
            var parameters = GetItemFactory().CreateItemSearchParameters();
            var dataResult = manager.GetItems<ProductCacheModel>(parameters);
            if (dataResult.Success && (dataResult.Content?.Count() ?? 0) > 0)
            {
                var customers = DrKnowAll.GetCustomer();
                var categoryRelations = DrKnowAll.GetProductCategoryRelation();

                var data = new HashSet<ProductCacheModel>(dataResult.Content.Select(o =>
                {
                    // Customer Name
                    o.CustomerName = customers.FirstOrDefault(c => c.UID == o.CustomerUID)?.Name;

                    // Category
                    o.CategoryUID = (categoryRelations.FirstOrDefault(r => r.ItemUID == o.UID)?.CategoryUID ?? Guid.Empty);

                    return o;
                }));

                knowledge.SetData(data);
            }
            Instance.Remember(enumType.ToString(), knowledge);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static void ReloadProductCategory()
        {
            var enumType = DrKnowAllKeys.ProductCategory;

            var knowledge = Instance.Recollect<IItemCategoryModel>(enumType.ToString());

            var manager = GetItemFactory().CreateItemManager();
            var parameters = GetItemFactory().CreateItemCategoryParameters();
            var dataResult = manager.GetCategories(parameters);
            if (dataResult.Success)
            {
                knowledge.SetData(dataResult.Content);
            }
            Instance.Remember(enumType.ToString(), knowledge);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static void ReloadProductCategoryRelation()
        {
            var enumType = DrKnowAllKeys.ProductCategoryRelation;

            var knowledge = Instance.Recollect<IItemCategoryRelationModel>(enumType.ToString());

            var manager = GetItemFactory().CreateItemManager();
            var dataResult = manager.GetAllCategoryRelation();

            if (dataResult.Success)
            {
                knowledge.SetData(dataResult.Content);
            }
            Instance.Remember(enumType.ToString(), knowledge);
        }

    }

}