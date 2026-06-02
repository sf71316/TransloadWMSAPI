using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IReplicationlogRepository
    {
        IActionResult<bool> Add(IReplicationlogModel model);
        IActionResult<bool> BatchAdd(IEnumerable<IReplicationlogModel> models);
    }
}
