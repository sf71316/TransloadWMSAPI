using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum WorkOrderPayloadStatus
    {
        WaitingForProcessing=100,
        Processing=200,
        Active=300,
        Inactive=0
    }
}
