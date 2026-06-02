using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities;

namespace YAEP.WMS.UniversalModule
{
    public class ExceptionTraceHandler : IExceptionTraceHandler
    {
        public string CategoryName { get; set; }
        public void OnException(Exception ex, string message = "")
        {

#if DEBUG
            if (!string.IsNullOrEmpty(message))
                System.Diagnostics.Debug.WriteLine(ex.Message, CategoryName);
            System.Diagnostics.Debug.WriteLine(ex.StackTrace, CategoryName);
#endif

        }
    }
}
