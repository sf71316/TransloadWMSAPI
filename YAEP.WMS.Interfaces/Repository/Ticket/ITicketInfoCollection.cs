using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketInfoCollection
    {
        IEnumerable<ITicketInfoListViewModel> PayloadData { get; set; }
        IEnumerable<ITicketInfoListViewModel> PodData { get; set; }
    }
}
