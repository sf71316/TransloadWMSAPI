using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Language.Resources;
using YAEP.Package.Constants;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Constant;

namespace YAEP.WMS.BLL.Module
{
    /*
     * 1. 檢查 Pallet UOM 資料是否有設定 Pallet
     * 2. 過濾除了 Pallet 以外的 Item
     * 3. 取得預計Allocate的清單
     */
    internal class OutboundAutoAssignWorkOrderExecutor : AbstractAutoAssignWorkOrderExecutor
    {
        public OutboundAutoAssignWorkOrderExecutor(IAutoAssignAgentProviders providers) : base(providers)
        {

        }
        public override IActionResult<bool> Execute(IAutoAssignProcessArgs args)
        {
            var e = args as IAutoAssignOutboundProcessArgs;

            // 查 Pallet UOM 設定
            var packageUomListResult = this.Providers.PackageUomManager.GetPackageUomList();
            if (packageUomListResult.Content?.Count() == 0)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NO_PALLET_UOM);
            }

            var palletUom = packageUomListResult.Content.FirstOrDefault(o => o.ID.Equals("Pallet", StringComparison.OrdinalIgnoreCase) &&
                                                                                                        o.Status == (int)PackageUomStatus.Active);
            if (palletUom == null)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NO_PALLET_UOM);
            }

            // 過濾除了 Pallet 以外的 Item
            var palletManifests = e.VesselManifests.Where(o => e.Packages.FirstOrDefault(p => p.UID == o.ReceivePackageUID)?.UOM == palletUom.UID);
            if (palletManifests?.Count() == 0)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_AUTO_ASSIGN_NO_ALLOWED_ITEM);
            }


            // 取得預計Allocate的清單
            var allocateListParameters = new List<AllocatedPlannerInnerParameter>();

            foreach (var item in palletManifests)
            {
                allocateListParameters.Add(new AllocatedPlannerInnerParameter()
                {
                    VesselManifestUID = item.VesselMainifestUID,
                    ItemUID = item.ItemUID,
                    PackageUID = item.ReceivePackageUID,
                });
            }
            //TODO UI 無法決定該Manifest 是Pkg/Truck 出貨 預設看Truckload
            AllocatedPlannerInitParameters initParameters = new AllocatedPlannerInitParameters
            {
                PackageManager = this.Providers.PackageManager,
                PackageMappingCache = this.Providers.PackageCacheManager,
                PackageUomManager = this.Providers.PackageUomManager,
                VesselManager = this.Providers.VesselManager,
                ProductCache = this.Providers.ProductCacheManager,
                WarehouseManger = this.Providers.WarehouseManager,
                OrderType = (int)OrderType.Truckload,
                TracingAgent = this.Providers.TracingAgent
            };

            var allocatePlanner = AbstractAllocatePlanner.GetInstance(initParameters, AllocateType.GeneralAllocate);
            var planResultList = allocatePlanner.PlanByWMS(allocateListParameters, false, false);

            //if (planResultList.Count() == 0)
            //{
            //    return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_AUTO_ASSIGN_NO_ALLOWED_ITEM);
            //}
            if (!planResultList.All(p => p.IsComplete))
            {
                List<string> message = new List<string>();
                var insufficientinfo = planResultList.Where(p => !p.IsComplete);
                var iteminfos = this.Providers.ProductCacheManager.GetItems(insufficientinfo.Select(p => p.ItemUID));
                foreach (var item in insufficientinfo)
                {
                    var iteminfo = iteminfos.FirstOrDefault(p => p.UID == item.ItemUID);
                    message.Add($"Item:{iteminfo.Name} short qty:{item.ShortageQty}");
                }
                var messageContext = string.Format(Resource.MANIFEST_WORKORDER_AUTO_ASSIGN_INSUFFICIENT_QTY, string.Join(",", message));
                return ActionResultTemplates.Error(messageContext);
            }
            var manifestType = (ManifestType)e.Manifest.Type;

            // Work Order
            var outboundWorkOrder = new AssignedOutboundWorkOrderCollection();
            outboundWorkOrder.VesselUID = e.Vessel.UID;
            outboundWorkOrder.ServiceType = manifestType;

            foreach (var plan in planResultList)
            {
                // Payload
                foreach (var paitem in plan.Items)
                {
                    var payloadModel = new AssignedOutboundWorkOrderPayload()
                    {
                        PayloadUID = paitem.PayloadUID,
                        ItemUID = plan.ItemUID,
                        VesselMainifestUID = plan.VesselManifestUID,
                        AllocatedQty = paitem.AllocatedQty,
                    };
                    outboundWorkOrder.Items.Add(payloadModel);
                }
            }

            var converter = new AssignedParameterConverter();
            var workOrder = converter.OutboundParameterConvert(outboundWorkOrder);

            var parameters = this.Providers.WorkOrderAssignAgentParameters;
            var agent = AbstractWorkOrderAssignAgent.GetAgent(manifestType, parameters);
            var result = agent.Execute(workOrder);

            // Pod
            if (result.Success)
            {
                var workOrderUIDResult = this.Providers.WorkOrderAssignAgentParameters.WorkOrderRepository.GetWorkOrderUID(e.Vessel.UID);
                if (!workOrderUIDResult.Success && !workOrderUIDResult.Content.HasValue)
                {
                    return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER);
                }

                var workOrderUID = workOrderUIDResult.Content.Value;
                foreach (var item in workOrder.Items)
                {
                    var payloadResult = this.Providers.WorkOrderAssignAgentParameters.WorkOrderPayloadRepository.GetList(new { UID = item.PayloadUID });
                    if (payloadResult.Success && (payloadResult.Content?.Count() ?? 0) > 0)
                    {
                        var payload = payloadResult.Content.FirstOrDefault();

                        var workorderPod = new WorkOrderPodInnerModel();
                        var workOrderPodSeq = this.Providers.WorkOrderAssignAgentParameters.SequenceAgent.GetWorkOrderSeqenceByTimeSerial(manifestType);
                        workorderPod.PodUID = Guid.NewGuid();
                        workorderPod.UID = Guid.NewGuid();
                        workorderPod.ID = workOrderPodSeq;
                        workorderPod.Name = workOrderPodSeq;
                        //workorderPod.OperationSuggestion = workOrder.OperationSuggestion;
                        workorderPod.Type = workOrder.StorageMethod;
                        workorderPod.WorkOrderUID = workOrderUID;
                        workorderPod.Status = (int)WorkOrderPodStatus.Draft;
                        workorderPod.CreatedBy = this.Providers.WorkOrderAssignAgentParameters.AuthenticationInfo.Account;
                        workorderPod.Weight = payload.Weight;
                        workorderPod.Volume = payload.Volume;

                        var addPodResult = this.Providers.WorkOrderAssignAgentParameters.WorkOrderPodRepository.AddWorkOrderPod(workorderPod);
                        if (addPodResult.Success)
                        {
                            this.Providers.WorkOrderAssignAgentParameters.WorkOrderManager.AssignedPayloadtoPod(workorderPod.UID, new Guid[] { payload.UID });
                        }
                    }
                }
            }

            return ActionResultTemplates.Result(true);
        }

        public override IActionResult<List<Func<IActionResult<bool>>>> ExecuteReturnAction(IAutoAssignProcessArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
