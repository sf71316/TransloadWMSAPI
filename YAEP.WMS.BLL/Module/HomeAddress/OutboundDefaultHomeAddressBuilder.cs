using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundDefaultHomeAddressBuilder : AbstractOutboundHomeAddressBuilder
    {
        protected IVesselManager VesselManager { get; set; }
        protected IWarehouseManger WarehouseManger { get; set; }
        protected ProductCacheManager ProductCacheManager { get; set; }
        protected PackageCacheManager PackageCacheManager { get; set; }
        protected IPackageVersionManager PackageVersionManager { get; set; }
        protected IPackageVersionRepository PackageVersionRepository { get; set; }
        public OutboundDefaultHomeAddressBuilder(OutboundHomeAddressBuilderInitParameters initParameters) :
            base(initParameters)
        {
            this.VesselManager = initParameters.VesselManager;
            this.ProductCacheManager = initParameters.ProductCacheManager;
            this.PackageCacheManager = initParameters.PackageCacheManager;
            this.WarehouseManger = initParameters.WarehouseManger;
            this.PackageVersionManager = initParameters.PackageVersionManager;
            this.PackageVersionRepository = initParameters.PackageVersionRepository;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="orderType"></param>
        /// <param name="itemUIDs"></param>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        public override OutboundHomeAddressMap GetAllocatedHomeAddress(int orderType,
            Dictionary<int, IEnumerable<Guid>> itemUIDs, Guid warehouseUID)
        {
            Dictionary<Guid, IEnumerable<HomeAddressSlotSsageInfoModel>> mappingTable =
               new Dictionary<Guid, IEnumerable<HomeAddressSlotSsageInfoModel>>();
            ConcurrentDictionary<Guid, IEnumerable<ILocationItemViewModel>> omappingTable =
               new ConcurrentDictionary<Guid, IEnumerable<ILocationItemViewModel>>();
            var ItemGroup = itemUIDs.Values.SelectMany(x => x).GroupBy(g => g).Select(p => p.Key);
            IActionResult<IEnumerable<ISlotUsageInfoModel>> slotsUsage = null;
            IActionResult<IEnumerable<IHomeAddressRelationModel>> homeAddressRelation = null;
            var param = new GetAvailableInventoryInnerListParameters();
            param.Items = itemUIDs;
            param.WarehouseUID = warehouseUID;
            param.SlotStatuses = new SlotStatus[] { SlotStatus.Out, SlotStatus.InAndOut };
            param.IsincludeReceivingQty = true;
            IActionResult<IEnumerable<ILocationItemViewModel>> OnhandResult = null;

            //TODO 目前HOME LOCATION 覆蓋方式邏輯需要修正，要先依HOME LOCATION 優先度 是第一層，之後才查 SLOT上的 優先度
            using (var act = this._tracingAgent.StartActivity("get slot usage info "))
            {
                if (orderType == (int)OrderType.Packaging)
                {
                    slotsUsage = this.WarehouseManger.GetSlotUsageInfoByOutboundPKG(warehouseUID);
                    homeAddressRelation = this.WarehouseManger
                   .GetHomeAddressRelation(ItemGroup, HomeAddressType.Allocated, HomeAddressOutboundType.Packaging);
                }
                else
                {
                    slotsUsage = this.WarehouseManger.GetSlotUsageInfoByOutboundTL(warehouseUID);
                    homeAddressRelation = this.WarehouseManger
                   .GetHomeAddressRelation(ItemGroup, HomeAddressType.Allocated, HomeAddressOutboundType.Truckload);
                }
            }

            using (var act2 = this._tracingAgent.StartActivity("get onhand"))
            {
                OnhandResult = this.VesselManager.GetAvailableInventoryData(param);
                var onhandlogs = OnhandResult.Content.Select(p => new
                {
                    ItemUID = p.ItemUID,
                    OriginalPackageUID = p.OriginalPackageUID,
                    Quantity = p.Quantity,
                    PackageUID = p.OriginalPackageUID,
                    Status = p.Status,
                    SlotName = p.SlotName,
                    WarehouseName = p.WarehouseName,
                    SlotUID = p.SlotUID,
                });
            }

            //附加各產品版本建立時間
            processPkgVersion(OnhandResult);
            var onhandgrp = OnhandResult.Content.GroupBy(p => p.ItemUID);
            if (slotsUsage.Content.Count() > 0)
            {
                //1.Mapping to SlotUsage
                using (var act3 = this._tracingAgent.StartActivity("Mapping to SlotUsage"))
                {
                    foreach (var itemUID in ItemGroup)
                    {
                        List<HomeAddressSlotSsageInfoModel> _subhomeAddressModels = new List<HomeAddressSlotSsageInfoModel>();
                        slotsUsage.Content.ToList().ForEach(p =>
                        {
                            var e = new HomeAddressSlotSsageInfoModel(p);
                            e.ItemUID = itemUID;
                            _subhomeAddressModels.Add(e);
                        });
                        //2.Home address relation add sequence to SlotUsage
                        _subhomeAddressModels.ForEach(p =>
                        {
                            if (homeAddressRelation.Content.Any(x => x.ItemUID == p.ItemUID))
                            {
                                var _relation = homeAddressRelation.Content.FirstOrDefault(x =>
                                x.SlotUID == p.SlotUID && x.ItemUID == p.ItemUID);
                                if (_relation != null)
                                {
                                    p.AllocatedSequence = _relation.Sequence;
                                }
                                else //找不到對應的出貨優先度則設最小優先度
                                {
                                    p.AllocatedSequence = int.MaxValue;
                                }
                            }
                        });
                        mappingTable.Add(itemUID, _subhomeAddressModels);
                    }
                }
                //3. mapping to availableinventory 
                using (var act5 = this._tracingAgent.StartActivity("mapping to availableinventory"))
                {
                    Parallel.ForEach(onhandgrp, itemgrp =>
                   {
                       var homeaddress = mappingTable.FirstOrDefault(i => i.Key == itemgrp.Key);
                       foreach (var onHanditem in itemgrp)
                       {
                           var ha = homeaddress.Value.FirstOrDefault(x => x.SlotUID == onHanditem.SlotUID);
                           if (ha != null)
                           {
                               onHanditem.Sequence = ha.AllocatedSequence;
                           }
                           else //找不到對應的出貨優先度則設最小優先度
                           {
                               onHanditem.Sequence = int.MinValue;
                           }
                       }
                       omappingTable.TryAdd(itemgrp.Key, itemgrp);
                   });
                }
                if (onhandgrp.Count() == 0)
                {
                    foreach (var onHanditem in ItemGroup)
                    {
                        omappingTable.TryAdd(onHanditem, new List<LocationItemInnerViewModel>());
                    }
                }
                return new OutboundHomeAddressMap(omappingTable.ToDictionary(kvp => kvp.Key,
                                                          kvp => kvp.Value));
            }
            return new OutboundHomeAddressMap(omappingTable.ToDictionary(kvp => kvp.Key,
                                                          kvp => kvp.Value));
        }
        private void processPkgVersion(IActionResult<IEnumerable<ILocationItemViewModel>> onhandResult)
        {
            ConcurrentBag<IPackageVersionViewModel> pkgvercollection = new ConcurrentBag<IPackageVersionViewModel>();
            var index = 0;

            var grp = onhandResult.Content.Select(x => x.OriginalPackageUID).GroupBy(g => index++ / 2000);
            using (var act = this._tracingAgent.StartActivity("get pkg version data by db"))
            {
                foreach (var item in grp)
                {
                    //var verlist = retryProcess<IEnumerable<IPackageVersionViewModel>>(
                    //    () => this.PackageVersionManager.GetPackageVersionList(item));
                    var verlist = retryProcess<IEnumerable<IPackageVersionViewModel>>(
                       () => this.PackageVersionRepository.GetPackageVersionByPackage(item));
                    //pkgvercollection.AddRange(verlist);
                    foreach (var vitem in verlist)
                    {
                        pkgvercollection.Add(vitem);
                    }
                }
                // Parallel.ForEach(grp, item =>
                //{
                //    //var verlist = retryProcess<IEnumerable<IPackageVersionViewModel>>(
                //    //    () => this.PackageVersionManager.GetPackageVersionList(item));
                //    var verlist = retryProcess<IEnumerable<IPackageVersionViewModel>>(
                //      () => this.PackageVersionRepository.GetPackageVersionByPackage(item));
                //    //pkgvercollection.AddRange(verlist);
                //    foreach (var vitem in verlist)
                //    {
                //        pkgvercollection.Add(vitem);
                //    }
                //});
            }
            //var pkgvercollection = this.PackageVersionManager.GetPackageVersionList(onhandResult.Content.Select(p => p.OriginalPackageUID));
            if (pkgvercollection.Count > 0)
            {
                using (var act = this._tracingAgent.StartActivity("mapping pkg verion"))
                {
                    var onhandgrp = onhandResult.Content.GroupBy(p => p.ItemUID);
                    foreach (var itemonhand in onhandgrp)
                    {
                        foreach (var item in itemonhand)
                        {
                            var pkgver = pkgvercollection.FirstOrDefault(p => p.PackageUID == item.OriginalPackageUID);
                            if (pkgver != null)
                            {
                                item.PackageSerialNumber = pkgver.SerialNumber;
                            }
                        }
                    }
                }
            }
        }
    }
}
