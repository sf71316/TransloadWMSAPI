using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IBulkPickWorkOrderPayloadRelationModel
    {
        Guid UID { get; set; }
        Guid BulkPickWorkOrderPayloadUID { get; set; }
        Guid OriginalWorkOrderPayloadUID { get; set; }
        int Status { get; set; }
    }
}
