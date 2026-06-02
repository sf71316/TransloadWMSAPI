using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    internal class DefaultConnectionSettings : IConnectionSettings
    {
        public DefaultConnectionSettings()
        {
            var connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"];
            this.ProviderName = connectionSettings.ProviderName;
            this.ConnectionString = connectionSettings.ConnectionString;
        }
        public string ProviderName { get; }
        public string ConnectionString { get; }
        public int? CommandTimeout { get; } = 60*10;
    }
}
