using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IPayloadTransactionLogRepository
    {
        IActionResult<IEnumerable<IPayloadTransactionLogViewModel>> GetTranascationList(IPayloadTransactionLogParameters Parameters);

        IActionResult<bool> AddLog(IPayloadTransactionLogModel Model);
         IActionResult<bool> BatchAddLog(IEnumerable<IPayloadTransactionLogModel> Models);
    }
}
