using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundWorkOrderAssignAgent : AbstractWorkOrderAssignAgent
    {
        WorkOrderResult workOrderResult;
        protected override bool BeforeGenerateCheckData(IAssignedWorkOrderCollection parameters, ref List<string> errors)
        {
            errors = new List<string>();
            var groups = parameters.Items.GroupBy(p => p.VesselMainifestUID);
            var vesselManifestInfo = this.Managers.VesselManifestRepository.GetList(new { UID = groups.Select(p => p.Key) });
            var workorderPayloadInfo = this.Managers.WorkOrderPayloadRepository.GetList(new { VesselManifestUID = groups.Select(p => p.Key) });
            if (vesselManifestInfo.Success)
            {
                foreach (var item in groups)
                {
                    var vf = vesselManifestInfo.Content.FirstOrDefault(p => p.UID == item.Key);
                    var wf = workorderPayloadInfo.Content.Where(p => p.VesselManifestUID == item.Key && p.Type != (int)WorkOrderPayloadType.FutureAllocated);
                    if (vf != null)
                    {
                        //manifestItem Package 與 VesselManifest Package 相同
                        var allocatedQty = item.Sum(p => p.ReceivePackageQty) + wf.Sum(p => p.Qty);
                        if (vf.Qty < allocatedQty)
                        {
                            errors.Add(string.Format(Resource.MANIFEST_OUTBOUND_ALLOCATED_QTY_OVER, vf.Name));
                        }
                    }
                }
                return base.BeforeGenerateCheckData(parameters, ref errors);
            }
            else
            {
                return false;
            }
        }
        public OutboundWorkOrderAssignAgent(IWorkOrderAssignAgentParameters Parameters)
            : base(Parameters)
        {

        }
        public override IActionResult<WorkOrderResult> Execute(IAssignedWorkOrderCollection Parameters,
            AbstractAutoAssignedParameters parameters = null)
        {
            workOrderResult = new WorkOrderResult();
            //  var rs = this.GetExtensionActionResultContainer<bool>();
            IActionResult<ISlotModel> _assignedslotModel = null;
            IManifestModel maninfestInfo = null;
            WorkOrderAssignAgentExecuteParameters _wparameters = new WorkOrderAssignAgentExecuteParameters();
            this.ActionResult.Success = true;
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            this._workorder = new WorkOrderInnerModel();
            this.workOrderPayloadModelList = new List<IWorkOrderPayloadModel>();
            this.podlabels = new List<ILabelModel>();
            List<string> Errors = new List<string>();
            //if (BeforeGenerateCheckData(Parameters, ref Errors))
            //{
            var _workorderStatus = (int)WorkOrderStatus.Draft;
            var _workorderPayloadStatus = (int)WorkOrderPayloadStatus.WaitingForProcessing;
            if (parameters != null && (parameters as OutboundAutoAssignedParameters).ForceWorkOrderOpen)
            {
                _workorderStatus = (int)WorkOrderStatus.Open;
                // _workorderPayloadStatus = (int)WorkOrderPayloadStatus.WaitingForProcessing;
            }
            if (parameters == null)
                maninfestInfo = this.GetManifest(Parameters.VesselUID);
            else
                maninfestInfo = parameters.Manifest;
            if (maninfestInfo != null)
            {
                //get workorder uid

                if (parameters != null)
                {
                    // add workorder
                    //var _workOrderSeq = this.Managers.SequenceAgent.GetWorkOrderSeqence(Parameters.VesselUID);
                    var _workOrderSeq = this.Managers.SequenceAgent.GetWorkOrderSeqenceByTimeSerial(ManifestType.Outbound);
                    _workorder.UID = Guid.NewGuid();
                    _workorder.ID = _workOrderSeq;
                    _workorder.ManifestUID = maninfestInfo.UID;
                    _workorder.VesselUID = Parameters.VesselUID;
                    _workorder.Status = _workorderStatus;
                    _workorder.Type = 1;
                    _workorder.CreatedBy = this.Managers.AuthenticationInfo.Account;
                    _workorder.CreatedOn = DateTime.UtcNow;
                    this.WorkOrderUID = _workorder.UID;

                    //_action.Push(() => this.Managers.WorkOrderRepository.AddWorkOrder(_workorder));
                }
                else
                {
                    var _workorderUID = this.Managers.WorkOrderRepository.GetWorkOrderUID(Parameters.VesselUID);
                    this.WorkOrderUID = _workorderUID.Content.Value;
                }

                //set default dummyslot 
                var outboundparam = parameters as OutboundAutoAssignedParameters;
                if (outboundparam != null)
                {
                    if (outboundparam.LandingSlot.Success)
                    {
                        _assignedslotModel = outboundparam.LandingSlot;
                    }
                    else
                    {
                        if (outboundparam.OutboundRequest.UsePackingStation)
                        {
                            if (this.slotModelByPackignStation == null)
                            {
                                this.slotModelByPackignStation = this.Managers.warehouseManger.GetDefaultLandingZone(
                                maninfestInfo.WarehouseUID, SlotType.StagingArea_Parcel);
                            }
                            _assignedslotModel = this.slotModelByPackignStation;
                        }
                        else
                        {
                            if (this.slotModel == null)
                            {
                                this.slotModel = this.Managers.warehouseManger.GetDummySlot(maninfestInfo.WarehouseUID);
                            }
                            _assignedslotModel = this.slotModel;
                        }
                    }
                }
                else
                {
                    if (this.slotModel == null)
                    {
                        this.slotModel = this.Managers.warehouseManger.GetDummySlot(maninfestInfo.WarehouseUID);
                    }
                    _assignedslotModel = this.slotModel;
                }
                var _workorderPodUID = Guid.NewGuid();
                #region add workorder payload
                foreach (var item in Parameters.Items)
                {

                    IEnumerable<IVesselManifestModel> vesselInfo = null;
                    if (parameters == null)
                        vesselInfo = this.Managers.VesselManifestRepository
                            .GetList(new { UID = item.VesselMainifestUID }).Content;
                    else
                        vesselInfo = parameters.VesselItems.Where(p => p.UID == item.VesselMainifestUID);
                    var e = new WorkOrderPayloadInnerModel();
                    var payloadInfo = item.OnhandPayloadItems.FirstOrDefault(p => p.UID == item.PayloadUID.Value);
                    if (payloadInfo != null)
                    {
                        var _pkg = this.Managers.PackageCacheManager.GetPackage(payloadInfo.PackageUID);
                        var _workOrderPayloadSeq = this.Managers.SequenceAgent.GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType.Outbound);

                        e.UID = Guid.NewGuid();
                        e.ID =
                        e.Name = _workOrderPayloadSeq;
                        e.Type = (int)item.WorkorderPayloadType;
                        e.WorkOrderUID = this.WorkOrderUID;
                        e.WorkOrderPodUID = _workorderPodUID;
                        e.PayloadUID = Guid.NewGuid();
                        e.AllocatedPayloadUID = item.PayloadUID.Value;
                        e.PayloadPackageUID = payloadInfo.PackageUID;
                        e.ItemUID = item.ItemUID;
                        e.ItemGroupUID = item.ItemGroupUID;
                        e.PackageUID = vesselInfo.First().PackageUID;
                        e.Qty = item.ReceivePackageQty;
                        e.VesselManifestUID = item.VesselMainifestUID;
                        e.WorkOrderPodUID = _workorderPodUID;
                        if (Parameters.LoadingZoneSlotUID.HasValue)
                        {
                            e.LoadingZoneSlotUID = Parameters.LoadingZoneSlotUID.Value;
                        }
                        else
                        {
                            if (_assignedslotModel.Success)
                            {
                                e.LoadingZoneSlotUID = _assignedslotModel.Content.UID;
                            }
                        }
                        e.Status = _workorderPayloadStatus;
                        if (_pkg != null)
                        {
                            e.Weight = this.ProductManager.CaculateTTLWeight(_pkg, e.Qty);
                            e.Volume = ProductManager.CalculateCUFT(_pkg, e.Qty);
                        }
                        e.CreatedBy = this.Managers.AuthenticationInfo.Account;
                        e.CreatedOn = DateTime.UtcNow;
                        e.SlotUID = payloadInfo.SlotUID;

                        this.workOrderPayloadModelList.Add(e);
                        //split  payload data
                    }
                    else if (item.PayloadType == PayloadType.FutureAllocated)
                    {
                        var _workOrderPayloadSeq = this.Managers.SequenceAgent.GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType.Outbound);
                        e.UID = Guid.NewGuid();
                        e.ID =
                        e.Name = _workOrderPayloadSeq;
                        e.Type = (int)item.WorkorderPayloadType;
                        e.WorkOrderUID = _workorder.UID;
                        e.WorkOrderPodUID = _workorderPodUID;
                        e.PayloadUID = item.PayloadUID.Value;
                        e.ItemUID = item.ItemUID;
                        e.ItemGroupUID = item.ItemGroupUID;
                        e.PackageUID = vesselInfo.First().PackageUID;
                        e.PayloadPackageUID = item.ReceivePackageUID;
                        e.Qty = item.ReceivePackageQty;
                        e.VesselManifestUID = item.VesselMainifestUID;
                        if (Parameters.LoadingZoneSlotUID.HasValue)
                        {
                            e.LoadingZoneSlotUID = Parameters.LoadingZoneSlotUID.Value;
                        }
                        else
                        {
                            if (_assignedslotModel.Success)
                            {
                                e.LoadingZoneSlotUID = _assignedslotModel.Content.UID;
                            }
                        }
                        e.Status = _workorderPayloadStatus;
                        e.Weight = 0;
                        e.Volume = 0;
                        e.CreatedBy = this.Managers.AuthenticationInfo.Account;
                        e.CreatedOn = DateTime.UtcNow;
                        e.SlotUID = item.SlotUID;
                        this.workOrderPayloadModelList.Add(e);
                    }
                    else
                    {
                        this.ActionResult.Success = false;
                        this.ActionResult.Message += Resource.MANIFEST_WORKORDER_NOT_FIND_VESSELMANIFST;
                        //找不到Vesselmanifest data ?
                    }
                    WorkorderAssignedResult war = new WorkorderAssignedResult();
                    war.VesselManifestUID = item.VesselMainifestUID;
                    war.WorkorderPayloadUID = e.UID;
                    war.WorkorderPodUID = _workorderPodUID;
                    war.Quantity = e.Qty;
                    workOrderResult.WorkorderAssignedResults.Add(war);
                }
                #endregion
                #region add workorder pod
                WorkorderPod = new WorkOrderPodInnerModel();
                var workOrderPodSeq = this.Managers.SequenceAgent.GetWorkOrderPodSeqenceByTimeSerial(ManifestType.Outbound);
                WorkorderPod.PodUID = Guid.NewGuid();
                WorkorderPod.UID = _workorderPodUID;
                WorkorderPod.ID = workOrderPodSeq;
                WorkorderPod.Name = Parameters.ExternalBarcode;
                WorkorderPod.Type = _workorder.Type;
                WorkorderPod.WorkOrderUID = this.WorkOrderUID;
                WorkorderPod.Status = (int)WorkOrderPodStatus.Open;
                WorkorderPod.CreatedBy = this.Managers.AuthenticationInfo.Account;
                WorkorderPod.CreatedOn = DateTime.UtcNow;
                WorkorderPod.Weight = this.workOrderPayloadModelList.Sum(p => p.Weight);
                WorkorderPod.Volume = this.workOrderPayloadModelList.Sum(p => p.Volume);
                #endregion
                #region  & execute work order pod mapping pallet barcode
                if (outboundparam != null &&
                    outboundparam.OutboundRequest.OrderType == (int)OrderType.Truckload)
                {
                    LabelInnerModel podqtylabel = new LabelInnerModel
                    {
                        Type = LabelType.Pallet_Self,
                        UID = Guid.NewGuid(),
                        BelongToType = LabelBelongType.Pod,
                        BelongToUID = WorkorderPod.PodUID.Value,
                        Content = Parameters.ExternalBarcode,
                        Status = (int)LabelStatus.Active,
                        FileUID = Guid.Empty
                    };
                    this.podlabels.Add(podqtylabel);
                }
                #endregion
                _wparameters.OnhandPayloadItems.AddRange(Parameters.Items.SelectMany(p => p.OnhandPayloadItems));
                //_action.Push(() => this.Managers.WorkOrderPayloadRepository.AddPayload(this.workOrderPayloadModelList));
                //_action.Push(() => this.Managers.WorkOrderPodRepository.AddWorkOrderPod(workorderPod));
                //if (outboundparam.OutboundRequest.OrderType == (int)OrderType.Truckload)
                //{
                //    _action.Push(() => this.Managers.WorkOrderManager.SetWorkOrderPodBarcode(workorderPod,
                //                        Parameters.ExternalBarcode, false));
                //}

            }
            else
            {
                Errors.Add("not find manifest data.");
            }

            _wparameters.WorkOrder = _workorder;
            _wparameters.WorkOrderPayload = this.workOrderPayloadModelList;
            //}
            //else
            //{
            //    this.ActionResult.Success = false;
            //    this.ActionResult.Message = string.Join(",", Errors.ToArray());
            //}

            if (this.ActionResult.Success)
            {
                return this.InnerExecute(_action, _wparameters);
            }
            else
            {
                return this.ActionResult;
            }
        }
        protected override IActionResult<bool> BeforeExecuteCheckData(IWorkOrderAssignAgentExecuteParameters parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            //調整成FullAllocated 已經全部Allocated 不需再檢查
            //try
            //{
            //List<string> _result = new List<string>();
            ////orkOrderPayloadType.FutureAllocated 的時候不用檢查庫存是否存夠，直接跳過即可
            //var _itemGroup = parameters.WorkOrderPayload.Where(p => p.Type != (int)WorkOrderPayloadType.FutureAllocated).GroupBy(p => p.ItemUID);
            //foreach (var item in _itemGroup)
            //{
            //    var payloadInfo = this.Managers.InventoryManager.GetPayload(item.Select(p => p.PayloadUID).ToArray(), PayloadType.Stock);
            //    if (payloadInfo.Success && payloadInfo.Content.Count() > 0)
            //    {
            //        var _plTTLqty = payloadInfo.Content.Sum(p =>
            //            this.Managers.PackageCacheManager.GetReceivePackageUomQuantity(p.PackageUID,
            //            this.Managers.PackageCacheManager.GetMinPackage(p.PackageUID).UID,
            //             p.Quantity).Content);
            //        var _allocatedqty = item.Sum(p =>
            //            this.Managers.PackageCacheManager.GetReceivePackageUomQuantity(p.PackageUID,
            //             this.Managers.PackageCacheManager.GetMinPackage(p.PackageUID).UID,
            //            p.Qty).Content);
            //        if (_allocatedqty > _plTTLqty) //insufficient
            //        {
            //            var _p = this.Managers.ProductCacheManager.GetItem(item.Key);
            //            _result.Add(string.Format(Resource.MANIFEST_WORKORDER_OUTBOUND_INSUFFICIENTQTY,
            //                        _p.Name, _plTTLqty, _allocatedqty));
            //        }
            //    }
            //    else
            //    {
            //        _result.Add(Resource.MANIFEST_WORKORDER_NOT_FIND_PAYLOAD);
            //    }
            //}
            //if (_result.Count == 0)
            //{
            //    rs.Content = rs.Success = true;
            //}
            //else
            //{
            //    rs.Content = rs.Success = false;
            //    rs.Message = string.Join(",", _result);
            //}
            //}
            //catch (Exception ex)
            //{
            //    rs.Message = ex.Message;
            //    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
            //    rs.Success = false;
            //    rs.InnerException = ex;
            //}
            return rs;

        }
        protected override IActionResult<WorkOrderResult> InnerExecute(ConcurrentStack<Func<IActionResult<bool>>> Actions,
            IWorkOrderAssignAgentExecuteParameters parameters)
        {

            var _rs = this.BeforeExecuteCheckData(parameters);
            if (_rs.Success)
            {
                var param = new AllocateExecutorParameters
                {
                    InventoryManager = this.Managers.InventoryManager,
                    ProductUtility = this.ProductManager,
                    WorkOrderPayloadRepository = this.Managers.WorkOrderPayloadRepository,
                    LabelManager = this.Managers.LabelManager,
                    PackageMappingCache = this.Managers.PackageCacheManager,
                    SequenceAgent = this.Managers.SequenceAgent,
                    TracingAgent = this.Managers.TracingAgent
                };
                var executor = new FullAllocatedExecutor(param);
                //split payload
                var actionResult = executor.GetAllcoateExecuteResult(parameters.WorkOrderPayload, parameters.OnhandPayloadItems);
                if (actionResult.Success)
                {

                    workOrderResult.WorkOrderUID = this.WorkOrderUID;
                    workOrderResult.WorkOrderPayload = this.workOrderPayloadModelList;
                    workOrderResult.WorkorderPod = this.WorkorderPod;
                    workOrderResult.Workorder = this._workorder;
                    workOrderResult.PodLabels = this.podlabels;
                    workOrderResult.AllocatedExecutorResult = actionResult.Content;

                    this.ActionResult.Content = workOrderResult;
                    this.ActionResult.Success = actionResult.Success;
                    this.ActionResult.Message = actionResult.Message;
                }
                else
                {
                    this.ActionResult.Success = actionResult.Success;
                    this.ActionResult.Message = actionResult.Message;
                }

            }
            return this.ActionResult;
        }

    }
}
