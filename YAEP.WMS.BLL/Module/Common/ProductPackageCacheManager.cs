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
    internal class ProductPackageCacheManager
    {
        List<IProductPackageExtendModel> _itemPackageCache;
        private IEnumerable<IGroupUserViewModel> _GroupUsers;
        Func<YAEP.Core.Item.Interfaces.IItemManager> _itemManagerFunc;
        Func<List<IProductPackageExtendModel>> _productPackageCacheMethod;
        ILogInfiltrator _Logger { get; set; }
        ITracingAgent _TraceAgent { get; set; }

        public ProductPackageCacheManager(Func<List<IProductPackageExtendModel>> productPackageCacheMethod, Func<YAEP.Core.Item.Interfaces.IItemManager> itemManagerFunc, ILogInfiltrator log = null,
            ITracingAgent tracingAgent = null)
        {
            _Logger = log;
            _TraceAgent = tracingAgent;
            //_ItemCollection = cache;
            _itemManagerFunc = itemManagerFunc;
            //this._ItemCollection = cache;
            //this._builtinItemRep = itemRepository;
            _productPackageCacheMethod = productPackageCacheMethod;
            LoadCache();
        }
        public ProductPackageCacheManager(Func<List<IProductPackageExtendModel>> productPackageCacheMethod, IActionResult<IEnumerable<IGroupUserViewModel>> groups
            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemManagerFunc
            , ILogInfiltrator log = null)
            : this(productPackageCacheMethod, itemManagerFunc, log)
        {
            this._GroupUsers = groups.Content;
        }
        public void LoadCache()
        {
            this._itemPackageCache = this._productPackageCacheMethod.Invoke();
        }
        public IProductPackageExtendModel GetItem(Guid productid, [CallerMemberName] string memberName = "")
        {
            return this.GetItems(new Guid[] { productid }, memberName)?.FirstOrDefault();
        }
        public IEnumerable<IProductPackageExtendModel> GetItems(IEnumerable<Guid> productid, [CallerMemberName] string memberName = "")
        {
            //loadAllitemtoCache();

            if (productid != null)
            {
                //WriteLog("start group by parameter", "GetItems", Logger.INFO, productid);
                var groupbyItem = productid.GroupBy(g => g).Select(p => p.Key).ToList();
                //WriteLog("end group by parameter", "GetItems", Logger.INFO, productid);

                //WriteLog("search cache data start", "GetItems", Logger.INFO, groupbyItem);
                return this._itemPackageCache.Where(p => groupbyItem.Any(x => p.UID == x)).ToList();
                //WriteLog("search cache data end", "GetItems", Logger.INFO);
            }
            else
            {
                return this._itemPackageCache;
            }
        }
        public IEnumerable<IProductPackageExtendModel> GetItem(string productid, IEnumerable<Guid> customeruid,
            IEnumerable<IGroupUserViewModel> groupUserViews)
        {
            return GetItems(new List<string>() { productid }, customeruid, groupUserViews);
        }
        public IEnumerable<IProductPackageExtendModel> GetItems(IEnumerable<string> productid, IEnumerable<Guid> customeruid,
            IEnumerable<IGroupUserViewModel> groupUserViews)
        {
            var _ItemManager = this._itemManagerFunc.Invoke();
            this._GroupUsers = groupUserViews;

            var itemGrp = productid.GroupBy(g => g).Select(p => p.Key);
            var item = this._itemPackageCache.Where(p => (customeruid.Count() == 0 || customeruid.Contains(new Guid(p.CustomerUID)))
            && productid.Any(x => x.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
            && this._GroupUsers.Any(y => y.GroupUID == p.GroupUID)).ToList();
            return item;
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
}
