using System;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IBolModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int? Type { get; set; }
        string RefNo { get; set; }
        string Phone { get; set; }
        Guid ManifestUID { get; set; }
        Guid ShipViaUID { get; set; }
        Guid ShipMethodUID { get; set; }
        string Contact { get; set; }
        string ShipToZip { get; set; }
        string ShipToAddress { get; set; }
        string ShipToCity { get; set; }
        string ShipToState { get; set; }
        string ShipToCountry { get; set; }
        string ShipFromZip { get; set; }
        string ShipFromAddress { get; set; }
        string ShipFromCity { get; set; }
        string ShipFromState { get; set; }
        string ShipFromCountry { get; set; }
        DateTime? DeliveryDate { get; set; }
        DateTime? ETA { get; set; }
        DateTime? RevETA { get; set; }
        BolStatus Status { get; set; }
        string Description { get; set; }
        DateTime? CreatedOn { get; set; }

        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}