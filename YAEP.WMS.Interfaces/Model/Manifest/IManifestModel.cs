using System;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IManifestModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int Type { get; set; }
        Guid WarehouseUID { get; set; }
        Guid PartyUID { get; set; }
        string RefNo { get; set; }
        decimal? Volume { get; set; }
        decimal? Weight { get; set; }
        ManifestStatus Status { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}