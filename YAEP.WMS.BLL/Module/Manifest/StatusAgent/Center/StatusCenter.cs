using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal partial class StatusCenter
    {
        internal IAuthenticationProvider authenticationProvider;
        protected StatusManageAgent _agent;
        protected ITracingAgent TracingAgent;
        public StatusCenter(StatusManageAgent statusManageAgent, ITracingAgent tracingAgent, IAuthenticationProvider authProvider)
        {
            this._agent = statusManageAgent;
            authenticationProvider = authProvider;
            TracingAgent = tracingAgent;
        }
        public IActionResult<IManifestViewModel> ProcessManifest(Guid manifestUID)
        {
            var rs = ActionResultTemplates.Result<IManifestViewModel>();
            var manifestInfo = this._agent.DataModules.ManifestRepository.GetInfo(manifestUID);
            if (manifestInfo.Success)
            {
                if ((new ManifestStatus[] { ManifestStatus.Draft, ManifestStatus.Reject }).Contains(manifestInfo.Content.Status))
                {
                    var rs2 = this.SubmitManifest(manifestInfo.Content);
                    rs.Success = rs2.Success;
                    if (rs2.Success)
                    {
                        var viewModel = new ManifestInnerViewModel(manifestInfo.Content);
                        viewModel.Status = ManifestStatus.Open;
                        viewModel.StatusName = ManifestStatus.Open.ToString();
                        rs.Content = viewModel;
                    }
                    else
                    {
                        rs.Message = rs2.Message;
                    }
                }
                else
                {
                    rs.Success = true;
                    rs.Message = Resource.MANIFEST_STATUS_NOACTION;
                }
            }
            else
            {

                rs.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;

            }
            return rs;
        }
        public IActionResult<IBolModel> ProcessBOL(Guid bolUID)
        {
            var rs = ActionResultTemplates.Result<IBolModel>();
            var bolInfo = this._agent.DataModules.BolRepository.GetBol(new { UID = bolUID });

            if (bolInfo.Success && bolInfo.Content != null)
            {
                var manifestInfo = this._agent.DataModules.ManifestRepository.GetInfo(bolInfo.Content.ManifestUID);
                if (manifestInfo.Success)
                {
                    if (bolInfo.Content.Status == BolStatus.Draft)//TODO 是否需檢查Vessel 是否分配完成?
                    {
                        if (manifestInfo.Content.Status == ManifestStatus.Open)
                        {

                            var rs2 = this._agent.Bol.CheckVesselAssignedComplete(bolUID);
                            if (rs2.Content != null)
                            {
                                if (rs2.Content.Count() == 0)
                                {
                                    var result = this.SubmitBol(bolInfo.Content);
                                    rs.Success = result.Success;
                                    if (result.Success)
                                    {
                                        bolInfo.Content.Status = BolStatus.Review;
                                        bolInfo.Content.StatusName = bolInfo.Content.Status.ToString();
                                        rs.Content = bolInfo.Content;
                                    }
                                    else
                                    {
                                        rs.Message = result.Message;
                                        rs.Success = false;
                                    }
                                }
                                else
                                {
                                    rs.Message = Resource.MANIFEST_BOL_UNCOMPLETE_ASSIGNED + string.Join(",", rs2.Content);
                                    rs.Success = false;
                                }

                            }
                            else
                            {
                                rs.Message = Resource.MANIFEST_NOT_FIND_VESSEL_DATA;
                                rs.Success = false;
                            }
                        }
                        else
                        {
                            rs.Message = Resource.MANIFEST_BOL_SUBMIT_STATUS_ERROR;
                        }
                    }
                    else if (bolInfo.Content.Status == BolStatus.Reject)
                    {
                        if (manifestInfo.Content.Status == ManifestStatus.Open)
                        {
                            var result = this.SubmitBol(bolInfo.Content);
                            rs.Success = result.Success;
                            if (result.Success)
                            {
                                bolInfo.Content.Status = BolStatus.Review;
                                bolInfo.Content.StatusName = bolInfo.Content.Status.ToString();
                                rs.Content = bolInfo.Content;
                            }
                            else
                            {
                                rs.Message = result.Message;
                                rs.Success = false;
                            }
                        }
                        else
                        {
                            rs.Message = Resource.MANIFEST_BOL_SUBMIT_STATUS_ERROR;
                        }
                    }
                    else if (bolInfo.Content.Status == BolStatus.Review)
                    {
                        if (manifestInfo.Content.Status == ManifestStatus.Open)
                        {
                            var result = this.ApproveBol(bolInfo.Content);
                            rs.Success = result.Success;
                            if (result.Success)
                            {
                                bolInfo.Content.Status = BolStatus.Open;
                                bolInfo.Content.StatusName = bolInfo.Content.Status.ToString();
                                rs.Content = bolInfo.Content;
                            }
                        }
                        else
                        {
                            rs.Message = Resource.MANIFEST_BOL_APPROVE_STATUS_ERROR;
                        }
                    }
                    else
                    {
                        rs.Success = true;
                        rs.Message = Resource.MANIFEST_STATUS_NOACTION;
                    }
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;
                }

            }
            else
            {

                rs.Message = Resource.MANIFEST_NOT_FIND_BOL_DATA;

            }
            return rs;
        }
        public IActionResult<IBolModel> RejectBOL(Guid boluid, string ModifiedBy)
        {
            var rs = ActionResultTemplates.Result<IBolModel>();
            var bolInfo = this._agent.DataModules.BolRepository.GetBol(new { UID = boluid });

            if (bolInfo.Success && bolInfo.Content != null)
            {
                var vesselInfo = this._agent.DataModules.VesselRepository.GetList(new { BOLUID = bolInfo.Content.UID });
                var workorderInfo = this._agent.DataModules.WorkOrderRepository.GetList(new { VesselUID = vesselInfo.Content.Select(p => p.UID) });

                try
                {
                    if (new int[] { (int)BolStatus.Review, (int)BolStatus.Open }.Any(
                        p => p == (int)bolInfo.Content.Status))
                    {
                        if (bolInfo.Content.Status == BolStatus.Open)
                        {
                            var ticketInfos = this._agent.DataModules.TicketInfoRepository.GetDataByBol(bolInfo.Content.UID);
                            if (ticketInfos.Success && ticketInfos.Content.Count() > 0)
                            {
                                if (ticketInfos.Content.Any(x => x.Status > (int)TicketInfoStatus.Open))
                                {
                                    rs.Success = false;
                                    rs.Message = Resource.MANIFEST_BOL_REJECT_FAILURE_TICKET_WORKING;
                                    return rs;
                                }

                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = Resource.MANIFEST_NOT_FIND_TICKETINFO; ;
                            }
                        }
                        var Result = new List<IActionResult<bool>>();
                        //bol stataus change to reject
                        Result.Add(this._agent.Bol
                            .ChangeBolStatus(bolInfo.Content.UID, BolStatus.Reject));
                        //vessel status change to draft
                        //vessel manifest change to draft
                        Result.Add(this._agent.Vessel.ChangeVesselByBol(bolInfo.Content.UID, VesselStatus.Open));
                        Result.Add(this._agent.Vessel.ChangeVesselManifestStatusByBOL(bolInfo.Content.UID, VesselManifestStatus.Draft));
                        //workorder change to draft
                        //workorder pod change to draft
                        //workorder payload change to draft
                        foreach (var item in workorderInfo.Content)
                        {
                            Result.Add(this._agent.WorkOrder.ChangeAllWorkOrderStatus(item.UID, WorkOrderStatus.Draft,
                                WorkOrderPodStatus.Draft, WorkOrderPayloadStatus.WaitingForProcessing));
                        }
                        //void ticket
                        var param = new VoidTicketInnerParameters();
                        param.BolUID = boluid;
                        param.ModifiedBy = ModifiedBy;
                        Result.Add(this._agent.DataModules.TicketManager.VoidTicket(param));
                        if (!Result.All(x => x.Success))
                        {
                            rs.Message = string.Join(",", Result.Select(x => x.Message));
                        }
                        else
                        {
                            bolInfo.Content.Status = BolStatus.Reject;
                            bolInfo.Content.StatusName = bolInfo.Content.Status.ToString();
                            rs.Content = bolInfo.Content;
                            rs.Success = true;
                        }
                    }
                    else
                    {
                        rs.Message = Resource.MANIFEST_BOL_STATUS_MUST_IN_REVIEW;
                    }
                }
                catch (Exception ex)
                {
                    rs.Success = false;
                    rs.Message = ex.Message;
                }

            }
            else
            {
                rs.Message = Resource.MANIFEST_NOT_FIND_BOL_DATA;
            }
            return rs;
        }
        public IActionResult<IManifestViewModel> RejectManifest(Guid manifestUID)
        {
            var rs = ActionResultTemplates.Result<IManifestViewModel>();
            var manifestInfo = this._agent.DataModules.ManifestRepository.GetInfo(manifestUID);
            if (manifestInfo.Success)
            {
                //var manifestItems = this._agent.DataModules.ManifestItemRepository.GetManifestItemList(manifestInfo.Content.UID);
                //if (manifestItems.Success)
                //{
                BolSearchInnerParameters parm = new BolSearchInnerParameters();
                parm.ManifestUID = manifestInfo.Content.UID;
                var bolInfos = this._agent.DataModules.BolRepository.GetList(parm);
                if (bolInfos.Success)
                {
                    if (bolInfos.Content.Count() > 0)
                    {
                        //BOL狀態 需在Review之前
                        if (!bolInfos.Content.All(p => (int)p.Status < (int)BolStatus.Review))
                        {
                            rs.Message = string.Format(Resource.MANIFEST_BOL_STATUS_MUST_LESS_THAN, BolStatus.Review);
                            return rs;
                        }

                    }

                    //modified manifest status
                    var rs1 = this._agent.DataModules.ManifestRepository
                        .ChangeManifestStatus(manifestUID, ManifestStatus.Reject);
                    //modified manifest item status
                    var rs2 = this._agent.DataModules.ManifestItemRepository
                        .ChangeManifestStatus(manifestUID, ManifestItemListStatus.Reject);
                    if (rs1.Success && rs2.Success)
                    {
                        ManifestInnerViewModel e = new ManifestInnerViewModel(manifestInfo.Content);
                        e.Status = ManifestStatus.Reject;
                        e.StatusName = e.Status.ToString();
                        rs.Success = true;
                        rs.Content = e;
                    }
                    else
                    {
                        rs.Message = rs1.Message + " " + rs2.Message;
                    }

                }
                else
                {
                    rs.Message = Resource.MANIFEST_NOT_FIND_BOL_DATA;
                }
                //}
                //else
                //{
                //    rs.Message = manifestItems.Message;
                //}
            }
            else
            {
                rs.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;
            }
            return rs;
        }
        public IActionResult<bool> RollbackBOL(Guid boluid, string ModifiedBy)
        {
            TransactionScope scope = null;
            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            var bolInfo = this._agent.DataModules.BolRepository.GetBol(new { UID = boluid });

            if (bolInfo.Success && bolInfo.Content != null)
            {
                var manifestInfo = this._agent.DataModules.ManifestRepository.GetData(new { UID = bolInfo.Content.ManifestUID });
                if (manifestInfo.Content.Type == (int)ManifestType.InventoryCounting)
                {
                    rs.Success = false;
                    rs.Message = string.Format
                        (Resource.COMMON_NOT_SUPPORT, ManifestType.InventoryCounting);

                }
                if (rs.Success)
                {
                    var vesselInfo = this._agent.DataModules.VesselRepository.GetList(new { BOLUID = bolInfo.Content.UID });
                    var workorderInfo = this._agent.DataModules.WorkOrderRepository.GetList(
                        new { VesselUID = vesselInfo.Content.Select(p => p.UID) });


                    try
                    {
                        if ((int)bolInfo.Content.Status > (int)BolStatus.Open)
                        {
                            //Inbound rollback 時需判斷payload 是否被使用
                            if (manifestInfo.Content.Type == (int)ManifestType.Inbound)
                            {
                                var wPayload = this._agent.DataModules.WorkOrderPayloadRepository
                                                .GetList(new { WorkOrderUID = workorderInfo.Content.Select(x => x.UID) });
                                var payloadInfo = this._agent.DataModules.InventoryManager
                                                .GetPayload(wPayload.Content.Select(x => x.PayloadUID));
                                //比對w.payload & payload count 
                                if (wPayload.Content.Count() == payloadInfo.Content.Count())
                                {
                                    rs.Success = wPayload.Content.Sum(x => x.Qty) == payloadInfo.Content.Sum(p => p.Quantity);
                                    if (!rs.Success)
                                    {
                                        rs.Message = Resource.MANIFEST_ORDER_INBOUND_ROLLBACKTICKET_USED;
                                    }
                                }
                                else
                                {
                                    rs.Success = false;
                                    rs.Message = Resource.MANIFEST_ORDER_INBOUND_ROLLBACKTICKET_USED;
                                }
                            }
                            if (rs.Success)
                            {
                                var ticketResult = this._agent.DataModules.TicketRepository
                                    .GetList(new { WorkOrderUID = workorderInfo.Content.Select(p => p.UID) });
                                var Result = new List<IActionResult<bool>>();
                                //bol stataus change to open
                                Result.Add(this._agent.Bol
                                    .ChangeBolStatus(bolInfo.Content.UID, BolStatus.Open));
                                //check all bol is open
                                var Allbol = this._agent.DataModules
                                    .BolRepository.GetList(new { ManifestUID = bolInfo.Content.ManifestUID });
                                if (Allbol.Success)
                                {
                                    if (Allbol.Content.All(p => p.Status == BolStatus.Open))
                                    {
                                        Result.Add(this._agent.Manifest
                                            .ChangeManifestStatus(bolInfo.Content.ManifestUID,
                                            ManifestStatus.Open, ManifestItemListStatus.Open));
                                    }
                                }
                                //manifest status to open
                                Result.Add(this._agent.Manifest.ChangeManifestStatus(bolInfo.Content.ManifestUID, ManifestStatus.Open));
                                // mainifest item status to open
                                Result.Add(this._agent.Manifest.ChangeManifestItemStatusByBol(bolInfo.Content.UID, ManifestItemListStatus.Open));
                                //vessel status change to open
                                //vessel manifest change to open
                                Result.Add(this._agent.Vessel.ChangeVesselByBol(bolInfo.Content.UID, VesselStatus.Open));
                                Result.Add(this._agent.Vessel.ChangeVesselManifestStatusByBOL(
                                    bolInfo.Content.UID, VesselManifestStatus.Open));
                                //workorder change to draft
                                //workorder pod change to draft
                                //workorder payload change to draft
                                foreach (var item in workorderInfo.Content)
                                {
                                    Result.Add(this._agent.WorkOrder.ChangeAllWorkOrderStatus(item.UID, WorkOrderStatus.Open,
                                        WorkOrderPodStatus.Open, WorkOrderPayloadStatus.WaitingForProcessing));
                                }
                                //rollback ticket
                                Result.Add(this._agent.Ticket.RollbackTicket(manifestInfo.Content.WarehouseUID, ticketResult.Content));


                                //this._agent.DataModules.TicketManager
                                //var param = new VoidTicketInnerParameters();
                                //param.BolUID = boluid;
                                //param.ModifiedBy = ModifiedBy;
                                //Result.Add(this._agent.DataModules.TicketManager.VoidTicket(param));

                                if (!Result.All(x => x.Success))
                                {
                                    rs.Message = string.Join(",", Result.Select(x => x.Message));
                                    rs.Success = false;
                                }
                                else
                                {
                                    bolInfo.Content.Status = BolStatus.Open;
                                    bolInfo.Content.StatusName = bolInfo.Content.Status.ToString();
                                    rs.Content = true;
                                    rs.Success = true;

                                }
                            }
                        }
                        else
                        {
                            rs.Content = false;
                            rs.Success = true;
                        }

                    }
                    catch (Exception ex)
                    {
                        rs.Success = false;
                        rs.Message = ex.Message;

                    }
                }
            }
            else
            {
                rs.Content = false;
                rs.Message = Resource.MANIFEST_NOT_FIND_BOL_DATA;
            }
            return rs;
        }

        public List<Func<IActionResult<bool>>> CheckAllManifestStatus(
            Func<IActionResult<IEnumerable<IStatusCheckModel>>> _statusModelAction,
            string modifiedBy)
        {

            //CheckStatus [Manifest,ManifestItem, BOL, Vessel, VesselManifest , WorkOrder, WorkOrderPod,WorkOrderPayload,Payload, Label, Ticket, TicketInfo]
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
            List<Func<IActionResult<bool>>> _action = new List<Func<IActionResult<bool>>>();

            IEnumerable<IStatusCheckModel> _status = _statusModelAction.Invoke().Content;
            if (_status != null)
            {
                var _checkTicket = _status.GroupBy(p => p.TicketUID);
                #region check Ticket info status -> ticket status
                foreach (var item in _checkTicket)
                {
                    var _ticketStatus = TicketStatus.Complete;
                    //var _bulkPickStatus = BulkPickStatus.Open;
                    if (item.All(p => p.TicketInfoStatus == (int)TicketInfoStatus.Complete))
                    {

                        item.ToList().ForEach(p => p.TicketStatus = (int)_ticketStatus);
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
                }
                #endregion
                var _checkWorkorderPayload = _status.GroupBy(p => p.WorkOrderPayloadUID);

                #region check ticket status -> workorder payload status
                foreach (var item in _checkWorkorderPayload)
                {
                    var _wstatus = WorkOrderPayloadStatus.Active;

                    if (item.All(p => p.TicketStatus == (int)TicketStatus.Complete))
                    {
                        item.ToList().ForEach(p => p.WorkOrderPayloadStatus = (int)_wstatus);
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
                    var _Status = WorkOrderPodStatus.Complete;

                    if (item.All(p => p.WorkOrderPayloadStatus == (int)WorkOrderPayloadStatus.Active))
                    {
                        item.ToList().ForEach(p => p.WorkOrderPodStatus = (int)_Status);
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

                    var _Status = WorkOrderStatus.Complete;

                    if (item.All(p => p.WorkOrderPayloadStatus == (int)WorkOrderPayloadStatus.Active))
                    {
                        item.ToList().ForEach(p => p.WorkOrderStatus = (int)_Status);
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
                    var _Status = VesselStatus.Complete;
                    var _iStatus = VesselManifestStatus.Complete;

                    if (item.All(p => p.WorkOrderStatus == (int)WorkOrderStatus.Complete))
                    {
                        if (item.Key != Guid.Empty)
                        {
                            item.ToList().ForEach(p => p.VesselStatus = (int)_Status);
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
                    var _Status = BolStatus.Complete;

                    if (_Status != BolStatus.Draft)
                    {
                        if (item.Key != Guid.Empty)
                        {
                            if (item.All(p => p.VesselStatus == (int)VesselStatus.Complete))
                            {
                                item.ToList().ForEach(p => p.BolStatus = (int)_Status);

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
                }
                #endregion
                var _checkManifest = _status.GroupBy(p => p.ManifestUID);
                #region check bol status -> manifest status
                foreach (var item in _checkManifest)
                {
                    var _Status = ManifestStatus.Open;
                    var _iStatus = ManifestItemListStatus.Open;
                    if (item.All(p => p.BolStatus == (int)BolStatus.Complete))
                    {
                        _Status = ManifestStatus.Complete;
                        _iStatus = ManifestItemListStatus.Complete;
                    }
                    this.TracingAgent.Trace($"trace manifest status change :{_Status} {_iStatus}");
                    _action.Add(() => this._agent.Manifest.ChangeManifestStatus(item.Key, _Status, _iStatus, modifiedBy));

                }
                #endregion

                //將收集的table key轉成修改狀態的action
                #region convert to Action
                #region Ticket
                foreach (var item in _ticketProcessList)
                {
                    _action.Add(() => this._agent.Ticket.ChangeTicketStatus(item.Value, item.Key,
                        modifiedBy));
                }
                #endregion
                #region Workorder Payload
                foreach (var item in _wpayloadProcessList)
                {
                    _action.Add(() => this._agent.WorkOrder.ChangeWorkOrderPayloadStatus(item.Value, item.Key,
                        modifiedBy));
                }
                #endregion
                #region Workorder Pod
                foreach (var item in _wpodProcessList)
                {
                    _action.Add(() => this._agent.WorkOrder.ChangeWorkOrderPodStatus(item.Value, item.Key,
                        modifiedBy));
                }
                #endregion
                #region Workorder 
                foreach (var item in _workorderProcessList)
                {
                    _action.Add(() => this._agent.WorkOrder.ChangeWorkOrderStatus(item.Value, item.Key,
                                    modifiedBy));
                }
                #endregion
                //#region Workorder /Workorder Payload/ Workorder Pod
                //foreach (var item in _workorderProcessList)
                //{
                //    _action.Add(() => this._agent.WorkOrder.ChangeAllWorkOrderStatus(item.Value,
                //        item.Key, WorkOrderPodStatus.Complete, WorkOrderPayloadStatus.Active));
                //}
                //#endregion
                #region Vessel
                foreach (var item in _vesselProcessList)
                {
                    _action.Add(() => this._agent.Vessel.BatchChangeVesselStatus(item.Value, item.Key,
                                    modifiedBy));
                }
                foreach (var item in _vesselManifestProcessList)
                {
                    _action.Add(() => this._agent.Vessel.BatchChangeVesselManiestStatus(item.Value, item.Key,
                                    modifiedBy));
                }
                #endregion
                #region BOL
                foreach (var item in _bolProcessList)
                {
                    _action.Add(() => this._agent.Bol.ChangeBolStatus(item.Value, item.Key,
                                    modifiedBy));
                }
                #endregion
                #endregion
            }
            return _action;
        }
    }
}
