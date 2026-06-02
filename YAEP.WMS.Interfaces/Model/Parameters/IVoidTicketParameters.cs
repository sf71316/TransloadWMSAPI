using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IVoidTicketParameters
    {
        Guid? ManifestUID { get; set; }
        Guid? BolUID { get; set; }
        Guid? WorkOrderUID { get; set; }
        Guid[] VesselUID { get; set; }
        string ModifiedBy { get; set; }
    }
}
