using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class FutureFullAllocatePlanner : AbstractAllocatePlanner, IAllocatePlanner
    {
        public FutureFullAllocatePlanner(AllocatedPlannerInitParameters allocatedPlannerInitParameters)
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
            throw new NotImplementedException();
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
            var result = new ConcurrentBag<AllocatedPlannerResult>();
            var preallocatedresult = new ConcurrentBag<AllocatedPlannerResult>();
            var oparam = new OutboundHomeAddressBuilderInitParameters();
            IActionResult<List<ILocationItemViewModel>> temporaryPayloadlist = null;
            oparam.ProductCacheManager = this._ProductCacheManager;
            oparam.PackageCacheManager = this._PackageMappingCache;
            oparam.VesselManager = this._VesselManager;
            oparam.WarehouseManger = this._WarehouseManger;
            oparam.PackageVersionManager = this._PackageVersionManager;
            oparam.PackageVersionRepository = this._PackageVersionRepository;
            oparam.TracingAgent = this._tracingAgent;
            OutboundHomeAddressMap homeAddressMap;
            IEnumerable<ILocationItemViewModel> OnhandResult;
            var builder = AbstractOutboundHomeAddressBuilder.GetInstance(oparam);

            try
            {
                #region 整張規劃庫存並將庫存預扣
                using (var act1 = this._tracingAgent.StartActivity("取得訂單所需要的庫存"))
                {

                    //取得home location 優先度配置表 
                    using (var act2 = this._tracingAgent.StartActivity("將庫存與Home location 資料進行合併"))
                    {
                        homeAddressMap = builder.GetAllocatedHomeAddress(
                            this.OrderType,
                             //parameters.GroupBy(g => g.ItemUID).Select(p => p.Key).ToArray(),
                             parameters.GroupBy(g => new
                             {
                                 g.OnhandType
                             }).ToDictionary(o => o.Key.OnhandType, o => o.Select(x => x.ItemUID)),
                            warehouseUID);
                        OnhandResult = homeAddressMap.GetAllLocationItems();
                    }
                    // onhand 換算到最小包裝數量,以便後續計算
                    using (var act3 = this._tracingAgent.StartActivity("換算到最小包裝數量"))
                    {
                        Parallel.ForEach(OnhandResult, item =>
                        {
                            var minipkg = this._PackageMappingCache.GetMinPackage(item.OriginalPackageUID);
                            item.Quantity = this._PackageMappingCache.GetReceivePackageUomQuantity(item.OriginalPackageUID, minipkg.UID, item.Quantity).Content;
                        });

                    }
                }
                using (var act2 = this._tracingAgent.StartActivity("進行整張訂單庫存規劃"))
                {
                    //統計所有訂單各產品要求的數量
                    var itmrequestttl = parameters.GroupBy(g => new
                    {
                        ItemUID = g.ItemUID,
                        PackageUID = g.PackageUID,
                        OnhandType = g.OnhandType
                    }).Select(p => new AllocatedRequesttotalModel
                    {
                        ItemUID = p.Key.ItemUID,
                        PackageUID = p.Key.PackageUID,
                        Qty = p.Sum(x => x.Qty),
                        OnhandType = p.Key.OnhandType,
                        VesselManifestCollection = p
                    });

                    Parallel.ForEach(itmrequestttl, new ParallelOptions
                    {
                        //MaxDegreeOfParallelism = Environment.ProcessorCount / 2
                    }, itemgrp =>
                    {
                        var allocated = new List<AllocatedItem>(); //allocated 分配結果使用什麼Payload,數量多少
                        int onhand = 0;//當時能分配的onhand (會轉換成當時package 的數量)
                        IEnumerable<ILocationItemViewModel> availableOnhandinfos;
                        AllocatedPlannerResult e = new AllocatedPlannerResult();
                        availableOnhandinfos = homeAddressMap.FindOnhandSequenceList(itemgrp.ItemUID, itemgrp.OnhandType);//找到產品

                        e.ItemUID = itemgrp.ItemUID;
                        e.VesselManifestCollection = itemgrp.VesselManifestCollection;
                        onhand = availableOnhandinfos.Sum(p =>
                        {
                            var minipkg = this._PackageMappingCache.GetMinPackage(p.OriginalPackageUID);
                            return this._PackageMappingCache
                            .GetReceivePackageUomQuantity(p.OriginalPackageUID, minipkg.UID, p.Quantity).Content;
                        });
                        if (ProcessAllocatedOnhand(itemgrp, availableOnhandinfos, passPackageVersion, ref allocated, ref onhand, true))
                        {
                            e.Items = allocated;
                            e.IsComplete = true;

                        }
                        else
                        {
                            e.IsComplete = false;
                            e.ShortageQty = onhand - itemgrp.Qty;
                            e.Onhand = onhand;
                        }
                        preallocatedresult.Add(e);
                    });

                    #region Debug
                    //foreach (var itemgrp in itmrequestttl)
                    //{
                    //    var allocated = new List<AllocatedItem>(); //allocated 分配結果使用什麼Payload,數量多少
                    //    int onhand = 0;//當時能分配的onhand (會轉換成當時package 的數量)
                    //    IEnumerable<ILocationItemViewModel> availableOnhandinfos;
                    //    AllocatedPlannerResult e = new AllocatedPlannerResult();
                    //    availableOnhandinfos = homeAddressMap.FindOnhandSequenceList(itemgrp.ItemUID, itemgrp.OnhandType);//找到產品

                    //    e.ItemUID = itemgrp.ItemUID;
                    //    e.VesselManifestCollection = itemgrp.VesselManifestCollection;
                    //    onhand = availableOnhandinfos.Sum(p =>
                    //    {
                    //        var minipkg = this._PackageMappingCache.GetMinPackage(p.OriginalPackageUID);
                    //        return this._PackageMappingCache
                    //        .GetReceivePackageUomQuantity(p.OriginalPackageUID, minipkg.UID, p.Quantity).Content;
                    //    });
                    //    if (ProcessAllocatedOnhand(itemgrp, availableOnhandinfos, passPackageVersion, ref allocated, ref onhand, true))
                    //    {
                    //        e.Items = allocated;
                    //        e.IsComplete = true;

                    //    }
                    //    else
                    //    {
                    //        e.IsComplete = false;
                    //        e.ShortageQty = onhand - itemgrp.Qty;
                    //        e.Onhand = onhand;
                    //    }
                    //    preallocatedresult.Add(e);
                    //}
                    #endregion
                }

                if (preallocatedresult.All(p => p.IsComplete) && preallocatedresult.Count > 0)
                {
                    using (var act3 = this._tracingAgent.StartActivity("進行整張訂單配貨"))
                    {
                        temporaryPayloadlist = this._AllocatedExecutor.ExecuteAllocated(preallocatedresult.SelectMany(p => p.Items));
                    }
                }
                #endregion
                #region 照Vessel進行規劃
                if (temporaryPayloadlist != null && temporaryPayloadlist.Success)
                {
                    using (var act4 = this._tracingAgent.StartActivity("照Vessel進行規劃"))
                    {
                        var vesselgroup = parameters.GroupBy(g => g.ItemUID);
                        //建立暫存庫存副本

                        var temporaryPayloadlistClone = temporaryPayloadlist.Content.Clone<List<ILocationItemViewModel>>();
                        Parallel.ForEach(vesselgroup, new ParallelOptions
                        {
                            //MaxDegreeOfParallelism = Environment.ProcessorCount / 2
                        }, itemgrp =>
                        {
                            foreach (var item in itemgrp)
                            {
                                var allocated = new List<AllocatedItem>(); //allocated 分配結果使用什麼Payload,數量多少
                                int onhand = 0;//當時能分配的onhand (會轉換成當時package 的數量)
                                IEnumerable<ILocationItemViewModel> availableOnhandinfos;
                                AllocatedPlannerResult e = new AllocatedPlannerResult();

                                availableOnhandinfos = temporaryPayloadlist.Content
                                .Where(p => p.ItemUID == item.ItemUID && p.Type == item.OnhandType);//找到產品
                                                                                                    //暫存庫存副本給後續Allocated 使用
                                var avaonhandclone = temporaryPayloadlistClone.Where(p => p.ItemUID == item.ItemUID && p.Type == item.OnhandType);
                                e.ItemUID = item.ItemUID;
                                e.VesselManifestUID = item.UID;
                                e.VesselUID = item.VesselUID;
                                if (ProcessAllocatedOnhand(item, availableOnhandinfos, passPackageVersion, ref allocated, ref onhand))
                                {
                                    e.Items = allocated;
                                    var onhands = avaonhandclone.Where(p => allocated.Any(x => x.PayloadUID == p.PayloadUID));
                                    e.OnhandPayloadItems.AddRange(onhands);
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

                        });
                        #region debug
                        //foreach (var itemgrp in vesselgroup)
                        //{
                        //    foreach (var item in itemgrp)
                        //    {

                        //        var allocated = new List<AllocatedItem>(); //allocated 分配結果使用什麼Payload,數量多少
                        //        int onhand = 0;//當時能分配的onhand (會轉換成當時package 的數量)
                        //        IEnumerable<ILocationItemViewModel> availableOnhandinfos;
                        //        AllocatedPlannerResult e = new AllocatedPlannerResult();

                        //        availableOnhandinfos = temporaryPayloadlist.Content.Where(p => p.ItemUID == item.ItemUID);//找到產品
                        //        //暫存庫存副本給後續Allocated 使用
                        //        var avaonhandclone = temporaryPayloadlistClone.Where(p => p.ItemUID == item.ItemUID);
                        //        e.ItemUID = item.ItemUID;
                        //        e.VesselManifestUID = item.UID;
                        //        e.VesselUID = item.VesselUID;
                        //        if (ProcessAllocatedOnhand(item, availableOnhandinfos, passPackageVersion, ref allocated, ref onhand))
                        //        {
                        //            e.Items = allocated;
                        //            var onhands = avaonhandclone.Where(p => allocated.Any(x => x.PayloadUID == p.PayloadUID));
                        //            e.OnhandPayloadItems.AddRange(onhands);
                        //            e.IsComplete = true;
                        //        }
                        //        else
                        //        {
                        //            e.IsComplete = false;
                        //            e.ShortageQty = onhand - item.Qty;
                        //            e.Onhand = onhand;
                        //        }
                        //        result.Add(e);
                        //    }
                        //}
                        #endregion
                    }
                }
                else
                {
                    this._tracingAgent.Trace($"temporary payload not completed");
                    if (preallocatedresult.All(p => p.IsComplete))//總規劃完成，但取庫存卻失敗
                    {
                        result = new ConcurrentBag<AllocatedPlannerResult>();
                    }
                    else
                    {
                        result = preallocatedresult;
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                this._tracingAgent.Trace($"temporary payload plan exception {ex.Message}", ex);
                result = new ConcurrentBag<AllocatedPlannerResult>();
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
             IEnumerable<ILocationItemViewModel> availableonhandinfo, bool passPackageVersion,
             ref List<AllocatedItem> allocated, ref int onhand)
        {

            onhand = availableonhandinfo.Sum(p =>
            {
                var minipkg = this._PackageMappingCache.GetMinPackage(p.OriginalPackageUID);
                return this._PackageMappingCache
                .GetReceivePackageUomQuantity(p.OriginalPackageUID, minipkg.UID, p.Quantity).Content;
            });
            //minipkg
            var requestminipkg = this._PackageMappingCache.GetMinPackage(vesselManifestInfo.PackageUID);
            //先計算目前onhand 比較總量夠不夠
            if (true)
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
                    //if (a_requestminipkgqty > 0) 必需分配完所有slot onhand 
                    //{
                    //    var e = new AllocatedItem();
                    //    e.PayloadUID = Guid.NewGuid();
                    //    e.AllocatedPackageUID = requestminipkg.UID;
                    //    e.AllocatedQty = a_requestminipkgqty;
                    //    e.AllocateType = AllocateType.FutureAllocate;
                    //    allocated.Add(e);
                    //    return true;
                    //}
                    if (a_requestminipkgqty == 0)
                        break;
                }
                if (a_requestminipkgqty == 0)
                {
                    return true;
                }
                else
                {
                    this._tracingAgent.Trace($"itemuid:{vesselManifestInfo.ItemUID} onhand:{onhand}  request remain qty:{a_requestminipkgqty}");
                    var e = new AllocatedItem();
                    e.PayloadUID = Guid.NewGuid();
                    e.AllocatedPackageUID = requestminipkg.UID;
                    //e.AllocatedQty = vesselManifestInfo.Qty;
                    e.AllocatedQty = a_requestminipkgqty;
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
