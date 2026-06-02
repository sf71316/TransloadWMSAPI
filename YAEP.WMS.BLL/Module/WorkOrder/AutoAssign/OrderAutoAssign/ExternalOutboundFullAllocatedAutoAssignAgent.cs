using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class ExternalOutboundFullAllocatedAutoAssignAgent : AbstractExternalAutoOutboundAssignedAgent
    {
        public ExternalOutboundFullAllocatedAutoAssignAgent(IAutoAssignAgentProviders providers) : base(providers)
        {

        }
        /// <summary>
        /// 進行整張訂單Allocated
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override OutboundAutoAssignedResult Execute(OutboundAutoAssignedParameters parameters)
        {
            #region settings
            List<bool> _workordergenerate = new List<bool>();
            AllocatedInnerResponse allocatedInnerResponse = new AllocatedInnerResponse();
            IEnumerable<AllocatedPlannerResult> allcoatedResult = null;
            List<IActionResult<bool>> allocatedResult = new List<IActionResult<bool>>();
            OutboundAutoAssignedResult result = new OutboundAutoAssignedResult();
            var param = new AllocateExecutorParameters
            {
                InventoryManager = this.Providers.WorkOrderAssignAgentParameters.InventoryManager,
                ProductUtility = new ProductUtility(),
                WorkOrderPayloadRepository = this.Providers.WorkOrderAssignAgentParameters.WorkOrderPayloadRepository,
                LabelManager = this.Providers.WorkOrderAssignAgentParameters.LabelManager,
                PackageMappingCache = this.Providers.WorkOrderAssignAgentParameters.PackageCacheManager,
                SequenceAgent = this.Providers.WorkOrderAssignAgentParameters.SequenceAgent,
                TracingAgent = this.Providers.WorkOrderAssignAgentParameters.TracingAgent
            };


            var executor = new FullAllocatedTemporaryOnhandExecutor(param);
            AllocatedPlannerInitParameters initParameters = new AllocatedPlannerInitParameters
            {
                PackageManager = this.Providers.PackageManager,
                PackageMappingCache = this.Providers.PackageCacheManager,
                PackageUomManager = this.Providers.PackageUomManager,
                PackageVersionManager = this.Providers.PackageVersionManager,
                PackageVersionRepository = this.Providers.PackageVersionRepository,
                VesselManager = this.Providers.VesselManager,
                ProductCache = this.Providers.ProductCacheManager,
                WarehouseManger = this.Providers.WarehouseManager,
                OrderType = parameters.OutboundRequest.OrderType,
                TracingAgent = this.Providers.TracingAgent,
                AllocatedExecutor = executor
            };
            #endregion
            try
            {
                result.Response = allocatedInnerResponse;
                using (var tran1 = this.Providers.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot))
                {
                    #region 規劃庫存和取得整張訂單暫存庫存資料
                    using (var activity = this.Providers.TracingAgent.StartActivity("規劃訂單"))
                    {
                        //規畫Vessel Allocated 是否有足夠onhand
                        //TODO Allocated/future allocate Planner 整合待討論
                        var allocatePlanner = AbstractAllocatePlanner.GetInstance(initParameters, parameters.OutboundRequest.AllocateMode);
                        allcoatedResult = allocatePlanner
                           .ExternalOrderPlanByWMS(parameters.Manifest.WarehouseUID, parameters.VesselItems
                           , parameters.PassPackageVersion, parameters.IsChinaWarehouse);//取𢔽配貨規劃表            

                    }
                    #endregion
                    if (allcoatedResult != null &&
                        (allcoatedResult.All(p => p.IsComplete)
                                    && allcoatedResult.Count() > 0))//是否有缺貨
                    {
                        tran1.Commit();
                    }
                    else
                    {

                        tran1.Rollback();
                    }

                }
                this.Providers.DbEntities.ReInitConnectionInstance();
                if (!allcoatedResult.All(p => p.IsComplete) || allcoatedResult.Count() == 0)//是否有缺貨
                {
                    using (var activity = this.Providers.TracingAgent.StartActivity("產生缺貨Response"))
                    {
                        allocatedInnerResponse.IsComplete = false;
                        if (allcoatedResult.Count() > 0)
                        {
                            foreach (var item in allcoatedResult)
                            {
                                var requestItems = parameters.OutboundRequest.Items
                                    .Where(p => item.VesselManifestCollection.Any(x => x.UID == p.VesselManifestUID));
                                //var vesselmanifestInfo = parameters.VesselItems.FirstOrDefault(p => p.UID == item.VesselManifestUID);
                                //var vesselInfo = parameters.Vessel.FirstOrDefault(p => p.UID == vesselmanifestInfo.VesselUID);
                                var itemResponse = new AllocatedItemInnerResponse(requestItems.FirstOrDefault());
                                itemResponse.IsComplete = item.IsComplete;
                                itemResponse.Onhand = requestItems.Sum(p => p.Qty);// item.Onhand;
                                                                                   //itemResponse.ShipViaRefUID = vesselInfo.BolUID;
                                itemResponse.ShortageQty = item.ShortageQty;
                                itemResponse.ComponentType = requestItems.FirstOrDefault().ComponentType;
                                allocatedInnerResponse.Results.Add(itemResponse);
                            }
                        }
                        else
                        {
                            allocatedInnerResponse.Message = "allocated failure ,please try again";
                            this.Providers.TracingAgent.Trace("allocated plan failure or execute allocated failure");
                        }
                    }
                }
                else
                {
                    using (var tran2 = this.Providers.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot))
                    {
                        #region 生成Work order 資料
                        var manifestType = ManifestType.Outbound;
                        AbstractWorkOrderAssignAgent agent = null;
                        IActionResult<ISlotModel> futeureLocation;
                        //有貨
                        var locationlist = this.Providers.WarehouseManager
                        .GetLocations(allcoatedResult
                        .Select(p => p.Items.Where(item => item.AllocateType == AllocateType.GeneralAllocate))
                        .SelectMany(x => x.Select(y => y.PayloadUID)));
                        //先取得futeureLocation
                        futeureLocation = this.Providers.WarehouseManager.GetFutureSlot(parameters.Manifest.WarehouseUID);
                        //Allocated 規畫完結果轉成產生workorder資料
                        #region 回傳給Shipping資料
                        foreach (var item in allcoatedResult)
                        {
                            var requestItem = parameters.OutboundRequest.Items
                                .FirstOrDefault(p => p.VesselManifestUID == item.VesselManifestUID);
                            foreach (var item2 in item.Items)
                            {
                                var vesselmanifestInfo = parameters.VesselItems.FirstOrDefault(p => p.UID == item.VesselManifestUID);
                                var vesselInfo = parameters.Vessel.FirstOrDefault(p => p.UID == vesselmanifestInfo.VesselUID);
                                var itemResponse = new AllocatedItemInnerResponse(requestItem);
                                //allocated後再設定
                                // itemResponse.Onhand = item2.AllocatedQty;
                                itemResponse.ItemRefUID = item.ItemUID;
                                if (item2.AllocateType == AllocateType.FutureAllocate)
                                {
                                    if (futeureLocation.Content != null)
                                    {
                                        itemResponse.Location = futeureLocation.Content.Name;
                                    }
                                    else
                                    {
                                        throw new Exception("Future allocated slot is not set");
                                    }
                                }
                                else
                                {
                                    var locationInfo = locationlist.Content.FirstOrDefault(x => x.PayloadUID == item2.PayloadUID);
                                    itemResponse.Location = locationInfo.SlotName;
                                }
                                itemResponse.ShipViaRefUID = vesselInfo.BolUID;
                                itemResponse.IsComplete = true;
                                allocatedInnerResponse.Results.Add(itemResponse);
                            }
                        }
                        #endregion
                        //var workOrderPodSeqBatch = this.Providers.WorkOrderAssignAgentParameters.SequenceAgent
                        //    .GetWorkOrderPodSeqence(Guid.Empty, parameters.Vessel.Count());
                        var wparameters = this.Providers.WorkOrderAssignAgentParameters;
                        agent = AbstractWorkOrderAssignAgent.GetAgent(manifestType, wparameters);

                        if (parameters.OutboundRequest.UsePackingStation)
                        {

                            parameters.LandingSlot = this.Providers.WarehouseManager.GetDefaultLandingZone(
                                parameters.Manifest.WarehouseUID, SlotType.StagingArea_Parcel);
                        }
                        else
                        {
                            parameters.LandingSlot = this.Providers.WarehouseManager.GetDummySlot(parameters.Manifest.WarehouseUID);
                        }
                        using (var activity = this.Providers.TracingAgent.StartActivity("分配庫存資料"))
                        {
                            List<WorkOrderResult> workOrderResults = new List<WorkOrderResult>();
                            using (var activity2 = this.Providers.TracingAgent.StartActivity($"生成 全部Vessel Workorder 資料"))
                            {
                                foreach (var vessel in parameters.Vessel)
                                {

                                    var _palletbarcode = "";
                                    var outboundWorkOrder = new AssignedOutboundWorkOrderCollection();
                                    outboundWorkOrder.VesselUID = vessel.UID;
                                    outboundWorkOrder.ServiceType = manifestType;
                                    var assignedItems = allcoatedResult.Where(p => p.VesselUID == vessel.UID);
                                    outboundWorkOrder.Items = new List<IAssignedOutboundWorkOrderPayload>();
                                    IAssignedWorkOrderCollection workOrder;
                                    foreach (var plan in assignedItems)
                                    {
                                        var request = parameters.OutboundRequest.Items.FirstOrDefault(x => x.VesselManifestUID == plan.VesselManifestUID);
                                        _palletbarcode = request.PalletBarcode;
                                        // Payload
                                        foreach (var paitem in plan.Items)
                                        {
                                            var payloadModel = new AssignedOutboundWorkOrderPayload()
                                            {
                                                PayloadUID = paitem.PayloadUID,
                                                ItemUID = plan.ItemUID,
                                                ItemGroupUID = request.ItemGroupUID,
                                                VesselMainifestUID = plan.VesselManifestUID,
                                                AllocatedQty = paitem.AllocatedQty,
                                                AllocateType = paitem.AllocateType,
                                                OnhandPayloadItems = plan.OnhandPayloadItems
                                            };
                                            if (paitem.AllocateType == AllocateType.FutureAllocate)
                                            {
                                                payloadModel.SlotUID = futeureLocation.Content.UID;
                                                payloadModel.PickPackageUID = paitem.AllocatedPackageUID;
                                            }
                                            outboundWorkOrder.Items.Add(payloadModel);
                                        }
                                    }
                                    //outboundWorkOrder.Items = items;
                                    var converter = new AssignedParameterConverter();
                                    workOrder = converter.OutboundParameterConvert(outboundWorkOrder);
                                    workOrder.StorageMethod = (int)StorageMethod.NewPallet;
                                    workOrder.ExternalBarcode = _palletbarcode;
                                    //agent.ExistTransactionScope = true;

                                    //寫入 WorkOrder, Workpayload
                                    var wresult = agent.Execute(workOrder, parameters);
                                    workOrderResults.Add(wresult.Content);


                                }
                            }
                            #region 執行生成 Workor order SQL &  執行
                            using (var activity2 = this.Providers.TracingAgent.StartActivity($"執行生成 Workor order SQL &  執行"))
                            {
                                using (var w1 = this.Providers.TracingAgent
                                    .StartActivity($"generate &execute workorder "))
                                {
                                    #region generate &execute workorder 
                                    var wobj = workOrderResults.Select(p => p.Workorder);
                                    allocatedResult.Add(this.Providers
                                        .WorkOrderAssignAgentParameters
                                        .WorkOrderRepository.AddWorkOrder(wobj));
                                    #endregion
                                }
                                using (var w2 = this.Providers.TracingAgent
                                    .StartActivity($"generate  &execute work order payload "))
                                {
                                    #region generate  &execute work order payload
                                    var wpayloadobj = workOrderResults.SelectMany(p => p.WorkOrderPayload.Select(x => x));
                                    allocatedResult.Add(this.Providers
                                        .WorkOrderAssignAgentParameters
                                        .WorkOrderPayloadRepository.AddPayload(wpayloadobj));
                                    #endregion
                                }
                                using (var w3 = this.Providers.TracingAgent
                                    .StartActivity($"generate  &execute work order pod"))
                                {
                                    #region generate  &execute work order pod
                                    var wpodobj = workOrderResults.Select(p => p.WorkorderPod);
                                    allocatedResult.Add(this.Providers
                                      .WorkOrderAssignAgentParameters
                                      .WorkOrderPodRepository.AddWorkOrderPod(wpodobj));
                                    #endregion
                                }

                                #region  & execute work order pod mapping pallet barcode
                                if (parameters.OutboundRequest.OrderType == (int)OrderType.Truckload)
                                {
                                    using (var w4 = this.Providers.TracingAgent
                                        .StartActivity($"execute work order pod mapping pallet barcode count:{workOrderResults.SelectMany(p => p.PodLabels).Count()}"))
                                    {
                                        allocatedResult.Add(this.Providers
                                                     .WorkOrderAssignAgentParameters
                                                     .LabelManager
                                                     .AddLabels(workOrderResults.SelectMany(p => p.PodLabels).ToArray()));
                                    }
                                }
                                #endregion
                                if (allocatedResult.All(p => p.Success))
                                {
                                    #region execute payload sql


                                    using (var w4 = this.Providers.TracingAgent
                                    .StartActivity($"execute payload sql count:{workOrderResults.SelectMany(p => p.AllocatedExecutorResult.Payloads).Count()}"))
                                    {
                                        allocatedResult.Add(this.Providers
                                        .WorkOrderAssignAgentParameters
                                        .InventoryManager
                                        .BatchAddPayload(workOrderResults.SelectMany(p => p.AllocatedExecutorResult.Payloads)));

                                        this.Providers
                                     .WorkOrderAssignAgentParameters
                                     .LabelManager
                                     .BatchReturnCloneLabel(workOrderResults.SelectMany(p => p.AllocatedExecutorResult.CloneLabels));
                                    }
                                    #endregion

                                    #region Mapping Allocated Response object
                                    if (allocatedResult.All(p => p.Success))
                                    {
                                        foreach (var item in workOrderResults)
                                        {


                                            //assigned response object
                                            var responseItem = allocatedInnerResponse.Results.Where(p => item.WorkorderAssignedResults.Any(
                                                                x => x.VesselManifestUID == p.VesselManifestUID
                                                               ));
                                            var usedwpayload = new List<Guid>();
                                            foreach (var ritem in responseItem)
                                            {

                                                var wpayload = item.WorkorderAssignedResults
                                                    .FirstOrDefault(p => p.VesselManifestUID == ritem.VesselManifestUID
                                                    && !usedwpayload.Contains(p.WorkorderPayloadUID));
                                                if (wpayload != null)
                                                {
                                                    ritem.PalletRefUID = wpayload.WorkorderPodUID;
                                                    ritem.ProcessItemUID = wpayload.WorkorderPayloadUID;
                                                    ritem.Onhand = wpayload.Quantity;
                                                    usedwpayload.Add(wpayload.WorkorderPayloadUID);
                                                }
                                                else //allocated 後找不到w.payload 可以對應(上面已經決定要拿幾個payload Ln#72)
                                                {
                                                    var wrs = ActionResultTemplates.OK();
                                                    wrs.Message = "no more assigned payload";
                                                    wrs.Success = false;
                                                    allocatedResult.Add(wrs);
                                                    break;
                                                }
                                            }

                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                            if (allocatedResult.All(p => p.Success))
                            {
                                var wrs = ActionResultTemplates.OK();
                                wrs.Success = true;
                                wrs.Message = result.Response.Message;
                                allocatedResult.Add(wrs);
                            }
                            else
                            {
                                var wrs = ActionResultTemplates.OK();
                                wrs.Success = false;

                                allocatedResult.Add(wrs);
                            }

                        }
                        #endregion

                        #region 執行建立Manifestinfo資料SQL
                        if (allocatedResult.All(p => p.Success))
                        {
                            var action = parameters.ManifestGenerateFuncs;
                            using (var a5 = this.Providers.TracingAgent.StartActivity("生成manifestinfo 資料"))
                            {
                                foreach (var item in action)
                                {
                                    if (allocatedResult.All(p => p.Success))
                                    {
                                        allocatedResult.Add(item.Invoke());
                                    }
                                }
                            }

                        }
                        #endregion

                        #region 生成Ticket資料
                        if (allocatedResult.All(p => p.Success))
                        {
                            using (var a6 = this.Providers.TracingAgent.StartActivity("生成Ticket資料"))
                            {
                                allocatedResult.AddRange(parameters.TicketGenerateFuncs.Invoke());
                            }
                        }
                        #endregion

                        if (allocatedResult.All(p => p.Success))
                        {
                            result.Response.IsComplete = true;
                            var crs = executor.ClearTemporaryOnhand(allcoatedResult.SelectMany(p => p.OnhandPayloadItems.Select(x => x.UID)));
                            if (crs.Success)
                            {
                                tran2.Commit();
                            }
                            else
                            {
                                this.Providers.TracingAgent.Trace("clear temporary onhand failure");
                                tran2.Rollback();
                            }
                        }
                        else
                        {
                            tran2.Rollback();
                            var _fail = string.Join(" | ", allocatedResult.Where(p => !p.Success).Select(p => string.IsNullOrEmpty(p.Message) ? "(no msg)" : p.Message));
                            result.Response.Message = "[autoassign-fail] " + _fail;
                            this.Providers.TracingAgent.Trace("allocated failure " + _fail);

                        }
                    }
                    #region 當Allocated 失敗要把暫存庫存還回去
                    using (var tran4 = this.Providers.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot))
                    {
                        if (!allocatedResult.All(p => p.Success))
                        {
                            var crs = executor.RecoveryTemporaryOnhand(allcoatedResult.SelectMany(p => p.OnhandPayloadItems.Select(x =>
                          new DeallocatedParameters
                          {
                              PayloadUID = x.UID,
                              RecoveryPayloadType = x.Type
                          })));
                            if (crs.Success)
                            {
                                tran4.Commit();
                            }
                            else
                            {
                                this.Providers.TracingAgent.Trace($"Recovery onhand occur exception {crs.Message}", crs.InnerException);
                                tran4.Rollback();
                            }

                        }
                    }
                    #endregion


                }
            }
            catch (Exception ex)
            {
                result.Response.IsComplete = false;
                result.Response.Message = "[autoassign-EX] " + ex.Message + " @@ " + (ex.InnerException != null ? ex.InnerException.Message + " @@ " : "") + ex.StackTrace;
                using (var tran3 = this.Providers.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot))
                {
                    #region 當規劃後發生異常時需將庫存還回去
                    if (allcoatedResult != null)
                    {
                        var rs = executor.RecoveryTemporaryOnhand(allcoatedResult.SelectMany(p => p.OnhandPayloadItems.Select(x => new DeallocatedParameters
                        {
                            PayloadUID = x.UID,
                            RecoveryPayloadType = x.Type
                        })));
                        if (rs.Success)
                        {
                            tran3.Commit();
                        }
                        else
                        {
                            this.Providers.TracingAgent.Trace($"Recovery onhand occur exception {rs.Message}", rs.InnerException);
                            tran3.Rollback();
                        }
                    }
                    else
                    {
                        tran3.Rollback();
                        result.Response.IsComplete = false;
                        result.Response.Message = "allocated failure";
                    }
                    #endregion
                }
            }

            return result;
        }
    }
}
