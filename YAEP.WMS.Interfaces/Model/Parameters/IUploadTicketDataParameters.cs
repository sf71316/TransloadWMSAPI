using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IUploadTicketDataParameter : IUploadTicketData
    {
        ITicketInfoParameter Item { get; set; }
    }
    public interface IUploadTicketData
    {

        MoveTicketCommand TicketInfoCommand { get; set; }
        TicketType ServiceItem { get; set; }

    }
    public interface ITicketInfoParameter
    {
        bool IsAllPass { get; set; }
        bool IsAllShortage { get; set; }
        bool IsPodScan { get; set; }
        Guid TicketInfoUID { get; set; }
        Guid WorkOrderPodUID { get; set; }
        ScanType ScanType { get; set; }
        IEnumerable<IUploadTicketBarcode> Barcode { get; set; }
        int? ActQty { get; set; }
        int? ShtQty { get; set; }
        int? SavQty { get; set; }
        int? Status { get; set; }
    }


}
