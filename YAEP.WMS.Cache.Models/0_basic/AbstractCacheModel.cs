using System;

namespace YAEP.WMS.Cache.Models
{
    public abstract class AbstractCacheModel
    {
        public DateTime CacheDateTime { get; set; } = DateTime.Now;
    }
}
