using System;
using System.Collections.Generic; 
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class BulkPickSearchParameters : IBulkPickSearchParameters
    {
        public Guid[] UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string PartyName { get; set; }
        public string CustomerPartyName { get; set; }
        public List<int> Status { get; set; } = new List<int>();
    }
}
