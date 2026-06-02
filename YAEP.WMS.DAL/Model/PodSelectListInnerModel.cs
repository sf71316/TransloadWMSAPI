using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class PodSelectListInnerModel : IPodSelectListModel
    {
        public Guid PodUID { get; set; }
        public string BolRefNo { get; set; }
        public string VesselRefNo { get; set; }
        public string AreaName { get; set; }
        public string BinName { get; set; }
        public string SlotName { get; set; }
        public decimal VolumeLimit { get; set; }
        public decimal WeightLimit { get; set; }
        public decimal TTLUsedVolume { get; set; }
        public decimal TTLUsedWeight { get; set; }
        public string PodName { get; set; }
        public string WarehouseName { get; set; }
    }
}
