using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class AssignedPalletToTruckRequest : IAssignedPalletToTruckRequest
    {
        public string CarrierTruckName {get;set;}
        public Guid CarrierTypeUID {get;set; }
        public IEnumerable<string> carrierPalletIDs {get;set;}
        public Guid WarehouseUID {get;set;}
    }
}