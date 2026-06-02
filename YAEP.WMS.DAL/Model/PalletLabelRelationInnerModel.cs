using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces.Model.InnerModel;

namespace YAEP.WMS.DAL.Model
{
    internal class PalletLabelRelationInnerModel : IPalletLabelRelationModel
    {
        public int ManifestType { get; set; }
        public int LabelStatus { get; set; }
        public int LabelType { get; set; }
        public Guid ManifestUID { get; set; }
        public string Barcode { get; set; }
        public Guid PackageUID { get; set; }
        public int PayloadQty { get; set; }
        public Guid ItemUID { get; set; }
        public bool IsPack { get; set; }
        public Guid SlotUID { get; set; }
        public Guid PayloadUID { get; set; }
        public Guid WorkorderPayloadUID { get; set; }
        public Guid WorkorderPodUID { get; set; }
        public int LabelBelongType { get; set; }
        public int MoveTicketInfoStatus { get; set; }
    }
}
