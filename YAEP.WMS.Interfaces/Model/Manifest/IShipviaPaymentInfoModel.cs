using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IShipviaPaymentInfoModel
    {
        Guid UID { get; set; }
        Guid PartyUID { get; set; }
        int Type { get; set; }
        string Account { get; set; }
        string Password { get; set; }
        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}
