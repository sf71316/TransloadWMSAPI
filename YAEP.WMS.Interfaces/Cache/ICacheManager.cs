using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces.Cache
{
    public interface ICacheManager<T> where T : class
    {
        T CacheInStance { get; set; }
        void RefreshAll();
    }
}
