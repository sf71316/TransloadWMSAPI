using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class ChangePalletToOtherTruckRequest : IChangePalletToOtherTruckRequest
    {
        public Guid CarrierTypeUID { get; set; }
        public List<Guid> CarrierPalletUIDs { get; set; }
        public string CarrierTruckName { get; set; }
        public Guid WarehouseUID { get; set; }
    }
}