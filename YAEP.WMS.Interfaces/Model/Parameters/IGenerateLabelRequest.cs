using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IGenerateLabelRequest
    {
        BarcodeMethod BarcodeMethod { get; set; }
        BarcodeKind BarcodeKind { get; set; }

        //BarcodeType BarcodeType { get; set; }
        Guid BelongToUID { get; set; }
        int BelongToType { get; set; }
        HttpPostedFile File { get; set; }
        int GenerateQty { get; set; }
    }
}
