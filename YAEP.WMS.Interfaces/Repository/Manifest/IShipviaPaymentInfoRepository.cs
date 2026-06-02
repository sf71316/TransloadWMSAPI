using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IShipviaPaymentInfoRepository
    {
        IActionResult<IEnumerable<IShipviaPaymentInfoModel>> GetList(object condition);
    }
}
