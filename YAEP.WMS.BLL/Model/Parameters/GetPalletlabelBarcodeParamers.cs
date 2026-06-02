using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces.Model.InnerModel;

namespace YAEP.WMS.BLL.Model.Parameters
{
    internal class GetPalletlabelBarcodeParamers : IGetPalletlabelBarcodeParamers
    {
        public IEnumerable<Guid> PodUIDs { get; set; }
    }
}
