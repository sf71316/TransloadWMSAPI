using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Interfaces.Cache;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractLitedbCahcneManager : ILiteDbCahcneManager
    {
        IAppSettings _AppSettings;
        public AbstractLitedbCahcneManager(IAppSettings appSettings)
        {
            this._AppSettings = appSettings;
        }
        public IInventoryCacheDataProvider InventoryCache => throw new NotImplementedException();

        public LiteDatabase CacheInStance { get; set; }

        public void RefreshAll()
        {
            throw new NotImplementedException();
        }
    }
}
