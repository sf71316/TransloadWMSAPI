using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractOutboundHomeAddressBuilder
    {
        protected ITracingAgent _tracingAgent;
        public AbstractOutboundHomeAddressBuilder(OutboundHomeAddressBuilderInitParameters initParameters)
        {
            this._tracingAgent = initParameters.TracingAgent;
        }
        public static AbstractOutboundHomeAddressBuilder GetInstance(OutboundHomeAddressBuilderInitParameters initParameters)
        {
            return new OutboundDefaultHomeAddressBuilder(initParameters);
        }
        protected T retryProcess<T>(Func<IActionResult<T>> p)
        {
            int maxRetry = 3;
            int current = 0;
            while (maxRetry >= current)
            {
                var rs = p.Invoke();
                if (rs.Success)
                {
                    this._tracingAgent.Trace($"Invoke method {p.Method.Name} successfully", rs.Content);
                    return rs.Content;
                }
                else
                {
                    this._tracingAgent.Trace($"Invoke method {p.Method.Name} failure", rs.Message, rs.InnerException);
                    current++;
                }
            }
            return default(T);
        }
        public abstract OutboundHomeAddressMap GetAllocatedHomeAddress(
            int orderType, Dictionary<int, IEnumerable<Guid>> itemUIDs, Guid warehouseUID);

    }
}
