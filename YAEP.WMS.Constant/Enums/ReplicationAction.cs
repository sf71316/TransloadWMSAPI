using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant
{
    public enum ReplicationAction
    {
        Receiving=100,
        CancelReceiving=101,
        Receivied =200,
        Outbound=300,
        Allocated=400,
        Deallocated=500,
        ModitiedOnhand=600,
        Move=700
    }
    public enum ReplicationOperate
    {
        Receive = 100,
        Allocated = 200,
        Inventory=300,
    }
}
