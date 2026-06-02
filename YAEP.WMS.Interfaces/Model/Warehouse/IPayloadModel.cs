using System;

namespace YAEP.WMS.Interfaces
{
    public interface IPayloadModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int? Type { get; set; }
        Guid PODUID { get; set; }
        Guid SlotUID { get; set; }
        Guid VesselUID { get; set; }
        Guid ItemUID { get; set; }
        int Quantity { get; set; }
        Guid? OriginalPayloadUID { get; set; }
        Guid PackageUID { get; set; }
        decimal VolumeLimit { get; set; }
        decimal WeightLimit { get; set; }
        int Status { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}