using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum SlotType
    {
        Regular = 1,
        LandingZone = 2,
        InboundTemp = 100,
        OutboundTemp = 200,
        OpenStorageArea = 300,
        Rack_LTL_Parcel = 400,
        Rack_LTL = 500,
        Rack_Parcel = 600,
        PackingArea = 700,
        StagingArea_LTL = 800,
        StagingArea_Parcel = 900,
        Dummy=1000,
        FutureAllocated = 1100,
    }
}
