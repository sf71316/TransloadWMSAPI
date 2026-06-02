using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundHomeAddressMap
    {
        Dictionary<Guid, IEnumerable<ILocationItemViewModel>> _Source;
        public OutboundHomeAddressMap(Dictionary<Guid, IEnumerable<ILocationItemViewModel>> source)
        {
            this._Source = source;
        }
        public IEnumerable<ILocationItemViewModel> FindOnhandSequenceList(Guid itemUID, int onhandType)
        {

            var mapping = this._Source.FirstOrDefault(p => p.Key == itemUID).Value;
            if (mapping != null)
            {
                return mapping.Where(p => p.Type == onhandType).OrderBy(o => o.Sequence).ThenBy(o2 => o2.SlotId);
            }
            return new List<LocationItemInnerViewModel>();
        }
        public IEnumerable<ILocationItemViewModel> GetAllLocationItems()
        {

            return this._Source.SelectMany(x => x.Value);
        }


    }
}
