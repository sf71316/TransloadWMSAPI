using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface INotificationReceiverRepository
    {
        IActionResult<bool> Delete(object condition);
        IActionResult<INotificationReceiverModel> GetNotifyConfig(object condition);
        IActionResult<IEnumerable<INotificationReceiverModel>> GetNotifyConfigCollection(object condition);
        IActionResult<bool> Add(object Model);
        IActionResult<bool> Edit(object Model, object condition);
        IActionResult<bool> IsNotify(Guid belongtoUID);
    }
}
