using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.Interfaces;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Identities.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Module
{
    internal class ProductCacheManager
    {
        //ConcurrentDictionary<ProductInfo, IItemModel> _ItemCollection;
        List<IProductExtendModel> _ItemCache;
        //private IItemManager _ItemManager;
        private IEnumerable<IGroupUserViewModel> _GroupUsers;
        Func<YAEP.Core.Item.Interfaces.IItemManager> _itemManagerFunc;
        Func<List<IProductExtendModel>> _productCacheMethod;
        //private IItemRepository _builtinItemRep;
        ILogInfiltrator _Logger { get; set; }
        ITracingAgent _TraceAgent { get; set; }
        public ProductCacheManager(Func<List<IProductExtendModel>> productCacheMethod, Func<YAEP.Core.Item.Interfaces.IItemManager> itemManagerFunc, ILogInfiltrator log = null,
            ITracingAgent tracingAgent = null)
        {
            _Logger = log;
            _TraceAgent = tracingAgent;
            //_ItemCollection = cache;
            _itemManagerFunc = itemManagerFunc;
            //this._ItemCollection = cache;
            //this._builtinItemRep = itemRepository;
            _productCacheMethod = productCacheMethod;
            LoadCache();
        }
        public List<IProductExtendModel> ItemCache
        {
            get
            {
                //var rwlocker = LockBuilder.ProductPackageLock;
                //try
                //{
                //    rwlocker.AcquireReaderLock(2 * 60 * 1000);

                //}
                //catch (Exception ex)
                //{
                //    return new List<IProductExtendModel>();
                //}
                //finally
                //{
                //    rwlocker.ReleaseReaderLock();
                //}
                return this._ItemCache;
            }
            set
            {
                this._ItemCache = value;
            }
        }


        public ProductCacheManager(Func<List<IProductExtendModel>> productCacheMethod, IActionResult<IEnumerable<IGroupUserViewModel>> groups
            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemManagerFunc
            , ILogInfiltrator log = null)
            : this(productCacheMethod, itemManagerFunc, log)
        {
            this._GroupUsers = groups.Content;
        }
        public void LoadCache()
        {
            this.ItemCache = this._productCacheMethod.Invoke();
        }
        //public IEnumerable<IItemModel> GetItemWithoutCache(IEnumerable<string> ItemNos)
        //{
        //    var _ItemManager = this._itemManagerFunc.Invoke();
        //    ItemInnerParameterize param = new ItemInnerParameterize();
        //    param.ListOfItemID.AddRange(ItemNos);
        //    return _ItemManager.GetItems(param).Content;
        //}
        public IItemModel GetItem(Guid productid, [CallerMemberName] string memberName = "")
        {
            return this.GetItems(new Guid[] { productid }, memberName)?.FirstOrDefault();
        }
        public IEnumerable<IProductExtendModel> GetItems(IEnumerable<Guid> productid, [CallerMemberName] string memberName = "")
        {
            //loadAllitemtoCache();

            if (productid != null)
            {
                //WriteLog("start group by parameter", "GetItems", Logger.INFO, productid);
                var groupbyItem = productid.GroupBy(g => g).Select(p => p.Key).ToList();
                //WriteLog("end group by parameter", "GetItems", Logger.INFO, productid);

                //WriteLog("search cache data start", "GetItems", Logger.INFO, groupbyItem);
                return this.ItemCache.Where(p => groupbyItem.Any(x => p.UID == x)).ToList();
                //WriteLog("search cache data end", "GetItems", Logger.INFO);
            }
            else
            {
                return this.ItemCache;
            }
        }


        public IEnumerable<IProductExtendModel> GetItem(string productid, IEnumerable<Guid> customeruid,
            IEnumerable<IGroupUserViewModel> groupUserViews)
        {
            this._GroupUsers = groupUserViews;
            var item = this.ItemCache.Where(p => (customeruid.Count() == 0
            || customeruid.Contains(new Guid(p.CustomerUID)))
            && p.Name.Equals(productid, StringComparison.OrdinalIgnoreCase)
            && this._GroupUsers.Any(y => y.GroupUID == p.GroupUID));

            //var item = this._ItemCache.Where(p =>
            //{
            //    var a2 = p.Name.Equals(productid, StringComparison.OrdinalIgnoreCase);
            //    var a1 = (customeruid.Count() == 0
            // || customeruid.Contains(new Guid(p.CustomerUID)));
            //    var a3 = this._GroupUsers.Any(y => y.GroupUID == p.GroupUID);
            //    return a1 && a2 && a3;
            //});

            return item;
        }
        public IEnumerable<IProductExtendModel> GetItems(IEnumerable<string> productid, Guid? customeruid,
            IEnumerable<IGroupUserViewModel> groupUserViews)
        {
            var _ItemManager = this._itemManagerFunc.Invoke();
            this._GroupUsers = groupUserViews;

            var itemGrp = productid.GroupBy(g => g).Select(p => p.Key);
            var item = this.ItemCache.Where(p => p.CustomerUID == customeruid.Value.ToString("D")
            && productid.Any(x => x.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
            && this._GroupUsers.Any(y => y.GroupUID == p.GroupUID)).ToList();
            return item;
        }

        public IPartyModel GetCustomer(Guid groupUID, string customerID, IPartyManager partyManager)
        {
            var searchPartyParameters = new PartyParameterize();
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
        [Obsolete("舊方法，請改用NewCombineToActualItem")]
        public IActionResult<ProductExtendModel> CombineToActualItem(IEnumerable<VirtualItemInfo> virtualItems)
        {
            if ((virtualItems?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>(nameof(virtualItems));
            }
            if (virtualItems.GroupBy(o => o.ActualProduct).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductExtendModel>("The property 'ActualProduct' cannot be difference.");
            }
            if (virtualItems.GroupBy(o => o.CustomerUID).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductExtendModel>("The property 'CustomerUID' cannot be difference.");
            }

            var customerUID = virtualItems.FirstOrDefault()?.CustomerUID ?? Guid.Empty;
            string actualProduct = virtualItems.FirstOrDefault()?.ActualProduct;

            if (String.IsNullOrWhiteSpace(actualProduct))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>("ActualProduct");
            }

            if (customerUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>("CustomerUID");
            }

            var getCurrentVirtualItemsResult = this.GetVirtualItems(actualProduct, new Guid[] { customerUID });
            if (!getCurrentVirtualItemsResult.Success)
            {
                return ActionResultTemplates.Error<ProductExtendModel>(getCurrentVirtualItemsResult.Message);
            }
            if ((getCurrentVirtualItemsResult.Content?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.Error<ProductExtendModel>("This item no setting virtual items.");
            }

            var result = ActionResultTemplates.Result<ProductExtendModel>();
            var currentVirtualItems = getCurrentVirtualItemsResult.Content;

            int combinedQuantity = -1;
            foreach (var currentVirtualItem in currentVirtualItems)
            {
                //var virtualItem = virtualItems.FirstOrDefault(o => o.ProductId.Equals(currentVirtualItem.ID, StringComparison.OrdinalIgnoreCase));
                var virtualItem = virtualItems.FirstOrDefault(o => o.ProductUID == currentVirtualItem.UID);
                // 參數缺少虛擬 Item
                if (virtualItem == null)
                {
                    result.Message = $"missing item - '{currentVirtualItem.ID}' ";
                    break;
                }
                // 數量無法整除
                if ((virtualItem.Quantity % currentVirtualItem.BoxQuantity) > 0)
                {
                    result.Message = $"Indivisible quantity ({virtualItem.Quantity} / {currentVirtualItem.BoxQuantity}) - '{currentVirtualItem.ID}' ";
                    break;
                }

                // 檢查組合後的數量是否一致
                int currentCombinedQuantity = (virtualItem.Quantity / currentVirtualItem.BoxQuantity);
                if (combinedQuantity > 0 && combinedQuantity != currentCombinedQuantity)
                {
                    result.Message = $"The quantity dissmatch - '{currentVirtualItem.ID}' ";
                    break;
                }

                combinedQuantity = currentCombinedQuantity;
            }

            if (String.IsNullOrWhiteSpace(result.Message))
            {
                var getActualItemResult = this.GetActualItem(actualProduct, new Guid[] { customerUID });

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
        public IActionResult<ProductExtendModel> NewCombineToActualItem(IEnumerable<VirtualItemInfo> virtualItems)
        {
            List<VirtualItemInfo> merageCollection = new List<VirtualItemInfo>();
            if ((virtualItems?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>(nameof(virtualItems));
            }
            if (virtualItems.GroupBy(o => o.ActualProduct).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductExtendModel>("The property 'ActualProduct' cannot be difference.");
            }
            if (virtualItems.GroupBy(o => o.CustomerUID).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductExtendModel>("The property 'CustomerUID' cannot be difference.");
            }

            var customerUID = virtualItems.FirstOrDefault()?.CustomerUID ?? Guid.Empty;
            string actualProduct = virtualItems.FirstOrDefault()?.ActualProduct;

            if (String.IsNullOrWhiteSpace(actualProduct))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>("ActualProduct");
            }

            if (customerUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>("CustomerUID");
            }

            var getCurrentVirtualItemsResult = this.GetVirtualItems(actualProduct, new Guid[] { customerUID });
            if (!getCurrentVirtualItemsResult.Success)
            {
                return ActionResultTemplates.Error<ProductExtendModel>(getCurrentVirtualItemsResult.Message);
            }
            if ((getCurrentVirtualItemsResult.Content?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.Error<ProductExtendModel>("This item no setting virtual items.");
            }

            var result = ActionResultTemplates.Result<ProductExtendModel>();
            var currentVirtualItems = getCurrentVirtualItemsResult.Content;

            int combinedQuantity = -1;
            //合併欲比對的PUOM資料
            foreach (var currentVirtualItem in currentVirtualItems)
            {
                var findpuom = virtualItems.Where(p => p.PUOM == currentVirtualItem.PUOM);
                var e = new VirtualItemInfo();
                if (findpuom != null && findpuom.Count() > 0)
                {
                    e.ActualProduct = findpuom.FirstOrDefault().ActualProduct;
                    e.CustomerUID = findpuom.FirstOrDefault().CustomerUID;
                    e.ProductId = findpuom.FirstOrDefault().ProductId;
                    e.ProductUID = findpuom.FirstOrDefault().ProductUID;
                    e.PUOM = findpuom.FirstOrDefault().PUOM;
                    e.ProductId = findpuom.FirstOrDefault().ProductId;
                    e.Quantity = findpuom.Sum(p => p.Quantity);
                    merageCollection.Add(e);
                }
            }
            foreach (var currentVirtualItem in currentVirtualItems)
            {
                //var virtualItem = virtualItems.FirstOrDefault(o => o.ProductId.Equals(currentVirtualItem.ID, StringComparison.OrdinalIgnoreCase));
                var virtualItem = merageCollection.FirstOrDefault(o => o.PUOM == currentVirtualItem.PUOM);
                // 參數缺少虛擬 Item
                if (virtualItem == null)
                {
                    result.Message = $"missing item - '{currentVirtualItem.ID}' ";
                    result.TypeCode = 100;
                    break;
                }
                // 數量無法整除
                if ((virtualItem.Quantity % currentVirtualItem.BoxQuantity) > 0)
                {
                    result.Message = $"Indivisible quantity ({virtualItem.Quantity} / {currentVirtualItem.BoxQuantity}) - '{currentVirtualItem.ID}' ";
                    result.TypeCode = 200;
                    break;
                }

                // 檢查組合後的數量是否一致
                int currentCombinedQuantity = (virtualItem.Quantity / currentVirtualItem.BoxQuantity);
                if (combinedQuantity > 0 && combinedQuantity != currentCombinedQuantity)
                {
                    result.Message = $"The quantity dissmatch - '{currentVirtualItem.ID}' ";
                    result.TypeCode = 300;
                    break;
                }

                combinedQuantity = currentCombinedQuantity;
            }

            if (String.IsNullOrWhiteSpace(result.Message))
            {
                var getActualItemResult = this.GetActualItem(actualProduct, new Guid[] { customerUID });

                //比對成功後再換算母產品數量
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
        /// <param name="virtualItems"></param>
        /// <param name="passvaildQty">略過虛擬產品數量的驗証，只驗証虛擬產品組成是否正確，回傳的Qty會不正確</param>
        /// <returns></returns>
        public IActionResult<ProductExtendModel> NewCombineToActualItem(IEnumerable<VirtualItemInfo> virtualItems, bool passvaildQty)
        {
            List<VirtualItemInfo> merageCollection = new List<VirtualItemInfo>();
            if ((virtualItems?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>(nameof(virtualItems));
            }
            if (virtualItems.GroupBy(o => o.ActualProduct).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductExtendModel>("The property 'ActualProduct' cannot be difference.");
            }
            if (virtualItems.GroupBy(o => o.CustomerUID).Count() > 1)
            {
                return ActionResultTemplates.ArgumentExceptionResult<ProductExtendModel>("The property 'CustomerUID' cannot be difference.");
            }

            var customerUID = virtualItems.FirstOrDefault()?.CustomerUID ?? Guid.Empty;
            string actualProduct = virtualItems.FirstOrDefault()?.ActualProduct;

            if (String.IsNullOrWhiteSpace(actualProduct))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>("ActualProduct");
            }

            if (customerUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>("CustomerUID");
            }

            var getCurrentVirtualItemsResult = this.GetVirtualItems(actualProduct, new Guid[] { customerUID });
            if (!getCurrentVirtualItemsResult.Success)
            {
                return ActionResultTemplates.Error<ProductExtendModel>(getCurrentVirtualItemsResult.Message);
            }
            if ((getCurrentVirtualItemsResult.Content?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.Error<ProductExtendModel>("This item no setting virtual items.");
            }

            var result = ActionResultTemplates.Result<ProductExtendModel>();
            var currentVirtualItems = getCurrentVirtualItemsResult.Content;

            int combinedQuantity = -1;
            //合併欲比對的PUOM資料
            foreach (var currentVirtualItem in currentVirtualItems)
            {
                var findpuom = virtualItems.Where(p => p.PUOM == currentVirtualItem.PUOM);
                var e = new VirtualItemInfo();
                if (findpuom != null && findpuom.Count() > 0)
                {
                    e.ActualProduct = findpuom.FirstOrDefault().ActualProduct;
                    e.CustomerUID = findpuom.FirstOrDefault().CustomerUID;
                    e.ProductId = findpuom.FirstOrDefault().ProductId;
                    e.ProductUID = findpuom.FirstOrDefault().ProductUID;
                    e.PUOM = findpuom.FirstOrDefault().PUOM;
                    e.ProductId = findpuom.FirstOrDefault().ProductId;
                    e.Quantity = findpuom.Sum(p => p.Quantity);
                    merageCollection.Add(e);
                }
            }
            foreach (var currentVirtualItem in currentVirtualItems)
            {
                //var virtualItem = virtualItems.FirstOrDefault(o => o.ProductId.Equals(currentVirtualItem.ID, StringComparison.OrdinalIgnoreCase));
                var virtualItem = merageCollection.FirstOrDefault(o => o.PUOM == currentVirtualItem.PUOM);
                // 參數缺少虛擬 Item
                if (virtualItem == null)
                {
                    result.Message = $"missing item - '{currentVirtualItem.ID}' ";
                    result.TypeCode = 100;
                    break;
                }
                // 數量無法整除
                if (((virtualItem.Quantity % currentVirtualItem.BoxQuantity) > 0) || passvaildQty)
                {
                    result.Message = $"Indivisible quantity ({virtualItem.Quantity} / {currentVirtualItem.BoxQuantity}) - '{currentVirtualItem.ID}' ";
                    result.TypeCode = 200;
                    break;
                }

                // 檢查組合後的數量是否一致
                int currentCombinedQuantity = (virtualItem.Quantity / currentVirtualItem.BoxQuantity);
                if ((combinedQuantity > 0 && combinedQuantity != currentCombinedQuantity) || passvaildQty)
                {
                    result.Message = $"The quantity dissmatch - '{currentVirtualItem.ID}' ";
                    result.TypeCode = 300;
                    break;
                }

                combinedQuantity = currentCombinedQuantity;
            }

            if (String.IsNullOrWhiteSpace(result.Message))
            {
                var getActualItemResult = this.GetActualItem(actualProduct, new Guid[] { customerUID });

                //比對成功後再換算母產品數量
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
        public IActionResult<IEnumerable<ProductExtendModel>> GetVirtualItems(string actualProductID, IEnumerable<Guid> customerUID)
        {
            var _ItemManager = this._itemManagerFunc.Invoke();
            var parameters = new ItemInnerParameterize();
            foreach (var item in customerUID)
            {
                parameters.ItemProperties.Add(new ItemPropertySearchModel()
                {
                    Name = "CustomerUID",
                    Value = item.ToString()
                });
            }
            parameters.ItemProperties.Add(new ItemPropertySearchModel()
            {
                Name = "ActualProduct",
                Value = actualProductID
            });

            var result = _ItemManager.GetItems<ProductExtendModel>(parameters);

            return result;
        }
        public IEnumerable<IProductExtendModel> GetVirtualItemsByCache(string actualProductID,
            IEnumerable<Guid> customerUID, IEnumerable<IGroupUserViewModel> groupUserViewModels)
        {
            var _ItemManager = this._itemManagerFunc.Invoke();
            var actiteminfo = this.GetItem(actualProductID, customerUID, groupUserViewModels);
            var result = this.ItemCache.Where(p => p.ActualProduct == actiteminfo.FirstOrDefault().ID);

            return result;
        }
        private IActionResult<ProductExtendModel> GetActualItem(string productId, IEnumerable<Guid> customerUID)
        {
            var _ItemManager = this._itemManagerFunc.Invoke();
            if (String.IsNullOrWhiteSpace(productId))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>(nameof(productId));
            }

            if (customerUID == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<ProductExtendModel>(nameof(customerUID));
            }

            var parameters = new ItemInnerParameterize();
            parameters.ID = productId;
            foreach (var item in customerUID)
            {
                parameters.ItemProperties.Add(new ItemPropertySearchModel()
                {
                    Name = "CustomerUID",
                    Value = item.ToString()
                });
            }

            var result = _ItemManager.GetItems<ProductExtendModel>(parameters);
            if (result.Success && (result.Content?.Count() ?? 0) > 0)
            {
                var r = ActionResultTemplates.Result<ProductExtendModel>();
                r.Success = true;
                r.Content = result.Content.FirstOrDefault();
                return r;
            }
            else
            {
                return ActionResultTemplates.Error<ProductExtendModel>(result.Message, innerException: result.InnerException);
            }
        }
        private void WriteLog(string message, string type, string level, object requestobj = null)
        {

            if (_TraceAgent != null)
            {
                var reqobjstr = "";
                if (requestobj != null)
                {
                    try
                    {
                        reqobjstr = Newtonsoft.Json.JsonConvert.SerializeObject(requestobj);
                    }
                    catch { }
                }
                this._TraceAgent.Trace(message, reqobjstr);
            }
            if (_Logger != null)
            {
                var reqobjstr = "";
                if (requestobj != null)
                {
                    try
                    {
                        reqobjstr = Newtonsoft.Json.JsonConvert.SerializeObject(requestobj);
                    }
                    catch { }
                }
                this._Logger.Log(message, type, "", level, (int)YAEP.Constants.BelongToTypes.Item,
                    application: WMSAPIParameters.CONNECT_LOG_NAME, jsonBefore: reqobjstr);
            }
        }
        private T retryProcess<T>(Func<IActionResult<T>> p)
        {
            int maxRetry = 3;
            int current = 0;
            while (maxRetry >= current)
            {
                var rs = p.Invoke();
                if (rs.Success)
                {
                    return rs.Content;
                }
                else
                {
                    WriteLog($"Invoke method {p.Method.Name} failure {rs.Message}", p.Method.Name, Logger.ERROR);
                    current++;
                }
            }
            return default(T);
        }

    }
    internal class ProductInfo
    {
        public string ProductId { get; set; }
        public Guid ProductUID { get; set; }
        public Guid CustomerUID { get; set; }
    }

    internal class VirtualItemInfo
    {
        public string ProductId { get; set; }
        public string PUOM { get; set; }
        public Guid ProductUID { get; set; }
        public Guid CustomerUID { get; set; }
        public string ActualProduct { get; set; }
        public int Quantity { get; set; }
    }

}
