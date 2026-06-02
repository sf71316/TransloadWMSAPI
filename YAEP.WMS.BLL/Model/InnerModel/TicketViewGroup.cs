using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    public class TicketViewGroup : ITicketViewGroup
    {
        public string ItemID { get; set; }
        public string UOM { get; set; }
        public int Qty { get; set; }
    }
    public class TicketViewGroupItem : ITicketViewGroupItem
    {
        public string PodName { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public string UOM { get; set; }
        public IEnumerable<ITicketViewGroup> Group { get; set; }
        public IEnumerable<ITicketInfoListViewModel> Items { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public int StorageType { get; set; }
        public string PodBarcode { get; set; }
        public WorkOrderPodProcessStatus WorkOrderPodStatus { get; set; }
        public string WorkOrderPodStatusName { get; set; }
    }
}
