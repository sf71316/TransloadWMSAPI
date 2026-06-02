using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;
using System.Diagnostics;

namespace YAEP.WMS.Interfaces
{
    public interface ITracingAgent
    {
        void Init(IAppConfigure appConfigure, ILogInfiltrator locallogInfiltrator, ILogInfiltrator logInfiltrator, IAuthenticationProvider authenticationProvider);
        void BeginTracing(string ExternalTraceKey = "", object startObject = null);
        void Trace(string message, object beforeObject = null, object afterObject = null, [CallerMemberName] string callerName = "", bool useCallername = false);
        void Debug(string message);
        Activity StartActivity(string activityname);
        void EndTracing(object endObject = null);
        PayloadTransactionLogTypes GetTransactionLogType();
        ITransactionInfo TransactionInfo { get; set; }
        IAuthenticationProvider AuthenticationProvider { get; }

    }
}
