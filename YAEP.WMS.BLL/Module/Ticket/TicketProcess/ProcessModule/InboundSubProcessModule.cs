using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundSubProcessModule : AbstractProcessModule
    {
        public InboundSubProcessModule(
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
                    //this.tracingAgent.Trace("use inbound UploaddatabyPod");
                    return this.UploaddatabyPod(this.UploadData);
                }
                else
                {
                    //this.tracingAgent.Trace("use inbound UploaddatabyRegular");
                    return this.UploaddatabyRegular(this.UploadData);
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;

        }
        private IActionResult<bool> UploaddatabyPod(IEnumerable<ITicketInfoParameter> data)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {

                List<TicketInfoInnerParameter> param = new List<TicketInfoInnerParameter>();

                IActionResult<IEnumerable<ITicketInfoModel>> ticketInfos = null;
                if (!data.All(p => p.IsAllPass))
                {
                    ticketInfos = this.ticketManager.GetPodBelongTicket(data.SelectMany(x => x.Barcode.Select(p => p.Barcode)));
                    //this.tracingAgent.Trace("use inbound UploaddatabyPod GetPodBelongTicket get  data", param);
                }
                else
                {
                    ticketInfos = this.ticketManager.GetTicketInfoList(new { UID = data.Select(p => p.TicketInfoUID) });
                    //this.tracingAgent.Trace("use inbound UploaddatabyPod GetTicketInfoList get data", param);
                }

                if (ticketInfos.Content.Count() > 0)
                {
                    foreach (var item in ticketInfos.Content)
                    {
                        TicketInfoInnerParameter e = new TicketInfoInnerParameter();
                        e.ActQty = item.EstQty;
                        e.TicketInfoUID = item.UID;
                        e.ScanType = ScanType.NoNeedToScan;
                        e.IsAllPass = data.All(p => p.IsAllPass);
                        if (data.All(p => p.IsAllShortage))
                        {
                            e.IsAllShortage = true;
                            e.ActQty = 0;
                            e.ShtQty = item.EstQty;
                            e.Status = (int)TicketInfoStatus.Glitch;
                        }
                        param.Add(e);
                    }

                    return this.UploaddatabyRegular(param);
                }
                else
                {
                    this.tracingAgent.Trace("use inbound UploaddatabyPod not find data");
                    rs.Message = "";
                    return rs;
                }
            }
            catch (Exception ex)
            {
                this.tracingAgent.Trace("use inbound UploaddatabyPod exception", ex.Message, ex.StackTrace);
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        private IActionResult<bool> UploaddatabyRegular(IEnumerable<ITicketInfoParameter> Data)
        {
            this.tracingAgent.TransactionInfo.Action = TransactionlogAction.Receiving;
            List<Func<IActionResult<bool>>> actions = new List<Func<IActionResult<bool>>>();
            List<IActionResult<bool>> _result = new List<IActionResult<bool>>();
            TicketInfoStatus _ticketInfoStatus = TicketInfoStatus.Draft;
            List<Guid> prePodlist = new List<Guid>();
            // 1-1 get ticket data
            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            var _ticketInfos = this.ticketManager.GetTicketProcessModel(Data.Select(p => p.TicketInfoUID)?.ToArray());
            var _relticketinfos = this.ticketManager.GetTicketInfoList(new { TicketUID = _ticketInfos.Content.Select(x => x.TicketUID) });
            var _wopls = this.workOrderManager.GetWorkOrderPayload(new { PayloadUID = _ticketInfos.Content.Select(p => p.PayloadUID) });
            var _pods = this.inventoryManager.GetPod(_ticketInfos.Content.Select(x => x.PodUID).ToArray());
            if (_wopls.Success)
            {
                // 1-1-1 check parent ticket is complete 
                if (_ticketInfos.Content.Count() == 0)
                {
                    rs.Content = rs.Success = false;
                    rs.Message = "Ticket has been completed.";
                }
                else if (_ticketInfos.Content.All(x => x.ParentTickets.Count() > 0 &&
                x.ParentTickets.All(p => p.Status < (int)TicketStatus.Glitch)))
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

                    //using (var scope = this.GetNewTransactionScope(TransactionScopeOption.RequiresNew))
                    //{
                    //TransactionScope scope = null;
                    //if (!this.IsExistTransaction)
                    //    scope = this.GetTransactionScope();
                    // 1-1-2 log mobile upload scan barcode
                    LogScanbarcode(Data, "Mobile inbound");
                    this.tracingAgent.Trace($"inbound ticket ready to processing", _ticketInfos.Content);
                    foreach (var item in _ticketInfos.Content)
                    {
                        var converter = AbstractTicketConverter.GetInstance(item.ManifestType, item.Type);
                        converter.Convert(item);
                        var _pkg = this.packageManager.GetPackage(item.TargetPackage.Value).Content;
                        var _uomInfo = this.packageUomManager.GetPackageUom(_pkg.UOM).Content;
                        var _woplCount = this.workOrderManager.GetWorkOrderPayload(new { WorkOrderPODUID = item.WorkOrderPodUID });
                        //當Pod Type=NewPallet & UOM=Pallet 且workorder payload數量只有1個label 需改成payload 取得
                        if (_woplCount.Content != null && _woplCount.Content.Count() == 1
                            && item.StorageType ==
                            (int)StorageMethod.NewPallet &&
                             _uomInfo.Name.Equals(WMSAPIParameters.PALLET_UOM_KEYNAME, StringComparison.OrdinalIgnoreCase))
                        {
                            item.Barcodes = this.labelRepository
                                .GetLabels(_woplCount.Content.Select(x => x.PayloadUID).ToArray()).Content;
                        }
                        List<ILabelModel> uploadlabelModels = new List<ILabelModel>();
                        //find upload data
                        var _uploaddata = Data.FirstOrDefault(p => p.TicketInfoUID == item.UID);

                        if (_uploaddata != null)
                        {
                            if (_uploaddata.IsAllPass)
                            {
                                if (_uploaddata.Barcode == null)
                                    _uploaddata.Barcode = new List<IUploadTicketBarcode>();
                                uploadlabelModels.AddRange(item.Barcodes.Where(p => p.Status == (int)LabelStatus.Active));
                                item.ActQty = item.EstQty;
                                _ticketInfoStatus = TicketInfoStatus.Complete;
                                item.Status = (int)_ticketInfoStatus;
                            }
                            else if (_uploaddata.IsAllShortage)
                            {
                                item.ShtQty = item.EstQty;
                                _ticketInfoStatus = TicketInfoStatus.Glitch;
                                item.Status = (int)_ticketInfoStatus;
                            }
                            else
                            {
                                if (_uploaddata.Barcode != null && _uploaddata.Barcode.Count() > 0)//check barcode 
                                {
                                    var _usedLabel = item.Barcodes
                                        .Where(p => _uploaddata.Barcode.Select(x => x.Barcode)
                                        .Contains(p.Content) && p.Status == (int)LabelStatus.Used);
                                    if (_usedLabel.Count() == _uploaddata.Barcode.Count() && _uploaddata.ScanType == ScanType.Unique)
                                    {
                                        rs.Success = false;
                                        rs.Message = string.Format(Resource.TICKET_BARCODE_HAD_SCANED, string.Join(",", _usedLabel.Select(x => x.Content)));
                                    }
                                    else
                                    {

                                        uploadlabelModels.AddRange(item.Barcodes.Where(y =>
                                        _uploaddata.Barcode.Select(x => x.Barcode)
                                        .Where(p => !_usedLabel.Any(x => x.Content == p)).Contains(y.Content)));
                                    }
                                }
                                #region 2-1 check ast qty  priority AstQty> barcode code count
                                var _operationQty = (_uploaddata.ScanType == ScanType.NoNeedToScan || _uploaddata.ScanType == ScanType.NoUnique) ?
                                                    _uploaddata.ActQty.Value : uploadlabelModels.Count;
                                item.ActQty += _operationQty;

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
                                if (!item.TargetSlotUID.HasValue)
                                {
                                    rs.Success = false;
                                    rs.Message = Resource.TICKET_LOST_TARGETSLOTUID;
                                }
                                if (!item.TargetPackage.HasValue)
                                {
                                    rs.Success = false;
                                    rs.Message = Resource.TICKET_LOST_TARGETPACKAGE;
                                }
                                #endregion
                            }
                            if (rs.Success)
                            {
                                #region Process
                                try
                                {
                                    #region modified ticketinfo data
                                    actions.Add(() =>
                                    {
                                        //this.tracingAgent.Trace($"Process Ticket info #{item.UID}");
                                        // 1-1 update ticketinfo 
                                        //var beforeTi = this.ticketInfoRepository.GetData(item.UID);
                                        var updateTicketInfo = this.ticketManager.UpdateTicketInfo(item);
                                        //var afterTi = this.ticketInfoRepository.GetData(item.UID);
                                        //this.tracingAgent.Trace($"Update inbound ticket info", item, afterTi);
                                        return updateTicketInfo;
                                    });
                                    #endregion
                                    if (!Data.All(p => p.IsAllShortage))
                                    {
                                        if (_ticketInfoStatus == TicketInfoStatus.Complete || _ticketInfoStatus == TicketInfoStatus.Glitch)
                                        {
                                            //如果操作數量大於0才會進行處理
                                            if (item.ActQty > 0)
                                            {
                                                #region 1-2 update label status to active
                                                actions.Add(() =>
                                                {
                                                    var rs12 = this.labelRepository
                                                          .ChangeLabelStatus(item.Barcodes.Select(x => x.UID).ToArray(), LabelStatus.Active);
                                                    this.tracingAgent.Trace($"after update label status to active", rs12);
                                                    return rs12;
                                                });
                                                #endregion
                                                #region 2-1 add payload data & Add Pod data
                                                if (item.StorageType == 1)// add pod data
                                                {
                                                    //1.判斷當下資料POD是否有重覆 (還沒commit的資料)
                                                    //2.判斷已經存在DB的POD是否重覆(已經commit的資料)
                                                    if (!_pods.Content.Any(x => x.UID == item.PodUID) &&
                                                        !prePodlist.Any(x => x == item.PodUID))
                                                    {
                                                        var workorderPod = this.workOrderManager.GetWorkOrderPod(new { PodUID = item.PodUID });
                                                        var _seq = this.sequenceAgent.GetPodSeqenceByTimeSerial(PayloadType.Stock);
                                                        PodInnerModel _pod = new PodInnerModel();
                                                        _pod.UID = item.PodUID;
                                                        _pod.ID = _seq;
                                                        _pod.IsPack = true;
                                                        _pod.Status = (int)PodStatus.Open;
                                                        //TODO Work order payload 沒資料(從workorderpod取得)
                                                        if (workorderPod.Success)
                                                        {
                                                            _pod.VolumeLimit = workorderPod.Content.Volume;
                                                            _pod.WeightLimit = workorderPod.Content.Weight;
                                                        }
                                                        actions.Add(() =>
                                                        {

                                                            var adpd = this.inventoryManager.AddPod(_pod);
                                                            this.tracingAgent.Trace($"after add pod data", adpd);
                                                            return adpd;
                                                        });
                                                        prePodlist.Add(item.PodUID);
                                                    }

                                                }
                                                #endregion
                                                #region add payload data
                                                PayloadInnerModel _payload = new PayloadInnerModel();
                                                var _wopl = _wopls.Content.FirstOrDefault(p => p.PayloadUID == item.PayloadUID);
                                                if (_wopl != null)
                                                {
                                                    var _seq_pl = this.sequenceAgent.GetPayloadSeqenceByTimeSerial(PayloadType.Stock);

                                                    _payload.UID = item.PayloadUID;
                                                    _payload.ID = _seq_pl;
                                                    _payload.ItemUID = item.ItemUID;
                                                    _payload.PackageUID = item.TargetPackage.Value;
                                                    _payload.PODUID = item.PodUID;

                                                    //if (item.StorageType == (int)StorageMethod.NewPallet)
                                                    //    _payload.Quantity = item.OriginalQty;
                                                    //else
                                                    _payload.Quantity = item.ActQty;
                                                    _payload.SlotUID = item.TargetSlotUID.Value;
                                                    _payload.Status = (int)PayloadStatus.WaitingForProcessing;
                                                    _payload.VesselUID = item.VesselUID;
                                                    _payload.Type = (int)PayloadType.Stock;
                                                    _payload.VolumeLimit = _wopl.Volume.HasValue ? _wopl.Volume.Value : 0;
                                                    _payload.WeightLimit = _wopl.Weight.HasValue ? _wopl.Weight.Value : 0;

                                                    actions.Add(() =>
                                                    {
                                                        var rsadpy = this.inventoryManager.AddPayload(_payload);
                                                        this.tracingAgent.Trace($"after add payload data", rsadpy);
                                                        return rsadpy;
                                                    });
                                                }
                                                else
                                                {
                                                    rs.Success = false;
                                                    rs.Message = Resource.TICKET_LOST_WORKORDER_PAYLOAD_DATA;
                                                    break;
                                                }
                                                #endregion
                                                #region 3-1 modified inventory onhand
                                                InsertInventoryParameter iparam = new InsertInventoryParameter();
                                                iparam.ItemUID = _payload.ItemUID;
                                                iparam.Qty = _payload.Quantity;
                                                iparam.SlotUID = _payload.SlotUID;
                                                iparam.TargetPackageUID = item.TargetPackage.Value;
                                                iparam.Type = InventoryType.Stock;
                                                iparam.WarehouseUID = item.WarehouseUID;
                                                iparam.UseMiniPackage = true;
                                                actions.Add(() =>
                                                {
                                                    var rs31 = this.inventoryManager.InsertInventory(new InsertInventoryParameter[] { iparam });
                                                    this.tracingAgent.Trace($"after modified inventory onhand qty", iparam, rs31);
                                                    return rs31;
                                                });
                                                #endregion
                                                #region 4-1 Transcation log
                                                var _log = this.GetTxLog(item);
                                                _log.QtyBeforeTX = 0;
                                                _log.QtyAfterTX = item.ActQty;
                                                _log.Type = (int)this.tracingAgent.GetTransactionLogType();



                                                actions.Add(() =>
                                                {
                                                    var rs4 = this.inventoryManager.AddLog(_log);
                                                    this.tracingAgent.Trace($"after add Transcation log", rs4);
                                                    return rs4;
                                                });
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            if (_uploaddata.ScanType == ScanType.Unique)
                                                actions.Add(() => this.labelRepository.ChangeLabelStatus(uploadlabelModels.Select(x => x.UID).ToArray(), LabelStatus.Used));
                                        }
                                        // 5-1 Modify ticket status, check all status
                                        //actions.AddRange(this.CheckDataStatus(item.TicketUID, (TicketType)item.Type));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    WriteLog(ex.Message + " " + ex.StackTrace, "", "", "error", (int)YAEP.Constants.BelongToTypes.Mobile, item.UID.ToString());
                                    this.tracingAgent.Trace($"process inbound expcetion", ex.Message, ex.StackTrace);
                                    rs.Message = ex.Message + " " + ex.StackTrace;
                                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                                    rs.Success = false;
                                    rs.InnerException = ex;
                                }

                                #endregion
                            }
                        }

                    }
                    this.tracingAgent.Trace($"before execute sql");
                    #region Exceute SQL
                    if (rs.Success)
                    {
                        this.TransactionAction.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                        //var option = GetTransactionScopeOption(timeout: 30 * 60);
                        //using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                        //{
                        try
                        {
                            //6-1execute sql
                            foreach (var item in actions)
                            {
                                if (_result.All(p => p.Success))
                                {
                                    _result.Add(item.Invoke());
                                }
                            }

                            //6-2 check manifest status
                            var statusModels = this.ticketManager
                        .GetBatchManifestStatusCollection(_ticketInfos.Content.Select(p => p.TicketUID));

                            var chkaction = this.CheckDataStatus(_ticketInfos.Content.First().TicketUID,
                                 (TicketType)_ticketInfos.Content.First().Type, statusModels.Content);
                            foreach (var item in chkaction)
                            {
                                if (_result.All(p => p.Success))
                                {
                                    _result.Add(item.Invoke());
                                }
                            }
                            //6-3 check move ticket can process
                            //如果TicketInfo Actqty=0則對應的move ticket item 則自動列為glitch，若全部都glitch則move ticket自動完成(glitch)
                            var _aticketInfos = this.ticketManager.GetTicketInfoList(
                                new { TicketUID = _ticketInfos.Content.GroupBy(g => g.TicketUID).Select(p => p.Key) });
                            if (_aticketInfos.Content.Any(p => p.ActQty == 0 && (int)TicketInfoStatus.Glitch == p.Status))
                            {
                                actions.Clear();
                                var _bticketinfos = _aticketInfos.Content.Where(p => p.ActQty == 0 &&
                                                    p.Status == (int)TicketInfoStatus.Glitch);
                                var _mticketinfos = this.ticketInfoRepository.GetBelongToMoveTicketInfo(_bticketinfos.Select(x => x.UID));
                                foreach (var item in _mticketinfos.Content)
                                {
                                    item.ShtQty = item.EstQty;
                                    item.Status = (int)TicketInfoStatus.Glitch;
                                    actions.Add(() =>
                                    {
                                        var updateTicketInfo = this.ticketManager.UpdateTicketInfo(item);
                                        return updateTicketInfo;
                                    });
                                }
                                foreach (var item in actions)
                                {
                                    if (_result.All(p => p.Success))
                                    {
                                        _result.Add(item.Invoke());
                                    }
                                }
                                //6-3-1 recheck status for move ticket
                                var mstatusModels = this.ticketManager
                                    .GetBatchManifestStatusCollection(_mticketinfos.Content.Select(p => p.TicketUID));
                                var mchkaction = this.CheckDataStatus(_mticketinfos.Content.First().TicketUID,
                                 (TicketType)_mticketinfos.Content.First().Type, mstatusModels.Content);
                                foreach (var item in mchkaction)
                                {
                                    if (_result.All(p => p.Success))
                                    {
                                        _result.Add(item.Invoke());
                                    }
                                }
                            }
                            if (_result.All(p => p.Success))
                            {
                                this.TransactionAction.CommitTransaction();
                            }
                            else
                            {
                                this.TransactionAction.RollbackTransaction();
                                this.tracingAgent.Trace("execute sql transaction abort");
                            }
                        }
                        catch (Exception ex)
                        {
                            rs.Success = false;
                            this.TransactionAction.RollbackTransaction();
                            this.tracingAgent.Trace("execute sql error", ex);
                        }
                        //}
                    }
                    #endregion
                    this.TransactionAction.DisposeConnectionInstance();
                    this.TransactionAction.ReInitConnectionInstance();
                    //6-1 repliate data to subscriber
                    if (_result.All(p => p.Success) && rs.Success)
                    {
                        //var syncRs = ;
                        //_result.Add(syncRs);
                        this.CompleteUnexecutedMethod.Enqueue(() =>
                        this.replicationManager.Receivied(_ticketInfos.Content.FirstOrDefault().PartyUID,
                      _ticketInfos.Content.Select(p => p.UID)));
                        this.tracingAgent.Trace($"after repliate data to subscriber (in queue)", _ticketInfos.Content.Select(p => p.UID));
                    }
                    if (_result.Count > 0 && _result.All(p => p.Success) && rs.Success)
                    {
                        //this.tracingAgent.Trace($"inbound ticket complete transactionscope status: {TransactionAction.Current?.TransactionInformation.Status.ToString()}");
                        //this.transacationScope.Complete(scope);
                        //scope.Complete();
                        rs.Content = rs.Success = true;
                    }
                    else
                    {
                        rs.Content = rs.Success = false;
                        rs.Message = string.Join("\n\n", _result.Where(x => !x.Success).Select(p => p.Message));
                        this.tracingAgent.Trace($"inbound had error", _result.Where(x => !x.Success));
                    }
                    //if (scope != null)
                    //{
                    //    scope.Dispose();
                    //}
                    //}
                }
            }
            else
            {
                rs.Success = false;
                rs.Message = Resource.TICKET_LOST_WORKORDER_PAYLOAD_DATA;
            }
            return rs;
        }


    }
}
