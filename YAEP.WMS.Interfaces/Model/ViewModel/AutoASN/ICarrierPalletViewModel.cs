using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICarrierPalletViewModel
    {
        string CarrierType { get; set; }
        string CarrierTruckName { get; set; }
        string CarrierTruckID { get; set; }
        string CarrierPalletName { get; set; }
        string CarrierPalletID { get; set; }
        Guid CarrierTruckUID { get; set; }
        Guid CarrierPalletUID { get; set; }
        DateTime? CreateDate { get; set; }
        DateTime? DepartureDate { get; set; }
        int CarrierPalletStatus { get; set; }
        int PackageCount { get; set; }
        string CarrierPalletStatusName { get; set; }
        string Locid { get; set; }
    }
}
