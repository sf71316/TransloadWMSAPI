using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class AssignedPackageToPalletRequest : IAssignedPackageToPalletRequest
    {
        public string CarrierPalletID {get;set;}
        public List<string> CarrierPackageid {get;set;}
        public Guid WarehouseUID {get;set;}
    }
}