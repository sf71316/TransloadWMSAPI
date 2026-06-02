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
    [Table("WMS_TicketInfo")]
    [DbTable("WMS_TicketInfo")]
    public class TicketInfoModel : ITicketInfoModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid TicketUID { get; set; }
        [ExplicitKey]
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid? WorkOrderPodUID { get; set; }
        public Guid? WorkOrderPayloadUID { get; set; }
        //  public Guid ItemUID { get; set; }
        public int EstQty { get; set; }
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
        //  public Guid? OriginalPackage { get; set; }
        //  public Guid? TargetPackage { get; set; }
        //  public Guid? OriginalSlotUID { get; set; }
        //  public Guid? TargetSlotUID { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
    }
}
