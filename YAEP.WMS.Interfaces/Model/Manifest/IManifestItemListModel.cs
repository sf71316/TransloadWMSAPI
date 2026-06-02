using System;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IManifestItemListModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int Type { get; set; }
        Guid ManifestUID { get; set; }
        Guid PackageUID { get; set; }
        string PackageName { get; set; }
        string VersionName { get; set; }
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        string ItemName { get; set; }
        string ItemDescription { get; set; }
        int? PackageQty { get; set; }
        int OnhandType { get; set; }
        decimal? Volume { get; set; }
        decimal? Weight { get; set; }
        ManifestItemListStatus Status { get; set; }
        string StatusName { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}