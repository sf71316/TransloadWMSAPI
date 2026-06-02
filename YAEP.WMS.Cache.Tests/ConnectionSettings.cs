using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Data.ORM.Interfaces;

namespace YAEP.WMS.Cache.Tests
{
    public class ConnectionSettings : IConnectionSettings
    {
        public ConnectionSettings()
        {
            var connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"];
            this.ProviderName = connectionSettings.ProviderName;
            this.ConnectionString = connectionSettings.ConnectionString;
        }
        public string ProviderName { get; }
        public string ConnectionString { get; }
        public int? CommandTimeout { get; } = 9999999;
    }
}