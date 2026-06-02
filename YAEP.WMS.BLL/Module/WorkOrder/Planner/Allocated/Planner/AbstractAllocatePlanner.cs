using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractAllocatePlanner : IAllocatePlanner
    {
        protected IVesselManager _VesselManager;
        protected IPackageVersionManager _PackageVersionManager;
        protected IPackageVersionRepository _PackageVersionRepository;
        //protected IPackageManager _PackageManager;
        protected IWarehouseManger _WarehouseManger;
        protected PackageCacheManager _PackageMappingCache;
        protected ProductCacheManager _ProductCacheManager;
        protected ITracingAgent _tracingAgent;
        protected FullAllocatedTemporaryOnhandExecutor _AllocatedExecutor;
        protected int OrderType { get; set; }
        public AbstractAllocatePlanner(AllocatedPlannerInitParameters allocatedPlannerInitParameters)
        {
            //this._PackageManager = allocatedPlannerInitParameters.PackageManager;
            this._VesselManager = allocatedPlannerInitParameters.VesselManager;
            this._PackageMappingCache = allocatedPlannerInitParameters.PackageMappingCache;
            _ProductCacheManager = allocatedPlannerInitParameters.ProductCache;
            _WarehouseManger = allocatedPlannerInitParameters.WarehouseManger;
            OrderType = allocatedPlannerInitParameters.OrderType;
            _PackageVersionManager = allocatedPlannerInitParameters.PackageVersionManager;
            _tracingAgent = allocatedPlannerInitParameters.TracingAgent;
            _AllocatedExecutor = allocatedPlannerInitParameters.AllocatedExecutor;
            _PackageVersionRepository = allocatedPlannerInitParameters.PackageVersionRepository;
        }

        public abstract IEnumerable<AllocatedPlannerResult> ExternalOrderPlanByWMS(Guid warehouseUID,
            IEnumerable<IVesselManifestModel> parameters, bool passPackageVersion, bool isChinaWarehouse);

        public abstract IEnumerable<AllocatedPlannerResult> PlanByWMS(
            IEnumerable<AllocatedPlannerInnerParameter> parameters, bool passPackageVersion, bool isChinaWarehouse);


        protected virtual IEnumerable<AllocatedPlannerResult> ProcessPlanByWMS(Guid warehouseUID, IEnumerable<IVesselManifestModel> parameters,
           bool passPackageVersion, bool isChinaWarehouse)
        {
            var result = new ConcurrentBag<AllocatedPlannerResult>();
            //var result = new List<AllocatedPlannerResult>();
            var oparam = new OutboundHomeAddressBuilderInitParameters();
            oparam.ProductCacheManager = this._ProductCacheManager;
            oparam.PackageCacheManager = this._PackageMappingCache;
            oparam.VesselManager = this._VesselManager;
            oparam.WarehouseManger = this._WarehouseManger;
            oparam.PackageVersionManager = this._PackageVersionManager;
            oparam.PackageVersionRepository = this._PackageVersionRepository;
            oparam.TracingAgent = this._tracingAgent;
            OutboundHomeAddressMap homeAddressMap;
            IEnumerable<ILocationItemViewModel> OnhandResult;
            using (var act1 = this._tracingAgent.StartActivity("prepare onhand data "))
            {
                var builder = AbstractOutboundHomeAddressBuilder.GetInstance(oparam);
                //取得home location 優先度配置表 
                using (var act2 = this._tracingAgent.StartActivity("get home location and onhand"))
                {
                    homeAddressMap = builder.GetAllocatedHomeAddress(this.OrderType,
                        //parameters.GroupBy(g => g.ItemUID).Select(p => p.Key).ToArray(),
                        parameters.GroupBy(g => new
                        {
                            g.OnhandType
                        }).ToDictionary(o => o.Key.OnhandType, o => o.Select(x => x.ItemUID)),
                    warehouseUID);
                    OnhandResult = homeAddressMap.GetAllLocationItems();
                }
                // onhand 換算到最小包裝數量,以便後續計算
                using (var act3 = this._tracingAgent.StartActivity("cal min pkg"))
                {
                    //foreach (var item in OnhandResult)
                    //{
                    //    var minipkg = this._PackageMappingCache.GetMinPackage(item.OriginalPackageUID);
                    //    item.Quantity = this._PackageMappingCache.GetReceivePackageUomQuantity(item.OriginalPackageUID, minipkg.UID, item.Quantity).Content;
                    //}
                    Parallel.ForEach(OnhandResult, item =>
                    {
                        var minipkg = this._PackageMappingCache.GetMinPackage(item.OriginalPackageUID);
                        item.Quantity = this._PackageMappingCache.GetReceivePackageUomQuantity(item.OriginalPackageUID, minipkg.UID, item.Quantity).Content;
                    });

                }
            }
            //TODO 一般產品和虛擬產品分不同邏輯
            //var paramgroup = parameters.Select(x => x as VesselManifestItemInnerModel).GroupBy(g => g.ItemGroupUID);
            //var paramregular = paramgroup.Where(p => !p.Key.HasValue).SelectMany(o => o);
            //var paramvi = paramgroup.Where(p => p.Key.HasValue);
            #region 一般產品
            using (var act4 = this._tracingAgent.StartActivity("allocate planning"))
            {
                //TODO 先group item 後再用平行處理各別進行allocated plan
                var vesselgroup = parameters.GroupBy(g => g.ItemUID);
                Parallel.ForEach(vesselgroup, new ParallelOptions
                {
                    //MaxDegreeOfParallelism = Environment.ProcessorCount / 2
                }, itemgrp =>
                {
                    //using (var act5 = this._tracingAgent.StartActivity($" planning onhand data"))
                    //{
                    //act5.SetTag("Planning count", itemgrp.Count());
                    //act5.SetTag("ItemUID", itemgrp.Key);
                    //act5.SetTag("BolUID", itemgrp.First().BolUID);
                    foreach (var item in itemgrp)
                    {

                        var allocated = new List<AllocatedItem>(); //allocated 分配結果使用什麼Payload,數量多少
                        int onhand = 0;//當時能分配的onhand (會轉換成當時package 的數量)
                        IEnumerable<ILocationItemViewModel> availableOnhandinfos;
                        AllocatedPlannerResult e = new AllocatedPlannerResult();

                        availableOnhandinfos = homeAddressMap.FindOnhandSequenceList(item.ItemUID, item.OnhandType);//找到產品

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
                    //}
                });

            }
            #endregion


            return result.ToList();
        }
        /// <summary>
        /// Allocated 規劃庫存評估方法
        /// </summary>
        /// <param name="vesselManifestInfo"></param>
        /// <param name="availableonhandinfo"></param>
        /// <param name="passPackageVersion"></param>
        /// <param name="allocated"></param>
        /// <param name="onhand"></param>
        /// <returns></returns>
        protected virtual bool ProcessAllocatedOnhand(IVesselManifestModel vesselManifestInfo,
            IEnumerable<ILocationItemViewModel> availableonhandinfo, bool passPackageVersion, ref List<AllocatedItem> allocated, ref int onhand)
        {

            //onhand = availableonhandinfo.Sum(p => this._PackageMappingCache
            //.GetReceivePackageUomQuantity(p.OriginalPackageUID, vesselManifestInfo.PackageUID, p.Quantity).Content);
            //using (var act = this._tracingAgent.StartActivity("cal item total onhand"))
            //{
            onhand = availableonhandinfo.Sum(p =>
                {
                    var minipkg = this._PackageMappingCache.GetMinPackage(p.OriginalPackageUID);
                    return this._PackageMappingCache
                    .GetReceivePackageUomQuantity(p.OriginalPackageUID, minipkg.UID, p.Quantity).Content;
                });
            //}
            //先計算目前onhand 比較總量夠不夠
            if (onhand >= vesselManifestInfo.Qty)
            {
                //using (var act2 = this._tracingAgent.StartActivity("allocated planning by slot"))
                //{
                //minipkg
                var requestminipkg = this._PackageMappingCache.GetMinPackage(vesselManifestInfo.PackageUID);
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
                                    //比對request 的pkg uid 是否與onhand mini pkg 同級 
                                    if (requestminipkg.UOM == a_onhandminipkg.UOM)
                                    {
                                        e.AllocatedQty = a_requestminipkgqty;
                                    }
                                    else //不同級比對目前尚未完成測試
                                    {
                                        //最小包裝的要求數量 (不同版本包裝最小包裝數量一致)
                                        var reqqty = this._PackageMappingCache.GetReceivePackageUomQuantity
                                       (requestminipkg.UID, vesselManifestInfo.PackageUID, a_requestminipkgqty).Content;

                                        e.AllocatedQty = reqqty;
                                    }
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
                    return false;
                //}
            }
            else
            {
                return false;
            }
        }
        protected virtual bool ProcessAllocatedOnhandByVirtualItem(IEnumerable<IVesselManifestModel> vesselManifestInfos,
            IEnumerable<ILocationItemViewModel> availableonhandinfo, bool passPackageVersion, ref List<AllocatedItem> allocated, ref int onhand)
        {
            return false;
            //List<bool> viResult = new List<bool>();
            //List<int> viOnhand = new List<int>();
            //List<int> viRequestQty = new List<int>();
            //int requestQty = 0;
            ////計算該虛擬產品中最小庫存，確保不會overbooking
            ////計算該虛擬產品中最大要求量
            //foreach (var item in vesselManifestInfos.GroupBy(p => p.ItemUID))
            //{
            //    viOnhand.Add(
            //        availableonhandinfo.Where(p => p.ItemUID == item.Key).Sum(p =>
            //            {
            //                var minipkg = this._PackageMappingCache.GetMinPackage(p.OriginalPackageUID);
            //                return this._PackageMappingCache
            //                .GetReceivePackageUomQuantity(p.OriginalPackageUID, minipkg.UID, p.Quantity).Content;
            //            })
            //        );
            //    viRequestQty.Add(item.Sum(p => p.Qty));
            //}
            ////正常情況各虛擬產品所有 onhand 總合應都相等
            //onhand = viOnhand.Min();
            ////正常情況各虛擬產品request qty應都相等
            //requestQty = viRequestQty.Max();

            ////先計算目前onhand 比較總量夠不夠
            //if (onhand >= requestQty)
            //{
            //    //1.第1個虛擬產品先照原本模式進行配貨規劃
            //    //2.其餘虛擬產品則照第一個虛擬產品的規劃結果slot進行配貨規劃
            //    //minipkg
            //    var requestminipkg = this._PackageMappingCache.GetMinPackage(vesselManifestInfos.PackageUID);
            //    //allocated minipkgqty
            //    var a_requestminipkgqty = this._PackageMappingCache.GetReceivePackageUomQuantity
            //        (vesselManifestInfos.PackageUID, requestminipkg.UID, vesselManifestInfos.Qty).Content;

            //    var groupbySlot = availableonhandinfo.GroupBy(g => new { g.SlotUID, g.Sequence }).OrderBy(o => o.Key.Sequence);
            //    foreach (var onhandByslot in groupbySlot)//onhandByslot
            //    {
            //        var onhandPayloads = findNearestPayload(onhandByslot, vesselManifestInfos, a_requestminipkgqty);
            //        foreach (var onhandPayload in onhandPayloads)
            //        {
            //            if (onhandPayload != null)
            //            {
            //                var a_onhandminipkg = this._PackageMappingCache.GetMinPackage(onhandPayload.OriginalPackageUID);
            //                //當無設定略過包裝版本
            //                if (requestminipkg.UID != a_onhandminipkg.UID && !passPackageVersion)
            //                {
            //                    break;
            //                }
            //                var e = new AllocatedItem();
            //                //onhand mini pkg qty
            //                //var o_minipkgqty = item.Quantity;
            //                if (onhandPayload.Quantity == 0)
            //                    continue;
            //                if (a_requestminipkgqty <= onhandPayload.Quantity) //如果配置數量小於等於庫存 
            //                {
            //                    e.PayloadUID = onhandPayload.UID;
            //                    //將 mini pkg allocated qty 轉成與payload 相符的數量
            //                    if (!passPackageVersion)
            //                    {
            //                        e.AllocatedQty = this._PackageMappingCache.GetReceivePackageUomQuantity
            //                        (requestminipkg.UID, vesselManifestInfos.PackageUID, a_requestminipkgqty).Content;
            //                        e.AllocatedPackageUID = vesselManifestInfos.PackageUID;
            //                    }
            //                    else
            //                    {
            //                        e.AllocatedQty = this._PackageMappingCache.GetReceivePackageUomQuantity
            //                       (a_onhandminipkg.UID, onhandPayload.OriginalPackageUID, a_requestminipkgqty).Content;
            //                        e.AllocatedPackageUID = onhandPayload.OriginalPackageUID;
            //                    }
            //                    allocated.Add(e);
            //                    //扣除已分配的數量
            //                    onhandPayload.Quantity -= a_requestminipkgqty;
            //                    a_requestminipkgqty = 0;
            //                    break;
            //                }
            //                else
            //                {
            //                    e.PayloadUID = onhandPayload.UID;
            //                    e.AllocatedQty = onhandPayload.Quantity;
            //                    allocated.Add(e);
            //                    a_requestminipkgqty -= onhandPayload.Quantity;
            //                    onhandPayload.Quantity = 0;
            //                }

            //            }
            //        }
            //        if (a_requestminipkgqty == 0)
            //            break;
            //    }
            //    if (a_requestminipkgqty == 0)
            //        return true;
            //    else
            //        return false;

            //    return viResult.All(x => x);
            //}
            //else
            //{
            //    return false;
            //}
        }

        protected virtual IEnumerable<ILocationItemViewModel> findNearestPayload(IGrouping<object, ILocationItemViewModel> onhandByslot,
           Guid itemUID, int minPkgallocatedQty)
        {

            //item (實體item)在同個Slot中取Payload的優先順序
            //1.Package 版本最舊先拿
            //2.取出與"Allocated數量"最近的"onhand"的payload
            //3.若找不到則從payload 數量最大的開始拿
            var itemInfo = this._ProductCacheManager.GetItem(itemUID) as IProductExtendModel;

            //var nearest = onhandByslot.OrderBy(p => p.Labels.Min(x => minPkgallocatedQty - x.AddQty));
            if (string.IsNullOrEmpty(itemInfo.ActualProduct))
            {
                //找未拆過的Pallet
                var getPalletRS = onhandByslot;
                if (getPalletRS != null)
                {
                    //依離目前要求數量最近的Pallet做排序
                    var PalletSeqRS = getPalletRS.ToList();
                    if (PalletSeqRS != null && PalletSeqRS.Count() > 0)
                    {
                        PalletSeqRS = PalletSeqRS
                            .OrderBy(o => o.PackageSerialNumber)
                            .OrderBy(o =>
                            {
                                //var s = 0;
                                //if (o.Labels != null && o.Labels.Count() > 0)
                                //{
                                //    s = Math.Abs(o.Labels.Min(x => (x.AddQty - minPkgallocatedQty)));
                                //}
                                //else
                                //{
                                //    s = int.MaxValue;
                                //}
                                //return s;
                                return Math.Abs(o.Quantity - minPkgallocatedQty);
                            })
                       .ToList();
                        return PalletSeqRS;
                    }
                }
            }
            return onhandByslot.OrderBy(o => o.PackageSerialNumber).OrderBy(p => Math.Abs(p.Quantity - minPkgallocatedQty));


        }

        protected virtual bool ProcessAllocatedOnhand(AllocatedRequesttotalModel itemrequest,
            IEnumerable<ILocationItemViewModel> availableonhandinfo, bool passPackageVersion, ref List<AllocatedItem> allocated,
            ref int onhand, bool isFutureAllocate = false)
        {

            //先計算目前onhand 比較總量夠不夠
            if ((onhand >= itemrequest.Qty) || isFutureAllocate)//|| isFutureAllocate
            {
                //using (var act2 = this._tracingAgent.StartActivity("allocated planning by slot"))
                //{
                //minipkg
                var requestminipkg = this._PackageMappingCache.GetMinPackage(itemrequest.PackageUID);
                //allocated minipkgqty
                var a_requestminipkgqty = this._PackageMappingCache.GetReceivePackageUomQuantity
                    (itemrequest.PackageUID, requestminipkg.UID, itemrequest.Qty).Content;

                var groupbySlot = availableonhandinfo.GroupBy(g => new { g.SlotUID, g.Sequence }).OrderBy(o => o.Key.Sequence);
                foreach (var onhandByslot in groupbySlot)//onhandByslot
                {
                    var onhandPayloads = findNearestPayload(onhandByslot, itemrequest.ItemUID, a_requestminipkgqty);
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
                                    (requestminipkg.UID, itemrequest.PackageUID, a_requestminipkgqty).Content;
                                    e.AllocatedPackageUID = itemrequest.PackageUID;
                                }
                                else
                                {
                                    //比對request 的pkg uid 是否與onhand mini pkg 同級 
                                    if (requestminipkg.UOM == a_onhandminipkg.UOM)
                                    {
                                        e.AllocatedQty = a_requestminipkgqty;
                                    }
                                    else //不同級比對目前尚未完成測試
                                    {
                                        //最小包裝的要求數量 (不同版本包裝最小包裝數量一致)
                                        var reqqty = this._PackageMappingCache.GetReceivePackageUomQuantity
                                       (requestminipkg.UID, itemrequest.PackageUID, a_requestminipkgqty).Content;

                                        e.AllocatedQty = reqqty;
                                    }
                                    e.AllocatedPackageUID = a_onhandminipkg.UID;//與onhand 同版本最小包裝
                                }
                                allocated.Add(e);
                                //扣除已分配的數量
                                onhandPayload.Quantity -= a_requestminipkgqty;
                                a_requestminipkgqty = 0;
                                break;
                            }
                            else //因為該payload 不滿足，所以也先拿滿整個payload onhand 的量
                            {
                                //比對request 的pkg uid 是否與onhand mini pkg 同級 
                                //if (requestminipkg.UOM == a_onhandminipkg.UOM)
                                //{
                                //    e.AllocatedQty = onhandPayload.Quantity;
                                //}
                                //else //不同級比對目前尚未完成測試
                                //{
                                //    //最小包裝的要求數量 (不同版本包裝最小包裝數量一致)
                                //    var reqqty = this._PackageMappingCache.GetReceivePackageUomQuantity
                                //   (requestminipkg.UID, itemrequest.PackageUID, a_requestminipkgqty).Content;

                                //    e.AllocatedQty = reqqty;
                                //}
                                //e.PayloadUID = onhandPayload.UID;
                                //e.AllocatedPackageUID = a_onhandminipkg.UID;//與onhand 同版本最小包裝
                                //allocated.Add(e);
                                //a_requestminipkgqty -= onhandPayload.Quantity;
                                //onhandPayload.Quantity = 0;
                                e.PayloadUID = onhandPayload.UID;
                                e.AllocatedQty = onhandPayload.Quantity;
                                e.AllocatedPackageUID = a_onhandminipkg.UID;
                                allocated.Add(e);
                                a_requestminipkgqty -= onhandPayload.Quantity;
                                onhandPayload.Quantity = 0;
                            }

                        }

                    }
                    if (a_requestminipkgqty == 0)
                        break;
                }
                if (a_requestminipkgqty > 0 && isFutureAllocate) //如果還有剩餘數量，則進行future allocate
                {
                    var e = new AllocatedItem();
                    e.PayloadUID = Guid.NewGuid();
                    e.AllocatedPackageUID = itemrequest.PackageUID;
                    e.AllocatedQty = a_requestminipkgqty;
                    e.AllocateType = AllocateType.FutureAllocate;
                    allocated.Add(e);
                    a_requestminipkgqty = 0;
                }

                if (a_requestminipkgqty == 0)
                    return true;
                else
                    return false;
                //}
            }
            else
            {
                if (isFutureAllocate)
                {
                    var e = new AllocatedItem();
                    e.PayloadUID = Guid.NewGuid();
                    e.AllocatedPackageUID = itemrequest.PackageUID;
                    e.AllocatedQty = itemrequest.Qty;
                    e.AllocateType = AllocateType.FutureAllocate;
                    allocated.Add(e);
                    return true;
                }
                return false;
            }
        }
        public static AbstractAllocatePlanner GetInstance(AllocatedPlannerInitParameters parameters, AllocateType type)
        {
            switch (type)
            {
                case AllocateType.GeneralAllocate:
                    //return new AllocatePlanner(parameters);
                    return new FullAllocatedPlanner(parameters);
                case AllocateType.FutureAllocate:
                    //return new FutureAllocatePlanner(parameters);
                    return new FutureFullAllocatePlanner(parameters);
                default:
                    return new FullAllocatedPlanner(parameters);
            }
        }
    }

}
