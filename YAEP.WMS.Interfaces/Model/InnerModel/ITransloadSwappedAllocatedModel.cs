using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces.Model.InnerModel
{
    public interface ITransloadSwappedAllocatedModel
    {
        Guid UID { get; set; }
        Guid PayloadUID { get; set; }
        Guid PodUID { get; set; }
        Guid BarcodeUID { get; set; }
        Guid ReceivingWorkorderPayloadUID { get; set; }
        string Barcode { get; set; }
    }
}
