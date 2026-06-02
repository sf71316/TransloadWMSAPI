using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IEditOnhandParameters
    {
        Guid WarehouseUID { get; set; }
        Guid ItemUID { get; set; }
        Guid TargetPackageUID { get; set; }
        Guid SlotUID { get; set; }
        int Onhand { get; set; }
    }
}
