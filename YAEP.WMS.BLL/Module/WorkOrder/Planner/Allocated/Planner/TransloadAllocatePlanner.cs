using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.BLL.Extension;

namespace YAEP.WMS.BLL.Module
{
    internal class TransloadAllocatePlanner : AbstractAllocatePlanner
    {
        public TransloadAllocatePlanner(AllocatedPlannerInitParameters allocatedPlannerInitParameters)
         : base(allocatedPlannerInitParameters)
        {

        }
        /// <summary>
        /// 尚未開放使用
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="passPackageVersion"></param>
        /// <returns></returns>
        public override IEnumerable<AllocatedPlannerResult> PlanByWMS(IEnumerable<AllocatedPlannerInnerParameter> parameters,
            bool passPackageVersion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 提供外部系統要Allocated配貨規劃清單
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IEnumerable<AllocatedPlannerResult> ExternalOrderPlanByWMS(Guid warehouseUID,
            IEnumerable<IVesselManifestModel> parameters, bool passPackageVersion)
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
                    var itmrequestttl = parameters.Select(p => new AllocatedRequesttotalModel
                    {
                        ItemUID = p.ItemUID,
                        PackageUID = p.PackageUID,
                        Qty = p.Qty,
                        OnhandType = p.OnhandType,
                        VesselManifestCollection = new IVesselManifestModel[] { p }
                    });

                    Parallel.ForEach(itmrequestttl, new ParallelOptions
                    {
                        //MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3))
                        MaxDegreeOfParallelism = 1
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
                        if (ProcessAllocatedOnhand(itemgrp, availableOnhandinfos, passPackageVersion, ref allocated, ref onhand))
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
                            MaxDegreeOfParallelism = 1
                            //MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3))
                        }, itemgrp =>
                        {
                            foreach (var item in itemgrp)
                            {
                                var allocated = new List<AllocatedItem>(); //allocated 分配結果使用什麼Payload,數量多少
                                int onhand = 0;//當時能分配的onhand (會轉換成當時package 的數量)
                                IEnumerable<ILocationItemViewModel> availableOnhandinfos;
                                AllocatedPlannerResult e = new AllocatedPlannerResult();
                                //上面已經抓Ispack 資料這邊就不再判斷
                                availableOnhandinfos = temporaryPayloadlist.Content
                                .Where(p => p.ItemUID == item.ItemUID && p.Type == item.OnhandType);
                                //找到產品
                                //暫存庫存副本給後續Allocated 使用
                                var avaonhandclone = temporaryPayloadlistClone.Where(p => p.ItemUID == item.ItemUID && p.Type == item.OnhandType);
                                e.ItemUID = item.ItemUID;
                                e.VesselManifestUID = item.UID;
                                e.VesselUID = item.VesselUID;
                                if (ProcessAllocatedOnhand(item, availableOnhandinfos, passPackageVersion, ref allocated, ref onhand))
                                {
                                    allocated.ForEach(x =>
                                    {
                                        x.AllocateType = AllocateType.TransloadAllocate;
                                    });
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
            return result.ToList();
        }
        protected override bool ProcessAllocatedOnhand(AllocatedRequesttotalModel itemrequest,
           IEnumerable<ILocationItemViewModel> availableonhandinfo, bool passPackageVersion, ref List<AllocatedItem> allocated,
           ref int onhand, bool isFutureAllocate = false)
        {

            //先計算目前onhand 比較總量夠不夠
            if (onhand >= itemrequest.Qty)
            {
                //using (var act2 = this._tracingAgent.StartActivity("allocated planning by slot"))
                //{
                //minipkg
                var requestminipkg = this._PackageMappingCache.GetMinPackage(itemrequest.PackageUID);
                //allocated minipkgqty
                var a_requestminipkgqty = this._PackageMappingCache.GetReceivePackageUomQuantity
                    (itemrequest.PackageUID, requestminipkg.UID, itemrequest.Qty).Content;

                var groupbySlot = availableonhandinfo.Where(x => x.IsPack)
                                    .GroupBy(g => new { g.SlotUID, g.Sequence }).OrderBy(o => o.Key.Sequence);
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
                            e.AllocateType = AllocateType.TransloadAllocate;
                            //onhand mini pkg qty
                            //var o_minipkgqty = item.Quantity;
                            if (onhandPayload.Quantity == 0)
                                continue;
                            if (a_requestminipkgqty == onhandPayload.Quantity) //如果配置數量等於庫存 
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
                                e.ReceivingWorkorderPayloadUID = onhandPayload.ReceivingWorkorderPayloadUID;
                                e.Labels = onhandPayload.Labels;
                                allocated.Add(e);
                                //扣除已分配的數量
                                onhandPayload.Quantity -= a_requestminipkgqty;
                                a_requestminipkgqty = 0;
                                break;
                            }
                            else //若payload 不滿足則換下一個
                            {
                                continue;
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
    }
}
