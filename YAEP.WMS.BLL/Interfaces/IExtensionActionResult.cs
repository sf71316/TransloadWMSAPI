using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.BLL.Interfaces
{
    internal interface IExtensionActionResult<T> : IActionResult<T>
    {
        void AddReturnValue(string key, object value);
        R GetReturnValue<R>(string key);
        R GetReturnValueByIndex<R>(int index);

        bool TryGetValue<R>(string key, out R value);
    }
}
