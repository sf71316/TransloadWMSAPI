using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.Interfaces;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Identities.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Module
{
    internal class SlotCacheManager
    {
        IEnumerable<ISlotModel> _slotCache;
        private IEnumerable<IGroupUserViewModel> _GroupUsers;
        Func<IEnumerable<ISlotModel>> _slotCacheMethod;
        ILogInfiltrator _Logger { get; set; }
        ITracingAgent _TraceAgent { get; set; }

        public SlotCacheManager(Func<IEnumerable<ISlotModel>> slotCacheMethod, ILogInfiltrator log = null,
            ITracingAgent tracingAgent = null)
        {
            _Logger = log;
            _TraceAgent = tracingAgent;
            _slotCacheMethod = slotCacheMethod;
            LoadCache();
        }
        public SlotCacheManager(Func<List<ISlotModel>> slotCacheMethod, IActionResult<IEnumerable<IGroupUserViewModel>> groups
            , ILogInfiltrator log = null)
            : this(slotCacheMethod, log)
        {
            this._GroupUsers = groups.Content;
        }
        public void LoadCache()
        {
            this._slotCache = this._slotCacheMethod.Invoke();
        }
        public ISlotModel GetSlot(Guid slotuid, [CallerMemberName] string memberName = "")
        {
            return this.GetSlots(new Guid[] { slotuid }, memberName)?.FirstOrDefault();
        }
        public IEnumerable<ISlotModel> GetSlots(IEnumerable<Guid> slotuid, [CallerMemberName] string memberName = "")
        {
            if (slotuid != null)
            {
                var groupbyItem = slotuid.GroupBy(g => g).Select(p => p.Key).ToList();
                return this._slotCache.Where(p => groupbyItem.Any(x => p.UID == x));
            }
            else
            {
                return this._slotCache;
            }
        }
        public IEnumerable<ISlotModel> GetSlots(IEnumerable<Guid> warehouseuid, IEnumerable<Guid> areauid, IEnumerable<Guid> binuid)
        {
            var warehouse_grourp = (warehouseuid != null ? warehouseuid.GroupBy(g => g).Select(p => p.Key) : new List<Guid>());
            var area_grourp = (areauid != null ? areauid.GroupBy(g => g).Select(p => p.Key) : new List<Guid>());
            var bin_grourp = (binuid != null ? binuid.GroupBy(g => g).Select(p => p.Key) : new List<Guid>());
            if (this._slotCache != null)
            {
                return this._slotCache.Where(p => (warehouse_grourp.Count() == 0 || warehouse_grourp.Contains(p.WarehouseUID))
                && (area_grourp.Count() == 0 || !p.AreaUID.HasValue || (p.AreaUID.HasValue && area_grourp.Contains(p.AreaUID.Value)))
                && (bin_grourp.Count() == 0 || !p.BinUID.HasValue || (p.BinUID.HasValue && bin_grourp.Contains(p.BinUID.Value))));
            }
            return null;
        }
    }
}
