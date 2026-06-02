using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class TicketAssignedListParameters : ITicketAssignedListParameters
    {
        public TicketAssignedListParameters()
        {
        }

        public Guid? BolUID { get; set; }
        public Guid? ManifestUID { get; set; }
        public Guid? TicketUID { get; set; }
    }
}
