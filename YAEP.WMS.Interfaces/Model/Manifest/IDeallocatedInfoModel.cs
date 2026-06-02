using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IDeallocatedInfoModel
    {
        List<IPayloadModel> AllocatedPayload { get; set; }
        List<IPayloadModel> OriginalPayload { get; set; }
        List<ITicketInfoModel> TicketInfos { get; set; }
    }
}
