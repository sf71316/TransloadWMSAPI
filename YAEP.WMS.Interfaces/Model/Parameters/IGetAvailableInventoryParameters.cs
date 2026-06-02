using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetAvailableInventoryParameters
    {
        Guid? Warehouse { get; set; }
        Guid? AreaUID { get; set; }
        Guid? BinUID { get; set; }
        Guid? SlotUID { get; set; }
        Guid ItemUID { get; set; }
        string Option { get; set; }
        string OptionText { get; set; }
    }
}
