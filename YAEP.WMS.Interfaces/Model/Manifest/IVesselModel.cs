using System;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int? Type { get; set; }
        string RefNo { get; set; }
        Guid BolUID { get; set; }
        int Status { get; set; }
        // Transload 容器實體屬性（掛在 Vessel；RefNo=ConNo）。皆可空。
        string SealNo { get; set; }
        int? ContainerSize { get; set; }
        int? LoadingType { get; set; }
        int? StackableType { get; set; }
        DateTime? ArrivalDate { get; set; }
        decimal? Weight { get; set; }
        decimal? Volume { get; set; }
        string StatusName { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}