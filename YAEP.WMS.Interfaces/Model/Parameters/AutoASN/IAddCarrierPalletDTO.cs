using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAddCarrierPalletDTO
    {
        string PalletName { get; set; }
        Guid WarehouseUID { get; set; }
        Guid CarrierType { get; set; }
        int BatchCount { get; set; }
    }
}
