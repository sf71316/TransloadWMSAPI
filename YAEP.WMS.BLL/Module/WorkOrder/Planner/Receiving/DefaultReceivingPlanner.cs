using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    internal class DefaultReceivingPlanner : AbstractReceivingPlanner
    {
        public DefaultReceivingPlanner(ReceivingPlannerInitParameters parameters) : base(parameters)
        {

        }
        public override IActionResult<bool> Plan(InboundAutoAssignedParameters inboundparameters)
        {
            List<AssignedWorkOrderInnerCollection> listOfWorkOrders = new List<AssignedWorkOrderInnerCollection>();
            List<IActionResult<bool>> failureallocatedrs = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {

                // 優先度
                InboundHomeAddressBuilderInitParameters initParameters = new InboundHomeAddressBuilderInitParameters();
                initParameters.WarehouseManger = this.WarehouseManger;
                initParameters.ProductCacheManager = this.WorkOrderAssignAgentParameters.ProductCacheManager;
                initParameters.LogInfiltrator = inboundparameters.LogInfiltrator;
                var builder = AbstractInboundHomeAddressBuilder.GetInstance(initParameters);
                var homeAddress = builder.GetStorageHomeAddress(inboundparameters.Manifest.WarehouseUID,
                    inboundparameters.ManifestItems.Select(p => p.ItemUID));

                var lzs = this.WarehouseManger.GetDefaultLandingZone(inboundparameters.WarehouseUID, SlotType.InboundTemp);
                if (lzs.Success)
                {
                    foreach (var pallet in inboundparameters.LabelMapping)
                    {
                        var allowAllocated = true;
                        var vessels = new List<AssignedWorkOrderPayloadInnerModel>();
                        var rqs = inboundparameters.ReceivingRequest.Container
                                .SelectMany(x => x.Items)
                                .Where(y => y.Barcode == pallet.Key);
                        var vesselManifestCollection = inboundparameters.VesselItems.Where(x => pallet.Value.Contains(x.UID));
                        foreach (var vesselManifest in vesselManifestCollection)
                        {
                            // 多品項同櫃:LabelMapping 會把同櫃所有 vessel manifest 關到每個 barcode,
                            // 此 barcode(rqs) 不含此品項時 rq 為 null → 略過(否則 rq.PackageQty NRE,且會用錯數量找位)。
                            var rq = rqs.FirstOrDefault(x => x.ItemUID.Contains(vesselManifest.ItemUID));
                            if (rq == null) continue;
                            var packageModel = inboundparameters.PackageCacheManager.GetPackage(vesselManifest.PackageUID);
                            var latestSlot = homeAddress.FindSlot(vesselManifest.ItemUID, rqs.Sum(x => x.PackageQty), packageModel);
                            if (latestSlot != null)
                            {
                                var am = new AssignedWorkOrderPayloadInnerModel();
                                am.ItemUID = vesselManifest.ItemUID;
                                am.ReceivePackageQty = rq.PackageQty;
                                am.ReceivePackageUID = vesselManifest.PackageUID;
                                am.SlotUID = latestSlot.SlotUID;
                                am.ItemGroupUID = rq.ItemGroupUID;
                                am.VesselMainifestUID = vesselManifest.UID;
                                vessels.Add(am);

                            }
                            else
                            {
                                var ars = ActionResultTemplates.Result<bool>();
                                var itemInfo = this.WorkOrderAssignAgentParameters.ProductCacheManager.GetItem(vesselManifest.ItemUID);
                                ars.Success = false;
                                ars.Message = string.Format(Resource.INBOUND_OVER_VOLUME, pallet.Key, itemInfo.ID);
                                failureallocatedrs.Add(ars);
                                allowAllocated = false;
                            }
                        }
                        if (allowAllocated)
                        {
                            #region Work Order

                            var inboundWorkOrder = new AssignedWorkOrderInnerCollection();
                            inboundWorkOrder.VesselUID = inboundparameters.VesselItems.First().VesselUID;
                            inboundWorkOrder.LoadingZoneSlotUID = lzs.Content.UID;
                            inboundWorkOrder.StorageMethod = (int)StorageMethod.NewPallet;
                            inboundWorkOrder.ServiceType = ManifestType.Inbound;
                            inboundWorkOrder.ExternalBarcode = pallet.Key;
                            inboundWorkOrder.Items = new List<IAssignedWorkOrderPayload>();
                            foreach (var item in vessels)
                            {
                                inboundWorkOrder.Items.Add(item);
                            }

                            #endregion
                            listOfWorkOrders.Add(inboundWorkOrder);
                        }

                    }
                    if (failureallocatedrs.Count() == 0)
                    {
                        var agent = AbstractWorkOrderAssignAgent.GetAgent(ManifestType.Inbound, this.WorkOrderAssignAgentParameters);
                        List<IActionResult<WorkOrderResult>> result = new List<IActionResult<WorkOrderResult>>();
                        var _workPodCount = listOfWorkOrders.Count;
                        var _workpayloadCount = listOfWorkOrders.SelectMany(x => x.Items).Count();
                        agent.ImportSequence(
                            this.WorkOrderAssignAgentParameters.SequenceAgent.GetWorkOrderPodSeqence(
                            this.WorkOrderAssignAgentParameters.SequenceAgent.GetWorkOrderPodRootUID(), _workPodCount),
                            this.WorkOrderAssignAgentParameters.SequenceAgent.GetWorkOrderPayloadSeqenceByTimeSerial(
                                ManifestType.Inbound, _workpayloadCount)
                            );
                        foreach (var inboundWorkOrder in listOfWorkOrders)
                        {
                            result.Add(agent.Execute(inboundWorkOrder, inboundparameters));
                        }
                        if (!result.All(x => x.Success))
                        {
                            rs.Success = false;
                            rs.Message = string.Join(",", result.Select(x => x.Message));
                        }
                        else
                        {
                            rs.Success = true;
                        }
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = string.Join(",", failureallocatedrs.Select(x => x.Message));
                    }
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.INBOUND_RECEIVING_NOT_FIND_LANDINGZONE;
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
    }

}
