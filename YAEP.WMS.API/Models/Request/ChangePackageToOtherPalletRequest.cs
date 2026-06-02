using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class ChangePackageToOtherPalletRequest : IChangePackageToOtherPalletRequest
    {
        public string CarrierPalletID {get;set;}
        public List<Guid> CarrierPalletInfoUIDs {get;set;}
        public Guid WarehouseUID {get;set;}
    }
}