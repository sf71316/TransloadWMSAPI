using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces.Model
{
    public interface IProductPackageExtendModel
    {
        Guid UID { get; set; }
        //string ID { get; set; }
        string Name { get; set; }
        string UPC { get; set; }
        //int Status { get; set; }
        //int Type { get; set; }
        String CustomerUID { get; set; }
        Guid GroupUID { get; set; }
        string Description { get; set; }
    }
}
