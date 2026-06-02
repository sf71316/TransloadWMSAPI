using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselManifestWorkOrderPayloadInfo
    {
        IEnumerable<IVesselModel> VesselManifestModel { get; set; }
        IEnumerable<IWorkOrderPayloadModel> WorkOrderPayloadModel { get; set; }
    }
}
