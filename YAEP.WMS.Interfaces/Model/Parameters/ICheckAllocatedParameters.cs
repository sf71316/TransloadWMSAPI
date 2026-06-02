using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICheckAllocatedParameters
    {
        Guid VesselMainifestUID { get; set; }
        IList<ICheckAllocatedItem> Items { get; set; }
    }
    public interface ICheckAllocatedItem
    {
        Guid ItemUID { get; set; }
        Guid PayloadUID { get; set; }
        int AllocatedQty { get; set; }
    }
}
