using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class PayloadTransactionLogParameters : IPayloadTransactionLogParameters
    {
        public PayloadTransactionLogParameters()
        {

        }
        public Guid? CustomerUID { get; set; }
        public Guid? WarehouseUID { get; set; }
        public int[] LogTypes { get; set; }
        public int[] PayloadTypes { get; set; }
        public Guid? TargetArea { get; set; }
        public Guid? TargetBin { get; set; }
        public Guid? TargetSlot { get; set; }
        public string VesselRefNo { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string VesselNo { get; set; }
        public string BolNo { get; set; }
        public string TicketID { get; set; }
        public Guid[] ItemUIDs { get; set; }
        public Guid? OriginalArea { get; set; }
        public Guid? OriginalBin { get; set; }
        public Guid? OriginalSlot { get; set; }
        public DateTime? LogStartDate { get; set; }
        public DateTime? LogEndDate { get; set; }
        public string RefNo { get; set; }
    }
}
