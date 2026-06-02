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
    [Table("WMS_Payload_TransactionLog")]
    [DbTable("WMS_Payload_TransactionLog")]
    public class PayloadTransactionLogModel : IPayloadTransactionLogModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public Guid WorkOrderPayloadUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid PayloadUID { get; set; }
        public int QtyBeforeTX { get; set; }
        public int QtyAfterTX { get; set; }
        public Guid? OriginalSlotUID { get; set; }
        public Guid? OriginalPackage { get; set; }
        public Guid? TargetSlotUID { get; set; }
        public Guid TargetPackage { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? TicketInfoUID { get; set; }
    }
}
