using General.Data.SQLConditionConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Core.Party.Interfaces;
using YAEP.Data.ORM.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.LittleBird.CapPBSC.Models;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.InnerModel;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Manager
{
    public class WarehouseManager : AbstractManager, IWarehouseManger
    {
        private static object _ThisLock = new object();
        public WarehouseManager(
                IWarehouseRepository repository, IManifestItemListRepository manifestItemListRepository, IPayloadRepository payloadRepository, ISlotRepository slotRepository, IItemManager itemManager, IHomeAddressRelationRepository homeAddressRelationRepository,
                IAuthenticationProvider authenticationInfoProvider, ISequenceAgent sequenceAgent, IAppSettings appSettings, IGroupManager groupManager,
                IPackageManager packageManager, IPackageUomManager packageUomManager, IPartyManager partyManager, IObjectRelationalMappingLayer dbentities
            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IRefreshDrKnowAll refreshDKA, IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository
            ) : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager, packageManager, packageUomManager,
                itemManager, partyManager, itemmgmterfunc, dbentities, refreshDKA, itemRepository, packageVersionRepository)
        {
            this.Repository = repository;
            this.ManifestItemListRepository = manifestItemListRepository;
            this.PayloadRepository = payloadRepository;
            this.SlotRepository = slotRepository;
            this.ItemManager = itemManager;
            this.HomeAddressRelationRepository = homeAddressRelationRepository;
        }
        //private IItemManager ItemManager { get; set; }
        public IActionResult<bool> DeleteWarehouse(IWarehouseDeleteParameters parameters)
        {
            if (parameters == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(parameters));
            }
            if (parameters.UID == null || parameters.UID.Count() == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult($"{nameof(parameters)}.{nameof(parameters.UID)}");
            }

            return this.Repository.Delete(parameters);
        }

        public IActionResult<bool> AddWarehouse(IWarehouseModel warehouse)
        {
            if (warehouse == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(warehouse));
            }

            if (warehouse.UID == Guid.Empty)
            {
                warehouse.UID = Guid.NewGuid();
            }

            return this.Repository.Add(warehouse);
        }

        public IActionResult<bool> EditWarehouse(IWarehouseModel warehouse)
        {
            if (warehouse == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(warehouse));
            }
            if (warehouse.UID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult($"{nameof(warehouse)}.{nameof(warehouse.UID)}");
            }

            return this.Repository.Update(warehouse);
        }

        public string GenerateBarcode(BarcodeType type)
        {
            lock (_ThisLock)
            {
                System.Threading.Thread.Sleep(1);
                return this.genBarCode(type);
            }
        }
        private string genBarCode(BarcodeType PurUom)
        {
            string IEA = "00";                              //1-2
            string Pkg = "0";                             //3
            string CountryCode = "1";
            string result = "";
            Pkg = ((int)PurUom).ToString();
            result = Pkg + CountryCode + DateTime.Now.ToString("yyMMddHHmmssfff").PadLeft(15, '0');
            return IEA + result + getCheckSUM(result);
        }

        private string getCheckSUM(string Source)
        {
            if (Source.Length == 17)
            {
                var odd = Source.Where((p, index) => index % 2 == 0).Select(p => Convert.ToInt32(char.ToString(p))).Sum() * 3;
                var even = Source.Where((p, index) => index % 2 != 0).Select(p => Convert.ToInt32(char.ToString(p))).Sum();
                int res = 10 - ((odd + even) % 10);
                return (res == 10) ? "0" : res.ToString();
            }
            return "-";
        }

        public IActionResult<IEnumerable<IComponentViewModel>> GetWarehouseNameList()
        {

            return this.Repository.GetWarehouseNameList();
        }

        public IActionResult<Guid> GetPodInSlot(IGetPodInSlotParameters Parameters)
        {
            return this.Repository.GetPodInSlot(Parameters);
        }
        public IActionResult<IDeallocatedPayloadInfoModel> FindDeallocatedRelatedPayloadCollection(IEnumerable<Guid> allocatedPayloadUID)
        {
            return this.PayloadRepository.FindDeallocatedRelatedPayloadCollection(allocatedPayloadUID);
        }
        public IActionResult<bool> EditPayload(IPayloadModel payloadModel)
        {
            return this.PayloadRepository.UpatePayload(payloadModel);
        }
        public IActionResult<bool> ReplenishmentPayload(IPayloadModel payloadModel)
        {

            return this.PayloadRepository.ReplenishmentPayload(payloadModel);
        }

        public IActionResult<IWarehouseModel> GetWarehouse(Guid warehouseUID)
        {
            if (warehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IWarehouseModel>(nameof(warehouseUID));
            }

            return this.Repository.GetWarehouse(warehouseUID);
        }

        public IActionResult<IEnumerable<IWarehouseModel>> GetWarehouseList()
        {
            return this.Repository.GetWarehouseList();
        }

        public IActionResult<IEnumerable<ILocationInfoViewModel>> GetLocationInfoList(Guid? warehouseUID, Guid? areaUID, Guid? binUID, Guid? slotUID)
        {
            if (warehouseUID.HasValue && warehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ILocationInfoViewModel>>(nameof(warehouseUID));
            }
            if (areaUID.HasValue && areaUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ILocationInfoViewModel>>(nameof(areaUID));
            }
            if (binUID.HasValue && binUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ILocationInfoViewModel>>(nameof(binUID));
            }
            if (slotUID.HasValue && slotUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ILocationInfoViewModel>>(nameof(slotUID));
            }

            return this.Repository.GetLocationInfoList(warehouseUID, areaUID, binUID, slotUID);
        }

        public IActionResult<bool> PodIsExist(Guid PodUID)
        {
            return this.Repository.PodIsExist(PodUID);
        }

        public IActionResult<int> GetAssignedPackageQty(Guid packageUID)
        {
            var mainfestitempkgttlqty = this.ManifestItemListRepository.GetManifestItemListByPackageQty(packageUID);
            var payloadttlqty = this.PayloadRepository.GetPayloadByPackageQty(packageUID);

            var rs = ActionResultTemplates.Result<int>();
            try
            {
                if (mainfestitempkgttlqty.Success && payloadttlqty.Success)
                {
                    rs.Content = mainfestitempkgttlqty.Content + payloadttlqty.Content;
                    rs.Success = true;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = mainfestitempkgttlqty.Message + " " + payloadttlqty.Message;
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

        public IActionResult<IEnumerable<IPodSelectListModel>> GetPodSelectList(Guid wuid)
        {

            return this.Repository.GetPodSelectList(wuid);
        }
        public IActionResult<ISlotModel> GetDummySlot(Guid warehouseUID)
        {
            var rs = ActionResultTemplates.Result<ISlotModel>();
            var result = this.Repository.GetSlotByType(warehouseUID, new SlotType[] { SlotType.OutboundTemp });
            if (result.Success && result.Content.Count() > 0)
            {
                rs.Success = true;
                rs.Content = result.Content.First();
            }
            else
            {
                rs.Message = Resource.WAREHOUSE_NOT_FIND_LOADING_ZONE;
            }
            return rs;
        }
        public IActionResult<ISlotModel> GetDefaultLandingZone(Guid warehouseUID, SlotType slotType)
        {
            var rs = ActionResultTemplates.Result<ISlotModel>();
            var result = this.Repository.GetDefaultLoadingZone(warehouseUID, slotType);
            if (result.Success && result.Content.Count() > 0)
            {
                var defaultLoadingSlot = result.Content;
                if (defaultLoadingSlot.Count() > 0)
                {
                    rs.Success = true;
                    rs.Content = defaultLoadingSlot.First();
                }
                else
                {
                    rs.Success = true;
                    rs.Content = result.Content.First();
                }

            }
            else
            {
                rs.Message = Resource.WAREHOUSE_NOT_FIND_LOADING_ZONE;
            }
            return rs;
        }

        public IActionResult<IPayloadModel> GetRecoveryPayload(Guid payloadUID)
        {
            return this.PayloadRepository.GetRecoveryPayload(new { UID = payloadUID });
        }

        public IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfo(
            Guid warehouseUID, IEnumerable<SlotType> slotTypes, IEnumerable<SlotStatus> slotStatuses)
        {
            if (warehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ISlotUsageInfoModel>>(nameof(warehouseUID));
            }
            if (slotTypes == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ISlotUsageInfoModel>>(nameof(slotTypes));
            }
            if (slotStatuses == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ISlotUsageInfoModel>>(nameof(slotStatuses));
            }
            return this.Repository.GetSlotUsageInfo(warehouseUID, slotTypes, slotStatuses, ManifestType.Inbound);
        }
        public IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfoByInbound(Guid warehouseUID)
        {
            return this.GetSlotUsageInfo(warehouseUID,
                new SlotType[] { SlotType.Rack_LTL, SlotType.Rack_Parcel,
                    SlotType.Rack_LTL_Parcel, SlotType.OpenStorageArea },
                new SlotStatus[] { SlotStatus.In, SlotStatus.InAndOut });
        }

        public IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfoByOutboundTL(Guid warehouseUID)
        {
            return this.GetSlotUsageInfo(warehouseUID,
                new SlotType[] { SlotType.Rack_LTL, SlotType.Rack_LTL_Parcel, SlotType.OpenStorageArea },
                new SlotStatus[] { SlotStatus.Out, SlotStatus.InAndOut });
        }
        public IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfoByOutboundPKG(Guid warehouseUID)
        {
            return this.GetSlotUsageInfo(warehouseUID,
               new SlotType[] { SlotType.Rack_Parcel, SlotType.Rack_LTL_Parcel, SlotType.OpenStorageArea },
               new SlotStatus[] { SlotStatus.Out, SlotStatus.InAndOut });
        }
        public IActionResult<IEnumerable<IPayloadLocationModel>> GetLocations(IEnumerable<Guid> payloadUIDs)
        {
            return this.Repository.GetLocations(payloadUIDs);
        }
        public IActionResult<ISlotModel> GetFutureSlot(Guid warehouseUID)
        {
            var rs = ActionResultTemplates.Result<ISlotModel>();
            var slots = this.Repository.GetSlotByType(warehouseUID, new SlotType[] { SlotType.FutureAllocated });
            rs.Content = slots.Content.FirstOrDefault();
            rs.Success = true;
            return rs;
        }
        public IActionResult<IEnumerable<IHomeAddressRelationModel>> GetHomeAddressRelation(IEnumerable<Guid> ItemUIDs,
            HomeAddressType homeAddressType, HomeAddressOutboundType? homeAddressOutboundType = null)
        {
            int? _hvalue = null;
            if (homeAddressOutboundType.HasValue)
            {
                _hvalue = (int)homeAddressOutboundType.Value;
            }
            return this.HomeAddressRelationRepository.GetData(ItemUIDs, (int)homeAddressType, _hvalue);
        }

        public void Importhomelocation(List<dynamic> homelocations, IGroupManager groupManager)
        {
            var source = homelocations.Select(a =>
             {
                 var e = new ImportHomeLocation
                 {
                     prod_id = Convert.ToString(a.prod_id),
                     Inbound = Convert.ToString(a.Inbound),
                     Outbound = Convert.ToString(a.prod_id)
                 };
                 return e;
             });
            List<string> notfinditem = new List<string>();
            List<dynamic> itemMapping = new List<dynamic>();
            var collection = source.Select(x => x.prod_id);
            var groups = groupManager.GetGroupUserViewByUser(this.AuthProvider.GetAuthenticationInfo().UID);
            var _productCacheManager = this.ProductCacheManager;
            var itemids = _productCacheManager.GetItems(collection, new Guid("632689e6-c643-43ee-ae85-7384e20e587a"), groups.Content);
            foreach (var item in collection)
            {
                //IEnumerable<IItemModel> pitem = _productCacheManager
                //    .GetItems(new string[] { item }, new Guid("632689e6-c643-43ee-ae85-7384e20e587a"), groups.Content);
                IEnumerable<IItemModel> pitem = itemids.Where(p => item.Contains(p.ID));
                if (pitem != null && pitem.Count() > 0)
                {
                    itemMapping.Add(new
                    {
                        Prodid = item,
                        ProdUID = pitem.FirstOrDefault().UID

                    });
                }
                else
                {
                    notfinditem.Add(Convert.ToString(item));
                }
            }

            var slotids = this.SlotRepository.GetList(new { WarehouseUID = "3C336ED8-4A9D-4DF2-94AD-4EBFBC8BCA76" })
                .Content.Where(x => x.Type > 99);

            List<string> notfindslot = new List<string>();
            List<HomeAddressRelationModel> hcollection = new List<HomeAddressRelationModel>();
            List<IActionResult<bool>> result = new List<IActionResult<bool>>();
            this.HomeAddressRelationRepository.ClearAll();
            foreach (var item in source)
            {
                var pitem = itemMapping.FirstOrDefault(x => x.Prodid == item.prod_id);
                if (pitem != null)
                {
                    string inboundStr = Convert.ToString(item.Inbound);
                    string outboundStr = Convert.ToString(item.Outbound);
                    if (!string.IsNullOrEmpty(inboundStr))
                    {
                        var inboundproi = inboundStr.Split(',');
                        int inboundIndex = 1;
                        foreach (var iitem in inboundproi)
                        {
                            if (!string.IsNullOrEmpty(iitem))
                            {
                                if (pitem != null)
                                {
                                    var sl = slotids.FirstOrDefault(x => x.ID == iitem.Trim());
                                    if (sl != null)
                                    {
                                        HomeAddressRelationModel h1 = new HomeAddressRelationModel();
                                        h1.ItemCategoryUID = Guid.Empty;
                                        h1.ItemUID = pitem.ProdUID;
                                        h1.Type = 1;
                                        h1.UID = Guid.NewGuid();
                                        h1.Sequence = inboundIndex++;
                                        h1.SlotUID = sl.UID;
                                        h1.Status = 100;
                                        h1.CreatedOn = h1.ModifiedOn = DateTime.Now;
                                        hcollection.Add(h1);
                                    }
                                    else
                                    {
                                        notfindslot.Add("inbound " + iitem);
                                    }
                                }
                                else
                                {
                                    notfinditem.Add(Convert.ToString(item.prod_id));
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(outboundStr))
                    {
                        int outboundIndex = 1;
                        var outboundproi = outboundStr.Split(',');
                        foreach (var oitem in outboundproi)
                        {
                            if (!string.IsNullOrEmpty(oitem))
                            {
                                if (pitem != null)
                                {
                                    var sl = slotids.FirstOrDefault(x => x.ID == oitem.Trim());
                                    if (sl != null)
                                    {
                                        HomeAddressRelationModel h2 = new HomeAddressRelationModel();
                                        h2.ItemCategoryUID = Guid.Empty;
                                        h2.ItemUID = pitem.ProdUID;
                                        h2.Type = 2;
                                        h2.UID = Guid.NewGuid();
                                        h2.Sequence = outboundIndex++;
                                        h2.SlotUID = sl.UID;
                                        h2.Status = 100;
                                        h2.CreatedOn = h2.ModifiedOn = DateTime.Now;
                                        hcollection.Add(h2);
                                    }
                                    else
                                    {
                                        notfindslot.Add("outbound " + oitem);
                                    }
                                }
                            }
                        }
                    }
                }


            }
            result.Add(this.HomeAddressRelationRepository.Insert(hcollection));
            System.IO.File.WriteAllLines($"D:\\importhomelocation_notfinditem_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.txt", notfinditem);
            System.IO.File.WriteAllLines($"D:\\importhomelocation_notfindslot_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.txt", notfindslot.GroupBy(p => p).Select(g => g.Key));
            if (result.All(x => x.Success))
            {


            }

        }

        public IActionResult<IEnumerable<ILocationItemViewModel>> GetAvailableInventoryData(IGetAvailableInventoryDataInnerListParameters param)
        {
            return this.Repository.GetAvailableInventoryList(param);
        }

        public IActionResult<IEnumerable<IPayloadModel>> GetOnhandPayload(Guid warehouseUID,
            IEnumerable<Guid> itemUID, int[] slotStatus)
        {
            return this.PayloadRepository.GetOnhandPayload(warehouseUID, itemUID, slotStatus);
        }

        public IActionResult<IEnumerable<ISlotModel>> CheckSlot(Guid warehouseUID, IEnumerable<string> slotNames)
        {
            var slots = this.SlotRepository.GetList(new { warehouseUID = warehouseUID, name = slotNames });
            return slots;
        }

        public IActionResult<bool> TestInventorySync(int times, int interval)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<IActionResult<bool>> rsCollection = new List<IActionResult<bool>>();
                var _InventoryReplicateClient = new OnhandReplication(this.TracingAgent);
                _InventoryReplicateClient.Agent = "WMS-Tester";
                for (int i = 1; i <= times; i++)
                {
                    var testModel = new WmsInventoryModel();
                    testModel.ItemNo = "tester";
                    testModel.LocationID = 9999;
                    testModel.Quantity = 0;
                    testModel.UID = Guid.NewGuid();
                    testModel.UpdateDate = DateTime.Now;
                    rsCollection.Add(_InventoryReplicateClient.Sync(new WmsInventoryModel[] { testModel }, Guid.NewGuid().ToString()));
                    Thread.Sleep(interval);
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

        public IActionResult<bool> TestAllocatedSync(int times, int interval)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<IActionResult<bool>> rsCollection = new List<IActionResult<bool>>();
                var _AllocatedReplicateClient = new AllocatedReplication(this.TracingAgent);
                _AllocatedReplicateClient.Agent = "WMS-Tester";
                for (int i = 1; i <= times; i++)
                {
                    var testModel = new WmsAllocatedModel();
                    rsCollection.Add(_AllocatedReplicateClient.Sync(new WmsAllocatedModel[] { testModel }, Guid.NewGuid().ToString()));
                    Thread.Sleep(interval);
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

        public IActionResult<bool> TestReceivingSync(int times, int interval)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<IActionResult<bool>> rsCollection = new List<IActionResult<bool>>();
                var _ReceiviedReplicateClient = new ReceiviedReplication(this.TracingAgent);
                _ReceiviedReplicateClient.Agent = "WMS-Tester";
                for (int i = 1; i <= times; i++)
                {
                    var testModel = new WmsReceivingModel();

                    rsCollection.Add(_ReceiviedReplicateClient.Sync(new WmsReceivingModel[] { testModel }, Guid.NewGuid().ToString()));
                    Thread.Sleep(interval);
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

        public IActionResult<int> GetSequence(Guid belongToUID, string belongToTag)
        {

            var rs = ActionResultTemplates.Result<int>();
            try
            {
                var index = this.SequenceAgent.GetSeqenceIndex(belongToUID, belongToTag);
                rs.Content = index;
                rs.Success = true;
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

        public IActionResult<IEnumerable<IWarehouseModel>> GetThirdPartyWarehouseNameList()
        {
            var groups = this.GetGroupUserViewByUser().Content.ToList().Select(x => x.GroupUID); 
            //this.Repository.GetWarehouseList();
            var condition = new QueryConditionTranslator<WarehouseInnerModel>();
            condition.AddCondition(c => groups.Contains(c.GroupUID) && c.Status>(int)WarehouseStatus.Inactive);
            return this.Repository.GetWarehouseList(condition);
        }

        private IWarehouseRepository Repository { get; set; }
        private IManifestItemListRepository ManifestItemListRepository { get; set; }
        private IPayloadRepository PayloadRepository { get; set; }
        private IHomeAddressRelationRepository HomeAddressRelationRepository { get; set; }
        private ISlotRepository SlotRepository { get; set; }


    }
}
