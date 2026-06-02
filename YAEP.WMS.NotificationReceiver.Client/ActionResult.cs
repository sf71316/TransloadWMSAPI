using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.NotificationSender.Client
{
    public class ActionResult<T> : IAPIResult<T>
    {
        public bool IsComplete { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public DateTime ResponseTime { get; set; }
        public T Data { get; set; }
    }
    public class ActionResult : IAPIResult<bool>
    {
        public bool IsComplete { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public DateTime ResponseTime { get; set; }
        public bool Data { get; set; }
    }
}
