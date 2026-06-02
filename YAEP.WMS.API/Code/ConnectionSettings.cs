using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Data.ORM.Interfaces;

namespace YAEP.WMS.Api.Code
{
    public class ConnectionSettings : IConnectionSettings
    {
        public ConnectionSettings()
        {
            var connectionSettings = System.Configuration.ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"];
            this.ProviderName = connectionSettings.ProviderName;
            this.ConnectionString = connectionSettings.ConnectionString;
            //this.CommandTimeout=connectionSettings.
        }
        public string ProviderName { get; }
        public string ConnectionString { get; }
        public int? CommandTimeout { get; } = 60*10;
    }
}