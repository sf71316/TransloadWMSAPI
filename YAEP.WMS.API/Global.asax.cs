using System;
using System.Globalization;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using YAEP.WMS.API;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Cache;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Cache.Redis.Controllers;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Resources;
using YAEP.WMS.API.Code;
using YAEP.WMS.API.Models;
using YAEP.WMS.Tracing.Jaeger;
using YAEP.WMS.Cache.CacheManager;

namespace YAEP.WMS.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private TracerProvider tracerProvider;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            if (!DrKnowAll.IsInitialized)
            {
                this.loadCache();
            }
            CacheManager.CreateInstance().LoadCache();

            DefaultAppSettings defaultAppSettings = new DefaultAppSettings();
            DefaultAppConfigure Config = new DefaultAppConfigure(defaultAppSettings);
            JaegerHelper.CreateInstance(Config);

        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            SetLanguage();
            //if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            //{
            //    //These headers are handling the "pre-flight" OPTIONS call sent by the browser
            //    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            //    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            //    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Authorization");
            //    HttpContext.Current.Response.AddHeader("Access-Control-Allow‌​-Credentials", "true");
            //    HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "9999999");
            //    HttpContext.Current.Response.End();
            //}

        }
        private void SetLanguage()
        {
            // 強迫.net framework 回傳英文訊息
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentUICulture =
                new System.Globalization.CultureInfo("en-US");
            CultureInfo culture;
            try
            {
                var language = Request.Headers.GetValues("Accept-Language");
                if (language != null && language.Count() > 0)
                {
                    culture = new System.Globalization.CultureInfo(language.First().Split(',')[0]);
                }
                else
                {
                    culture = new System.Globalization.CultureInfo("en-us");
                }

            }
            catch
            {
                culture = new System.Globalization.CultureInfo("en-us");
            }
            Language.Resources.Resource.Culture = culture;
        }

        private void loadCache()
        {
            HostingEnvironment.QueueBackgroundWorkItem(ctor =>
            {
                DrKnowAll.Reload(DrKnowAllKeys.ALL);

                DrKnowAll.IsInitialized = true;
            });
        }

    }
}
