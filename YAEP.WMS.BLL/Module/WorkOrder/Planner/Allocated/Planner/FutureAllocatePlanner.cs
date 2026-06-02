using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.BLL.Module
{
    internal class FutureAllocatePlanner : AbstractAllocatePlanner, IAllocatePlanner
    {
        public FutureAllocatePlanner(AllocatedPlannerInitParameters allocatedPlannerInitParameters)
            : base(allocatedPlannerInitParameters)
        {

        }
        /// <summary>
        /// 提供內部系統自動配貨規劃清單
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IEnumerable<AllocatedPlannerResult> PlanByWMS(IEnumerable<AllocatedPlannerInnerParameter> parameters,
            bool passPackageVersion, bool isChinaWarehouse)
        {
            var result = new List<AllocatedPlannerResult>();
            var vesselManifestinfos = this._VesselManager.GetVesselManifest(
                new { UID = parameters.Select(p => p.VesselManifestUID) });
            var mInfo = this._VesselManager.GetManifestInfo(vesselManifestinfos.Content.FirstOrDefault().UID);

            //TODO WMS UI Order Type 使用Truckload
            if (mInfo.Content != null)
            {
                return this.ProcessPlanByWMS(mInfo.Content.WarehouseUID, vesselManifestinfos.Content, passPackageVersion, isChinaWarehouse);
            }
            else
            {
                return new List<AllocatedPlannerResult>();
            }
        }
        /// <summary>
        /// 提供外部系統要Allocated配貨規劃清單
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IEnumerable<AllocatedPlannerResult> ExternalOrderPlanByWMS(Guid warehouseUID,
            IEnumerable<IVesselManifestModel> parameters, bool passPackageVersion, bool isChinaWarehouse)
        {
            return this.ProcessPlanByWMS(warehouseUID, parameters, passPackageVersion, isChinaWarehouse);
        }
        protected override IEnumerable<AllocatedPlannerResult> ProcessPlanByWMS(Guid warehouseUID, IEnumerable<IVesselManifestModel> parameters,
            bool passPackageVersion, bool isChinaWarehouse)
        {
            var result = new List<AllocatedPlannerResult>();
            var oparam = new OutboundHomeAddressBuilderInitParameters();
            oparam.ProductCacheManager = this._ProductCacheManager;
            oparam.PackageCacheManager = this._PackageMappingCache;
            oparam.VesselManager = this._VesselManager;
            oparam.WarehouseManger = this._WarehouseManger;
            oparam.PackageVersionManager = this._PackageVersionManager;
            oparam.PackageVersionRepository = this._PackageVersionRepository;
            oparam.TracingAgent = this._tracingAgent;
            var builder = AbstractOutboundHomeAddressBuilder.GetInstance(oparam);
            //取得home location 優先度配置表 
            var homeAddressMap = builder.GetAllocatedHomeAddress(this.OrderType,
                 //parameters.GroupBy(g => g.ItemUID).Select(p => p.Key).ToArray(),
                 parameters.GroupBy(g => new
                 {
                     g.OnhandType
                 }).ToDictionary(o => o.Key.OnhandType, o => o.Select(x => x.ItemUID)),
                 warehouseUID);
            var OnhandResult = homeAddressMap.GetAllLocationItems();
            // onhand 換算到最小包裝數量,以便後續計算
            foreach (var item in OnhandResult)
            {
                var minipkg = this._PackageMappingCache.GetMinPackage(item.OriginalPackageUID);
                item.Quantity = this._PackageMappingCache.GetReceivePackageUomQuantity(item.OriginalPackageUID, minipkg.UID, item.Quantity).Content;
            }
            //TODO 一般產品和虛擬產品分不同邏輯
            foreach (var item in parameters)
            {
                var allocated = new List<AllocatedItem>(); //allocated 分配結果使用什麼Payload,數量多少
                int onhand = 0;//當時能分配的onhand (會轉換成當時package 的數量)

                AllocatedPlannerResult e = new AllocatedPlannerResult();
                var availableOnhandinfos = homeAddressMap.FindOnhandSequenceList(item.ItemUID, item.OnhandType);//找到產品
                e.ItemUID = item.ItemUID;
                e.VesselManifestUID = item.UID;
                e.VesselUID = item.VesselUID;
                if (ProcessAllocatedOnhand(item, availableOnhandinfos, passPackageVersion, ref allocated, ref onhand))
                {
                    e.Items = allocated;
                    e.IsComplete = true;
                }
                else
                {
                    e.IsComplete = false;
                    e.ShortageQty = onhand - item.Qty;
                    e.Onhand = onhand;
                }
                result.Add(e);

            }
            return result;
        }


        /// <summary>
        /// Item配貨，對應Shipping 一次只一筆Package的Item
        /// </summary>
        /// <param name="vesselManifestInfo"></param>
        /// <param name="availableonhandinfo"></param>
        /// <param name="passPackageVersion"></param>
        /// <param name="allocated"></param>
        /// <param name="onhand"></param>
        /// <returns></returns>
        protected override bool ProcessAllocatedOnhand(IVesselManifestModel vesselManifestInfo,
             IEnumerable<ILocationItemViewModel> availableonhandinfo, bool passPackageVersion, ref List<AllocatedItem> allocated, ref int onhand)
        {
            //onhand = availableonhandinfo.Sum(p => this._PackageMappingCache
            //.GetReceivePackageUomQuantity(p.OriginalPackageUID, vesselManifestInfo.PackageUID, p.Quantity).Content);
            onhand = availableonhandinfo.Sum(p =>
            {
                var minipkg = this._PackageMappingCache.GetMinPackage(p.OriginalPackageUID);
                return this._PackageMappingCache
                .GetReceivePackageUomQuantity(p.OriginalPackageUID, minipkg.UID, p.Quantity).Content;
            });
            //minipkg
            var requestminipkg = this._PackageMappingCache.GetMinPackage(vesselManifestInfo.PackageUID);
            //先計算目前onhand 比較總量夠不夠
            if (onhand >= vesselManifestInfo.Qty)
            {
                //allocated minipkgqty
                var a_requestminipkgqty = this._PackageMappingCache.GetReceivePackageUomQuantity
                    (vesselManifestInfo.PackageUID, requestminipkg.UID, vesselManifestInfo.Qty).Content;
                var groupbySlot = availableonhandinfo.GroupBy(g => new { g.SlotUID, g.Sequence }).OrderBy(o => o.Key.Sequence);
                foreach (var onhandByslot in groupbySlot)//onhandByslot
                {
                    var onhandPayloads = findNearestPayload(onhandByslot, vesselManifestInfo.ItemUID, a_requestminipkgqty);
                    foreach (var onhandPayload in onhandPayloads)
                    {
                        if (onhandPayload != null)
                        {
                            var a_onhandminipkg = this._PackageMappingCache.GetMinPackage(onhandPayload.OriginalPackageUID);
                            //當無設定略過包裝版本
                            if (requestminipkg.UID != a_onhandminipkg.UID && !passPackageVersion)
                            {
                                break;
                            }
                            var e = new AllocatedItem();
                            //onhand mini pkg qty
                            //var o_minipkgqty = item.Quantity;
                            if (onhandPayload.Quantity == 0)
                                continue;
                            if (a_requestminipkgqty <= onhandPayload.Quantity) //如果配置數量小於等於庫存 
                            {
                                e.PayloadUID = onhandPayload.UID;
                                //將 mini pkg allocated qty 轉成與payload 相符的數量
                                if (!passPackageVersion)
                                {
                                    e.AllocatedQty = this._PackageMappingCache.GetReceivePackageUomQuantity
                                    (requestminipkg.UID, vesselManifestInfo.PackageUID, a_requestminipkgqty).Content;
                                    e.AllocatedPackageUID = vesselManifestInfo.PackageUID;
                                }
                                else
                                {
                                    e.AllocatedQty = this._PackageMappingCache.GetReceivePackageUomQuantity
                                   (a_onhandminipkg.UID, onhandPayload.OriginalPackageUID, a_requestminipkgqty).Content;
                                    e.AllocatedPackageUID = onhandPayload.OriginalPackageUID;
                                }
                                allocated.Add(e);
                                //扣除已分配的數量
                                onhandPayload.Quantity -= a_requestminipkgqty;
                                a_requestminipkgqty = 0;
                                break;
                            }
                            else
                            {
                                e.PayloadUID = onhandPayload.UID;
                                e.AllocatedQty = onhandPayload.Quantity;
                                allocated.Add(e);
                                a_requestminipkgqty -= onhandPayload.Quantity;
                                onhandPayload.Quantity = 0;
                            }

                        }
                    }
                    if (a_requestminipkgqty == 0)
                        break;
                }
                if (a_requestminipkgqty == 0)
                    return true;
                else
                {
                    this._tracingAgent.Trace($"itemuid:{vesselManifestInfo.ItemUID} onhand:{onhand}  request remain qty:{a_requestminipkgqty}");
                    var e = new AllocatedItem();
                    e.PayloadUID = Guid.NewGuid();
                    e.AllocatedPackageUID = requestminipkg.UID;
                    e.AllocatedQty = vesselManifestInfo.Qty;
                    e.AllocateType = AllocateType.FutureAllocate;
                    allocated.Add(e);
                    return true;
                }
            }
            else
            {
                this._tracingAgent.Trace($"itemuid:{vesselManifestInfo.ItemUID} onhand:{onhand}  request qty:{vesselManifestInfo.Qty}");
                var e = new AllocatedItem();
                e.PayloadUID = Guid.NewGuid();
                e.AllocatedPackageUID = requestminipkg.UID;
                e.AllocatedQty = vesselManifestInfo.Qty;
                e.AllocateType = AllocateType.FutureAllocate;
                allocated.Add(e);
                return true;
            }
        }


    }
}
