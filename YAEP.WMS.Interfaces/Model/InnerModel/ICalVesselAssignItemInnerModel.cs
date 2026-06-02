using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICalVesselAssignItemInnerModel
    {
        Guid VesselUID { get; set; }
        Guid ItemUID { get; set; }
        Guid PackageUID { get; set; }
        int PackageQty { get; set; }
    }
}
