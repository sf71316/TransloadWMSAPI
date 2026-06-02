using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  YAEP.WMS.NotificationSender.Client
{
    public class ResponseExceptionArg : EventArgs
    {
        public ExceptionType Type { get; set; }
        public IRestResponse Response { get; set; }
        public string Command { get; set; }
    }
}
