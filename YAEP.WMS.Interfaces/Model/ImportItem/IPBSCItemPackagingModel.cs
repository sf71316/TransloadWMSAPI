using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IPBSCItemPackagingModel
    {
        IPBSCItemModel Item { get; set; }
        IList<IPBSCVirtualItem> MultiBoxItem { get; set; }
        IList<IPBSCPackagingModel> Packages { get; set; }
    }
}
