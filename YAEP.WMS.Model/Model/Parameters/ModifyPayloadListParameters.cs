using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class ModifyPayloadListParameters : IGetModifyPayloadListParameters
    {
        public ModifyPayloadListParameters()
        {
            ItemNoList = new string[] { };
            PayloadStatus = new int[] { };
        }
        public Guid? WarehouseUID { get; set; }
        public Guid? CustomerUID { get; set; }
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public Guid? SlotUID { get; set; }
        public string[] ItemNoList { get; set; }
        public string PodBarcode { get; set; }
        //public string PayloadBarcode {get;set;}
        public string ManifestRefNo { get; set; }
        public string ManifestName { get; set; }
        public string ManifestType { get; set; }
        public string BolID { get; set; }
        public string VesselID { get; set; }
        public Guid[] ItemUID { get; set; }
        public int[] PayloadStatus { get; set; }
        public int[] PayloadType { get; set; }
        //public string PayloadPodBarcode { get; set; }
    }
}
