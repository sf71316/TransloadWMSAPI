using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class HomeAddrSlotModel : IHomeAddressReltationSlotModel
    {
        public Guid SlotUID { get; set; }
        public string SlotName { get; set; }
    }
}
