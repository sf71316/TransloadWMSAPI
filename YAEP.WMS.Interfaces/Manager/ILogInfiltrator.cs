using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ILogInfiltrator
    {
        string InfoString { get; }
        string ErrorString { get; }
        string WarnString { get; }
        string FatalString { get; }
        string DebugString { get; }
        void Log(string message, string type, string owner, string level,
         int belongToType, string belongToUID = "", string belongToRemark = "", string application = "", string subApplication = "",
         Exception exception = null, string ip = "", string jsonBefore = "", string jsonAfter = "");

       
    }
}
