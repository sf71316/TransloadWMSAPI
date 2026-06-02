using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IApiModel
    {
        Guid UID { get; set; }
        Guid WarehouseUID { get; set; }
        string  ApiKey { get; set; }
        bool IsEnable { get; set; }
        bool Https { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedOn { get; set; }
    }
}
