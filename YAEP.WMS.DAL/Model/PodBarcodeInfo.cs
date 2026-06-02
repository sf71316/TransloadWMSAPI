using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class PodBarcodeInfo : IPodBarcodeInfo
    {
        public string Barcode { get; set; }
        public int Qty { get; set; }
        public string ItemName { get; set; }
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public BarcodeType Type { get; set; }
        public LabelBelongType BelongToType { get; set; }
        public Guid BelongToUID { get; set; }
        public int Status { get; set; }
        public Guid PackageUID { get; set; }
    }
}
