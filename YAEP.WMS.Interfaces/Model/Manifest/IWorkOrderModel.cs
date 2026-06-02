using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        Guid ManifestUID { get; set; }
        Guid VesselUID { get; set; }
        int Status { get; set; }
        int Type { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }

    }
}
