using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface INominateReplenishmentModel
    {
        Guid? WarehouseUID { get; set; }
        IEnumerable<Guid> WorkOrderPayloadUIDList { get; set; }
    }
}
