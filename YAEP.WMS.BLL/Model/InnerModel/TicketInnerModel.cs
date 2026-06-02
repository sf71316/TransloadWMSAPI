using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class TicketInnerModel : ITicketModel
    {
        public Guid UID { get; set; }
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
    //TODO 相關聯的資料讀取層需取得必要資料
    internal class TicketInfoInnerModel : ITicketInfoModel
    {
        public TicketInfoInnerModel()
        {

        }
        public TicketInfoInnerModel(ITicketInfoModel oldModel)
        {
            this.TicketUID = oldModel.TicketUID;
            this.ID = oldModel.ID;
            this.Name = oldModel.Name;
            this.Type = oldModel.Type;
            this.WorkOrderPayloadUID = oldModel.WorkOrderPayloadUID;
            this.WorkOrderPodUID = oldModel.WorkOrderPodUID;
            this.Status = oldModel.Status;
            this.Description = oldModel.Description;
            this.OperationInstruction = oldModel.OperationInstruction;
            this.OperationSuggestion = oldModel.OperationSuggestion;
            this.EstQty =oldModel.EstQty;
            this.ActQty= oldModel.ActQty;
            this.ShtQty= oldModel.ShtQty;
            this.SavQty= oldModel.SavQty;
        }
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

    }

}
