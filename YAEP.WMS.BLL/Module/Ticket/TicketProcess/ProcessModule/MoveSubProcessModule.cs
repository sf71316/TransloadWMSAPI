using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using System.Collections.Concurrent;
using YAEP.WMS.Language.Resources;
using System.Transactions;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.NotificationReceiver.Common;
using Newtonsoft.Json;

namespace YAEP.WMS.BLL.Module
{
    internal class MoveSubProcessModule : AbstractProcessModule
    {
        public MoveSubProcessModule(
            ITicketProcessAgentParameter parameters, ILogInfiltrator logInfiltrator)
            : base(parameters, logInfiltrator)
        {

        }

        public override IActionResult<bool> Execute(IEnumerable<IUploadTicketDataParameter> Data,
            NotifySenderConfig sendInfo = null)
        {
            this.sendInfo = sendInfo;
            this.UploadData = Data.Select(p => p.Item);

            if (Data.All(x => x.TicketInfoCommand == MoveTicketCommand.OffPosition))
            {
                return this.OffPositionProcess();
            }
            else if (Data.All(x => x.TicketInfoCommand == MoveTicketCommand.Undo))
            {
                var rs = ActionResultTemplates.OK();
                rs.Success = false;
                rs.Message = Resource.TICKET_MOVE_NOTSUPPORT;
                return rs;
                //return this.UndoProcess();
            }
            else if (Data.All(x => x.TicketInfoCommand == MoveTicketCommand.OnPosition) || Data.All(p => p.Item.IsAllPass))
            {
                return this.OnPositionProcess();
            }
            else
            {
                var rs = ActionResultTemplates.Result<bool>();
                rs.Success = rs.Content;
                rs.Message = Resource.TICKET_UNDEFINED_COMMAND;
                return rs;
            }
        }

        private IActionResult<bool> OnPositionProcess()
        {
            this.tracingAgent.TransactionInfo.Action = TransactionlogAction.Move;
            List<Func<IActionResult<bool>>> actions = new List<Func<IActionResult<bool>>>();
            List<IActionResult<bool>> _result = new List<IActionResult<bool>>();

            TicketInfoStatus _ticketInfoStatus = TicketInfoStatus.Draft;
            // 1-1 get ticket data
            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            var _ticketInfos = this.ticketManager.GetTicketProcessModel(this.UploadData.Select(p => p.TicketInfoUID)?.ToArray());
            if (_ticketInfos.Content.All(p => p.Status == (int)TicketInfoStatus.OffPosition) ||
                this.UploadData.All(p => p.IsAllPass))
            {
                var _payloads = this.inventoryManager.GetPayloadList(new { UID = _ticketInfos.Content.Select(p => p.PayloadUID) });
                // 1-1-2 log mobile upload scan barcode
                LogScanbarcode(this.UploadData, (ManifestType)_ticketInfos.Content.First().ManifestType + " Move " +
                MoveTicketCommand.OnPosition.ToString());
                //TransactionScope scope = null;
                //if (!this.IsExistTransaction)
                //    scope = this.GetTransactionScope();
                foreach (var item in _ticketInfos.Content)
                {
                    var converter = AbstractTicketConverter.GetInstance(item.ManifestType, item.Type);
                    converter.Convert(item);
                    List<ILabelModel> uploadlabelModels = new List<ILabelModel>();
                    //find upload data
                    var _uploaddata = this.UploadData.FirstOrDefault(p => p.TicketInfoUID == item.UID);
                    if (_uploaddata != null)
                    {
                        #region process label data


                        if (_uploaddata.IsAllPass)
                        {
                            uploadlabelModels.AddRange(item.Barcodes.Where(p => p.Status == (int)LabelStatus.Active));
                            item.ActQty = item.EstQty;
                        }
                        else
                        {
                            if (_uploaddata.Barcode != null && _uploaddata.Barcode.Count() > 0)//check barcode 
                            {
                                uploadlabelModels.AddRange(item.Barcodes
                                    .Where(p => _uploaddata.Barcode.Select(x => x.Barcode).Contains(p.Content)));
                                //上架不用處理ActQty , 下架已經處理過
                                //var _operationQty = (_uploaddata.ScanType == ScanType.NoNeedToScan || _uploaddata.ScanType == ScanType.NoUnique) ?
                                //                    _uploaddata.ActQty.Value : uploadlabelModels.Count;
                                //item.ActQty = _operationQty;
                            }
                            if (!item.TargetSlotUID.HasValue || !item.OriginalSlotUID.HasValue)
                            {
                                rs.Success = false;
                                rs.Message = Resource.TICKET_LOST_SLOT_INFO;
                            }
                            if (!item.TargetPackage.HasValue || !item.OriginalPackage.HasValue)
                            {
                                rs.Success = false;
                                rs.Message = Resource.TICKET_LOST_PACKAGE_INFO;
                            }
                        }
                        #endregion
                        // check upload qty move to offposition command
                        if (rs.Success)
                        {
                            #region Process
                            _ticketInfoStatus = TicketInfoStatus.Complete;
                            item.Status = (int)TicketInfoStatus.Complete;

                            try
                            {
                                #region process ticketinfo/label/payload data
                                // 1-1 update ticketinfo 
                                item.ModifiedBy = this.AuthenticationProvider.GetAuthenticationInfo().MemberName;
                                item.ModifiedOn = DateTime.UtcNow;
                                actions.Add(() => this.ticketManager.UpdateTicketInfo(item));

                                //暫不處理0722
                                // _result.Add(this.inventoryManager.ChangePodStauts(item.PodUID, PodStatus.Open));
                                if (uploadlabelModels.Count > 0)
                                {
                                    actions.Add(() => this.labelRepository.ChangeLabelStatus(uploadlabelModels.Select(x => x.UID).ToArray(), LabelStatus.Active));
                                }
                                //1-1 modifed payload data
                                var _payload = _payloads.Content.FirstOrDefault(p => p.UID == item.PayloadUID);
                                if (_payload != null)
                                {
                                    //1-2  payload status
                                    _payload.Status = (int)PayloadStatus.Active;
                                    _payload.SlotUID = item.TargetSlotUID.Value;
                                    if (_payload != null &&
                                      new int[] { (int)ManifestType.Inbound, (int)ManifestType.Move }
                                       .Any(x => x == item.ManifestType))
                                    {
                                        _payload.Type = (int)PayloadType.Stock;
                                        _payload.Status = (int)PayloadStatus.Active;
                                    }
                                    actions.Add(() => this.inventoryManager.UpdatePayload(_payload));

                                }
                                #endregion
                                #region 3-1 change inventory onhand location
                                var moveQty = 0;
                                //從TicketInfo 關聯方式決定要用什麼包裝當onhand 
                                //1:Pod(Pallet) ->對應到payload 包裝的數量
                                //2:Payload ->操作數量
                                if (item.MappingType == 2)
                                    moveQty = item.ActQty;
                                else
                                    moveQty = item.PayloadQty;
                                //如果來源和目的Slot不同才需要移動
                                if (item.TargetSlotUID.Value != item.OriginalSlotUID.Value)
                                {

                                    InsertInventoryParameter iparam = new InsertInventoryParameter();
                                    iparam.ItemUID = item.ItemUID;
                                    iparam.Qty = moveQty * -1;
                                    iparam.SlotUID = item.OriginalSlotUID.Value;
                                    iparam.TargetPackageUID = item.OriginalPackage.Value;
                                    if (item.OriginalPayloadType.HasValue)
                                    {
                                        iparam.Type = (InventoryType)item.OriginalPayloadType;
                                    }
                                    else
                                    {
                                        iparam.Type = InventoryType.Stock;
                                    }
                                    iparam.WarehouseUID = item.WarehouseUID;
                                    iparam.UseMiniPackage = true;
                                    actions.Add(() => this.inventoryManager
                                    .InsertInventory(new InsertInventoryParameter[] { iparam }));

                                    InsertInventoryParameter iparam2 = new InsertInventoryParameter();
                                    iparam2.ItemUID = item.ItemUID;
                                    iparam2.Qty = moveQty;
                                    iparam2.SlotUID = item.TargetSlotUID.Value;
                                    iparam2.TargetPackageUID = item.TargetPackage.Value;
                                    if (item.OriginalPayloadType.HasValue)
                                    {
                                        iparam2.Type = (InventoryType)item.OriginalPayloadType;
                                    }
                                    else
                                    {
                                        iparam2.Type = InventoryType.Stock;
                                    }

                                    iparam2.WarehouseUID = item.WarehouseUID;
                                    iparam2.UseMiniPackage = true;
                                    actions.Add(() => this.inventoryManager
                                    .InsertInventory(new InsertInventoryParameter[] { iparam2 }));


                                }
                                #endregion
                                #region 4-1 Transcation log
                                var _log = this.GetTxLog(item);
                                _log.QtyBeforeTX = 0;
                                _log.QtyAfterTX = 0;
                                _log.Type = (int)this.tracingAgent.GetTransactionLogType();
                                actions.Add(() => this.inventoryManager.AddLog(_log));
                                #endregion



                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.Message + " " + ex.StackTrace, "", "", "error", (int)YAEP.Constants.BelongToTypes.Mobile, item.UID.ToString());
                                rs.Message = ex.Message + " " + ex.StackTrace;
                                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                                rs.Success = false;
                                rs.InnerException = ex;
                            }

                            #endregion
                        }
                    }
                    rs.Success = true;
                }
                if (rs.Success)
                {
                    
                    var option = GetTransactionScopeOption(timeout: 30 * 60);
                    //2022/3/2  使用快照隔離會造成同一Pod 處理時在修改Workorder pod 狀態會衝突
                    option.IsolationLevel = IsolationLevel.ReadCommitted;
                    using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                    {
                        try
                        {
                            //6-1execute sql
                            foreach (var item in actions)
                            {
                                _result.Add(item.Invoke());
                            }
                            //6-2 check manifest status
                            var statusModels = this.ticketManager
                                .GetBatchManifestStatusCollection(_ticketInfos.Content.Select(p => p.TicketUID));
                            var chkaction = this.CheckDataStatus(_ticketInfos.Content.First().TicketUID,
                                 (TicketType)_ticketInfos.Content.First().Type, statusModels.Content);
                            foreach (var item in chkaction)
                            {
                                _result.Add(item.Invoke());
                            }
                            if (_result.All(p => p.Success) &&
                                     Transaction.Current?.TransactionInformation.Status != TransactionStatus.Aborted)
                                scope.Complete();
                            else
                                this.tracingAgent.Trace("execute sql transaction abort");
                        }
                        catch (Exception ex)
                        {
                            rs.Success = false;
                            this.tracingAgent.Trace("execute sql error", ex);
                        }
                    }
                }
                if (_result.All(p => p.Success) && rs.Success)
                {
                    #region 5-1 sync data to subscriber
                    var repliatemovedata = _ticketInfos.Content.Select(p =>
                    {
                        var minpkg = this.packageManagerCache.GetMinPackage(p.TargetPackage.Value);
                        var syncqty = 0;
                        var payloadtype = 0;
                        //inbound 都為pallet 移動，故需取得payload實際數量
                        if ((ManifestType)p.ManifestType == ManifestType.Inbound)
                        {
                            syncqty = this.packageManagerCache
                            .GetReceivePackageUomQuantity(p.TargetPackage.Value, minpkg.UID, p.PayloadQty).Content;
                            payloadtype = p.PayloadType;
                        }
                        else
                        {
                            syncqty = this.packageManagerCache
                            .GetReceivePackageUomQuantity(p.TargetPackage.Value, minpkg.UID, p.ActQty).Content;
                            payloadtype = p.OriginalPayloadType.Value;
                        }
                        var e = new WMSReplicateMoveModel
                        {
                            ItemUID = p.ItemUID,
                            TicketUID = p.TicketUID,
                            OriginalSlotUID = p.OriginalSlotUID.Value,
                            Quantity = syncqty,
                            ManifestType = p.ManifestType,
                            TargetSlotUID = p.TargetSlotUID.Value,
                            ItemGroup = p.ItemGroupUID,
                            PayloadUID = p.PayloadUID,
                            PayloadType = payloadtype
                        };

                        return e;
                    });
                    this.CompleteUnexecutedMethod.Enqueue(() => replicationManager.Move(repliatemovedata));
                    #endregion
                    #region 5-2 資料異動通知外部系統(Shipping)
                    List<NotificationSendTaskModel> tasks = new List<NotificationSendTaskModel>();
                    foreach (var item in _ticketInfos.Content)
                    {
                        if (this.sendInfo != null)
                        {
                            var request = new OutboundTicketInfoProcessingRequest();
                            var processItem = new NotificationProcessInfo();
                            var task = new NotificationSendTaskModel();
                            task.UID = Guid.NewGuid();
                            task.EventName = EventHelper.OUTBOUND_MOVE_TICKET_INFO_COMPLETED;
                            task.TicketInfoUID = item.UID;
                            task.ReceiverSecret = this.sendInfo.ReceiverSecret;
                            task.ReceiverUrl = this.sendInfo.ReceiverUrl;
                            task.RefNo = item.RefNo;
                            task.RetryCount = 0;
                            task.Status = (int)SenderTaskStatus.InQueue;
                            processItem.PickQty = item.ActQty;

                            processItem.ProcessItemUID = item.WorkOrderPayloadUID.Value;
                            request.ProcessItems.Add(processItem);
                            request.RefNo = item.RefNo;
                            request.Sender = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                            task.Message = JsonConvert.SerializeObject(request);
                            //this.CompleteUnexecutedMethod.Enqueue(() => this.notificationSenderTaskRepository.Add(task));
                            tasks.Add(task);
                        }
                        else
                        {
                            if (item.ManifestType == (int)ManifestType.BlukPick)
                            {
                                //TODO: BulkPick move notification not finish (缺貨部分)
                                var bulkPickOrignalInfos = this.bulkPickManager
                                         .GetBulkPickOriginalNotificationInfo(new Guid[] { item.UID });
                                foreach (var bitem in bulkPickOrignalInfos.Content)
                                {
                                    var request = new OutboundTicketInfoProcessingRequest();
                                    var processItem = new NotificationProcessInfo();
                                    var task = new NotificationSendTaskModel();
                                    task.UID = Guid.NewGuid();
                                    task.EventName = EventHelper.OUTBOUND_MOVE_TICKET_INFO_COMPLETED;
                                    task.TicketInfoUID = item.UID;
                                    task.ReceiverSecret = bitem.ReceiverSecret;
                                    task.ReceiverUrl = bitem.ReceiverUrl;
                                    task.RefNo = bitem.RefNo;
                                    task.RetryCount = 0;
                                    task.Status = (int)SenderTaskStatus.InQueue;
                                    processItem.PickQty = bitem.ActQty;

                                    processItem.ProcessItemUID = bitem.WorkOrderPayloadUID;
                                    request.ProcessItems.Add(processItem);
                                    request.RefNo = item.RefNo;
                                    request.Sender = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                                    task.Message = JsonConvert.SerializeObject(request);
                                    tasks.Add(task);
                                    //this.CompleteUnexecutedMethod.Enqueue(() => this.notificationSenderTaskRepository.Add(task));
                                }
                            }
                        }
                    }
                    if (tasks.Count > 0)
                    {
                        this.CompleteUnexecutedMethod.Enqueue(() => this.notificationSenderTaskRepository.BatchAdd(tasks));
                    }
                    #endregion
                    if (_ticketInfos.Content.FirstOrDefault().ManifestType.Equals((int)ManifestType.Inbound))
                    {
                        this.CompleteUnexecutedMethod.Enqueue(() => manifestManger.CheckReplenishmentSync(_ticketInfos.Content));
                    }
                }
                if (_result.All(p => p.Success) && rs.Success)
                {
                    this.tracingAgent.Trace($"Move ticket all complete transactionscope status: {Transaction.Current?.TransactionInformation.Status.ToString()}");
                    //this.transacationScope.Complete(scope);
                    rs.Content = rs.Success = true;
                }
                else
                {
                    rs.Content = rs.Success = false;
                    rs.Message += string.Join("\n\n", _result.Select(p => p.Message));
                }
                //if (scope != null)
                //{
                //    scope.Dispose();
                //}
            }
            else
            {
                rs.Success =
                rs.Content = false;
                rs.Message = Resource.TICKET_MOVE_SEQMENT_INCORRECT;
            }

            return rs;
        }

        private IActionResult<bool> OffPositionProcess()
        {
            List<Func<IActionResult<bool>>> actions = new List<Func<IActionResult<bool>>>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                IActionResult<IEnumerable<ILabelModel>> _receivingQtyBarcodes = null;
                var scanBarcode = this.UploadData.SelectMany(x => x.Barcode.Select(y => y.Barcode));
                rs.Success = true;
                List<IActionResult<bool>> _result = new List<IActionResult<bool>>();
                var _ticketInfos = this.ticketManager.GetTicketProcessModel(this.UploadData
                    .Select(p => p.TicketInfoUID)?.ToArray());
                //上架時取得掃瞄過的Barcode 是否是Receiving qty barcode  
                if (_ticketInfos.Content.All(p => p.ManifestType == (int)ManifestType.Outbound))
                {

                    _receivingQtyBarcodes = this.labelRepository.GetLabels(new
                    {
                        Content = scanBarcode,
                        Type = (int)LabelType.Pallet_OrginalTracking
                    });
                }
                //驗証相關的Ticket 是否完成 (inbound)
                // 1-1-1 check parent ticket is complete 
                if (_ticketInfos.Content.Count() == 0)
                {
                    rs.Content = rs.Success = false;
                    rs.Message = "Ticket has been completed.";
                }
                else if (_ticketInfos.Content.All(x =>
                    x.ParentTickets.Count() > 0 && x.ParentTickets.All(p => p.Status < (int)TicketStatus.Glitch)))
                {
                    var _notCompleteTicket = _ticketInfos.Content.SelectMany(y => y.ParentTickets)
                        .Where(p => p.Status < (int)TicketStatus.Glitch).GroupBy(g => g.ID);
                    if (_ticketInfos.Content.All(x => x.ManifestType == (int)ManifestType.Inbound))
                    {
                        if (_ticketInfos.Content.SelectMany(p => p.InboundPartentTicketInfos)
                            .All(p => p.Status < (int)TicketStatus.Glitch))
                        {
                            var _notCompleteTicketInfo = _ticketInfos.Content.SelectMany(p => p.InboundPartentTicketInfos)
                       .Where(p => p.Status < (int)TicketStatus.Glitch).GroupBy(g => g.ID);
                            rs.Content = rs.Success = false;
                            rs.Message = "Parent Ticket info not complete " + string.Join(",", _notCompleteTicketInfo.Select(x => x.Key));
                        }
                    }
                    else
                    {
                        rs.Content = rs.Success = false;
                        rs.Message = "Parent Ticket not complete " + string.Join(",", _notCompleteTicket.Select(x => x.Key));
                    }
                }
                // 1-1-2 是否有future allocated
                if (_ticketInfos.Content.Any(p => p.PayloadType == (int)PayloadType.FutureAllocated))
                {
                    rs.Success = false;
                    rs.Message = Resource.TICKET_MOVETICKET_HAS_FUTURE_ALLOCATED;
                }
                if (rs.Success)
                {
                    // 1-1-2 log mobile upload scan barcode
                    LogScanbarcode(this.UploadData, MoveTicketCommand.OffPosition.ToString());
                    //TransactionScope scope = null;
                    //if (!this.IsExistTransaction)
                    //    scope = this.GetTransactionScope();
                    List<Guid> modifiedpayloaduids = new List<Guid>();
                    try
                    {

                        foreach (var item in _ticketInfos.Content)
                        {

                            List<ILabelModel> uploadlabelModels = new List<ILabelModel>();

                            var _uploaddata = this.UploadData.FirstOrDefault(p => p.TicketInfoUID == item.UID);
                            if (_uploaddata.IsAllPass)
                            {
                                if (_uploaddata.Barcode == null)
                                    _uploaddata.Barcode = new List<IUploadTicketBarcode>();
                                uploadlabelModels.AddRange(item.Barcodes.Where(p => p.Status == (int)LabelStatus.Active));
                                item.ActQty = item.EstQty;
                                item.Status = (int)TicketInfoStatus.Complete;
                            }
                            {
                                if (_uploaddata.Barcode.Count() > 0)//check barcode 
                                {
                                    var _usedLabel = item.Barcodes
                                        .Where(p => _uploaddata.Barcode.Select(x => x.Barcode).Contains(p.Content) && p.Status == (int)LabelStatus.Used);
                                    if (_usedLabel.Count() == _uploaddata.Barcode.Count())
                                    {
                                        rs.Success = false;
                                        rs.Message = string.Format(Resource.TICKET_BARCODE_HAD_SCANED, string.Join(",", _usedLabel.Select(x => x.Content)));
                                    }
                                    else
                                    {

                                        uploadlabelModels.AddRange(item.Barcodes
                                        .Where(p => _uploaddata.Barcode.Select(x => x.Barcode).Contains(p.Content) && !_usedLabel.Any(x => x.Content == x.Content)));
                                    }
                                }
                                // 2-1 check ast qty  priority AstQty> barcode code count
                                //排除104 Pallet OringalTracking 類型的Label ，它屬於辨識對應數量用途，非真正拿來識別Pallet 
                                var _operationQty = (_uploaddata.ScanType == ScanType.NoNeedToScan || _uploaddata.ScanType == ScanType.NoUnique) ?
                                                    _uploaddata.ActQty.Value :
                                                    uploadlabelModels.Where(p => p.Type != LabelType.Pallet_OrginalTracking).Count();

                                item.ActQty += _operationQty;
                            }
                            if (_uploaddata.SavQty.HasValue)
                            {
                                item.SavQty += _uploaddata.SavQty.Value;
                            }
                            if (_uploaddata.ShtQty.HasValue)
                            {
                                item.ShtQty += _uploaddata.ShtQty.Value;
                            }
                            //1-1-2掃瞄過的Barcode 是Receiving qty barcode 則disabled
                            if (item.ManifestType == (int)ManifestType.Outbound)
                            {
                                var _belongtoReceivingbarcode = _receivingQtyBarcodes.Content
                                    .Where(p => scanBarcode.Any(x => x == p.Content));
                                if (_belongtoReceivingbarcode != null && _belongtoReceivingbarcode.Count() > 0)
                                {
                                    //_result.Add(this.labelRepository.ChangeLabelStatus(
                                    //    _belongtoReceivingbarcode.Select(x => x.UID).ToArray(),
                                    //    LabelStatus.Inactive,
                                    //    $"TicketInfo#{item.UID} this barcode"));
                                    actions.Add(() => this.labelRepository.ChangeLabelStatus(
                                        _belongtoReceivingbarcode.Select(x => x.UID).ToArray(),
                                        LabelStatus.Inactive,
                                        $"TicketInfo#{item.UID} this barcode"));
                                }
                            }
                            if ((new int[] { (int)ManifestType.Outbound,
                                (int)ManifestType.BlukPick,(int)ManifestType.Move}.Any(p => p == item.ManifestType) &&
                                item.EstQty == item.ActQty + item.ShtQty + item.SavQty) ||
                                (item.ManifestType == (int)ManifestType.Inbound && item.ActQty == item.EstQty))
                            {
                                //2-2 outbound 下架有缺件/損壞時, 進行outbound 額外處理流程
                                if (item.ShtQty > 0 || item.SavQty > 0
                                    && this.appConfigure.IsFixFailureByMoveTicket
                                    && (item.ManifestType != (int)ManifestType.Inbound ||
                                    item.ManifestType != (int)ManifestType.InventoryCounting))
                                {
                                    var param = new TicketExtraProcessParameters()
                                    {
                                        AppConfigure = this.appConfigure,
                                        TicketInfoRepository = this.ticketInfoRepository,
                                        TicketManager = this.ticketManager,
                                        TicketInfoAssigneeRelationRepository = this.ticketInfoAssigneeRelationRepository,
                                        BulkPickManager = this.bulkPickManager,
                                        TicketRepository = this.ticketRepository,
                                        WorkOrderManager = this.workOrderManager,
                                        WorkOrderPayloadRepository = this.workOrderPayloadRepository,
                                        WorkOrderRepository = this.workOrderRepository,
                                        WorkOrderPodRepository = this.workOrderPodRepository,
                                        AuthenticationProvider = this.AuthenticationProvider,
                                        InventoryManager = this.inventoryManager,
                                        ReplicationManager = this.replicationManager,
                                        CompleteUnexecutedMethod = this.CompleteUnexecutedMethod
                                    };

                                    var processModule = new OutboundTicketExtraProcessModule(param);
                                    //_result.Add(processModule.GenerateUnAllocatedItem(item));
                                    actions.Add(() => processModule.GenerateUnAllocatedItem(item));
                                }
                                //當損壞/短缺數量=預計操作數量代表ticket item後續無需再操作
                                if (item.EstQty == item.ShtQty + item.SavQty)
                                {
                                    item.Status = (int)TicketInfoStatus.Complete;
                                }
                                else
                                {
                                    item.Status = (int)TicketInfoStatus.OffPosition;
                                }
                                if (uploadlabelModels.Count > 0)
                                {
                                    if (_uploaddata.ScanType == ScanType.Unique)
                                    {
                                        //_result.Add(this.labelRepository.ChangeLabelStatus(uploadlabelModels
                                        //    .Select(x => x.UID).ToArray(), LabelStatus.Used));
                                        actions.Add(() => this.labelRepository.ChangeLabelStatus(uploadlabelModels
                                         .Select(x => x.UID).ToArray(), LabelStatus.Used));
                                    }

                                }
                                actions.Add(() => this.ticketManager.UpdateTicketInfo(item));
                                //_result.Add(this.ticketManager.UpdateTicketInfo(item));
                                modifiedpayloaduids.Add(item.PayloadUID);

                                //var _payload = this.inventoryManager.GetPayload(item.PayloadUID);
                                //if (_payload.Content.Type != (int)PayloadType.ExtraAllocated)
                                //{
                                //    _result.Add(this.inventoryManager.ChangePayloadType(item.PayloadUID, (int)PayloadType.Allocated));//1:on hand 2:allocated
                                //}
                                if (item.PodUID != Guid.Empty)
                                {
                                    actions.Add(() => this.inventoryManager.ChangePodStauts(item.PodUID, PodStatus.OffPosition));
                                    //_result.Add(this.inventoryManager.ChangePodStauts(item.PodUID, PodStatus.OffPosition));
                                }

                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = Resource.TICKET_ACTQTY_MUST_EQUAL_ESTQTY;
                            }

                        }
                        actions.Add(() => this.inventoryManager.ChangePayloadStauts(modifiedpayloaduids, PayloadStatus.OffPosition));

                        var option = GetTransactionScopeOption(timeout: 30 * 60);
                        using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                        {
                            try
                            {
                                //6-1execute sql
                                foreach (var item in actions)
                                {
                                    _result.Add(item.Invoke());
                                }
                                //6-2 check manifest status
                                var statusModels = this.ticketManager
                      .GetBatchManifestStatusCollection(_ticketInfos.Content.Select(p => p.TicketUID));
                                var chkaction = this.CheckDataStatus(_ticketInfos.Content.First().TicketUID,
                                     (TicketType)_ticketInfos.Content.First().Type, statusModels.Content);
                                foreach (var item in chkaction)
                                {
                                    _result.Add(item.Invoke());
                                }
                                if (_result.All(p => p.Success) &&
                                     Transaction.Current?.TransactionInformation.Status != TransactionStatus.Aborted)
                                    scope.Complete();
                                else
                                    this.tracingAgent.Trace("execute sql transaction abort");
                            }
                            catch (Exception ex)
                            {
                                rs.Success = false;
                                this.tracingAgent.Trace("execute sql error", ex);
                            }
                        }
                        if (_result.Count > 0 && _result.All(p => p.Success) && rs.Success)
                        {
                            rs.Content = rs.Success = true;
                        }
                        else
                        {
                            rs.Content = rs.Success = false;
                            rs.Message += string.Join("\n\n", _result.Select(p => p.Message));
                        }
                    }
                    catch (Exception ex1)
                    {
                        rs.Message = ex1.Message + " " + ex1.StackTrace;
                        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                        rs.Success = false;
                        rs.InnerException = ex1;
                    }
                    //if (scope != null)
                    //{
                    //    scope.Dispose();
                    //}
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message + " " + ex.StackTrace; 
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
    }
}
