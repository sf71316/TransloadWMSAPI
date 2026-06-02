using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces.Model.InnerModel;

namespace YAEP.WMS.BLL.Model.Parameters
{
    internal class GetPalletlabelBarcodeByMobileParamers : IGetPalletlabelBarcodeByMobileParamers
    {
        public GetPalletlabelBarcodeByMobileParamers()
        {
            this.Items = new List<IPlaylabelBarcodeItems>();
        }
        public List<IPlaylabelBarcodeItems> Items { get; set; }
    }
    internal class PlaylabelBarcodeItems : IPlaylabelBarcodeItems
    {
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
    }
}
