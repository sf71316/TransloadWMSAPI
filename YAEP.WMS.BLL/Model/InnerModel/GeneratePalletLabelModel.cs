using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class GeneratePalletLabelModel : IGeneratePalletLabelModel, IGenerateLabelImage
    {
        public DateTime ReceivingDate { get; set; }
        public string CustPON { get; set; }
        public string SysPon { get; set; }
        public string ContainerNo { get; set; }
        public int ReceivingQty { get; set; }
        public string Notes { get; set; }
        public byte[] Barcode { get; set; }
        public string BarcodeContent { get; set; }
    }
}
