using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class SearchCarrierTruckParameters : ISearchCarrierTruckParameters
    {
        public int[] CarrierTruckStatus { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public Guid WarehouseUID { get; set; }
        public string TimeZone { get; set; }
    }
}