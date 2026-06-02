using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAllocatedModel
    {
        Guid WarehouseUID { get; set; }
        Guid ItemUID { get; set; }
        Guid PackageUID { get; set; }
        int Quantity { get; set; }
        int OriginalPayloadType { get; set; }
    }
}
