using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class SetWorkOrderPodBarcodeParameters : ISetWorkOrderPodBarcodeParameters
    {
        public Guid BarcodeUID { get; set; }
        public Guid PodUID { get; set; }
    }
}