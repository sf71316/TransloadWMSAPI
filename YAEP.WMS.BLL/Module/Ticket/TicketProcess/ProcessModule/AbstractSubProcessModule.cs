using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractProcessModule
    {
        protected IInventoryManager inventoryManager;
        protected ILabelRepository labelRepository;
        protected ILogInfiltrator logger;
        protected IManifestManger manifestManger;
        protected IBulkPickManager bulkPickManager;
        protected IPackageManager packageManager;
        protected IPackageUomManager packageUomManager;
        protected NotifySenderConfig sendInfo;
        protected ISequenceAgent sequenceAgent;
        protected StatusManageAgent statusManageAgent;
        protected ITicketManager ticketManager;
        //protected ITransacationScope transacationScope;
        protected IWarehouseManger warehouseManger;
        protected IWorkOrderManager workOrderManager;
        protected IWorkOrderPayloadRepository workOrderPayloadRepository;
        protected IWorkOrderRepository workOrderRepository;
        protected IWorkOrderPodRepository workOrderPodRepository;
        protected INotificationSenderTaskRepository notificationSenderTaskRepository;
        protected PackageCacheManager packageManagerCache;
        protected IAppConfigure appConfigure;
        protected ITicketInfoRepository ticketInfoRepository;
        protected ITicketInfoAssigneeRelationRepository ticketInfoAssigneeRelationRepository;
        protected ITicketRepository ticketRepository;
        protected ReplicationManager replicationManager;
        protected ITracingAgent tracingAgent;
        protected ITransactionAction TransactionAction { get; set; }
        protected IAuthenticationProvider AuthenticationProvider { get; set; }
        internal AbstractProcessModule(
            ITicketProcessAgentParameter parameters, ILogInfiltrator logInfiltrator)
        {
            this.tracingAgent = parameters.TracingAgent;
            this.AuthenticationProvider = parameters.AuthenticationProvider;
            this.workOrderManager = parameters.WorkOrderManager;
            this.ticketManager = parameters.TicketManager;
            this.manifestManger = parameters.ManifestManager;
            this.inventoryManager = parameters.InventoryManager;
            this.warehouseManger = parameters.WarehouseManger;
            this.sequenceAgent = parameters.SequenceAgent;
            this.packageManager = parameters.PackageManager;
            this.packageUomManager = parameters.PackageUomManager;
            this.labelRepository = parameters.LabelRepository;
            this.appConfigure = parameters.AppConfigure;
            this.bulkPickManager = parameters.BulkPickManager;
            //this.UploadData = uploadTicketData;
            this.logger = logInfiltrator;
            this.statusManageAgent = parameters.StatusAgent;
            //this.transacationScope = parameters.TransactionScopeAgent;
            this.notificationSenderTaskRepository = parameters.NotificationSenderTaskRepository;
            this.ticketInfoRepository = parameters.TicketInfoRepository;
            this.ticketInfoAssigneeRelationRepository = parameters.TicketInfoAssigneeRelationRepository;
            this.workOrderManager = parameters.WorkOrderManager;
            this.workOrderPayloadRepository = parameters.WorkOrderPayloadRepository;
            this.workOrderPodRepository = parameters.WorkOrderPodRepository;
            this.workOrderRepository = parameters.WorkOrderRepository;
            this.ProductManager = new ProductUtility();
            this.replicationManager = parameters.ReplicationManager;
            this.ticketRepository = parameters.TicketRepository;
            this.packageManagerCache = parameters.PackageCacheManager;
            this.CompleteUnexecutedMethod = new ConcurrentQueue<Func<IActionResult<bool>>>();
            TransactionAction = parameters.TransactionAction;
        }

        internal ProductUtility ProductManager { get; set; }
        //protected bool IsExistTransaction => this.transacationScope.ExistTransactionScope;
        protected IEnumerable<ITicketInfoParameter> UploadData { get; set; }
        public ConcurrentQueue<Func<IActionResult<bool>>> CompleteUnexecutedMethod { get; set; }
        protected ITicketRepository TicketRepository { get => ticketRepository; set => ticketRepository = value; }

        public abstract IActionResult<bool> Execute(IEnumerable<IUploadTicketDataParameter> Data,
            NotifySenderConfig sendInfo = null);

        internal static AbstractProcessModule GetProcessModule(TicketType ticketType,
                ITicketProcessAgentParameter parameters, ILogInfiltrator logInfiltrator)
        {
            switch (ticketType)
            {
                case TicketType.Receiving:
                    return new InboundSubProcessModule(parameters, logInfiltrator);
                case TicketType.Outbound:
                    return new OutboundSubProcessModule(parameters, logInfiltrator);
                case TicketType.Move:
                case TicketType.BulkPick:
                    return new MoveSubProcessModule(parameters, logInfiltrator);
                case TicketType.InventoryCounting:
                    return new InventoryCountingSubProcessModule(parameters, logInfiltrator);

            }
            return null;
        }

        protected decimal CalculateVolume(IPackageModel package, int qty)
        {
            return ProductManager.CalculateCUFT(package, qty);
        }

        internal ConcurrentStack<Func<IActionResult<bool>>> CheckDataStatus(Guid ticketUID,
            TicketType? serviceItem = null,
            IEnumerable<IStatusCheckModel> statusCheckModels = null)
        {
            #region ActionDictionary 
            var _ticketProcessList = new Dictionary<TicketStatus, List<Guid>>();
            var _wpayloadProcessList = new Dictionary<WorkOrderPayloadStatus, List<Guid>>();
            var _wpodProcessList = new Dictionary<WorkOrderPodStatus, List<Guid>>();
            var _workorderProcessList = new Dictionary<WorkOrderStatus, List<Guid>>();
            var _vesselManifestProcessList = new Dictionary<VesselManifestStatus, List<Guid>>();
            var _bulkPickProcessList = new Dictionary<BulkPickStatus, List<Guid>>();
            var _vesselProcessList = new Dictionary<VesselStatus, List<Guid>>();
            var _bolProcessList = new Dictionary<BolStatus, List<Guid>>();
            #endregion
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            if (serviceItem == null)
            {
                var ticketInfo = this.ticketManager.GetTicketModel(new { UID = ticketUID });
                serviceItem = (TicketType)ticketInfo.Content.Type.Value;
            }
            IEnumerable<IStatusCheckModel> _status = null;
            if (statusCheckModels != null)
            {
                _status = statusCheckModels;
            }
            else
            {
                _status = this.ticketManager.GetManifestStatusCollection(ticketUID).Content;
            }
            if (_status != null)
            {
                var _checkTicket = _status.GroupBy(p => p.TicketUID);
                #region check Ticket info status -> ticket status
                foreach (var item in _checkTicket)
                {
                    var _ticketStatus = TicketStatus.Draft;
                    var _bulkPickStatus = BulkPickStatus.Open;
                    //if (item.Any(p => p.TicketInfoStatus != (int)TicketInfoStatus.Draft)
                    //    && item.First().TicketStatus != (int)TicketStatus.Processing)
                    //{
                    //    _ticketStatus = TicketStatus.Processing;
                    //}
                    if (item.Any(p => p.TicketInfoStatus > (int)TicketInfoStatus.Open)
                        && item.First().TicketStatus != (int)TicketStatus.Processing
                        && !(new int[] { (int)TicketStatus.Glitch, (int)TicketStatus.Complete })
                        .Any(x => x == item.First().TicketStatus))
                    {
                        _ticketStatus = TicketStatus.Processing;
                    }
                    if (item.Any(p => p.TicketInfoStatus == (int)TicketInfoStatus.Glitch)
                     && item.First().TicketStatus != (int)TicketStatus.Glitch &&
                     item.All(p => p.TicketInfoStatus > (int)TicketInfoStatus.Processing))
                    {
                        _ticketStatus = TicketStatus.Glitch;
                    }
                    else if (item.Any(p => p.TicketInfoStatus == (int)TicketInfoStatus.OffPosition)
                      && item.First().TicketStatus != (int)TicketStatus.Processing)
                    {
                        _ticketStatus = TicketStatus.Processing;
                        // _action.Push(() => this.bulkPickManager
                        //.ChangeBulkPickStatus(new Guid[] { item.Key }, (int)BulkPickStatus.Processing));
                        // _bulkPickStatus = BulkPickStatus.Processing;
                    }
                    else if (item.All(p => p.TicketInfoStatus == (int)TicketInfoStatus.Complete)
                      && item.First().TicketStatus != (int)TicketStatus.Complete)
                    {
                        _ticketStatus = TicketStatus.Complete;

                        //_action.Push(() => this.bulkPickManager
                        //.ChangeBulkPickStatus(new Guid[] { item.Key }, (int)BulkPickStatus.Complete));
                        //_bulkPickStatus = BulkPickStatus.Complete;
                    }
                    if (_ticketStatus != TicketStatus.Draft)
                    {
                        Parallel.ForEach(item, p => p.TicketStatus = (int)_ticketStatus);
                        //_action.Push(() => this.ticketManager.ChangeTicketStatus(item.Key, _ticketStatus));
                        if (_ticketProcessList.ContainsKey(_ticketStatus))
                        {
                            _ticketProcessList[_ticketStatus].Add(item.Key);
                        }
                        else
                        {
                            List<Guid> keylist = new List<Guid>();
                            keylist.Add(item.Key);
                            _ticketProcessList.Add(_ticketStatus, keylist);
                        }
                    }
                    if (_bulkPickStatus != BulkPickStatus.Open)
                    {
                        if (_bulkPickProcessList.ContainsKey(_bulkPickStatus))
                        {
                            _bulkPickProcessList[_bulkPickStatus].Add(item.Key);
                        }
                        else
                        {
                            List<Guid> keylist = new List<Guid>();
                            keylist.Add(item.Key);
                            _bulkPickProcessList.Add(_bulkPickStatus, keylist);
                        }
                    }
                }
                #endregion
                var _checkWorkorderPayload = _status.GroupBy(p => p.WorkOrderPayloadUID);

                #region check ticket status -> workorder payload status
                foreach (var item in _checkWorkorderPayload)
                {
                    var _wstatus = WorkOrderPayloadStatus.Inactive;
                    //if (item.Any(p => p.TicketStatus != (int)TicketStatus.Draft)
                    //    && item.First().WorkOrderPayloadStatus != (int)WorkOrderPayloadStatus.Processing)
                    //{
                    //    _wstatus = WorkOrderPayloadStatus.Processing;
                    //}
                    if (item.Any(p => p.TicketStatus > (int)TicketStatus.Open)
                       && !item.All(p => new int[] { (int)TicketStatus.Complete,
                           (int)TicketStatus.Glitch }.Contains((int)p.TicketStatus))//判斷TicketStatus狀態是否全都都完成，反之則Processing

                       )
                    {
                        _wstatus = WorkOrderPayloadStatus.Processing;
                    }
                    if (item.Any(p => p.TicketStatus == (int)TicketStatus.Glitch)
                     && item.First().WorkOrderPayloadStatus != (int)WorkOrderPayloadStatus.Active)
                    {
                        _wstatus = WorkOrderPayloadStatus.Active;
                    }
                    else if (item.All(p => p.TicketStatus == (int)TicketStatus.Complete)
                      && item.First().WorkOrderPayloadStatus != (int)WorkOrderPayloadStatus.Active)
                    {
                        _wstatus = WorkOrderPayloadStatus.Active;
                    }
                    if (_wstatus != WorkOrderPayloadStatus.Inactive)
                    {
                        Parallel.ForEach(item, p => p.WorkOrderPayloadStatus = (int)_wstatus);
                        // _action.Push(() => this.statusManageAgent.WorkOrder.ChangeWorkOrderPayloadStatus(item.Key, _wstatus));
                        if (_wpayloadProcessList.ContainsKey(_wstatus))
                        {
                            _wpayloadProcessList[_wstatus].Add(item.Key);
                        }
                        else
                        {
                            List<Guid> keylist = new List<Guid>();
                            keylist.Add(item.Key);
                            _wpayloadProcessList.Add(_wstatus, keylist);
                        }
                    }
                }
                #endregion
                var _checkWorkorderPod = _status.GroupBy(p => p.WorkOrderPodUID);

                #region check workorder payload status -> workorder pod status
                //TODO　workorder pod change status issue
                foreach (var item in _checkWorkorderPod)
                {
                    var _Status = WorkOrderPodStatus.Draft;
                    if (item.Any(p => p.WorkOrderPayloadStatus == (int)WorkOrderPayloadStatus.Processing)
                        && (item.First().WorkOrderPodStatus != (int)WorkOrderPodStatus.Loading ||
                            item.First().WorkOrderPodStatus != (int)WorkOrderPodStatus.OffPosition))
                    {
                        if (serviceItem == TicketType.Move)
                            _Status = WorkOrderPodStatus.OffPosition;
                        else
                            _Status = WorkOrderPodStatus.Loading;
                    }

                    else if (item.All(p => p.WorkOrderPayloadStatus == (int)WorkOrderPayloadStatus.Active)
                      && item.First().WorkOrderPodStatus != (int)WorkOrderPodStatus.Complete)
                    {
                        _Status = WorkOrderPodStatus.Complete;
                    }
                    if (_Status != WorkOrderPodStatus.Draft)
                    {
                        Parallel.ForEach(item, p => p.WorkOrderPodStatus = (int)_Status);
                        //_action.Push(() => this.statusManageAgent.WorkOrder.ChangeWorkOrderPodStatus(item.Key, _Status));
                        if (_wpodProcessList.ContainsKey(_Status))
                        {
                            _wpodProcessList[_Status].Add(item.Key);
                        }
                        else
                        {
                            List<Guid> keylist = new List<Guid>();
                            keylist.Add(item.Key);
                            _wpodProcessList.Add(_Status, keylist);
                        }
                    }
                }
                #endregion
                var _checkWorkorder = _status.GroupBy(p => p.WorkOrderUID);

                #region check workorder pod status -> workorder  status
                foreach (var item in _checkWorkorder)
                {
                    var _notInStatus = new int[] {
                        (int)WorkOrderStatus.Receiving, (int)WorkOrderStatus.Shipping ,(int)WorkOrderStatus.OnTheWay};
                    var _Status = WorkOrderStatus.Draft;
                    if ((item.Any(p => p.WorkOrderPodStatus == (int)WorkOrderPodStatus.Loading)
                        && !_notInStatus.Contains(item.First().WorkOrderStatus)) ||
                        (item.Any(p => p.WorkOrderPodStatus == (int)WorkOrderPodStatus.OffPosition)
                        && !_notInStatus.Contains(item.First().WorkOrderStatus)))
                    {
                        if (serviceItem == TicketType.Receiving)
                            _Status = WorkOrderStatus.Receiving;
                        else if (serviceItem == TicketType.Outbound)
                            _Status = WorkOrderStatus.Shipping;
                        //else
                        //    _Status = WorkOrderStatus.OnTheWay;
                    }

                    else if (item.All(p => p.WorkOrderPodStatus == (int)WorkOrderPodStatus.Complete)
                      && item.First().WorkOrderStatus != (int)WorkOrderStatus.Complete)
                    {
                        _Status = WorkOrderStatus.Complete;
                    }
                    if (_Status != WorkOrderStatus.Draft)
                    {
                        Parallel.ForEach(item, p => p.WorkOrderStatus = (int)_Status);
                        //_action.Push(() => this.statusManageAgent.WorkOrder.ChangeWorkOrderStatus(item.Key, _Status));
                        if (_workorderProcessList.ContainsKey(_Status))
                        {
                            _workorderProcessList[_Status].Add(item.Key);
                        }
                        else
                        {
                            List<Guid> keylist = new List<Guid>();
                            keylist.Add(item.Key);
                            _workorderProcessList.Add(_Status, keylist);
                        }

                    }
                }
                #endregion
                var _checkVessel = _status.GroupBy(p => p.VesselUID);

                #region check workorder status -> vessel status
                foreach (var item in _checkVessel)
                {
                    var _Status = VesselStatus.Draft;
                    var _iStatus = VesselManifestStatus.Draft;
                    if ((item.Any(p => p.WorkOrderStatus == (int)WorkOrderStatus.Receiving)
                        && item.First().VesselStatus != (int)VesselStatus.Receiving))
                    {

                        _Status = VesselStatus.Receiving;
                        _iStatus = VesselManifestStatus.Receiving;
                    }
                    else if ((item.Any(p => p.WorkOrderStatus == (int)WorkOrderStatus.Shipping)
                        && item.First().VesselStatus != (int)VesselStatus.Shipping))
                    {
                        _Status = VesselStatus.Shipping;
                        _iStatus = VesselManifestStatus.Shipping;
                    }
                    //else if ((item.Any(p => p.WorkOrderStatus == (int)WorkOrderStatus.OnTheWay)
                    //    && item.First().VesselStatus != (int)VesselStatus.OnTheWay))
                    //{
                    //    _Status = VesselStatus.Processing;
                    //    _iStatus = VesselManifestStatus.Processing;
                    //}
                    else if (item.All(p => p.WorkOrderStatus == (int)WorkOrderStatus.Complete)
                      && item.First().VesselStatus != (int)VesselStatus.Complete)
                    {
                        _Status = VesselStatus.Complete;
                        _iStatus = VesselManifestStatus.Complete;
                    }
                    if (_Status != VesselStatus.Draft)
                    {
                        if (item.Key != Guid.Empty)
                        {
                            Parallel.ForEach(item, p => p.VesselStatus = (int)_Status);
                            //_action.Push(() => this.statusManageAgent.Vessel.ChangeVesselStatus(item.Key, _Status, _iStatus));
                            if (_vesselProcessList.ContainsKey(_Status))
                            {
                                _vesselProcessList[_Status].Add(item.Key);
                            }
                            else
                            {
                                List<Guid> keylist = new List<Guid>();
                                keylist.Add(item.Key);
                                _vesselProcessList.Add(_Status, keylist);
                            }
                            if (_vesselManifestProcessList.ContainsKey(_iStatus))
                            {
                                _vesselManifestProcessList[_iStatus].Add(item.Key);
                            }
                            else
                            {
                                List<Guid> keylist = new List<Guid>();
                                keylist.Add(item.Key);
                                _vesselManifestProcessList.Add(_iStatus, keylist);
                            }
                        }
                    }
                }
                #endregion
                var _checkBol = _status.GroupBy(p => p.BolUID);
                #region check vessel status -> bol status
                foreach (var item in _checkBol)
                {
                    var _Status = BolStatus.Draft;
                    if ((item.Any(p => p.VesselStatus == (int)VesselStatus.Receiving)
                        && item.First().BolStatus != (int)BolStatus.Receiving))
                    {
                        _Status = BolStatus.Receiving;
                    }
                    else if ((item.Any(p => p.VesselStatus == (int)VesselStatus.Shipping)
                        && item.First().BolStatus != (int)BolStatus.Shipping))
                    {
                        _Status = BolStatus.Shipping;
                    }
                    //else if ((item.Any(p => p.VesselStatus == (int)VesselStatus.Processing)
                    //    && item.First().BolStatus != (int)BolStatus.OntheWay))
                    //{
                    //    _Status = BolStatus.OntheWay;
                    //}
                    else if (item.All(p => p.VesselStatus == (int)VesselStatus.Complete)
                      && item.First().BolStatus != (int)BolStatus.Complete)
                    {
                        _Status = BolStatus.Complete;
                    }
                    if (_Status != BolStatus.Draft)
                    {
                        if (item.Key != Guid.Empty)
                        {
                            Parallel.ForEach(item, p => p.BolStatus = (int)_Status);
                            //_action.Push(() => this.statusManageAgent.Bol.ChangeBolStatus(item.Key, _Status));
                            if (_bolProcessList.ContainsKey(_Status))
                            {
                                _bolProcessList[_Status].Add(item.Key);
                            }
                            else
                            {
                                List<Guid> keylist = new List<Guid>();
                                keylist.Add(item.Key);
                                _bolProcessList.Add(_Status, keylist);
                            }
                        }
                    }
                }
                #endregion
                var _checkManifest = _status.GroupBy(p => p.ManifestUID);
                #region check bol status -> manifest status
                foreach (var item in _checkManifest)
                {
                    var _Status = ManifestStatus.Draft;

                    //從Manifest Item UID回推Vesselmanifestitem 是否有分配完成
                    IActionResult<IEnumerable<ICheckManifestItemStatusResultModel>> checkVesselItemRs =
                        this.manifestManger.GetCheckManifestItemStatusResult(item.Key);
                    var groupbyManifestItem = checkVesselItemRs.Content
                        .GroupBy(g => new
                        {
                            ManifestItemUID = g.ManifestItemUID,
                            Qty = g.OriginalQty,
                            PackageUID = g.OriginalPackageUID
                        });
                    List<Guid> mguid = new List<Guid>();
                    var _iStatus = ManifestItemListStatus.Complete;
                    foreach (var grp in groupbyManifestItem)
                    {

                        //var pkgtree = this._packageManager.GetPackageTree(grp.Key.PackageUID).Content;
                        //var minipkg = pkgtree.GetMinPackage();
                        var minipkg = this.packageManagerCache.GetMinPackage(grp.Key.PackageUID);
                        var minipkgOrignalQty = this.packageManager
                            .GetReceivePackageUomQuantity(grp.Key.PackageUID, minipkg.UID, grp.Key.Qty).Content;
                        //VesselStatus 與VesselManifest 連聯，故只判斷VesselStatus 是否complete
                        var allocatedQty = grp.Where(g =>
                        _status.FirstOrDefault(t => t.VesselUID == g.VesselUID).VesselStatus == (int)VesselStatus.Complete)
                            .Sum(p => this.packageManager
                            .GetReceivePackageUomQuantity(p.CompletePackageUID, minipkg.UID, p.CompleteQty).Content);
                        if (minipkgOrignalQty == allocatedQty)//如果Manifest item Qty與Vessel item qty 等於，則manifest item status= complete
                        {
                            if (item.Key != Guid.Empty)
                            {
                                _iStatus = ManifestItemListStatus.Complete;
                                grp.ToList().ForEach(p => p.ManifestItemStatus = (int)_iStatus);
                                mguid.Add(grp.Key.ManifestItemUID);
                            }
                        }
                    }
                    _action.Push(() => this.statusManageAgent.Manifest.BatchChangeManifestItemStatus(mguid, _iStatus));
                    //全部manifest item status =complete manifest status 才能Complete
                    if (checkVesselItemRs.Content.All(p => p.ManifestItemStatus == (int)ManifestItemListStatus.Complete))
                    {
                        if (item.Key != Guid.Empty)
                        {
                            _Status = ManifestStatus.Complete;
                            var _imStatus = ManifestItemListStatus.Complete;
                            _action.Push(() => this.statusManageAgent.Manifest.ChangeManifestStatus(item.Key, _Status));
                        }
                    }

                }
                #endregion

                //將收集的table key轉成修改狀態的action
                #region convert to Action
                #region Ticket
                foreach (var item in _ticketProcessList)
                {
                    _action.Push(() => this.ticketRepository.UpdateTicketStatus(item.Value, item.Key,
                                    this.AuthenticationProvider.GetAuthenticationInfo().Account));
                }
                #endregion
                #region Workorder Payload
                foreach (var item in _wpayloadProcessList)
                {
                    _action.Push(() => this.statusManageAgent.WorkOrder.ChangeWorkOrderPayloadStatus(item.Value, item.Key,
                                    this.AuthenticationProvider.GetAuthenticationInfo().Account));
                }
                #endregion
                #region Workorder Pod
                foreach (var item in _wpodProcessList)
                {
                    _action.Push(() => this.statusManageAgent.WorkOrder.ChangeWorkOrderPodStatus(item.Value, item.Key,
                                    this.AuthenticationProvider.GetAuthenticationInfo().Account));
                }
                #endregion
                #region BulkPick
                foreach (var item in _bulkPickProcessList)
                {
                    _action.Push(() => this.bulkPickManager.ChangeBulkPickStatus(item.Value, (int)item.Key));
                }
                #endregion
                #region Workorder 
                foreach (var item in _workorderProcessList)
                {
                    _action.Push(() => this.statusManageAgent.WorkOrder.ChangeWorkOrderStatus(item.Value, item.Key,
                                    this.AuthenticationProvider.GetAuthenticationInfo().Account));
                }
                #endregion
                #region Vessel
                foreach (var item in _vesselProcessList)
                {
                    _action.Push(() => this.statusManageAgent.Vessel.BatchChangeVesselStatus(item.Value, item.Key,
                                    this.AuthenticationProvider.GetAuthenticationInfo().Account));
                }
                foreach (var item in _vesselManifestProcessList)
                {
                    _action.Push(() => this.statusManageAgent.Vessel.BatchChangeVesselManiestStatus(item.Value, item.Key,
                                    this.AuthenticationProvider.GetAuthenticationInfo().Account));
                }
                #endregion
                #region BOL
                foreach (var item in _bolProcessList)
                {
                    _action.Push(() => this.statusManageAgent.Bol.ChangeBolStatus(item.Value, item.Key,
                                    this.AuthenticationProvider.GetAuthenticationInfo().Account));
                }
                #endregion
                #endregion
            }
            return _action;
        }
        protected TransactionOptions GetTransactionScopeOption(IsolationLevel? isolationLevel = null, int? timeout = null)
        {
            var _option = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.Snapshot,
                Timeout = new TimeSpan(0, 5, 0)
            };
            if (isolationLevel != null)
            {
                _option.IsolationLevel = isolationLevel.Value;
            }
            if (timeout != null)
            {
                _option.Timeout = new TimeSpan(0, timeout.Value, 0);
            }
            return _option;
        }
        protected IActionResult<bool> GetResult(bool Result, string message)
        {

            var rs = ActionResultTemplates.Result<bool>();
            rs.Content =
            rs.Success = Result;
            rs.Message = message;
            return rs;
        }
        //protected TransactionScope GetNewTransactionScope(TransactionScopeOption option = TransactionScopeOption.Required)
        //{
        //    return this.transacationScope.GetNewTransactionScope(option);
        //}
        //protected TransactionScope GetNewTransactionScope(IsolationLevel isolationLevel,
        //    TransactionScopeOption scope = TransactionScopeOption.Required)
        //{
        //    return this.transacationScope.GetNewTransactionScope(isolationLevel, scope);
        //}

        //protected TransactionScope GetTransactionScope(TransactionScopeOption option = TransactionScopeOption.Required)
        //{
        //    return this.transacationScope.GetDefaultTransactionScope(option);
        //}
        //protected TransactionScope GetTransactionScope(int timeout, TransactionScopeOption option = TransactionScopeOption.Required)
        //{
        //    return this.transacationScope.GetDefaultTransactionScope(timeout, option);
        //}
        protected IPayloadTransactionLogModel GetTxLog(ITicketProcessModel model)
        {
            var _log = new PayloadTransactionLogInnerModel();
            _log.UID = Guid.NewGuid();
            _log.ItemUID = model.ItemUID;
            _log.WarehouseUID = model.WarehouseUID;
            _log.OriginalPackage = model.OriginalPackage;
            _log.OriginalSlotUID = model.OriginalSlotUID;
            _log.TargetPackage = model.TargetPackage.Value;
            _log.TargetSlotUID = model.TargetSlotUID;
            _log.PayloadUID = model.PayloadUID;
            _log.TicketInfoUID = model.UID;
            // _log.QtyBeforeTX = 0;
            //_log.QtyAfterTX = model.ActQty;
            _log.Status = (int)PayloadTransactionLogStatus.Active;
            // _log.Type = (int)PayloadTxlogType.RECEIVING;
            //TODO txlog 需修改
            if (model.WorkOrderPayloadUID.HasValue)
                _log.WorkOrderPayloadUID = model.WorkOrderPayloadUID.Value;
            if (model.WorkOrderPodUID.HasValue)
                _log.WorkOrderPodUID = model.WorkOrderPodUID.Value;
            return _log;
        }
        protected void LogScanbarcode(IEnumerable<ITicketInfoParameter> data, string actionName = "")
        {
            var authInfo = this.AuthenticationProvider.GetAuthenticationInfo();
            foreach (var item in data)
            {
                foreach (var barcode in item.Barcode)
                {
                    this.WriteLog($"TicketInfo#{item.TicketInfoUID} scan barcode {barcode.Barcode} scanned {barcode.ScanQty} times",
                        actionName, authInfo.Account, logger.InfoString, (int)YAEP.Constants.BelongToTypes.Mobile,
                        item.TicketInfoUID.ToString());
                }
            }
        }
        protected void WriteLog(string message, string type, string owner, string level, int belongToType, string belongToUID)
        {
            this.logger.Log(message, type, owner, level, belongToType, belongToUID);
        }
    }
}
