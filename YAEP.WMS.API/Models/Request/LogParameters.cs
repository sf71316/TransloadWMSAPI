using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models
{
    public class LogParameters
    {
        public Guid belongtouid { get; set; }
        public int belongtotype { get; set; }
        public string message { get; set; }
    }
}