using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface INotificationSenderTaskRepository
    {
        IActionResult<bool> Delete(object condition);
        IActionResult<INotificationSenderTaskModel> GetData(object condition);
        IActionResult<bool> Add(object Model);
        IActionResult<bool> BatchAdd(IEnumerable<dynamic> Models);
        IActionResult<bool> Edit(object Model, object condition);

    }
}
