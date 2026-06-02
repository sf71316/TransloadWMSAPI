using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class CommonResponse : ICommonResponse
    {
        public bool IsComplete { get; set; }
        public string Message { get; set; }
    }
}
