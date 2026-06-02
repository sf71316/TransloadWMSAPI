using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum BolStatus
    {
        Void = 0,
        Draft = 100,
        Review = 180,
        Reject = 170,
        Open = 200,
        OntheWay = 300,
        Arrived = 400,
        Receiving = 500,
        Shipping = 600,
        Complete = 700
    }
}
