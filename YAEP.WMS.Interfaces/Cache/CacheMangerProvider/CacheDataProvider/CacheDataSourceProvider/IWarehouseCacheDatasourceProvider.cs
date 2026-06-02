using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces.Cache
{
    public interface IWarehouseCacheDatasourceProvider
    {
        #region Clear Cache Method
        IActionResult<IEnumerable<ISlotViewModel>> ClearSlotList();
        #endregion
        #region Update Cache Method
        IActionResult<IEnumerable<ISlotViewModel>> UpdateSlotList();
        #endregion

        #region Read Cache Method
        IActionResult<IEnumerable<ISlotViewModel>> GetSlotList();
        #endregion
    }
}
