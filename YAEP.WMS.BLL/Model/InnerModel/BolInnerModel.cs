using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class BolInnerModel : IBolModel
    {
        public Guid UID {get;set;}
        public string ID {get;set;}
        public string Name {get;set;}
        public int? Type {get;set;}
        public string RefNo {get;set;}
        public string Phone {get;set;}
        public Guid ManifestUID {get;set;}
        public Guid ShipViaUID {get;set;}
        public Guid ShipMethodUID {get;set;}
        public string Contact {get;set;}
        public string ShipToZip {get;set;}
        public string ShipToAddress {get;set;}
        public string ShipToCity {get;set;}
        public string ShipToState {get;set;}
        public string ShipToCountry {get;set;}
        public string ShipFromZip {get;set;}
        public string ShipFromAddress {get;set;}
        public string ShipFromCity {get;set;}
        public string ShipFromState {get;set;}
        public string ShipFromCountry {get;set;}
        public DateTime? DeliveryDate {get;set;}
        public DateTime? ETA {get;set;}
        public DateTime? RevETA {get;set;}
        public BolStatus Status {get;set;}
        public string Description {get;set;}
        public DateTime? CreatedOn {get;set;}
        public string CreatedBy {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime? ModifiedOn {get;set;}
    }
}
