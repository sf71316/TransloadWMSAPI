using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces.Model.InnerModel
{
    public interface IGetPalletlabelBarcodeParamers
    {
        IEnumerable<Guid> PodUIDs { get; set; }
    }
    public interface IGetPalletlabelBarcodeByMobileParamers
    {
        List<IPlaylabelBarcodeItems> Items { get; set; }
    }
    public interface IPlaylabelBarcodeItems
    {
        Guid ItemUID { get; set; }
        Guid SlotUID { get; set; }
    }
}
