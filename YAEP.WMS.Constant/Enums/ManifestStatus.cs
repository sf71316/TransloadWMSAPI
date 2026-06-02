using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum ManifestStatus
    {
        Void = 0,
        Draft = 100,
        Review = 200,
        Reject = 300,
        Open = 400,
        WaitingtoAssignWorker = 410,
        Complete = 500
    }
   
}
