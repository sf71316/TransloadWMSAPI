using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWarehouseComponentParameters
    {
        Guid? ConditionUID { get; set; }
        Guid? WarehouseUID { get; set; }
        string Name { get; set; }
        bool UnAssigned { get; set; }
    }
}
