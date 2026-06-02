using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YAEP.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractInboundImportModule
    {
        protected IIboundInitParameters Parameters { get; set; }
        protected IIboundInitExternalProvider Provider { get; set; }
        public AbstractInboundImportModule(InboundInitImportParameters parameters)
        {
            this.Parameters = parameters;
            this.Provider = parameters;
        }
        public abstract IActionResult<bool> Execute(HttpPostedFile httpPostedFile);
    }
}
