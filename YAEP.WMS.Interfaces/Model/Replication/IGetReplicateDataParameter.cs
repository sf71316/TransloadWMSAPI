using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetReplicateDataParameter
    {
        IEnumerable<Guid> TicketInfoUID { get; set; }
        IEnumerable<Guid> TicketUID { get; set; }
        IEnumerable<Guid> ManifestUID { get; set; }
        IEnumerable<Guid> BOLUID { get; set; }
        IEnumerable<Guid> WorkOrderPayloadUID { get; set; }
        int TicketInfoType { get; set; }
    }
}
