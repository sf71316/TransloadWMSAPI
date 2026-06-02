using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITransactionInfo
    {
        TransactionlogAction Action { get; set; }
        TransactionlogExternalfunction Externalfunction { get; set; }
        TransactionlogSubfunction Subfunction { get; set; }
    }
}
