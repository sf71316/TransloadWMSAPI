using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetAddItemListparameters
    {
        Guid? manifestuid { get; set; }
        Guid? vesseluid { get; set; }
    }
}
