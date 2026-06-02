using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class TicketProcessParentTicketInfoModel : TicketInfoInnerModel, ITicketProcessParentTicketInfoModel
    {
        public Guid TicketInfoUID { get; set; }
    }
    internal class TicketInfoInnerModel : ITicketInfoModel
    {
        public Guid UID { get; set; }
        public Guid TicketUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid? WorkOrderPodUID { get; set; }
        public Guid? WorkOrderPayloadUID { get; set; }
        public int EstQty { get; set; }
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid BolUID { get; set; }
    }
    public class TicketInfoAssignedInnerModel : IAssignedTicketInfoModel
    {
        public Guid UID { get; set; }
        public Guid TicketUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid? WorkOrderPodUID { get; set; }
        public Guid? WorkOrderPayloadUID { get; set; }
        public int EstQty { get; set; }
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public Guid BolUID { get; set; }
    }
}
