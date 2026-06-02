using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces.Model.InnerModel
{
    public interface IPalletLabelRelationModel
    {
        int ManifestType { get; set; }
        int LabelStatus { get; set; }
        int LabelType { get; set; }
        int LabelBelongType { get; set; }
        Guid ManifestUID { get; set; }
        string Barcode { get; set; }
        Guid PackageUID { get; set; }
        int PayloadQty { get; set; }
        Guid ItemUID { get; set; }
        bool IsPack { get; set; }
        Guid SlotUID { get; set; }
        Guid PayloadUID { get; set; }
        Guid WorkorderPayloadUID { get; set; }
        Guid WorkorderPodUID { get; set; }
        int MoveTicketInfoStatus { get; set; }


    }
}
