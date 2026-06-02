using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISearchCarrierPalletInfoParameters
    {
        List<Guid> CarrierPalletUIDs { get; set; }
        List<Guid> CarrierPalletInfoUIDs { get; set; }
        List<string> TrackingNo { get; set; }
        List<string> Syspon { get; set; }
    }
}
