using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;
using System.Diagnostics;
using System.Web;
using System.Data.SqlClient;

namespace YAEP.WMS.Tracing.Jaeger
{
    public class JaegerHelper
    {
        public ActivitySource ActivitySource { get; set; }
        public ActivityTraceId TraceId { get; private set; }
        public ActivitySpanId ParentSpanId { get; private set; }
        public ActivityTraceFlags ActivityTraceFlags { get; private set; }

        public JaegerHelper(ActivitySource activitySource)
        {
            ActivitySource = activitySource;
        }
        private static Lazy<TracerProvider> tracerProviderBuilder;

        //private static IAppConfigure _appConfigure;
        //private static ActivitySource activitySource;
        public static TracerProvider CreateInstance(IAppConfigure appConfigure)
        {
            //tracerProviderBuilder = new Lazy<TracerProvider>(() => InitProviderByGlobal(appConfigure));
            //_appConfigure = appConfigure
           // InitDbProviderByGlobal(appConfigure);
            return InitProviderByGlobal(appConfigure);
        }

        private static void InitDbProviderByGlobal(IAppConfigure appConfigure)
        {
            var traceProvider = Sdk.CreateTracerProviderBuilder()
                                   .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appConfigure.JaegerServiceName))
                                  .AddSqlClientInstrumentation(opt => opt.Enrich
                                     = (activity, eventName, rawObject) =>
                                     {
                                         if (eventName.Equals("OnCustom"))
                                         {
                                             if (rawObject is SqlCommand cmd)
                                             {
                                                 activity.SetTag("db.commandTimeout", cmd.CommandTimeout);
                                             }
                                         };
                                     })
                                   .AddSource("*")
                                   .AddJaegerExporter(opts =>
                                   {
                                       opts.AgentHost = appConfigure.JaegerServiceIP;
                                       opts.AgentPort = appConfigure.JaegerServicePort;
                                   })
                                   .Build();

        }

        private static TracerProvider InitProviderByGlobal(IAppConfigure appConfigure)
        {
            var traceProvider = Sdk.CreateTracerProviderBuilder()
                                  .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appConfigure.JaegerServiceName))
                                  //開啟Web api的監控
                                  .AddAspNetInstrumentation((options) => options.Enrich
                                            = (activity, eventName, rawObject) =>
                                            {
                                                if (eventName.Equals("OnStopActivity"))
                                                {
                                                    if (rawObject is HttpResponse httpResponse)
                                                    {
                                                        activity.DisplayName = HttpContext.Current?.Request.Path ?? activity.DisplayName;
                                                    }
                                                }
                                            })
                                  .AddSource("*")
                                  .AddJaegerExporter(opts =>
                                  {
                                      opts.AgentHost = appConfigure.JaegerServiceIP;
                                      opts.AgentPort = appConfigure.JaegerServicePort;
                                  })
                                  .Build();
            return traceProvider;

        }

        public static JaegerHelper InitActivitySource(IAppConfigure appConfigure)
        {

            var helper = new JaegerHelper(new ActivitySource(appConfigure.JaegerServiceName));
            return helper;
        }
        public void RelateActivity(Activity activity)
        {
            if (activity != null)
            {
                TraceId = activity.TraceId;
                ParentSpanId = activity.ParentSpanId;
                ActivityTraceFlags = activity.ActivityTraceFlags;
            }
        }
        public Activity GetActivity(string activityName = "")
        {
            var activity = ActivitySource.StartActivity(activityName);
            activity.SetParentId(TraceId, ParentSpanId, ActivityTraceFlags);
            return activity;
        }
    }
}
