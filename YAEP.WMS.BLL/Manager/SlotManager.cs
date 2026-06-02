using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.Interfaces;
using YAEP.Data.ORM.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public class SlotManager : AbstractManager, ISlotManager
    {
        ObjectCache _Cache = MemoryCache.Default;
        const string SLOT_CACHE_KEY = "SlotCache";

        private SlotCacheManager _slotCacheManager;
        private ISlotRepository Repository { get; set; }
        internal SlotCacheManager SlotCacheManager
        {
            get
            {
                return this._slotCacheManager;
            }
        }
        public SlotManager(ISequenceAgent sequenceAgent, ISlotRepository repository,
            IAuthenticationProvider authenticationInfoProvider, IAppSettings appSettings, IGroupManager groupManager,
            IPackageManager packageManager, IPackageUomManager packageUomManager, IItemManager itemManager
            , IPartyManager partyManager, IObjectRelationalMappingLayer dbentities
            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IRefreshDrKnowAll refreshDKA, IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository)
            : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager, packageManager
                  , packageUomManager, itemManager, partyManager, itemmgmterfunc, dbentities, refreshDKA, itemRepository,
                  packageVersionRepository)
        {
            this.Repository = repository;
            _slotCacheManager = new SlotCacheManager(this.GetSlotCache, tracingAgent: this.TracingAgent);
        }
        private IEnumerable<ISlotModel> GetSlotCache()
        {
            if (_Cache[SLOT_CACHE_KEY] == null)
            {
                _Cache.Add(SLOT_CACHE_KEY,
               this.GetList(new { })
                ,
                new CacheItemPolicy()
                {
                    SlidingExpiration = new TimeSpan(23, 0, 0)
                });
            }

            IActionResult<IEnumerable<ISlotModel>> test = _Cache[SLOT_CACHE_KEY] as IActionResult<IEnumerable<ISlotModel>>;
            return test.Content as IEnumerable<ISlotModel>;
        }
        public IActionResult<bool> SetMappingToBin(Guid? areaUID, Guid slotUID, Guid? binUID)
        {
            //if (areaUID == Guid.Empty)
            //{
            //    return ActionResultTemplates.ArgumentNullExceptionResult(nameof(areaUID));
            //}
            //if (slotUID == Guid.Empty)
            //{
            //    return ActionResultTemplates.ArgumentNullExceptionResult(nameof(slotUID));
            //}

            return this.Repository.SetSlotMappingToBin(areaUID, slotUID, binUID);
        }

        public IActionResult<IEnumerable<IComponentViewModel>> GetSlotNameList(IWarehouseComponentParameters parameters)
        {
            return this.Repository.GetSlotNameList(parameters);
        }

        public IActionResult<IEnumerable<ISlotModel>> GetList(dynamic condition)
        {
            return this.Repository.GetList(condition);
        }

        public IActionResult<bool> AddSlot(ISlotModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }

            if (model.UID == Guid.Empty)
            {
                model.UID = Guid.NewGuid();
            }
            model.Name = model.ID;
            //model.ID = this.SequenceAgent.GetSlotSeqence(Guid.Empty);

            return this.Repository.AddSlot(model);
        }

        public IActionResult<bool> EditSlot(ISlotModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }
            if (model.UID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model.UID));
            }

            return this.Repository.EditSlot(model);
        }

        public IActionResult<bool> DeleteSlot(Guid[] UID)
        {
            if (UID?.Count() == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(UID));
            }

            return this.Repository.DeleteSlot(UID);
        }

        public IActionResult<IEnumerable<ISlotViewModel>> GetSlotList(Guid? warehouseUID, Guid? areaUID, Guid? binUID)
        {
            if (warehouseUID.HasValue && warehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ISlotViewModel>>(nameof(warehouseUID));
            }
            if (areaUID.HasValue && areaUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ISlotViewModel>>(nameof(areaUID));
            }
            if (binUID.HasValue && binUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ISlotViewModel>>(nameof(binUID));
            }

            return this.Repository.GetSlotList(warehouseUID, areaUID, binUID);
        }
        public IActionResult<IEnumerable<ISlotSearchViewModel>> GetSearchSlotList(Guid? warehouseUID, string slotid)
        {

            if (string.IsNullOrEmpty(slotid))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<ISlotSearchViewModel>>(nameof(slotid));
            }

            return this.Repository.GetSearchSlotList(warehouseUID, slotid);
        }
        public IActionResult<IEnumerable<ILocation>> GetLocations(Guid[] slotUIDs)
        {
            return this.Repository.GetLocations(slotUIDs);
        }

        public IActionResult<IEnumerable<ISlotModel>> GetSlotListFromCache(IEnumerable<Guid> WarehouseUIDs)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ISlotModel>>();
            if (WarehouseUIDs.Count() > 0)
            {
                this.TracingAgent.Trace("Start getting slots from cache.");
                var item_list = this.SlotCacheManager.GetSlots(WarehouseUIDs, null, null);
                //var item_list = new List<ISlotModel>();
                this.TracingAgent.Trace("Got slots from cache.");

                if (item_list != null)
                {
                    rs.Content = item_list;
                    rs.Success = true;
                }
                else
                {
                    rs.Success = false;
                }
            }
            else
            {
                rs.Success = false;
            }

            return rs;
        }

        public void ClearSlotCache()
        {
            _Cache.Remove(SLOT_CACHE_KEY);
        }
    }
}
