using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAEP.WMS.Interfaces
{
    public interface IAllocatedResponse : ICommonResponse
    {
        List<IAllocatedItemResponse> Results { get; set; }
    }
    public interface IAllocatedItemResponse : IAllocatedItemRequest
    {
        bool IsComplete { get; set; }
        int ShortageQty { get; set; }
        int OnhandType { get; set; }
        int Onhand { get; set; }
        string Location { get; set; }
        Guid ItemRefUID { get; set; }
        Guid PalletRefUID { get; set; }
        Guid ShipViaRefUID { get; set; }
        Guid ProcessItemUID { get; set; }
        Guid ReceivingWorkorderPodUID { get; set; }
    }
}
