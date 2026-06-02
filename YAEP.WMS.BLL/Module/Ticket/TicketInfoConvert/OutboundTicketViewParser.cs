using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundTicketViewParser : InboundTicketViewParser
    {
        public OutboundTicketViewParser(ILabelRepository labelRepository,
            ProductCacheManager itemManager, PackageCacheManager packageManager, IPackageUomManager uomManager, IWarehouseAgent warehouseAgent)
            : base(labelRepository, itemManager, packageManager, uomManager, warehouseAgent)
        {
        }
    }
}
