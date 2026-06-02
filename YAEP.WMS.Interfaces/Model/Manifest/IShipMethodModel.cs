using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IShipMethodModel
    {
        Guid UID { get; set; }
        Guid PartyUID { get; set; }
        int Type { get; set; }
        string MethodName { get; set; }
        string MethodValue { get; set; }
        bool IsSignature { get; set; }
        int Status { get; set; }
        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}
