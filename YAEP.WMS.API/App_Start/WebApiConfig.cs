using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;
using YAEP.WMS.API.Code;

namespace YAEP.WMS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            if (ConfigHelper.IsDebug)
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            // Web API 設定和服務
            //    var constraintResolver = new DefaultInlineConstraintResolver()
            //    {
            //        ConstraintMap =
            //{
            //    ["apiVersion"] =typeof(ApiVersionRouteConstraint)
            //}
            //    };
            //    config.MapHttpAttributeRoutes(constraintResolver);
            //config.AddApiVersioning();
            // Web API 路由
            //config.MapHttpAttributeRoutes();
            config.EnableCors();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //MessageHandler 處理器

            //註冊驗證Model完整性的Attribute
            //config.Filters.Add(new CapWebAPI.Models.ValidateModelAttribute());

            // 預設傳回 JSON 格式.
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            var serializerSettings =
  GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
            var contractResolver =
              (DefaultContractResolver)serializerSettings.ContractResolver;
            contractResolver.IgnoreSerializableAttribute = true;
            //  config.Formatters.Remove(config.Formatters.JsonFormatter);
            //config.Formatters.Add(new JilFormatter());

        }
    }
}
