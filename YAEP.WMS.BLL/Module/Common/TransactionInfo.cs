using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    class TransactionInfo : ITransactionInfo
    {
        public TransactionlogAction Action { get; set; }
        public TransactionlogExternalfunction Externalfunction { get; set; }
        public TransactionlogSubfunction Subfunction { get; set; }
    }
}
