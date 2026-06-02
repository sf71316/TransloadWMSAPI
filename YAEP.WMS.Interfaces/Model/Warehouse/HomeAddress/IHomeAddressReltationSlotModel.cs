using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IHomeAddressReltationSlotModel
    {
        Guid SlotUID { get; set; }
        string SlotName { get; set; }
    }
}
