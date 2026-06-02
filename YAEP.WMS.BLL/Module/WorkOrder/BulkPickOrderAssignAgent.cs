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
    internal class BulkPickOrderAssignAgent : AbstractWorkOrderAssignAgent
    {
        public BulkPickOrderAssignAgent(IWorkOrderAssignAgentParameters Parameters)
           : base(Parameters)
        {

        }
        public override IActionResult<WorkOrderResult> Execute(IAssignedWorkOrderCollection Parameters,
            AbstractAutoAssignedParameters parameters = null)
        {
            this.ActionResult.Success = true;
            WorkOrderAssignAgentExecuteParameters _wparameters = new WorkOrderAssignAgentExecuteParameters();
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            var workorder = this.GeneratoreObject();
            var workorderPayload = new List<IWorkOrderPayloadModel>();
            var bulkPickWorkOrderPayloadRelations = new List<IBulkPickWorkOrderPayloadRelationModel>();
            List<string> Errors = new List<string>();
            // add workorder
            var _workOrderSeq = this.Managers.SequenceAgent.GetWorkOrderSeqenceByTimeSerial(ManifestType.BlukPick);
            workorder.UID = Guid.NewGuid();
            workorder.ID = _workOrderSeq;
            workorder.ManifestUID = Guid.Empty;
            workorder.VesselUID = Guid.Empty;
            workorder.Status = (int)WorkOrderStatus.Draft;
            workorder.Type = 1;
            workorder.CreatedBy = this.Managers.AuthenticationInfo.Account;
            this.WorkOrderUID = workorder.UID;
            //set default dummyslot 
            // add workorder payload

            var collcetion = Parameters.Items.Select(p => p as IBulkPikcAssignedWorkOrderPayload);
            Queue<string> seqwk = this.Managers.SequenceAgent.
                GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType.BlukPick, collcetion.Count());
            foreach (var item in collcetion)
            {
                var _pkg = this.Managers.PackageCacheManager.GetPackage(item.ReceivePackageUID);
                var _workOrderPayloadSeq = seqwk.Dequeue();
                var e = new BulkPickWorkOrderPayloadModel();
                e.UID = Guid.NewGuid();
                e.ID =
                e.Name = _workOrderPayloadSeq;
                e.Type = 1;
                e.WorkOrderUID = workorder.UID;
                e.WorkOrderPodUID = Guid.Empty;
                e.PayloadUID = item.PayloadUID.Value;
                e.PayloadPackageUID = item.ReceivePackageUID;
                e.ItemUID = item.ItemUID;
                e.ItemGroupUID = item.ItemGroupUID;
                e.PackageUID = item.ReceivePackageUID;
                e.Qty = item.ReceivePackageQty;
                e.VesselManifestUID = item.VesselMainifestUID;
                e.LoadingZoneSlotUID = item.SlotUID.Value;
                e.Status = (int)WorkOrderPayloadStatus.WaitingForProcessing;
                e.OriginalPayloadUID = item.OriginalPayloadUID;
                if (_pkg != null)
                {
                    e.Weight = this.ProductManager.CaculateTTLWeight(_pkg, e.Qty);
                    e.Volume = ProductManager.CalculateCUFT(_pkg, e.Qty);
                }
                e.CreatedBy = this.Managers.AuthenticationInfo.Account;
                e.SlotUID = item.SlotUID;
                e.LoadingZoneSlotUID = item.TargetSlotUID;
                workorderPayload.Add(e);
                foreach (var orginalwp in item.OriginalWorkOrderPayloadUID)
                {
                    var rel = new BulkPickWorkOrderPayloadRelationModel();
                    rel.BulkPickWorkOrderPayloadUID = e.UID;
                    rel.OriginalWorkOrderPayloadUID = orginalwp;
                    rel.Status = (int)BulkPickWorkOrderPayloadStatus.Active;
                    bulkPickWorkOrderPayloadRelations.Add(rel);
                }
            }
            //write data
            _action.Push(() => this.Managers.WorkOrderRepository.AddWorkOrder(workorder));
            _action.Push(() => this.Managers.WorkOrderPayloadRepository.AddPayload(workorderPayload));
            _action.Push(() => this.Managers.BulkPickWorkOrdrPayloadRelationRepository.Create(bulkPickWorkOrderPayloadRelations));
            _wparameters.WorkOrder = workorder;
            _wparameters.WorkOrderPayload = workorderPayload;


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
            var action = executor.GetForecAllcoateFunc(parameters.WorkOrderPayload);
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
