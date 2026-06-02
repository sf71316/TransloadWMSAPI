using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class HomeAddressRelationRequest
    {
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public int Type { get; set; }
        public int OutboundType { get; set; }
        public int Sequence { get; set; }
    }
}