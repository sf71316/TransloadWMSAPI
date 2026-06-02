using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketLabelViewModel
    {
        string Barcode { get; set; }
        string StatusName { get; set; }
        int Status { get; set; }
        /// <summary>
        /// WMS_label Type field
        /// </summary>
        int BarcodeType { get; set; }
        string BarcodeTypeName { get; set; }
        int BelongToType { get; set; }
        int AddQty { get; set; }
        Guid BelongToUID { get; set; }
        Guid AttachmentUID { get; set; }
    }
    public interface ILabelGenerateViewModel : ITicketLabelViewModel
    {
        Guid BarcodeUID { get; set; }
        Guid FileUID { get; set; }
    }
}
