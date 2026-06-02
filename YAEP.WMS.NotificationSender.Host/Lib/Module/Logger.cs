using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Constants;
using YAEP.WMS.Constant;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    public class Logger
    {
        static Lazy<Logger> _logger;
        static Logger()
        {
            _logger = new Lazy<Logger>(() => new Logger());
        }
        public static Logger GetLogger()
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

            Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrEmpty(application))
                    application = WMSAPIParameters.APPLICATION_NAME;
                this.innerlog(message, type, owner, level, belongToType, belongToUID,
                    belongToRemark, application, subApplication, exception, ip, jsonBefore, jsonAfter);
            });
        }
        private void innerlog(string message, string type, string owner, string level,
        int belongToType, string belongToUID = "", string belongToRemark = "", string application = "", string subApplication = "",
        Exception exception = null, string ip = "", string jsonBefore = "", string jsonAfter = "")
        {

            string channelByConfig = YAEP.LittleBird.Log.Config.GetChannel();
            string applicationByConfig = YAEP.LittleBird.Log.Config.GetApplication();

            if (String.IsNullOrWhiteSpace(ip))
            {
                try
                {
                    ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                    if (String.IsNullOrWhiteSpace(ip))
                    {
                        ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                }
                catch { }
            }

            var logLevel = YAEP.Log.Constants.LogLevels.Info;
            switch (level.ToLower())
            {
                case "debug":
                    logLevel = YAEP.Log.Constants.LogLevels.Debug;
                    break;
                case "error":
                    logLevel = YAEP.Log.Constants.LogLevels.Error;
                    break;
                case "warn":
                    logLevel = YAEP.Log.Constants.LogLevels.Warn;
                    break;
                case "fatal":
                    logLevel = YAEP.Log.Constants.LogLevels.Fatal;
                    break;
                case "info":
                default:

                    break;
            }

            var littleBird = new YAEP.LittleBird.Log.Client();
            bool success = littleBird.Build()
                             .Channel(!String.IsNullOrWhiteSpace(channelByConfig) ? channelByConfig : "Varys.Log")
                             .Content(message)
                             .Owner(owner)
                             .LogLevel(logLevel)
                             .LogType(type)
                             .IP(ip)
                             .Application(!String.IsNullOrWhiteSpace(application) ? application : applicationByConfig)
                             .SubApplication(subApplication)
                             .BelongToType((BelongToTypes)belongToType)
                             .BelongToRemark(belongToRemark)
                             .BelongToUID(belongToUID)
                             .ObjectBefore(jsonBefore)
                             .ObjectAfter(jsonAfter)
                             .Tweet();


        }
    }
}
