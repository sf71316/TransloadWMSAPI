using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundHomeAddressBuilderInitParameters
    {
        public ProductCacheManager ProductCacheManager { get; set; }
        public ILogInfiltrator LogInfiltrator { get; set; }
        public IWarehouseManger WarehouseManger { get; set; }
    }
}
