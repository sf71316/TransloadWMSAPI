using System;

namespace YAEP.WMS.Cache.Models
{
    public class Board
    {
        public string ID { get; set; }
        public bool IsLoaded { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddHours(240);
    }
}
