using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class ReceivingResult : IReceivingResponse
    {
        public bool IsComplete { get; set; }
        public string Message { get; set; }
    }
}
