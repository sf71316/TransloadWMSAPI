using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ILabelViewModel
    {
        string Barcode { get; set; }
        Guid BarcodeUID { get; set; }
        Guid FileUID { get; set; }

    }
}
