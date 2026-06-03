using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Hosting;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.Interfaces;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Data.ORM.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Identities.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.Utilities;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Cache.CacheManager;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Manager
{
    public abstract class AbstractManager : ILogInfiltrator, ITransactionAction, IDisposable
    {
        ObjectCache _Cache = MemoryCache.Default;
        private bool disposed = false;
        private CacheManager cacheManager;
        private ProductCacheManager _productCacheManager;
        private PackageCacheManager _packageCacheManager;
        private ProductPackageCacheManager _productPackageCacheManager;
        protected IAppSettings AppSettings { get; private set; }
        protected IGroupManager GroupManager { get; set; }
        protected IPartyManager PartyManager { get; set; }
        protected IPackageManager PackageManager { get; set; }
        protected IPackageUomManager PackageUomManager { get; set; }
        protected IPackageVersionRepository PackageVersionRepository { get; set; }
        protected IItemManager ItemManager { get; set; }
        public ITracingAgent TracingAgent { get; set; }
        protected IObjectRelationalMappingLayer DbEntities { get; set; }

        protected IRefreshDrKnowAll RefreshDrKnowAllManager { get; set; }
        internal PackageCacheManager PackageCacheManager
        {
            get
            {
                return _packageCacheManager;
            }
        }
        protected void ClearProductPackageCache()
        {
            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PRODUCT_PACKAGE_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PRODUCT_PACKAGE_CACHE_KEY);
            });


        }
        protected void ClearProductCache()
        {
            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PRODUCT_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PRODUCT_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PRODUCT_PACKAGE_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PRODUCT_PACKAGE_CACHE_KEY);
            });



        }
        protected void ClearPackageCache()
        {
            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PKG_DATA_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PKG_DATA_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear ITEM_PKG_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.ITEM_PKG_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PKG_VER_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PKG_VER_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PKG_VER_DATA_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PKG_VER_DATA_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PKG_UOM_DATA_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PKG_UOM_DATA_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear MIN_PKG_PRODUCT_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.MIN_PKG_PRODUCT_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PKG_TREE_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PKG_TREE_CACHE_KEY);
            });

            Policy.Create().Retry(Policy.DEFAULT_RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TracingAgent?.Trace("Clear PRODUCT_PACKAGE_CACHE cache error", args.Exception);
            }).Execute(() =>
            {
                _Cache.Remove(CacheManager.PRODUCT_PACKAGE_CACHE_KEY);
            });


        }
        internal ProductCacheManager ProductCacheManager
        {
            get
            {
                return this._productCacheManager;
            }
        }
        internal ProductPackageCacheManager ProductPackageCacheManager
        {
            get
            {
                return this._productPackageCacheManager;
            }
        }
        internal ProductPackageCacheManager ProductPackageManager
        {
            get
            {
                return this._productPackageCacheManager;
            }
        }
        public AbstractManager(IAuthenticationProvider authenticationInfoProvider,
            ISequenceAgent sequenceAgent, IAppSettings appSettings, IGroupManager groupManager,
            IPackageManager packageManager, IPackageUomManager packageUomManager, IItemManager itemManager,
            IPartyManager partyManager, Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IObjectRelationalMappingLayer dbentities, IRefreshDrKnowAll refreshDKA, IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository) : this()
        {
            Debug.WriteLine("");
            this.TracingAgent = new TracingAgent();
            this.AuthProvider = authenticationInfoProvider;
            this.AppSettings = appSettings;
            this.DbEntities = dbentities;
            this.SequenceAgent = sequenceAgent;
            this.ProductUtility = new ProductUtility();
            this.GroupManager = groupManager;
            this.PartyManager = partyManager;
            this.PackageManager = packageManager;
            this.PackageUomManager = packageUomManager;
            this.ItemManager = itemManager;
            this.PackageVersionRepository = packageVersionRepository;
            this.TracingAgent.Init(this.AppConfigure, InternalLogger.GetLogger(), Logger.GetLogger(), this.AuthProvider);
            this.cacheManager = CacheManager.CreateInstance();
            _packageCacheManager = new PackageCacheManager(
                LoadPackageCache, LoadPackageVersionCache, LoadPackageUomCache,
                GetMiniPackageCache(), GetPkgversionCollection(),
                GetpkgCollection(), GetPkgTreeSet(), GetItemPkgTrees(), packageManager, this.PackageUomManager, this);
            _productCacheManager = new ProductCacheManager(this.GetProductCache, itemmgmterfunc,
                tracingAgent: this.TracingAgent);
            _productPackageCacheManager = new ProductPackageCacheManager(this.GetProductPackageCache, itemmgmterfunc,
                tracingAgent: this.TracingAgent);

            this.RefreshDrKnowAllManager = refreshDKA;

        }
        public AbstractManager()
        {


        }
        protected RequestManager RequestManager
        {
            get
            {
                if (_Cache[RequestManager.CACHE_NAME] == null)
                {
                    bool lockTaken = false;
                    Monitor.TryEnter(LockBuilder.RequestMangerLocker, 1 * 1000, ref lockTaken);
                    if (lockTaken)
                    {
                        try
                        {
                            if (_Cache[RequestManager.CACHE_NAME] == null)
                            {
                                _Cache.Add(RequestManager.CACHE_NAME, new RequestManager(),
                                new CacheItemPolicy()
                                {
                                    SlidingExpiration = new TimeSpan(23, 0, 0)
                                });
                            }
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Monitor.Exit(LockBuilder.RequestMangerLocker);
                            }
                        }
                    }
                }
                return _Cache[RequestManager.CACHE_NAME] as RequestManager;
            }
        }
        protected IAppConfigure AppConfigure
        {
            get
            {
                return new DefaultAppConfigure(this.AppSettings);
            }
        }
        private List<IProductExtendModel> GetProductCache()
        {

            return this.cacheManager.GetProductCache();
        }
        private List<IProductPackageExtendModel> GetProductPackageCache()
        {
            return this.cacheManager.GetProductPackageCache();
        }
        #region Pkg Cache
        private ConcurrentDictionary<Guid, IPackageNode> GetMiniPackageCache()
        {
            if (_Cache[CacheManager.MIN_PKG_PRODUCT_CACHE_KEY] == null)
            {
                _Cache.Add(CacheManager.MIN_PKG_PRODUCT_CACHE_KEY, new ConcurrentDictionary<Guid, IPackageNode>(),
                    new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[CacheManager.MIN_PKG_PRODUCT_CACHE_KEY] as ConcurrentDictionary<Guid, IPackageNode>;
        }
        private ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> GetPkgversionCollection()
        {
            if (_Cache[CacheManager.PKG_VER_CACHE_KEY] == null)
            {
                _Cache.Add(CacheManager.PKG_VER_CACHE_KEY, new ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>>()
                    , new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[CacheManager.PKG_VER_CACHE_KEY] as ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>>;
        }
        private ConcurrentDictionary<Guid, IPackageModel> GetpkgCollection()
        {
            if (_Cache[CacheManager.PKG_CACHE_KEY] == null)
            {
                _Cache.Add(CacheManager.PKG_CACHE_KEY, new ConcurrentDictionary<Guid, IPackageModel>()
                    , new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[CacheManager.PKG_CACHE_KEY] as ConcurrentDictionary<Guid, IPackageModel>;
        }
        private ConcurrentBag<IPackageTree> GetPkgTreeSet()
        {
            if (_Cache[CacheManager.PKG_TREE_CACHE_KEY] == null)
            {
                _Cache.Add(CacheManager.PKG_TREE_CACHE_KEY, new ConcurrentBag<IPackageTree>()
                    , new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[CacheManager.PKG_TREE_CACHE_KEY] as ConcurrentBag<IPackageTree>;
        }
        private ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> GetItemPkgTrees()
        {
            if (_Cache[CacheManager.ITEM_PKG_CACHE_KEY] == null)
            {
                _Cache.Add(CacheManager.ITEM_PKG_CACHE_KEY, new ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>>()
                    , new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(23, 0, 0)
                    });
            }
            return _Cache[CacheManager.ITEM_PKG_CACHE_KEY] as ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>>;
        }

        private List<IPackageViewModel> LoadPackageCache()
        {

            return this.cacheManager.LoadPackageCache();
        }
        private List<IPackageVersionModel> LoadPackageVersionCache()
        {

            return this.cacheManager.LoadPackageVersionCache();
        }
        private List<IPackageUomModel> LoadPackageUomCache()
        {

            return this.cacheManager.LoadPackageUomCache();
        }
        #endregion
        protected void RefreshProductCache(Guid? itemUID = null)
        {
            var rwlocker = LockBuilder.ProductPackageLock;
            try
            {
                rwlocker.AcquireWriterLock(5 * 60 * 1000);
                if (itemUID != null)
                {
                    this.RefreshDrKnowAllManager.RefreshProduct(itemUID.Value);
                }
                using (var activity = this.TracingAgent?.StartActivity("Clear product cache"))
                {
                    this.ClearProductCache();
                }
                using (var activity = this.TracingAgent?.StartActivity("Clear package cache"))
                {
                    this.ClearPackageCache();
                }
                using (var activity = this.TracingAgent?.StartActivity("load product cache"))
                {
                    this.ProductCacheManager.LoadCache();
                }
                using (var activity = this.TracingAgent?.StartActivity("load package cache"))
                {
                    this.PackageCacheManager.LoadCache();
                }
                using (var activity = this.TracingAgent?.StartActivity("load product/package cache"))
                {
                    this.ProductPackageCacheManager.LoadCache();
                }
            }
            catch
            {

            }
            finally
            {
                if (rwlocker.IsWriterLockHeld)
                {
                    rwlocker.ReleaseWriterLock();
                }
            }

        }
        protected void LogConnectionSPID()
        {
            if (this.DbEntities.Connection != null)
            {
                try
                {
                    //   var spid = this.DbEntities.QueryFirst<int>("select @@SPID", null);
                    //  this.TracingAgent.Trace($"Current db session id:{spid}", spid);
                }
                catch (Exception ex)
                {
                    this.TracingAgent?.Trace($"Get spid failure", ex);
                }
            }
        }
        public dynamic GetPreviousActionStack()
        {
            //取得呼叫方的記錄
            dynamic callobj = null;
            var stack = new StackTrace(true);
            if (stack.FrameCount >= 1)
            {
                callobj = new
                {
                    FileName = stack.GetFrame(1).GetFileName(),
                    MethodName = stack.GetFrame(1).GetMethod(),
                    FileLineNumber = stack.GetFrame(1).GetFileLineNumber(),
                };

            }
            return callobj;
        }
        protected void LogConnectionStatus([CallerMemberName] string memberName = "")
        {
            dynamic connectionobj = new ExpandoObject();
            if (this.DbEntities.Connection != null)
            {
                connectionobj.ConnectionId = this.DbEntities.Connection?.GetHashCode();
                connectionobj.ConnectionStatus = this.DbEntities.Connection?.State.ToString();
            }
            else
            {
                connectionobj.Connection = "Connection has disappear";
            }
            if (this.DbEntities.Transaction != null)
            {
                connectionobj.TransactionId = this.DbEntities.Transaction?.GetHashCode();
                if (this.DbEntities.Transaction.Connection != null)
                {

                    connectionobj.TransactionIncludeConnection = this.DbEntities.Transaction.Connection?.GetHashCode();
                }
                else
                {
                    connectionobj.TransactionIncludeConnection = "Transaction include connection has disappear";
                }
            }
            else
            {
                connectionobj.TransactionId = "TransactionId has disappear";
            }

            //this.TracingAgent?.Trace($"Log connection status call by [{memberName}]", connectionobj, GetPreviousActionStack());
        }
        public void BeginTranaction(System.Data.IsolationLevel isolationLevel)
        {
            LogConnectionStatus();
            this.DbEntities.BeginTranaction(isolationLevel);
        }
        public void CloseConnection([CallerMemberName] string memberName = "")
        {
            LogConnectionStatus();
            if (this.DbEntities.Connection != null)
            {
                if (DbEntities.Connection.State == System.Data.ConnectionState.Open)
                {
                    this.DbEntities.Connection.Close();
                }
                else
                {
                    this.TracingAgent?.Trace($"Db connection has closed by{memberName}");
                }
            }
            else
            {
                this.TracingAgent?.Trace($"Not find tran call by{memberName}");
            }
        }
        public void DisposeConnectionInstance()
        {
            this.DbEntities.DbAdapter.Dispose();
        }
        public void ReInitConnectionInstance()
        {
            this.DbEntities.ReInitConnectionInstance();
        }
        public void CommitTransaction([CallerMemberName] string memberName = "")
        {
            LogConnectionStatus();
            if (this.DbEntities.Transaction != null)
            {

                this.DbEntities.Commit();
                //this.TracingAgent?.Trace($"Commit tran complete call by {memberName}");
            }
            else
            {
                this.TracingAgent?.Trace($"Not find tran call by {memberName}");
            }
        }
        public void RollbackTransaction([CallerMemberName] string memberName = "")
        {
            LogConnectionStatus();
            if (this.DbEntities.Transaction != null)
            {

                //this.TracingAgent?.Trace($"Rollback tran call by {memberName}");
                try
                {
                    this.DbEntities.Rollback();
                    //this.TracingAgent.Trace($"Rollback complete by {memberName}");
                }
                catch (Exception ex)
                {
                    LogConnectionStatus();
                    this.TracingAgent?.Trace($"Rollback tran failure", ex);
                }

            }
            else
            {
                this.TracingAgent?.Trace($"Not find tran call by{memberName}");
            }
        }
        protected IEnumerable<IPartyModel> GetCustomer(IEnumerable<Guid> groupUID, string customerID)
        {
            var searchPartyParameters = new PartyParameterize();
            searchPartyParameters.PartyTypeCategory = PartyTypeCategories.Customer;

            searchPartyParameters.ListOfGroupUID.AddRange(groupUID);
            searchPartyParameters.ID = customerID;
            var searchPartyResult = this.PartyManager.GetParties(searchPartyParameters);

            if (searchPartyResult.Success)
            {
                return searchPartyResult.Content;
            }

            return null;
        }
        protected IPartyModel GetCustomer(Guid groupUID, string customerID)
        {
            var searchPartyParameters = new PartyParameterize();
            searchPartyParameters.PartyTypeCategory = PartyTypeCategories.Customer;
            if (searchPartyParameters.ListOfGroupUID == null)
            {
                searchPartyParameters.ListOfGroupUID = new List<Guid>();
            }
            searchPartyParameters.ListOfGroupUID.Add(groupUID);
            searchPartyParameters.ID = customerID;
            var searchPartyResult = this.PartyManager.GetParties(searchPartyParameters);

            if (searchPartyResult.Success)
            {
                return searchPartyResult.Content?.FirstOrDefault();
            }

            return null;
        }
        internal IExtensionActionResult<R> GetExtensionActionResultContainer<R>(string message = null, R content = default(R), Exception innerException = null, int? typeCode = null)
        {
            var resultContainer = new ExtensionActionResultContainer<R>()
            {
                Success = true,
                Message = message,
                Content = content,
                InnerException = innerException,
                TypeCode = typeCode,

            };

            return resultContainer;
        }
        protected IActionResult<IEnumerable<IGroupUserViewModel>> GetGroupUserViewByUser()
        {
            return this.GroupManager.GetGroupUserViewByUser(this.AuthProvider.GetAuthenticationInfo().UID);
        }
        protected bool ExistTransactionScope { get; set; }
        protected dynamic GeneratoreObject()
        {
            return new ExpandoObject();
        }

        protected decimal CalculateVolume(IPackageModel package, int qty)
        {
            return ProductUtility.CalculateCUFT(package, qty);
        }
        protected void OutpubToDebugLine(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Log(string message, string type, string owner, string level,
         int belongToType, string belongToUID = "", string belongToRemark = "", string application = "", string subApplication = "",
         Exception exception = null, string ip = "", string jsonBefore = "", string jsonAfter = "")
        {

            var logger = Logger.GetLogger();
            logger.Log(message, type, owner, level, belongToType, belongToUID,
                belongToRemark, application, subApplication, exception, ip, jsonBefore, jsonAfter);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.DbEntities?.Dispose();
                if (disposing)
                {

                }
                disposed = true;
            }
        }
        ~AbstractManager()
        {
            Dispose(false);
        }
        protected IAuthenticationProvider AuthProvider { get; set; }
        protected ISequenceAgent SequenceAgent { get; set; }

        internal ProductUtility ProductUtility { get; set; }

        public string InfoString => Logger.INFO;

        public string ErrorString => Logger.ERROR;

        public string WarnString => Logger.WARN;

        public string FatalString => Logger.FATAL;

        public string DebugString => Logger.DEBUG;
    }
}

