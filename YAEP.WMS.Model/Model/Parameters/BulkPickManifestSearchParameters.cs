using System;
using System.Collections.Generic; 
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class BulkPickManifestSearchParameters : IBulkPickManifestSearchParameters
    {
        public Guid? CustomerUID { get; set; }
        public string RefNo { get; set; }
        public string Name { get; set; }
        public string DateBy { get; set; } = "ETD";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OptionText { get; set; } = "Shipvia";
        public string OptionValue { get; set; }
        public List<int> TicketInfoStatus { get; set; } = new List<int>();
        public List<int> TicketInfoType { get; set; } = new List<int>();
        public List<int> ManifestType { get; set; } = new List<int>();

    }
}
