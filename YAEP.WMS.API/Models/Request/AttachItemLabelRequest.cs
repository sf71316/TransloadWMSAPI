using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class AttachItemLabelRequest
    {
        public List<Guid> Payloaduid { get; set; }
    }
}