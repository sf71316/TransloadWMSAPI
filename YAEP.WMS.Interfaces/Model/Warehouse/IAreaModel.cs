using System;

namespace YAEP.WMS.Interfaces
{
    public interface IAreaModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        Guid ImageUID { get; set; }
        int? Type { get; set; }
        Guid WarehouseUID { get; set; }
        decimal? VolumeLimit { get; set; }
        decimal? WeightLimit { get; set; } 
        int Status { get; set; }
        int AllocatedSequence { get; set; }
        int StorageSequence { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}