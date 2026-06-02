using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class AddCarrierTruckDTO : IAddCarrierTruckDTO
    {
        public Guid UID {get;set;}
        public string TruckName {get;set;}
        public Guid WarehouseUID {get;set;}
        public Guid CarrierType {get;set;}
        public string CreatedBy {get;set;}
        public int BatchCount {get;set;}
    }
}