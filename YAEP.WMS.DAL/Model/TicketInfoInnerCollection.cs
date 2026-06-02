using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class TicketInfoInnerCollection : ITicketInfoCollection
    {
        public IEnumerable<ITicketInfoListViewModel> PayloadData { get; set; }
        public IEnumerable<ITicketInfoListViewModel> PodData { get; set; }
    }
}
