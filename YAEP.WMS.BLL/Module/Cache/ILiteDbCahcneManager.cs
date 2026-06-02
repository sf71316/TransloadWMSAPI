using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces.Cache;

namespace YAEP.WMS.BLL.Module
{
    internal interface ILiteDbCahcneManager : ICacheManager<LiteDatabase>
    {
        IInventoryCacheDataProvider InventoryCache { get; }
    }
}
