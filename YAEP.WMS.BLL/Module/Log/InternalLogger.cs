using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InternalLogger : ILogInfiltrator
    {
        static Lazy<ILogInfiltrator> _logger;
        static NLog.Logger _Locallogger;
        static InternalLogger()
        {
            _logger = new Lazy<ILogInfiltrator>(() => new InternalLogger());
            _Locallogger = LogManager.GetCurrentClassLogger();
        }
        public static ILogInfiltrator GetLogger()
        {
            return _logger.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="owner"></param>
        /// <param name="level">debug,error,warn,fatal,info</param>
        /// <param name="belongToType"></param>
        /// <param name="belongToUID"></param>
        /// <param name="belongToRemark"></param>
        /// <param name="application"></param>
        /// <param name="subApplication"></param>
        /// <param name="exception"></param>
        /// <param name="ip"></param>
        /// <param name="jsonBefore"></param>
        /// <param name="jsonAfter"></param>
        public void Log(string message, string type, string owner, string level,
        int belongToType, string belongToUID = "", string belongToRemark = "", string application = "", string subApplication = "",
        Exception exception = null, string ip = "", string jsonBefore = "", string jsonAfter = "")
        {
            if (string.IsNullOrEmpty(application))
                application = WMSAPIParameters.APPLICATION_NAME;
            WriteLocalLog(message, type, owner, level, belongToType, belongToUID,
            belongToRemark, application, subApplication, exception, ip, jsonBefore, jsonAfter);

        }
        private void WriteLocalLog(string message, string type, string owner, string level, int belongToType, string belongToUID,
            string belongToRemark, string application, string subApplication, Exception exception, string ip, string jsonBefore,
            string jsonAfter, bool realselogger = false)
        {
            var logEvenInfo = new LogEventInfo()
            {
                Level = NLogLevelParse(type),
                Message = message,
                Exception = exception,
            };
            logEvenInfo.Properties["CreatedOn"] = DateTime.UtcNow;
            logEvenInfo.Properties["Content"] = message;
            if (exception != null)
            {
                logEvenInfo.Properties["ExceptionMessage"] = exception.Message;
                logEvenInfo.Properties["ExceptionStackTrace"] = exception.StackTrace;
            }
            logEvenInfo.Properties["Level"] = level.ToUpper();
            logEvenInfo.Properties["MachineName"] = "";
            logEvenInfo.Properties["Source"] = "";
            logEvenInfo.Properties["Type"] = type;
            logEvenInfo.Properties["Logger"] = owner;
            logEvenInfo.Properties["IP"] = ip;
            logEvenInfo.Properties["Application"] = application;
            logEvenInfo.Properties["SubApplication"] = subApplication;
            logEvenInfo.Properties["Alias"] = "";
            logEvenInfo.Properties["ThreadId"] = Thread.CurrentThread.ManagedThreadId;
            logEvenInfo.Properties["BelongToType"] = belongToType;
            logEvenInfo.Properties["BelongToUID"] = belongToUID;
            logEvenInfo.Properties["BelongToRemark"] = belongToRemark;
            logEvenInfo.Properties["ObjectBefore"] = jsonBefore;
            logEvenInfo.Properties["ObjectAfter"] = jsonAfter;
            if (level.Equals(Logger.DEBUG, StringComparison.OrdinalIgnoreCase))
                _Locallogger.Debug(logEvenInfo);
            else if (level.Equals(Logger.FATAL, StringComparison.OrdinalIgnoreCase))
                _Locallogger.Fatal(logEvenInfo);
            else if (level.Equals(Logger.ERROR, StringComparison.OrdinalIgnoreCase))
                _Locallogger.Error(logEvenInfo);
            else if (level.Equals(Logger.INFO, StringComparison.OrdinalIgnoreCase))
                _Locallogger.Info(logEvenInfo);
            else if (level.Equals(Logger.WARN, StringComparison.OrdinalIgnoreCase))
                _Locallogger.Warn(logEvenInfo);
            else if (level.Equals(Logger.TRACE, StringComparison.OrdinalIgnoreCase))
                _Locallogger.Trace(logEvenInfo);
            if (realselogger)
                LogManager.Shutdown();
        }
        private LogLevel NLogLevelParse(string level)
        {
            LogLevel _level = LogLevel.Trace;
            if (level.Equals(Logger.DEBUG, StringComparison.OrdinalIgnoreCase))
                _level = LogLevel.Debug;
            if (level.Equals(Logger.FATAL, StringComparison.OrdinalIgnoreCase))
                _level = LogLevel.Fatal;
            if (level.Equals(Logger.ERROR, StringComparison.OrdinalIgnoreCase))
                _level = LogLevel.Error;
            if (level.Equals(Logger.INFO, StringComparison.OrdinalIgnoreCase))
                _level = LogLevel.Info;
            if (level.Equals(Logger.WARN, StringComparison.OrdinalIgnoreCase))
                _level = LogLevel.Warn;

            return _level;
        }
        public const string DEBUG = "DEBUG";
        public const string ERROR = "ERROR";
        public const string WARN = "WARN";
        public const string FATAL = "FATAL";
        public const string INFO = "INFO";

        public string InfoString => INFO;

        public string ErrorString => ERROR;

        public string WarnString => WARN;

        public string FatalString => FATAL;

        public string DebugString => DEBUG;
    }
}
