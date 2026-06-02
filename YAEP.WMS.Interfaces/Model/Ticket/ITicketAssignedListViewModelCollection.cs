using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketAssignedListViewModelCollection
    {
        IEnumerable<ITicketAssignedListViewModel> PayloadData { get; set; }
        IEnumerable<ITicketAssignedListViewModel> PodData { get; set; }
    }
}
