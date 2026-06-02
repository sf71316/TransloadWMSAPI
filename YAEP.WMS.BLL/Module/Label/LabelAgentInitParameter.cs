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
    internal class LabelAgentInitParameter
    {
        public PackageCacheManager PackageCacheManager { get; set; }
        public ProductCacheManager ProductCacheManager { get; set; }
        public ILabelManager LabelManager { get; set; }
        public IPackageUomManager PackageUomManager { get; set; }
        //public IItemManager ItemManager { get; set; }
    }
}
