using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    public interface ITicketViewGroupItem
    {
        string PodName { get; set; }
        Guid WorkOrderPodUID { get; set; }
        string UOM { get; set; }
        string OperationInstruction { get; set; }
        string OperationSuggestion { get; set; }
        int StorageType { get; set; }
        WorkOrderPodProcessStatus WorkOrderPodStatus { get; set; }
        string WorkOrderPodStatusName { get; set; }
        string PodBarcode { get; set; }
        IEnumerable<ITicketViewGroup> Group { get; set; }
        IEnumerable<ITicketInfoListViewModel> Items { get; set; }

    }
    public interface ITicketViewGroup
    {
        string ItemID { get; set; }
        string UOM { get; set; }
        int Qty { get; set; }
    }
}
