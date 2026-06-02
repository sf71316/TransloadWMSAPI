using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IPodSelectListModel
    {
        Guid PodUID { get; set; }
        string PodName { get; set; }
        string WarehouseName { get; set; }
        string BolRefNo { get; set; }
        string VesselRefNo { get; set; }
        string AreaName { get; set; }
        string BinName { get; set; }
        string SlotName { get; set; }
        decimal VolumeLimit { get; set; }
        decimal WeightLimit { get; set; }
        decimal TTLUsedVolume { get; set; }
        decimal TTLUsedWeight { get; set; }
    }
}
