using System.Collections.Specialized;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Cache
{
    internal class DefaultAppSettings : IAppSettings
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