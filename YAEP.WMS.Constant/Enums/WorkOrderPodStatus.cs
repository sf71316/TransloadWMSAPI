using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum WorkOrderPodStatus
    {
        Draft = 100,
        Open = 200,
        Loading = 300,
        Loaded = 400,
        InPosition = 500,
        OffPosition = 600,
        Complete = 700

    }
}
