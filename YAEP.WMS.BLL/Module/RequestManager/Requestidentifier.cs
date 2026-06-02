using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL
{
    public class Requestidentifier<T> : RequestidentifierBase where T : class, new()
    {
        public Requestidentifier()
        {
            this.RequestItem = new List<T>();
        }
        public List<T> RequestItem { get; set; }
    }
    public class RequestidentifierBase
    {
        public string Key { get; set; }
    }
}
