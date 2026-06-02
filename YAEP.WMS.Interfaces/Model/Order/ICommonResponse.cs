using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAEP.WMS.Interfaces
{
    public interface ICommonResponse
    {
        bool IsComplete { get; set; }
        string Message { get; set; }
    }
}
