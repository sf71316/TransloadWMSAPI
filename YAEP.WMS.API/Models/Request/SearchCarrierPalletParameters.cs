using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class SearchCarrierPalletParameters : ISearchCarrierPalletParameters
    {
        public Guid WarehouseUID {get;set;}
        public DateTime? StartDateTime {get;set;}
        public DateTime? EndDateTime {get;set;}
        public string SearchDateType {get;set;}
        public Guid? CarrierType {get;set;}
        public string CarrierTruckID {get;set;}
        public Guid? CarrierTruckUID {get;set;}
        public int?[] CarrierTruckStatus {get;set;}
        public int?[] CarrierPalletStatus {get;set;}
    }
}