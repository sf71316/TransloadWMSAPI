using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Hosting;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundSubProcessModule : AbstractProcessModule
    {
        public OutboundSubProcessModule(
            ITicketProcessAgentParameter parameters, ILogInfiltrator logInfiltrator)
            : base(parameters, logInfiltrator)
        {

        }

        public override IActionResult<bool> Execute(IEnumerable<IUploadTicketDataParameter> Data,
            NotifySenderConfig sendInfo = null)
        {
            this.sendInfo = sendInfo;
            this.UploadData = Data.Select(p => p.Item);
            var rs = ActionResultTemplates.Result<bool>();
            try
            {

                if (Data.All(p => p.Item.IsPodScan || p.Item.IsAllPass))
                {
                    this.tracingAgent.Trace("use outbound UploaddatabyPod");
                    return this.UploaddatabyPod();
                }
                else
                {
                    this.tracingAgent.Trace("use outbound UploaddatabyRegular");
                    return this.UploaddatabyRegular();
                }

            }
            catch (Exception ex)
            {
                this.tracingAgent.Trace("use OutboundSubProcessModule exception", ex.Message, ex.StackTrace);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        private IActionResult<bool> UploaddatabyPod()
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {

                List<TicketInfoInnerParameter> param = new List<TicketInfoInnerParameter>();

                IActionResult<IEnumerable<ITicketInfoModel>> ticketInfos = null;
                if (!this.UploadData.All(p => p.IsAllPass))
                {
                    ticketInfos = this.ticketManager.GetPodBelongTicket(this.UploadData.SelectMany(x => x.Barcode.Select(p => p.Barcode)));
                    this.tracingAgent.Trace($"Outbound UploaddatabyPod GetPodBelongTicket get data  ", this.UploadData, ticketInfos);
                }
                else
                {
                    ticketInfos = this.ticketManager.GetTicketInfoList(new { UID = this.UploadData.Select(p => p.TicketInfoUID) });
                    this.tracingAgent.Trace($"Outbound UploaddatabyPod GetTicketInfoList get data  ", this.UploadData, ticketInfos);
                }

                if (ticketInfos.Content.Count() > 0)
                {
                    foreach (var item in ticketInfos.Content)
                    {
                        TicketInfoInnerParameter e = new TicketInfoInnerParameter();
                        e.ActQty = item.EstQty;
                        e.TicketInfoUID = item.UID;
                        e.ScanType = ScanType.NoNeedToScan;
                        e.IsAllPass = this.UploadData.All(p => p.IsAllPass);
                        param.Add(e);
                    }
                    this.UploadData = param;
                    return this.UploaddatabyRegular();
                }
                else
                {
                    rs.Message = "";
                    return rs;
                }
            }
            catch (Exception ex)
            {
                this.tracingAgent.Trace($"OutboundSubProcessModule UploaddatabyPod exception ", ex.Message, ex.StackTrace);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        private IActionResult<bool> UploaddatabyRegular()
        {
            this.tracingAgent.TransactionInfo.Action = TransactionlogAction.Pack;
            List<Func<IActionResult<bool>>> actions = new List<Func<IActionResult<bool>>>();
            //RETEST IsAllPoss flow
            TicketInfoStatus _ticketInfoStatus = TicketInfoStatus.Draft;
            // 1-1 get ticket data
            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            var _ticketInfos = this.ticketManager.GetTicketProcessModel(UploadData.Select(p => p.TicketInfoUID)?.ToArray());

            List<IActionResult<bool>> _result = new List<IActionResult<bool>>();
            // 1-1-1 check parent ticket is complete 
            if (_ticketInfos.Content.Count() == 0)
            {
                rs.Content = rs.Success = false;
                rs.Message = "Ticket has been completed.";
            }
            else if (_ticketInfos.Content.All(x => x.ParentTickets.All(p => p.Status < (int)TicketStatus.Glitch)))
            {
                var _notCompleteTicket = _ticketInfos.Content.SelectMany(y => y.ParentTickets)
                    .Where(p => p.Status < (int)TicketStatus.Glitch).GroupBy(g => g.ID);
                rs.Content = rs.Success = false;
                rs.Message = "Parent Ticket not complete " + string.Join(",", _notCompleteTicket.Select(x => x.Key));
                var notfinish = _ticketInfos.Content.Where(x => x.ParentTickets.Count() > 0 &&
                      x.ParentTickets.All(p => p.Status < (int)TicketStatus.Glitch));
                this.tracingAgent.Trace($"Parent ticket info has not complete", notfinish);
            }
            if (rs.Success)
            {
                // 1-1-2 log mobile upload scan barcode
                LogScanbarcode(this.UploadData, "Mobile outbound");
                var _payloads = this.inventoryManager.GetPayloadList(new { UID = _ticketInfos.Content.Select(p => p.PayloadUID) });
                foreach (var item in _ticketInfos.Content)
                {
                    var converter = AbstractTicketConverter.GetInstance(item.ManifestType, item.Type);
                    converter.Convert(item);
                    List<ILabelModel> uploadlabelModels = new List<ILabelModel>();
                    //find upload data
                    var _uploaddata = this.UploadData.FirstOrDefault(p => p.TicketInfoUID == item.UID);
                    if (_uploaddata != null)
                    {
                        #region 驗証資料/Label/Qty
                        if (_uploaddata.IsAllPass)
                        {
                            if (_uploaddata.Barcode == null)
                                _uploaddata.Barcode = new List<IUploadTicketBarcode>();
                            uploadlabelModels.AddRange(item.Barcodes.Where(p => p.Status == (int)LabelStatus.Active));
                            item.ActQty = item.EstQty;
                            _ticketInfoStatus = TicketInfoStatus.Complete;
                            item.Status = (int)_ticketInfoStatus;
                        }
                        else
                        {
                            if (_uploaddata.Barcode != null && _uploaddata.Barcode.Count() > 0)//check barcode 
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
                            var _operationQty = (_uploaddata.ScanType == ScanType.NoNeedToScan || _uploaddata.ScanType == ScanType.NoUnique) ?
                                                _uploaddata.ActQty.Value : uploadlabelModels.Count;
                            item.ActQty += _operationQty;
                        }
                        if (item.ActQty == item.EstQty)
                        {
                            if (_uploaddata.SavQty.HasValue)
                            {
                                item.SavQty += _uploaddata.SavQty.Value;
                            }
                            if (_uploaddata.ShtQty.HasValue)
                            {
                                item.ShtQty += _uploaddata.ShtQty.Value;
                            }
                            if (item.EstQty <= item.ActQty + item.SavQty + item.ShtQty) //complete
                            {
                                if (item.ShtQty == 0 && item.SavQty == 0)
                                    _ticketInfoStatus = TicketInfoStatus.Complete;
                                else
                                    _ticketInfoStatus = TicketInfoStatus.Glitch;
                            }
                            else //not yet
                            {

                                _ticketInfoStatus = TicketInfoStatus.Processing;
                            }
                            item.Status = (int)_ticketInfoStatus;
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = Resource.TICKET_ACTQTY_MUST_EQUAL_ESTQTY;
                        }
                        if (!item.OriginalSlotUID.HasValue)
                        {
                            rs.Success = false;
                            rs.Message = Resource.TICKET_LOST_ORIGINALSLOTUID;
                        }
                        if (!item.TargetPackage.HasValue)
                        {
                            rs.Success = false;
                            rs.Message = Resource.TICKET_LOST_TARGETPACKAGE;
                        }
                        #endregion

                        if (rs.Success)
                        {
                            #region Process
                            try
                            {
                                #region process ticketinfo/ label data
                                //this.tracingAgent.Trace($"Process Ticket info in queue #{item.UID}");
                                // 1-1 update ticketinfo 
                                actions.Add(() =>
                                {
                                    //var beforeTi = this.ticketInfoRepository.GetData(item.UID);
                                    item.ModifiedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                                    item.ModifiedOn = DateTime.UtcNow;
                                    var updateTicketInfo = this.ticketManager.UpdateTicketInfo(item);
                                    //var afterTi = this.ticketInfoRepository.GetData(item.UID);
                                    //this.tracingAgent.Trace($"Update outbound ticket info", beforeTi, afterTi);
                                    return updateTicketInfo;
                                });

                                //1-2 update label status 
                                if (_uploaddata.ScanType == ScanType.Unique)
                                {
                                    actions.Add(() => this.labelRepository.ChangeLabelStatus(uploadlabelModels.Select(x => x.UID).ToArray(), LabelStatus.Used));
                                    //_result.Add(this.labelRepository.ChangeLabelStatus(uploadlabelModels.Select(x => x.UID).ToArray(), LabelStatus.Used));
                                }
                                //1-3 disable other label status
                                var otherLabel = item.Barcodes.Where(p => uploadlabelModels.All(x => x.UID != p.UID));
                                // 1-3-1 disable allocated payload remain label (自已還沒使用的label)
                                var updateLabelModel = new List<string>();
                                if (otherLabel.Count() > 0)
                                {
                                    updateLabelModel.AddRange(otherLabel.Where(x => new LabelType[] {
                                    LabelType.Box_EAN, LabelType.Box_SCC14,LabelType.Box_UPC,
                                    LabelType.Item_EAN,LabelType.Item_UPC
                                    }.All(p => p != x.Type)).Select(p => p.Content));
                                }
                                //1-3-2 disable original payload use label or other allocated payload label
                                //(如果Label是不可重覆的barcode ，則需要把其它payload 且有相同barcode刪除)
                                updateLabelModel.AddRange(uploadlabelModels
                                    .Where(x => new LabelType[] {
                                    LabelType.Box_EAN, LabelType.Box_SCC14,LabelType.Box_UPC,
                                    LabelType.Item_EAN,LabelType.Item_UPC
                                    }.All(p => p != x.Type))
                                    .Select(p => p.Content));
                                actions.Add(() => this.labelRepository
                                    .ChangeLabelStatus(updateLabelModel.ToArray(), LabelStatus.Inactive));
                                #endregion
                                if (_ticketInfoStatus == TicketInfoStatus.Complete)
                                {
                                    // 2-1 change payload data 
                                    if (item.StorageType == 1)// add pod data
                                    {
                                        //if (!this.warehouseManger.PodIsExist(item.PodUID).Success)
                                        //{
                                        // delete pod ?
                                        //}
                                    }

                                    //get payload data and change status to inactive
                                    //var _payload = this.inventoryManager.GetPayload(item.PayloadUID); ss
                                    var _payload = _payloads.Content.FirstOrDefault(p => p.UID == item.PayloadUID);
                                    if (_payload != null)
                                    {
                                        _payload.Status = (int)PayloadStatus.Inactive;
                                        actions.Add(() => this.inventoryManager.UpdatePayload(_payload));
                                        // 3-1 modified inventory onhand minus qty

                                        InsertInventoryParameter iparam = new InsertInventoryParameter();
                                        iparam.ItemUID = _payload.ItemUID;
                                        iparam.Qty = _payload.Quantity * -1;
                                        iparam.SlotUID = item.OriginalSlotUID.Value;
                                        iparam.TargetPackageUID = item.TargetPackage.Value;
                                        iparam.Type = (InventoryType)item.OriginalPayloadType;
                                        iparam.WarehouseUID = item.WarehouseUID;
                                        iparam.UseMiniPackage = true;
                                        actions.Add(() =>
                                        {
                                            var rs31 = this.inventoryManager.InsertInventory(new InsertInventoryParameter[] { iparam });
                                            this.tracingAgent.Trace($"after modified inventory onhand minus qty", iparam, rs31);
                                            return rs31;
                                        });

                                        //3-2 是否有加入BulkPick, 判斷Ticket是否完成，完成將BulkPick所屬的payload delete
                                        var movetickets = item.ParentTickets.Where(p => p.Type == (int)TicketType.Move);
                                        var moveticketInfos = this.ticketInfoRepository.GetList(
                                                            new { TicketUID = movetickets.Select(x => x.UID) }).Content;
                                        var bulkpickRs = this.bulkPickManager
                                                                .GetBulkPickIDByTicketInfo(moveticketInfos.Select(x => x.UID));
                                        if (bulkpickRs.Content != null && bulkpickRs.Content.Count() > 0)
                                        {
                                            var bulkPickInfos = this.bulkPickManager
                                                            .GetBulkPickInfoByTicketInfo(moveticketInfos.Select(x => x.UID));
                                            var ticketinfo = this.ticketInfoRepository
                                                .GetList(new { UID = bulkPickInfos.Content.Select(p => p.TicketInfoUID) });

                                            if (ticketinfo.Content != null &&
                                                ticketinfo.Content.All(p => p.Status >= (int)TicketInfoStatus.Glitch))
                                            {
                                                var ticketUID = bulkPickInfos.Content.First().TicketUID;
                                                var payloadbybulkpick = this.inventoryManager.GetListByTicket(ticketUID);
                                                //foreach (var pb in payloadbybulkpick.Content)
                                                //{
                                                //    var pbr = this.inventoryManager.ChangePayloadStauts(pb.UID, PayloadStatus.Inactive);
                                                //    _result.Add(pbr);
                                                //    pbrs.Add(pbr);
                                                //}
                                                actions.Add(() =>
                                                {
                                                    var rs2 = this.inventoryManager.ChangePayloadStauts(payloadbybulkpick.Content.Select(p => p.UID), PayloadStatus.Inactive);
                                                    this.tracingAgent.Trace($"after BulkPick ChangePayloadStauts", payloadbybulkpick.Content, rs2);
                                                    return rs2;
                                                });

                                            }
                                        }
                                        // 4-1 Transcation log
                                        var _log = this.GetTxLog(item);
                                        _log.QtyBeforeTX = 0;
                                        _log.QtyAfterTX = item.EstQty * -1;
                                        _log.Type = (int)this.tracingAgent.GetTransactionLogType();
                                        _log.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                                        _log.CreatedOn = DateTime.UtcNow;
                                        actions.Add(() => this.inventoryManager.AddLog(_log));

                                    }
                                    else
                                    {
                                        //??
                                        _result.Add(this.GetResult(false, Resource.TICKET_NOT_FIND_PAYLOAD));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.tracingAgent.Trace($"outbound processing exception", ex.Message, ex.StackTrace);
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
                // 5-1 Modify ticket status, check all status
                if (rs.Success)
                {

                    var statusRs = new List<IActionResult<bool>>();
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

                //7-1 repliate data to subscriber  確保上述動作都完成再同步onhand 資料
                if (rs.Success && _result.All(p => p.Success) && _ticketInfos.Content.Count() > 0)
                {
                    List<NotificationSendTaskModel> tasks = new List<NotificationSendTaskModel>();
                    foreach (var item in _ticketInfos.Content)
                    {
                        var request = new OutboundTicketInfoCompleteRequest();
                        var processItem = new NotificationProcessInfo();
                        var task = new NotificationSendTaskModel();
                        task.UID = Guid.NewGuid();
                        task.EventName = EventHelper.OUTBOUND_TICKET_INFO_COMPLETED;
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
                    if (tasks.Count > 0)
                    {
                        this.CompleteUnexecutedMethod.Enqueue(() => this.notificationSenderTaskRepository.BatchAdd(tasks));
                    }
                    var parameter = new ReplicateDataParameter();
                    parameter.TicketInfoUID = _ticketInfos.Content.Select(p => p.UID);
                    this.CompleteUnexecutedMethod.Enqueue(() => this.replicationManager.Outbound(parameter));
                    this.tracingAgent.Trace($"replicate data to subscriber (in queue)", parameter);
                }

                if (_result.Count > 0 && _result.All(p => p.Success))
                {
                    this.tracingAgent.Trace($"Outbound ticket complete transactionscope status: {Transaction.Current?.TransactionInformation.Status.ToString()}");
                    //this.transacationScope.Complete(scope);
                    rs.Content = rs.Success = true;
                }
                else
                {
                    rs.Content = rs.Success = false;
                    rs.Message = string.Join(",", _result.Where(x => !x.Success).Select(p => p.Message));
                }
                //if (scope != null)
                //{
                //    scope.Dispose();
                //}
            }
            return rs;
        }
    }
}
