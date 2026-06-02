using System;

namespace YAEP.WMS.Interfaces
{
    public interface ISlotUsageInfoModel
    {
        Guid SlotUID { get; set; }
        string SlotID { get; set; }
        decimal VolumeLimit { get; set; }
        decimal WeightLimit { get; set; }
        decimal Volume { get; set; }
        decimal Weight { get; set; }
        Guid WarehouseUID { get; set; }
        Guid? AreaUID { get; set; }
        Guid? BinUID { get; set; }
        int StorageSequence { get; set; }
        int AllocatedSequence { get; set; }
    }
}