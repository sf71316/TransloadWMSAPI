using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.Interfaces;
using YAEP.Data.ORM.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public class BinManager : AbstractManager, IBinManager
    {
        private IBinRepository Repository { get; set; }
        public BinManager(IBinRepository repository, IAuthenticationProvider authenticationInfoProvider,
            ISequenceAgent sequenceAgent, IAppSettings appSettings, IGroupManager groupManager,
            IPackageManager packageManager, IPackageUomManager packageUomManager
            , IItemManager itemManager, IPartyManager partyManager, IObjectRelationalMappingLayer dbentities
            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IRefreshDrKnowAll refreshDKA, IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository)
             : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager, packageManager
                   , packageUomManager, itemManager, partyManager, itemmgmterfunc, dbentities, refreshDKA,itemRepository,
                   packageVersionRepository)
        {
            this.Repository = repository;
        }

        public IActionResult<bool> SetMappingToArea(Guid areaUID, Guid binUID)
        {
            if (areaUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(areaUID));
            }
            if (binUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(binUID));
            }

            return this.Repository.SetBinMappingToArea(areaUID, binUID);
        }
        public IActionResult<IEnumerable<IBinViewModel>> GetBinList(Guid? warehouseUID, Guid? areaUID)
        {
            if (warehouseUID.HasValue && warehouseUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<IBinViewModel>>(nameof(warehouseUID));
            }
            if (areaUID.HasValue && areaUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<IBinViewModel>>(nameof(areaUID));
            }

            return this.Repository.GetBinList(warehouseUID, areaUID);
        }

        public IActionResult<IEnumerable<IComponentViewModel>> GetBinNameList(IWarehouseComponentParameters parameters)
        {
            return this.Repository.GetBinNameList(parameters);
        }

        public IActionResult<IEnumerable<IBinModel>> GetList(dynamic condition)
        {
            return this.Repository.GetList(condition);
        }

        public IActionResult<bool> AddBin(IBinModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }

            if (model.UID == Guid.Empty)
            {
                model.UID = Guid.NewGuid();
            }

            return this.Repository.AddBin(model);
        }

        public IActionResult<bool> EditBin(IBinModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }
            if (model.UID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model.UID));
            }

            return this.Repository.EditBin(model);
        }

        public IActionResult<bool> DeleteBin(Guid[] UID)
        {
            if (UID?.Count() == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(UID));
            }

            return this.Repository.DeleteBin(UID);
        }


    }
}
