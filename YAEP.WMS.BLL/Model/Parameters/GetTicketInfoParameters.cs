using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class GetTicketInfoParameters : IGetTicketInfoParameters
    {
        public IEnumerable<Guid> TicketUIDs { get; set; }
        public IEnumerable<Guid> TicketInfoUIDs { get; set; }
        public IEnumerable<String> TicketIDs { get; set; }
    }
}