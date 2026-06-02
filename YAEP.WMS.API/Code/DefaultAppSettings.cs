using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Code
{
    public class DefaultAppSettings : IAppSettings
    {
        public NameValueCollection AppSettings
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings;
            }
        }
    }
}