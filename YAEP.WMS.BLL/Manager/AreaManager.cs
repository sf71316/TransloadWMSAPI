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
    public class AreaManager : AbstractManager, IAreaManager
    {
        private readonly IAreaRepository Repository;
        public AreaManager(IAreaRepository repository,
            IAuthenticationProvider authenticationInfoProvider,
            ISequenceAgent sequenceAgent, IAppSettings appSettings, IGroupManager groupManager,
            IPackageManager packageManager, IPackageUomManager packageUomManager
            , IItemManager itemManager, IPartyManager partyManager, IObjectRelationalMappingLayer dbentities,
            Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IRefreshDrKnowAll refreshDKA, IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository)
            : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager, packageManager,
                  packageUomManager, itemManager, partyManager, itemmgmterfunc, dbentities, refreshDKA,itemRepository,
                  packageVersionRepository)
        {
            this.Repository = repository;
        }

        public IActionResult<IEnumerable<IComponentViewModel>> GetAreaNameList(IWarehouseComponentParameters parameters)
        {
            return this.Repository.GetAreaNameList(parameters);
        }

        public IActionResult<IEnumerable<IAreaModel>> GetList(dynamic condition)
        {
            return this.Repository.GetList(condition);
        }

        public IActionResult<bool> AddArea(IAreaModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }

            if (model.UID == Guid.Empty)
            {
                model.UID = Guid.NewGuid();
            }

            return this.Repository.AddArea(model);
        }

        public IActionResult<bool> EditArea(IAreaModel model)
        {
            if (model == null)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model));
            }
            if (model.UID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(model.UID));
            }

            return this.Repository.EditArea(model);
        }

        public IActionResult<bool> DeleteArea(Guid[] UID)
        {
            if (UID?.Count() == 0)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(UID));
            }

            return this.Repository.DeleteArea(UID);
        }

        public IActionResult<IEnumerable<IAreaViewModel>> GetAreaList(Guid? warehouseUID, Guid? areaUID)
        {
            if ((warehouseUID.HasValue && warehouseUID.Value == Guid.Empty ||
                areaUID.HasValue && areaUID.Value == Guid.Empty))
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IEnumerable<IAreaViewModel>>(nameof(warehouseUID));
            }

            return this.Repository.GetAreaList(warehouseUID, areaUID);
        }

    }
}
