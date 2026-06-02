using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class GetSequenceRequest
    {
        public Guid BelongToUID { get; set; }
        public string BelongToTag { get; set; }
    }
}