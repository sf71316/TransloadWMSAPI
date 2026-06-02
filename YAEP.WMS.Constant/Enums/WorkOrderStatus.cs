using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum WorkOrderStatus
    {
        Draft=100,
        Open=200,
        OnTheWay=300,
        Arrived=400,
        Receiving=500,
        Shipping=600,
        Complete=700
    }
}
