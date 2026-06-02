using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IResultModel
    {
        bool Success { get; set; }
        string Message { get; set; }
    }
}
