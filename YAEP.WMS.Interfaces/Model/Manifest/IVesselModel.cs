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
        string StatusName { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}