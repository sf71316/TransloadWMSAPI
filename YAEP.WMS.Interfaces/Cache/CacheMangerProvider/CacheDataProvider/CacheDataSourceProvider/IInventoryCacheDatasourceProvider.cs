using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces.Cache
{
    public interface IInventoryCacheDatasourceProvider
    {
        #region Clear Cache Method

        IActionResult<bool> ClearAvailableInventory();
        #endregion
        #region Update Cache Method
        IActionResult<bool> UpdateAvailableInventory();
        #endregion

        #region Read Cache Method
        IActionResult<bool> GetAvailableInventory();
        #endregion
    }
}
