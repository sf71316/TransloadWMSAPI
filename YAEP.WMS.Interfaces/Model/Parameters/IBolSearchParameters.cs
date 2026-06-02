using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IBolSearchParameters
    {
        Guid? ManifestUID { get; set; }
        IEnumerable<string> RefNo { get; set; }
    }
}
