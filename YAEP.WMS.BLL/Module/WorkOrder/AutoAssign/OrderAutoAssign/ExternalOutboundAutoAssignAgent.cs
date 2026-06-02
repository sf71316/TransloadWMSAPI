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
    internal class ExternalOutboundAutoAssignAgent : AbstractExternalAutoOutboundAssignedAgent
    {
        public ExternalOutboundAutoAssignAgent(IAutoAssignAgentProviders providers) : base(providers)
        {

        }
        /// <summary>
        /// 進行訂單Allocated by vessel
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override OutboundAutoAssignedResult Execute(OutboundAutoAssignedParameters parameters)
        {
            AllocatedInnerResponse allocatedInnerResponse = new AllocatedInnerResponse();
            Stopwatch sw = new Stopwatch();
            IEnumerable<AllocatedPlannerResult> allcoatedResult = null;
            OutboundAutoAssignedResult result = new OutboundAutoAssignedResult();
            using (var activity = this.Providers.TracingAgent.StartActivity("plan order"))
            {
                sw.Restart();
                ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
                result.Response = allocatedInnerResponse;
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
                    TracingAgent = this.Providers.TracingAgent
                };
                //規畫Vessel Allocated 是否有足夠onhand        
                var allocatePlanner = AbstractAllocatePlanner.GetInstance(initParameters, parameters.OutboundRequest.AllocateMode);
                allcoatedResult = allocatePlanner
                   .ExternalOrderPlanByWMS(parameters.Manifest.WarehouseUID, parameters.VesselItems, parameters.PassPackageVersion, parameters.IsChinaWarehouse);//取𢔽配貨規劃表            
                sw.Stop();
                Debug.WriteLine($"Allocated planned elapsed {sw.ElapsedMilliseconds}ms");
                sw.Restart();
            }
            if (!allcoatedResult.All(p => p.IsComplete))//是否有缺貨
            {
                using (var activity = this.Providers.TracingAgent.StartActivity("onhand shortage"))
                {
                    allocatedInnerResponse.IsComplete = false;
                    foreach (var item in allcoatedResult)
                    {
                        var requestItem = parameters.OutboundRequest.Items
                            .FirstOrDefault(p => p.VesselManifestUID == item.VesselManifestUID);
                        var vesselmanifestInfo = parameters.VesselItems.FirstOrDefault(p => p.UID == item.VesselManifestUID);
                        var vesselInfo = parameters.Vessel.FirstOrDefault(p => p.UID == vesselmanifestInfo.VesselUID);
                        var itemResponse = new AllocatedItemInnerResponse(requestItem);
                        itemResponse.IsComplete = item.IsComplete;
                        itemResponse.Onhand = item.Onhand;
                        itemResponse.ShipViaRefUID = vesselInfo.BolUID;
                        itemResponse.ShortageQty = item.ShortageQty;
                        itemResponse.ComponentType = requestItem.ComponentType;
                        allocatedInnerResponse.Results.Add(itemResponse);
                    }
                }
            }
            else
            {
                var manifestType = ManifestType.Outbound;
                AbstractWorkOrderAssignAgent agent = null;
                IActionResult<ISlotModel> futeureLocation;
                //有貨

                var locationlist = this.Providers.WarehouseManager
                .GetLocations(allcoatedResult.Select(p => p.Items.Where(item => item.AllocateType == AllocateType.GeneralAllocate)).SelectMany(x => x.Select(y => y.PayloadUID)));
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


                using (var activity = this.Providers.TracingAgent.StartActivity("allocating data"))
                {
                    
                    foreach (var vessel in parameters.Vessel)
                    {
                        using (var activity2 = this.Providers.TracingAgent
                            .StartActivity($"allocating data by vessel {vessel.RefNo} {vessel.UID}"))
                        {
                            Guid? workOrderUID = null;
                            var _palletbarcode = "";
                            var outboundWorkOrder = new AssignedOutboundWorkOrderCollection();
                            outboundWorkOrder.VesselUID = vessel.UID;
                            outboundWorkOrder.ServiceType = manifestType;
                            var assignedItems = allcoatedResult.Where(p => p.VesselUID == vessel.UID);
                            outboundWorkOrder.Items = new List<IAssignedOutboundWorkOrderPayload>();
                            IAssignedWorkOrderCollection workOrder;
                            using (var activity4 = this.Providers.TracingAgent.StartActivity($"mapping parameters"))
                            {
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
                                //agent.ExistTransactionScope = true;
                            }
                            //寫入 WorkOrder, Workpayload
                            var wresult = agent.Execute(workOrder, parameters);
                            if (wresult.Success)
                            {
                                sw.Stop();
                                Debug.WriteLine($"assigned workorder payload data elapsed {sw.ElapsedMilliseconds}ms");
                                sw.Restart();
                                //取得workorderUID
                                if (!workOrderUID.HasValue)
                                {
                                    using (var activity6 = this.Providers.TracingAgent.StartActivity($"check workorder data"))
                                    {
                                        var workOrderUIDResult = this.Providers.WorkOrderAssignAgentParameters
                                        .WorkOrderRepository.GetWorkOrderUID(vessel.UID);
                                        if (!workOrderUIDResult.Success || !workOrderUIDResult.Content.HasValue)
                                        {
                                            //當取不到workorder 代表配貨失敗
                                            result.Response.IsComplete = false;
                                            result.Response.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER;
                                        }
                                        else
                                        {
                                            workOrderUID = workOrderUIDResult.Content.Value;
                                            result.Response.IsComplete = true;
                                        }
                                    }
                                }
                                if (result.Response.IsComplete)
                                {
                                    using (var activity7 = this.Providers.TracingAgent.StartActivity($"Assign WorkOrderPod & WorkOrderPayload"))
                                    {
                                        #region Assign WorkOrderPod & WorkOrderPayload
                                        //取得workorderpayload
                                        var workOrderpayload = this.Providers.WorkOrderAssignAgentParameters
                                        .WorkOrderPayloadRepository.GetList(new { workOrderUID = workOrderUID });

                                        var payloads = workOrderpayload.Content.Where(p =>
                                        assignedItems.Any(x => x.VesselManifestUID == p.VesselManifestUID));
                                        if (payloads.Count() > 0)
                                        {
                                            var workorderPod = new WorkOrderPodInnerModel();
                                            //var workOrderPodSeq = workOrderPodSeqBatch.Dequeue();
                                            var workOrderPodSeq = this.Providers.WorkOrderAssignAgentParameters
                                                .SequenceAgent.GetWorkOrderPodSeqenceByTimeSerial(ManifestType.Outbound);
                                            workorderPod.PodUID = Guid.NewGuid();
                                            workorderPod.UID = Guid.NewGuid();
                                            workorderPod.ID = workOrderPodSeq;
                                            workorderPod.Name = _palletbarcode;
                                            //workorderPod.OperationSuggestion = workOrder.OperationSuggestion;
                                            workorderPod.Type = workOrder.StorageMethod;
                                            workorderPod.WorkOrderUID = workOrderUID.Value;
                                            workorderPod.Status = (int)WorkOrderPodStatus.Open;
                                            workorderPod.CreatedBy = this.Providers.WorkOrderAssignAgentParameters.AuthenticationInfo.Account;
                                            workorderPod.Weight = payloads.Sum(p => p.Weight);
                                            workorderPod.Volume = payloads.Sum(p => p.Volume);

                                            var addPodResult = this.Providers.WorkOrderAssignAgentParameters
                                                                .WorkOrderPodRepository.AddWorkOrderPod(workorderPod);
                                            if (addPodResult.Success)
                                            {
                                                var assignpodResult = this.Providers.WorkOrderAssignAgentParameters.WorkOrderManager
                                                      .AssignedPayloadtoPod(workorderPod.UID, payloads.Select(p => p.UID));
                                                //this.Providers.TracingAgent.Trace("generate workorder pod", assignpodResult);
                                                if (assignpodResult.Success)
                                                {
                                                    var uploadlabelResult = ActionResultTemplates.OK();
                                                    uploadlabelResult.Success = true;
                                                    if (parameters.OutboundRequest.OrderType == (int)OrderType.Truckload)
                                                    {
                                                        uploadlabelResult = this.Providers.WorkOrderManager
                                                                           .SetWorkOrderPodBarcode(workorderPod, _palletbarcode, false);
                                                        //this.Providers.TracingAgent.Trace("assigined shipping label", uploadlabelResult);
                                                    }
                                                    if (uploadlabelResult.Success)
                                                    {
                                                        //assignedItems
                                                        //var responseItem = allocatedInnerResponse.Results.Where(p =>
                                                        //parameters.OutboundRequest.Items.Any(x => x.VesselManifestUID == p.VesselManifestUID));
                                                        var responseItem = allocatedInnerResponse.Results.Where(p => assignedItems.Any(
                                                             x => x.VesselManifestUID == p.VesselManifestUID
                                                            ));
                                                        var usedwpayload = new List<Guid>();
                                                        foreach (var item in responseItem)
                                                        {

                                                            var wpayload = payloads
                                                                .FirstOrDefault(p => p.VesselManifestUID == item.VesselManifestUID
                                                                && !usedwpayload.Contains(p.UID));
                                                            if (wpayload != null)
                                                            {
                                                                item.PalletRefUID = workorderPod.UID;
                                                                item.ProcessItemUID = wpayload.UID;
                                                                item.Onhand = wpayload.Qty;
                                                                usedwpayload.Add(wpayload.UID);
                                                            }
                                                            else //allocated 後找不到w.payload 可以對應(上面已經決定要拿幾個payload Ln#72)
                                                            {
                                                                result.Response.IsComplete = false;
                                                                result.Response.Message = "no more assigned payload";
                                                                break;
                                                            }
                                                        }
                                                        result.Response.IsComplete &= true;
                                                    }
                                                    else
                                                    {
                                                        result.Response.IsComplete &= false;
                                                        result.Response.Message += " " + uploadlabelResult.Message;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    result.Response.IsComplete &= false;
                                                    result.Response.Message += " " + assignpodResult.Message;
                                                    break;
                                                }
                                                sw.Stop();
                                                Debug.WriteLine($"assigned workorder pod data elapsed {sw.ElapsedMilliseconds}ms");
                                                sw.Restart();
                                            }
                                            else
                                            {
                                                result.Response.IsComplete &= false;
                                                result.Response.Message += " " + addPodResult.Message;
                                                break;
                                            }
                                        }
                                        else
                                        {

                                            result.Response.IsComplete = false;
                                            result.Response.Message = "not find any assigned vessel item.";
                                        }

                                        if (result.Response.IsComplete)
                                        {
                                            result.WorkOrderUID = workOrderUID.Value;
                                        }
                                        else
                                        {
                                            result.Response.IsComplete = false;
                                        }
                                        #endregion
                                    }
                                }
                                else
                                {
                                    result.Response.IsComplete = false;
                                    result.Response.Message = Resource.MANIFEST_ORDER_ALLOCATED_FAILURE + " " + wresult.Message;
                                    result.Response.Results.Clear();
                                    break;
                                }

                            }
                            else
                            {
                                this.Providers.TracingAgent.Trace("generate workorder failure", wresult);
                                result.Response.IsComplete = false;
                                result.Response.Message = Resource.MANIFEST_ORDER_ALLOCATED_FAILURE + " " + wresult.Message;
                                result.Response.Results.Clear();
                                break;
                            }

                        }
                    }
                }
            }
            return result;
        }
    }
}
