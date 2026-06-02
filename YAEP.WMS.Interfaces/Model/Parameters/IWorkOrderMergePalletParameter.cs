using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderMergePalletParameter
    {
        IEnumerable<Guid> Mergefrom { get; set; }
        Guid Mergeto { get; set; }
    }
}
