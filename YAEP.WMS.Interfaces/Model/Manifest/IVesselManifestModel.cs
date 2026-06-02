using System;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselManifestModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int? Type { get; set; }
        Guid PartyUID { get; set; }
        Guid BolUID { get; set; }
        Guid VesselUID { get; set; }
        Guid ItemUID { get; set; }
        Guid PackageUID { get; set; }
        Guid ManifestItemUID { get; set; }
        string RefNo { get; set; }
        decimal Volume { get; set; }
        decimal Weight { get; set; }
        int Qty { get; set; }
        int OnhandType { get; set; }
        int? Status { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}