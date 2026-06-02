using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Package.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.BLL.Model
{
    internal class ItemInfo
    {
        public ItemInfo()
        {
            VirtualItems = new List<ProductExtendModel>();
        }
       
        public IItemModel Item { get; set; }
        public IPackageModel Package { get; set; }
        public IEnumerable<ProductExtendModel> VirtualItems { get; set; }
    }
}
