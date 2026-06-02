using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class PickAllParameters : IPickAllParameters
    {
        public IEnumerable<Guid> BolUID { get; set; }
        public IEnumerable<Guid> VesselUID { get; set; }
        public IEnumerable<Guid> WorkPayloadUID { get; set; }
        public int[] TicketInfoStatus { get; set; }
    }
}
