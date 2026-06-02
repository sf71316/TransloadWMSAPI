using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class BulkPickInfobyOutboundViewModel : IBulkPickInfobyOutboundViewModel
    {
        public Guid TicketUID {get;set;}
        public Guid BulkPickUID {get;set;}
        public string BulkPickID {get;set;}
        public Guid ItemUID {get;set;}
        public string PartyName {get;set;}
        public string ItemNo {get;set;}
        public int? EstQty {get;set;}
        public int? ActQty {get;set;}
        public int? ShtQty {get;set;}
        public int? SavQty {get;set;}
        public string From {get;set;}
        public string To {get;set;}
        public Guid TicketInfoUID {get;set;}
        public string TicketInfoRelationID {get;set;}
    }
}
