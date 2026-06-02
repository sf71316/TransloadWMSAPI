using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using System.Transactions;
using System.Collections.Concurrent;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractWorkOrderAssignAgent
    {
        protected Guid WorkOrderUID { get; set; }
        protected IWorkOrderAssignAgentParameters Managers;
        protected IActionResult<WorkOrderResult> ActionResult;
        internal ProductUtility ProductManager;
        protected IActionResult<ISlotModel> slotModel;
        protected IActionResult<ISlotModel> slotModelByPackignStation;
        public List<IWorkOrderPayloadModel> workOrderPayloadModelList;
        public List<ILabelModel> podlabels;
        public WorkOrderPodInnerModel WorkorderPod;
        public IWorkOrderModel _workorder;
        protected Queue<string> WorkOrderPodSeq { get; set; }
        protected Queue<string> WorkOrderPayloadSeq { get; set; }
        public void ImportSequence(Queue<string> workOrderPodSeq, Queue<string> workOrderPayloadSeq)
        {
            WorkOrderPodSeq = workOrderPodSeq;
            WorkOrderPayloadSeq = workOrderPayloadSeq;
        }
        public AbstractWorkOrderAssignAgent(IWorkOrderAssignAgentParameters Parameters)
        {
            Managers = Parameters;
            ActionResult = ActionResultTemplates.Result<WorkOrderResult>();
            this.ProductManager = new ProductUtility();
        }
        public virtual IActionResult<WorkOrderResult> Execute(IAssignedWorkOrderCollection Parameters,
            AbstractAutoAssignedParameters parameters = null)
        {
            IManifestModel maninfestInfo = null;
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            var _workorder = new WorkOrderInnerModel();
            var _workorderPod = new WorkOrderPodInnerModel();
            List<IWorkOrderPayloadModel> _workorderPayload = new List<IWorkOrderPayloadModel>();
            List<string> Errors = new List<string>();
            var _workorderStatus = (int)WorkOrderStatus.Draft;
            var _workorderPodStatus = (int)WorkOrderPodStatus.Draft;
            var _workorderPayloadStatus = (int)WorkOrderPayloadStatus.WaitingForProcessing;
            if ((parameters as InboundAutoAssignedParameters).ForceWorkOrderOpen)
            {
                _workorderStatus = (int)WorkOrderStatus.Open;
                _workorderPodStatus = (int)WorkOrderPodStatus.Open;
                //_workorderPayloadStatus = (int)WorkOrderPayloadStatus.Active;
            }
            if (BeforeGenerateCheckData(Parameters, ref Errors))
            {
                if (parameters == null)
                    maninfestInfo = this.GetManifest(Parameters.VesselUID);
                else
                    maninfestInfo = parameters.Manifest;
                if (maninfestInfo != null)
                {
                    //get workorder uid
                    var _workorderUID = this.Managers.WorkOrderRepository.GetWorkOrderUID(Parameters.VesselUID);
                    // if not uid -> add else get uid
                    if (_workorderUID.Success)
                    {

                        if (!_workorderUID.Content.HasValue)
                        {
                            // add workorder
                            var _workOrderSeq = this.Managers.SequenceAgent.GetWorkOrderSeqenceByTimeSerial(ManifestType.Inbound);
                            _workorder.UID = Guid.NewGuid();
                            _workorder.ID = _workOrderSeq;
                            _workorder.ManifestUID = maninfestInfo.UID;
                            _workorder.VesselUID = Parameters.VesselUID;
                            _workorder.Status = _workorderStatus;
                            _workorder.Type = 1;
                            _workorder.CreatedBy = this.Managers.AuthenticationInfo.Account;
                        }
                        else
                        {

                            _workorder.UID = _workorderUID.Content.Value;
                        }
                        this.WorkOrderUID = _workorder.UID;
                        string _workOrderPodSeq = "";
                        if (this.WorkOrderPodSeq == null)
                            _workOrderPodSeq = this.Managers.SequenceAgent.GetWorkOrderPodSeqenceByTimeSerial(ManifestType.Inbound);
                        else
                            _workOrderPodSeq = WorkOrderPodSeq.Dequeue();
                        _workorderPod.ID = _workOrderPodSeq;
                        _workorderPod.Name = _workOrderPodSeq;
                        _workorderPod.UID = Guid.NewGuid();
                        _workorderPod.OperationSuggestion = Parameters.OperationSuggestion;
                        if (Parameters.StorageMethod == (int)StorageMethod.NewPallet || Parameters.StorageMethod == (int)StorageMethod.RealPallet)
                        {
                            if (Parameters.StorageMethod == (int)StorageMethod.NewPallet)
                            {
                                _workorderPod.Type = (int)StorageMethod.NewPallet;
                                _workorderPod.PodUID = Guid.NewGuid();//新的Pod UID
                            }
                            else
                            {
                                _workorderPod.Type = (int)StorageMethod.RealPallet;
                                _workorderPod.PodUID = Parameters.PodUID;//現有的Pod UID
                            }
                        }
                        else
                        {
                            _workorderPod.Type = (int)StorageMethod.Slot;
                        }
                        _workorderPod.WorkOrderUID = _workorder.UID;

                        _workorderPod.Status = _workorderPodStatus;
                        _workorderPod.CreatedBy = this.Managers.AuthenticationInfo.Account;
                        //}
                        //else
                        //{
                        //    if (Parameters.StorageMethod == (int)StorageMethod.NewPallet)
                        //        _workorderPod.UID = Parameters.PodUID.Value;
                        //}
                        //set default landingzone 
                        var slotModel = this.Managers.warehouseManger
                            .GetDefaultLandingZone(maninfestInfo.WarehouseUID, SlotType.InboundTemp);
                        // add workorder payload
                        ItemInnerParameterize parm = new ItemInnerParameterize();
                        parm.ListOfItemUID.AddRange(Parameters.Items.Select(x => x.ItemUID));
                        var itemInfos = this.Managers.ItemManager.GetItems(parm);
                        if (itemInfos.Success)
                        {
                            Queue<string> _seqwp = null;
                            if (this.WorkOrderPayloadSeq == null)
                            {
                                _seqwp = this.Managers.SequenceAgent
                                            .GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType.Inbound, Parameters.Items.Count);
                            }
                            else
                            {
                                _seqwp = this.WorkOrderPayloadSeq;
                            }
                            foreach (var item in Parameters.Items)
                            {
                                var _pkg = this.Managers.PackageManager.GetPackage(item.ReceivePackageUID);
                                var _pkgUOM = this.Managers.PackageUomManager.GetPackageUom(_pkg.Content.UOM);
                                var itemInfo = itemInfos.Content.FirstOrDefault(p => p.UID == item.ItemUID);
                                var _workOrderPayloadSeq = _seqwp.Dequeue();
                                var e = new WorkOrderPayloadInnerModel();
                                e.UID = Guid.NewGuid();
                                e.ID =
                                e.Name = _workOrderPayloadSeq;
                                e.Type = 1;
                                e.WorkOrderUID = _workorder.UID;
                                e.WorkOrderPodUID = _workorderPod.UID;
                                if (Parameters.ServiceType == ManifestType.Inbound)
                                    e.PayloadUID = Guid.NewGuid();
                                else
                                    e.PayloadUID = item.PayloadUID.Value;
                                e.PayloadPackageUID = e.PackageUID;
                                e.ItemUID = item.ItemUID;
                                e.ItemGroupUID = item.ItemGroupUID;
                                e.PackageUID = item.ReceivePackageUID;
                                e.Qty = item.ReceivePackageQty;
                                e.VesselManifestUID = item.VesselMainifestUID;
                                e.Status = _workorderPayloadStatus;
                                //TODO assigned UPC/EAN Label
                                var labelAgentInitParameter = new LabelAgentInitParameter();
                                //labelAgentInitParameter.ItemManager = this.Managers.ItemManager;
                                labelAgentInitParameter.LabelManager = this.Managers.LabelManager;
                                labelAgentInitParameter.PackageCacheManager = this.Managers.PackageCacheManager;
                                labelAgentInitParameter.PackageUomManager = this.Managers.PackageUomManager;
                                labelAgentInitParameter.ProductCacheManager = this.Managers.ProductCacheManager;
                                var LabelAgent = new LabelAgent(labelAgentInitParameter);
                                _action.Push(() => LabelAgent.GenerateItemLabel(e.ItemUID, e.PackageUID, e.PayloadUID));

                                if (_pkg.Success)
                                {
                                    e.Weight = this.ProductManager.CaculateTTLWeight(_pkg.Content, e.Qty);
                                    e.Volume = ProductManager.CalculateCUFT(_pkg.Content, e.Qty);
                                }
                                e.CreatedBy = this.Managers.AuthenticationInfo.Account;

                                if (Parameters.StorageMethod == (int)StorageMethod.RealPallet)//Real Pallet
                                {
                                    if (Parameters.PodUID.HasValue && Parameters.PodUID.Value != Guid.Empty) // is exist pallet
                                    {
                                        //get pod belong to slot
                                        PodInSlotInnerModel _parameters = new PodInSlotInnerModel();
                                        _parameters.PodUID = Parameters.PodUID;
                                        var slot = this.Managers.warehouseManger.GetPodInSlot(_parameters);
                                        if (slot.Success)
                                        {
                                            e.SlotUID = slot.Content;
                                        }
                                    }
                                }
                                else if (Parameters.StorageMethod == (int)StorageMethod.Slot || Parameters.StorageMethod == (int)StorageMethod.NewPallet)//Slot
                                {
                                    e.SlotUID = Parameters.Items.First().SlotUID.Value;
                                }
                                if (Parameters.LoadingZoneSlotUID.HasValue)
                                {
                                    e.LoadingZoneSlotUID = Parameters.LoadingZoneSlotUID.Value;
                                }
                                else
                                {
                                    if (slotModel.Success)
                                    {
                                        e.LoadingZoneSlotUID = slotModel.Content.UID;
                                    }
                                }
                                _workorderPayload.Add(e);

                                //split  payload data
                            }
                            //sum pod ttl weight
                            _workorderPod.Weight = _workorderPayload.Sum(p => p.Weight);
                            _workorderPod.Volume = _workorderPayload.Sum(p => p.Volume);
                            //write data
                            if (!_workorderUID.Content.HasValue) //not have work order 
                            {
                                _action.Push(() => this.Managers.WorkOrderRepository.AddWorkOrder(_workorder));
                            }
                            if (_workorderPod.UID != Guid.Empty)
                            {
                                _action.Push(() => this.Managers.WorkOrderPodRepository.AddWorkOrderPod(_workorderPod));
                                if (Parameters.StorageMethod != (int)StorageMethod.RealPallet &&
                                    _workorderPod.PodUID.HasValue && _workorderPod.PodUID.Value != Guid.Empty)
                                {
                                    var model = new GeneratePalletLabelModel();
                                    if (parameters != null)
                                    {
                                        model.SysPon = maninfestInfo.RefNo;
                                        model.Notes = _workorderPod.OperationSuggestion;
                                        model.ContainerNo = parameters.Vessel.First().RefNo;
                                    }
                                    else
                                    {
                                        var vesselInfo = this.Managers.VesselRepository.GetData(new { UID = Parameters.VesselUID });
                                        model.SysPon = maninfestInfo.RefNo;
                                        model.Notes = _workorderPod.OperationSuggestion;
                                        model.ContainerNo = vesselInfo.Content.RefNo;
                                    }
                                    _action.Push(() =>
                                        this.Managers.WorkOrderManager.SetWorkOrderPodBarcode(_workorderPod.UID,
                                        Parameters.ExternalBarcode)
                                        );
                                }


                            }
                            //foreach (var item in _workorderPayload)
                            //{
                            //    // this._Module.WorkOrderPayloadRepository.AddPayload(item);
                            //    _action.Push(() => this.Managers.WorkOrderPayloadRepository.AddPayload(item));
                            //}
                            _action.Push(() => this.Managers.WorkOrderPayloadRepository.AddPayload(_workorderPayload));
                        }
                        else
                        {
                            Errors.Add(itemInfos.Message);
                        }
                    }
                }
                else
                {
                    Errors.Add(Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA);
                }
            }
            WorkOrderAssignAgentExecuteParameters _wparameters = new WorkOrderAssignAgentExecuteParameters();
            _wparameters.WorkOrder = _workorder;
            _wparameters.WorkOrderPod = _workorderPod;
            _wparameters.WorkOrderPayload = _workorderPayload;
            if (Errors.Count() == 0)
            {
                return this.InnerExecute(_action, _wparameters);
            }
            else
            {
                this.ActionResult.Success = false;
                this.ActionResult.Message = string.Join("<br>", Errors);
                return this.ActionResult;
            }
        }

        protected IManifestModel GetManifest(Guid vesselUID)
        {
            var g = this.Managers.WorkOrderRepository.GetManifestInfo(vesselUID);
            return g.Content;
        }

        protected virtual IActionResult<bool> BeforeExecuteCheckData(IWorkOrderAssignAgentExecuteParameters parameters)
        {
            return null;
        }
        protected virtual IActionResult<WorkOrderResult> InnerExecute(ConcurrentStack<Func<IActionResult<bool>>> Actions,
            IWorkOrderAssignAgentExecuteParameters parameters)
        {
            var crs = new WorkOrderResult();
            if (this.ActionResult.Success)
            {
                try
                {
                    List<IActionResult<bool>> _isAllcomplete = new List<IActionResult<bool>>();
                    var actions = Actions.Reverse();
                    foreach (var _action in actions)
                    {
                        if (_isAllcomplete.All(x => x.Success) || _isAllcomplete.Count == 0)
                        {
                            _isAllcomplete.Add(_action.Invoke());
                        }
                    }
                    crs.WorkOrderUID = this.WorkOrderUID;
                    ActionResult.Content = crs;
                    if (_isAllcomplete.All(p => p.Success))
                    {
                        //if (scope != null)
                        //    scope.Complete();
                        ActionResult.Success = true;
                    }
                    else
                    {
                        this.Managers.TracingAgent.Trace("execute workorder data sql error",
                            _isAllcomplete.Where(x => !x.Success));
                        ActionResult.Success = false;
                        ActionResult.Message = string.Join(",", _isAllcomplete.Where(x => !x.Success).Select(p => p.Message));
                    }

                }
                catch (Exception ex)
                {
                    this.Managers.TracingAgent.Trace("execute workorder data sql exception", ex);
                    ActionResult.Message = ex.Message;
                    ActionResult.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    ActionResult.Success = false;
                    ActionResult.InnerException = ex;
                }

            }
            return ActionResult;
        }
        public static AbstractWorkOrderAssignAgent GetAgent(ManifestType serviceItem, IWorkOrderAssignAgentParameters Parameters)
        {
            switch (serviceItem)
            {
                case ManifestType.Inbound:
                    return new InboundWorkOrderAssignAgent(Parameters);
                case ManifestType.Outbound:
                    return new OutboundWorkOrderAssignAgent(Parameters);
                case ManifestType.Move:
                    return new MoveWorkOrderAssignAgent(Parameters);
                case ManifestType.InventoryCounting:
                    return new InventoryCountingWorkOrderAssignAgent(Parameters);
                case ManifestType.BlukPick:
                    return new BulkPickOrderAssignAgent(Parameters);
                default:
                    break;
            }
            return null;
        }
        protected virtual bool BeforeGenerateCheckData(IAssignedWorkOrderCollection parameters, ref List<string> errors)
        {
            if (parameters.Items.Count() == 0)
            {
                errors.Add(Resource.MANIFEST_MUST_HAVE_ITEM);
            }
            //if (parameters.ManifestUID == Guid.Empty || parameters.ManifestUID == null)
            //{
            //    errors.Add($"{nameof(parameters.ManifestUID)} cannot empty.");
            //}
            if ((!new int[] { 1, 2, 3 }.Contains(parameters.StorageMethod)) &&
                parameters.ServiceType == ManifestType.Inbound)
            {
                errors.Add(string.Format(Resource.MANIFEST_WORKORDER_STOREAGE_METHOD_ERROR, nameof(parameters.StorageMethod)));
            }
            if (parameters.VesselUID == Guid.Empty || parameters.VesselUID == null)
            {
                errors.Add(string.Format(Resource.MANIFEST_CANNOT_EMPTY, nameof(parameters.VesselUID)));
            }
            if (!parameters.Items.All(p => p.ReceivePackageQty > 0))
            {
                errors.Add(string.Format(Resource.MANIFEST_CANNOT_EMPTY, Resource.COMMON_ITEM));
            }
            //TODO 檢查數量是否超過VesselManifest item qty
            return errors.Count == 0;
        }
        protected dynamic GeneratoreObject()
        {
            return new ExpandoObject();
        }

    }
}
