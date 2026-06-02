using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities;
using YAEP.WMS.BLL.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class ExtensionActionResultContainer<T> : ActionResultContainer<T>, IExtensionActionResult<T>
    {
        Dictionary<string, object> _InnerObjPool;
        public ExtensionActionResultContainer()
        {
            this._InnerObjPool = new Dictionary<string, object>();
        }

        public void AddReturnValue(string key, object value)
        {
            _InnerObjPool.Add(key, value);
        }

        public R GetReturnValue<R>(string key)
        {

            if (this._InnerObjPool.ContainsKey(key))
            {
                return (R)this._InnerObjPool[key];
            }
            return default(R);
        }

        public R GetReturnValueByIndex<R>(int index)
        {
            var data = this._InnerObjPool.ElementAt(index);
            return (R)data.Value;
        }

        public bool TryGetValue<R>(string key, out R value)
        {
            object innerValue;
            if (this._InnerObjPool.TryGetValue(key, out innerValue))
            {
                value = (R)innerValue;
                return true;
            }
            else
            {
                value = default(R);
                return false;
            }
        }
    }
}
