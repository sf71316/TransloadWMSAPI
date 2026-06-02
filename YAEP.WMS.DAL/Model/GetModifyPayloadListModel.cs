using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class GetModifyPayloadListModel : IGetModifyPayloadListModel
    {
        public GetModifyPayloadListModel()
        {
            this.Package = new List<IGetModifyPayloadPackageItem>();
        }
        public Guid PayloadUID { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public Guid ItemUID { get; set; }
        public IList<IGetModifyPayloadPackageItem> Package { get; set; }
        public Guid PackageUID { get; set; }
        public string PackageName { get; set; }
        public string AreaID { get; set; }
        public string BinID { get; set; }
        public string SlotID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid PodUID { get; set; }

        public int EachQty { get; set; }
        public int Qty { get; set; }
        public string ManifestRefNo { get; set; }
        public string ManifestName { get; set; }
        public string ManifestTypeName { get; set; }
        public string BolID { get; set; }
        public string VesselID { get; set; }
        public int ManifestType { get; set; }
        public string PayloadID { get; set; }
        public Guid WarehouseUID { get; set; }
        public int PayloadStatus { get; set; }
        public string PayloadStatusName { get; set; }
        public int SlotStatus { get; set; }
        public int SlotType { get; set; }
        public string SlotStatusName { get; set; }
        public string SlotTypeName { get; set; }
        public int PayloadType { get; set; }
        public string PayloadTypeName { get; set; }

        public bool IsVirtualItem { get; set; }
        public Guid AreaUID { get; set; }
        public Guid BinUID { get; set; }
    }
}
