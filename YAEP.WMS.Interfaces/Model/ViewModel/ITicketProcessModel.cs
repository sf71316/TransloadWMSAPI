using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    //TODO 相關聯的資料讀取層需取得必要資料
    public interface ITicketProcessModel : ITicketInfoModel, ITicketInfoCommonViewModel
    {
        int ManifestType { get; set; }
        int OriginalQty { get; set; }
        int PayloadQty { get; set; }
        Guid ItemUID { get; set; }
        Guid? ItemGroupUID { get; set; }
        Guid WarehouseUID { get; set; }
        Guid VesselUID { get; set; }
        Guid PayloadUID { get; set; }
        Guid PartyUID { get; set; }
        Guid PodUID { get; set; }
        int StorageType { get; set; }
        int PayloadType { get; set; }
        int? OriginalPayloadType { get; set; }
        string RefNo { get; set; }
        IEnumerable<ILabelModel> Barcodes { get; set; }
        IEnumerable<ITicketModel> ParentTickets { get; set; }
        IEnumerable<ITicketProcessParentTicketInfoModel> InboundPartentTicketInfos { get; set; }
    }
}
