using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class BulkPickViewModel : IBulkPickViewModel
    { 
        public Guid UID { get; set; }
        public string ID { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Name { get; set; }
        public string PartyName { get; set; }
        public Guid TicketUID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string TicketNo { get; set; }
    }
}
