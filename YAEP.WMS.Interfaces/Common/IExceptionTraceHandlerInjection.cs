using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities;

namespace YAEP.WMS.Interfaces
{
    public interface IExceptionTraceHandlerInjection
    {
        IExceptionTraceHandler Tracehandler { get; set; }
    }
}
