using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class MoveWorkOrderAssignAgent : AbstractWorkOrderAssignAgent
    {
        public MoveWorkOrderAssignAgent(IWorkOrderAssignAgentParameters Parameters)
            : base(Parameters)
        {

        }
        public override IActionResult<WorkOrderResult> Execute(IAssignedWorkOrderCollection Parameters,
            AbstractAutoAssignedParameters parameters = null)
        {
            //var _workorderStatus = (int)WorkOrderStatus.Draft;
            //var _workorderPodStatus = (int)WorkOrderPodStatus.Draft;
            //var _workorderPayloadStatus = (int)WorkOrderPayloadStatus.WaitingForProcessing;
            //if ((parameters as InboundAutoAssignedParameters).ForceWorkOrderOpen)
            //{
            //    _workorderStatus = (int)WorkOrderStatus.Open;
            //    _workorderPodStatus = (int)WorkOrderPodStatus.Open;
            //    _workorderPayloadStatus = (int)WorkOrderPayloadStatus.Active;
            //}
            IManifestModel maninfestInfo = null;
            WorkOrderAssignAgentExecuteParameters _wparameters = new WorkOrderAssignAgentExecuteParameters();
            this.ActionResult.Success = true;
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            var _workorder = this.GeneratoreObject();
            List<IWorkOrderPayloadModel> _workorderPayload = new List<IWorkOrderPayloadModel>();
            List<string> Errors = new List<string>();
            // add workorder
            var _workOrderSeq = this.Managers.SequenceAgent.GetWorkOrderSeqenceByTimeSerial(ManifestType.Move);
            _workorder.UID = Guid.NewGuid();
            _workorder.ID = _workOrderSeq;
            _workorder.ManifestUID = Guid.Empty;
            _workorder.VesselUID = Guid.Empty;
            _workorder.Status = (int)WorkOrderStatus.Open;
            _workorder.Type = 1;
            _workorder.CreatedBy = this.Managers.AuthenticationInfo.Account;
            this.WorkOrderUID = _workorder.UID;
            //set default dummyslot 
            // add workorder payload
            foreach (var item in Parameters.Items)
            {
                var payloadInfo = this.Managers.InventoryManager.GetPayload(item.PayloadUID.Value);
                if (payloadInfo.Success)
                {
                    var _pkg = this.Managers.PackageCacheManager.GetPackage(payloadInfo.Content.PackageUID);
                    var _workOrderPayloadSeq = this.Managers.SequenceAgent.GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType.Move);
                    var e = new WorkOrderPayloadInnerModel();
                    e.UID = Guid.NewGuid();
                    e.ID =
                    e.Name = _workOrderPayloadSeq;
                    e.Type = 1;
                    e.WorkOrderUID = _workorder.UID;
                    e.WorkOrderPodUID = Guid.Empty;
                    e.PayloadUID = item.PayloadUID.Value;
                    e.PayloadPackageUID = payloadInfo.Content.PackageUID;
                    e.ItemUID = item.ItemUID;
                    e.ItemGroupUID = item.ItemGroupUID;
                    e.PackageUID = item.ReceivePackageUID;
                    e.Qty = item.ReceivePackageQty;
                    e.VesselManifestUID = item.VesselMainifestUID;
                    e.LoadingZoneSlotUID = item.SlotUID.Value;
                    e.Status = (int)WorkOrderPayloadStatus.Active;
                    if (_pkg != null)
                    {
                        e.Weight = this.ProductManager.CaculateTTLWeight(_pkg, e.Qty);
                        e.Volume = ProductManager.CalculateCUFT(_pkg, e.Qty);
                    }
                    e.CreatedBy = this.Managers.AuthenticationInfo.Account;
                    e.SlotUID = payloadInfo.Content.SlotUID;
                    _workorderPayload.Add(e);
                    //split  payload data
                }
                else
                {
                    this.ActionResult.Success = false;
                    this.ActionResult.Message += Resource.MANIFEST_WORKORDER_NOT_FIND_VESSELMANIFST;
                    //找不到Vesselmanifest data ?
                }
            }
            //write data
            _action.Push(() => this.Managers.WorkOrderRepository.AddWorkOrder(_workorder));
            _action.Push(() => this.Managers.WorkOrderPayloadRepository.AddPayload(_workorderPayload));
            _wparameters.WorkOrder = _workorder;
            _wparameters.WorkOrderPayload = _workorderPayload;


            if (this.ActionResult.Success)
            {
                return this.InnerExecute(_action, _wparameters);
            }
            else
            {
                return this.ActionResult;
            }
        }
        protected override IActionResult<WorkOrderResult> InnerExecute(ConcurrentStack<Func<IActionResult<bool>>> Actions,
           IWorkOrderAssignAgentExecuteParameters parameters)
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
            AllocateExecutor executor = new AllocateExecutor(param);
            //split payload
            var action = executor.GetAllcoateFunc(parameters.WorkOrderPayload);
            if (action.Success)
            {
                var resultAction = action.Content.Reverse();
                foreach (var item in resultAction)
                {
                    Actions.Push(item);
                }
                this.ActionResult.Success = action.Success;
                this.ActionResult.Message = action.Message;
            }
            else
            {
                this.ActionResult.Success = action.Success;
                this.ActionResult.Message = action.Message;
            }
            return base.InnerExecute(Actions, parameters);

        }
    }
}
