using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Code
{
    public class ConfigHelper
    {
        public static bool IsDebug
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["YAEP.WMS.API.Debug"] == bool.TrueString;
            }

        }
    }
}