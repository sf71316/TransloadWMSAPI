using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class AddCarrierPalletDTO : IAddCarrierPalletDTO
    {
        public string PalletName {get;set;}
        public Guid WarehouseUID {get;set;}
        public Guid CarrierType {get;set;}
        public int BatchCount {get;set;}
    }
}