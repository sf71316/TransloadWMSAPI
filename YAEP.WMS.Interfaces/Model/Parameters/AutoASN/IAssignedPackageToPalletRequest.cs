using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAssignedPackageToPalletRequest
    {
        string CarrierPalletID { get; set; }
        List<string> CarrierPackageid { get; set; }
        Guid WarehouseUID { get; set; }
    }
}
