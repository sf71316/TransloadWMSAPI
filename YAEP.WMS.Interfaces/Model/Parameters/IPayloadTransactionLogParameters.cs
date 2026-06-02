using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IPayloadTransactionLogParameters
    {
        Guid? WarehouseUID { get; set; }
        Guid[] ItemUIDs { get; set; }
        int[] LogTypes { get; set; }
        int[] PayloadTypes { get; set; }
        Guid? CustomerUID { get; set; }
        Guid? TargetArea { get; set; }
        Guid? TargetBin { get; set; }
        Guid? TargetSlot { get; set; }
        Guid? OriginalArea { get; set; }
        Guid? OriginalBin { get; set; }
        Guid? OriginalSlot { get; set; }
        DateTime? LogStartDate { get; set; }
        DateTime? LogEndDate { get; set; }
        string VesselRefNo { get; set; }
        string RefNo { get; set; }
        string From { get; set; }
        string To { get; set; }
        string VesselNo { get; set; }
        string BolNo { get; set; }
        string TicketID { get; set; }
    }
}
