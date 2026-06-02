using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class ShipviaPaymentInfoInnerModel : IShipviaPaymentInfoModel
    {
        public Guid UID {get;set;}
        public Guid PartyUID {get;set;}
        public int Type {get;set;}
        public string Account {get;set;}
        public string Password {get;set;}
        public string CreatedBy {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime? CreatedOn {get;set;}
        public DateTime? ModifiedOn {get;set;}
    }
}
