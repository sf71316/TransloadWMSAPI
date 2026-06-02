using System;
using YAEP.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAutoAssignTicketExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        IActionResult<bool> Execute(IAutoAssignProcessArgs args);
    }
}
