using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetModifyPayloadListParameters
    {
        Guid? WarehouseUID { get; set; }
        Guid? CustomerUID { get; set; }
        Guid? AreaUID { get; set; }
        Guid? BinUID { get; set; }
        Guid? SlotUID { get; set; }
        string[] ItemNoList { get; set; }
        Guid[] ItemUID { get; set; }
        string PodBarcode { get; set; }
        //string PayloadBarcode { get; set; }
        //string PayloadPodBarcode { get; set; }
        string ManifestRefNo { get; set; }
        string ManifestName { get; set; }
        string ManifestType { get; set; }
        string BolID { get; set; }
        string VesselID { get; set; }
        int[] PayloadStatus { get; set; }
        int[] PayloadType { get; set; }
    }
}
