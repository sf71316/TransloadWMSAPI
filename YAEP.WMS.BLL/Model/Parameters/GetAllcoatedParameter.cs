using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ReplicateDataParameter : IGetReplicateDataParameter
    {
        public IEnumerable<Guid> TicketInfoUID { get; set; }
        public IEnumerable<Guid> TicketUID { get; set; }
        public IEnumerable<Guid> ManifestUID { get; set; }
        public IEnumerable<Guid> BOLUID { get; set; }
        public IEnumerable<Guid> WorkOrderPayloadUID { get; set; }
        public int TicketInfoType { get; set; }
    }
}
