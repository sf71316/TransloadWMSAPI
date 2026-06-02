using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL
{
    internal interface ITransactionAction
    {
        void BeginTranaction(IsolationLevel isolationLevel);
        void DisposeConnectionInstance();
        void ReInitConnectionInstance();
        void CommitTransaction([CallerMemberName] string memberName = "");
        void RollbackTransaction([CallerMemberName] string memberName = "");
    }
}
