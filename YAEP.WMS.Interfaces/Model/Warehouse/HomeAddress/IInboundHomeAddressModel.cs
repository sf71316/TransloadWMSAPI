using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IInboundHomeAddressModel
    {
        Guid ItemUID { get; set; }
        string ItemNo { get; set; }
        Guid SlotUID { get; set; }
        string SlotName { get; set; }
    }
}
