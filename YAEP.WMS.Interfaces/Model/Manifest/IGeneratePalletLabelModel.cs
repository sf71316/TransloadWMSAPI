using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGeneratePalletLabelModel: IGenerateLabelImage
    {
        DateTime ReceivingDate { get; set; }
        string CustPON { get; set; }
        string SysPon { get; set; }
        string ContainerNo { get; set; }
        int ReceivingQty { get; set; }
        string Notes { get; set; }
    }
}
