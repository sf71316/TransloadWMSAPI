using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.Package.Interfaces.Models;
using YAEP.Package.Constants;
using YAEP.WMS.Constant.Enums;
using YAEP.Core.Party.Interfaces.Models;
using System.Transactions;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundAutoAssignWorkOrderExecutor : AbstractAutoAssignWorkOrderExecutor
    {
        public InboundAutoAssignWorkOrderExecutor(IAutoAssignAgentProviders providers) : base(providers)
        {

        }
        public override IActionResult<bool> Execute(IAutoAssignProcessArgs args)
        {
            var e = args as IAutoAssignInboundProcessArgs;

            var warehouseUID = e.Manifest.WarehouseUID;
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

            var pmgr = this.ProductManager;
            var mergePalletManifests = palletManifests.Select(o =>
            {
                var package = e.Packages.FirstOrDefault(p => p.UID == o.ReceivePackageUID);
                // 計算 Volume & Weight
                var volume = pmgr.CalculateCUFT(package, 1);
                var weight = package.GrossWeight;
                var qty = o.FreeQty;

                o.Volume = volume * qty;
                o.Weight = weight * qty;

                // 篩選有足夠可用空間與重量的Slot
                var slots = e.SlotUsageInfos.Where(slot =>
                {
                    decimal usageVolume = (slot.VolumeLimit - slot.Volume);
                    decimal usageWeight = (slot.WeightLimit - slot.Weight);

                    return (usageVolume >= o.Volume) && (usageWeight >= o.Weight);
                });

                return new { Manifest = o, Slots = slots, Volume = volume, Weight = weight };
            });
            // 沒有任何項目有可Assign的Slot的話, 回傳錯誤訊息
            if (mergePalletManifests.Count(o => o.Slots?.Count() > 0) == 0)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NO_ALLOWED_SLOT);
            }

            var defaultLandingZoneResult = this.Providers.WarehouseManager
                .GetDefaultLandingZone(warehouseUID, SlotType.InboundTemp);
            var defaultLandingZone = defaultLandingZoneResult.Content;
            var manifestType = (ManifestType)e.Manifest.Type;

            var listOfWorkOrders = new List<AssignedWorkOrderInnerCollection>();

            // 建立 Work Order &  Pod & Payload
            foreach (var item in mergePalletManifests)
            {
                if (item.Slots?.Count() == 0)
                {
                    continue;
                }

                // 2019-02-11 一個Pallet 一個POD 一個Payload

                var vesselManifestUnassigned = item.Manifest;
                var qty = vesselManifestUnassigned.FreeQty;

                for (int i = 0; i < qty; i++)
                {
                    // 再次篩選足夠空間&重量的Slot
                    var slots = item.Slots.Where(slot =>
                    {
                        decimal usageVolume = (slot.VolumeLimit - slot.Volume);
                        decimal usageWeight = (slot.WeightLimit - slot.Weight);

                        return (usageVolume >= vesselManifestUnassigned.Volume) && (usageWeight >= vesselManifestUnassigned.Weight);
                    });
                    // 無任何Slot可用, 則跳出程序
                    if (slots.Count() == 0)
                    {
                        return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NO_ALLOWED_SLOT);
                        // continue;
                    }

                    #region Payload

                    // 優先度
                    // Storage Sequence > Volume > Weight
                    var orderedSlots = slots.OrderBy(slot => slot.StorageSequence).ThenBy(slot => (slot.VolumeLimit - slot.Volume)).ThenBy(slot => (slot.WeightLimit - slot.Weight)).ToArray();
                    var suitableSlot = orderedSlots.First();

                    var payloadModel = new AssignedWorkOrderPayloadInnerModel()
                    {
                        PayloadUID = Guid.NewGuid(),
                        // Name = vesselManifest.Name,
                        ItemUID = vesselManifestUnassigned.ItemUID,
                        VesselMainifestUID = vesselManifestUnassigned.VesselMainifestUID,
                        ReceivePackageQty = 1,
                        ReceivePackageUID = vesselManifestUnassigned.ReceivePackageUID,
                        SlotUID = suitableSlot.SlotUID,
                    };

                    // 記憶體物件加上空間&重量使用量
                    suitableSlot.Volume += item.Volume;
                    suitableSlot.Weight += item.Weight;

                    #endregion

                    #region Work Order

                    var inboundWorkOrder = new AssignedWorkOrderInnerCollection();
                    inboundWorkOrder.VesselUID = e.Vessel.UID;
                    inboundWorkOrder.LoadingZoneSlotUID = defaultLandingZone?.UID;
                    inboundWorkOrder.StorageMethod = (int)StorageMethod.Slot;
                    inboundWorkOrder.ServiceType = manifestType;
                    inboundWorkOrder.PodUID = Guid.NewGuid();
                    inboundWorkOrder.Items.Add(payloadModel);

                    #endregion

                    listOfWorkOrders.Add(inboundWorkOrder);
                }

                var parameters = this.Providers.WorkOrderAssignAgentParameters;
                var agent = AbstractWorkOrderAssignAgent.GetAgent(manifestType, parameters);
                foreach (var inboundWorkOrder in listOfWorkOrders)
                {
                    var r = agent.Execute(inboundWorkOrder);
                    if (!r.Success)
                    {
                        return ActionResultTemplates.Result(false, r.Message);
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
