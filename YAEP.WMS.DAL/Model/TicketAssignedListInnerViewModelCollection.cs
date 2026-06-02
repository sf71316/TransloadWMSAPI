using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class TicketAssignedListInnerViewModelCollection : ITicketAssignedListViewModelCollection
    {
        public IEnumerable<ITicketAssignedListViewModel> PayloadData { get; set; }
        public IEnumerable<ITicketAssignedListViewModel> PodData { get; set; }
    }
}
