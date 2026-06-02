using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    public class BolDeleteParameters : IBolDeleteParameters
    {
        public Guid[] UID { get; set; }
    }
}