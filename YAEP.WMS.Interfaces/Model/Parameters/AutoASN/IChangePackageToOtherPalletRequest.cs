using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IChangePackageToOtherPalletRequest
    {
        string CarrierPalletID { get; set; }
        List<Guid> CarrierPalletInfoUIDs { get; set; }
        Guid WarehouseUID { get; set; }
    }
}
