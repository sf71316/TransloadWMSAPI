using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IShipMethodRepository
    {
        IActionResult<IEnumerable<IShipMethodModel>> GetList(object condition);
    }
}
