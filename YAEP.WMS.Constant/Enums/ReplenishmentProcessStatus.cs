using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum ReplenishmentProcessStatus
    {
        NoNeedToDoReplenishment = 100,
        NoOnHandToDoReplenishment = 200,
        ReplenishmentFailed = 300,
        ReplenishmentCompleted = 400,
        DataHasDamage=500
    }
}
