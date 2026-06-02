using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IHaveTicketParameters
    {
        Guid[] workOrderPodGuids { get; set; }
        Guid[] workOrderPayloadGuids { get; set; }
    }
}
