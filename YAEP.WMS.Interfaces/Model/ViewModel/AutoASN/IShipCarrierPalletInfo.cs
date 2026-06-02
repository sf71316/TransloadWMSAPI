using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IShipCarrierPalletInfo
    {
        Guid UID { get; set; }
        Guid CarrierPalletUID { get; set; }
        Guid ShipviaUID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        string CustPON { get; set; }
        string CustID { get; set; }
        string Syspon { get; set; }
        string BolNo { get; set; }
        string TrackingNo { get; set; }
        int Status { get; set; }
        string StatusName { get; set; }
        DateTime? CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
        string ModifiedBy { get; set; }
    }
}
