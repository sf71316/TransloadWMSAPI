using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IPodBarcodeInfo
    {
        BarcodeType Type { get; set; }
        LabelBelongType BelongToType { get; set; }
        string ItemName { get; set; }
        Guid ItemUID { get; set; }
        Guid SlotUID { get; set; }
        Guid BelongToUID { get; set; }
        Guid PackageUID { get; set; }
        string Barcode { get; set; }
        int Qty { get; set; }
        int Status { get; set; }
    }
}
