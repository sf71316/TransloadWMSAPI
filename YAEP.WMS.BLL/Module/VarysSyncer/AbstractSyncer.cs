using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.LittleBird.WMS;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractSyncer<T> where T : class, new()
    {
        public string Agent { get; set; } = "admin";
        public abstract IActionResult<bool> Sync(IEnumerable<T> data, string replicateionKey);
    }
}
