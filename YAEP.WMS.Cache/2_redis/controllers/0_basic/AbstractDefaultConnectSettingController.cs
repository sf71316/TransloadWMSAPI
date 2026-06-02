using System;
using System.Configuration;
using YAEP.Data.NoSql.Redis.Interfaces;
using YAEP.Data.NoSql.Redis.ServiceStack;

namespace YAEP.WMS.Cache.Redis.Controllers
{
    public abstract class AbstractDefaultConnectSettingController<T> : AbstractController<T> where T : class, new()
    {
        public AbstractDefaultConnectSettingController(Func<T, object> getKeyFunc, bool isAutoGenerateIndex = false)
                    : this(CreateConnectSettngs(), getKeyFunc, isAutoGenerateIndex)
        {

        }
        public AbstractDefaultConnectSettingController(IRedisServerSettings redisServerSettings, Func<T, object> getKeyFunc, bool isAutoGenerateIndex = false)
                    : base(new ServiceStackRedisAgent<T>(redisServerSettings), getKeyFunc, isAutoGenerateIndex)
        {

        }

        private static IRedisServerSettings CreateConnectSettngs()
        {
            string host = ConfigurationManager.AppSettings[$"{CONFIG_KEY_PREFIX}.Host"];
            string portText = ConfigurationManager.AppSettings[$"{CONFIG_KEY_PREFIX}.Port"];
            string pwd = ConfigurationManager.AppSettings[$"{CONFIG_KEY_PREFIX}.Password"];

            int port = 6379;
            int.TryParse(portText, out port);

            return new RedisServerSettings()
            {
                Host = host,
                Port = port,
                Password = pwd
            };
        }

        private const string CONFIG_KEY_PREFIX = "YAEP.WMS.Cache.Redis";

        private class RedisServerSettings : IRedisServerSettings
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }
        }
    }
}
