using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class SearchCarrierPalletInfoParameters : ISearchCarrierPalletInfoParameters
    {
        public List<Guid> CarrierPalletUIDs {get;set;}
        public List<Guid> CarrierPalletInfoUIDs {get;set;}
        public List<string> TrackingNo {get;set;}
        public List<string> Syspon {get;set;}
    }
}