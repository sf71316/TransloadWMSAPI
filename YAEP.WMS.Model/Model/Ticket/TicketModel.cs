using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Ticket")]
    [DbTable("WMS_Ticket")]
    public class TicketModel : ITicketModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        [ExplicitKey]
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public Guid WorkOrderUID { get; set; }
        public Guid ServiceItemUID { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long TicketSequence { get; set; }
        public int ManifestType { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public Guid WarehouseUID { get; set; }
    }
}
