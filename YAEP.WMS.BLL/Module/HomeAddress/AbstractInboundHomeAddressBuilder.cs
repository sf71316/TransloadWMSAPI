using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractInboundHomeAddressBuilder
    {
        protected ProductCacheManager ProductCacheManager { get; set; }
        protected ILogInfiltrator LogInfiltrator { get; set; }
        protected IWarehouseManger WarehouseManger { get; set; }
        public AbstractInboundHomeAddressBuilder(InboundHomeAddressBuilderInitParameters initParameters)
        {
            ProductCacheManager = initParameters.ProductCacheManager;
            LogInfiltrator = initParameters.LogInfiltrator;
            WarehouseManger = initParameters.WarehouseManger;
        }
        public abstract InboundHomeAddressMap GetStorageHomeAddress(Guid warehouseUID, IEnumerable<Guid> itemUIDs);

        public static AbstractInboundHomeAddressBuilder GetInstance(InboundHomeAddressBuilderInitParameters initParameters)
        {
            return new InboundDefaultHomeAddressBuilder(initParameters);
        }
    }
}
