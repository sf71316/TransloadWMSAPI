using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using YAEP.Interfaces;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Tracing.Jaeger;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Runtime.CompilerServices;

namespace YAEP.WMS.BLL.Module
{
    internal class TracingAgent : ITracingAgent
    {
        int TraceIndex = 0;
        Stopwatch sw;
        Guid _TracingUID;
        string _ExternalTraceKey;
        IAppConfigure _AppConfigure;
        ILogInfiltrator _LogInfiltrator;
        ILogInfiltrator _LocallogInfiltrator;
        IAuthenticationProvider _AuthenticationProvider;
        //System.Diagnostics.Activity activity;
        JaegerHelper jaegerHelper;
        public ITransactionInfo TransactionInfo { get; set; }
        public IAuthenticationProvider AuthenticationProvider
        {
            get
            {

                return this._AuthenticationProvider;
            }
        }

        public TracingAgent()
        {
            this._TracingUID = Guid.Empty;
            this.TransactionInfo = new TransactionInfo();
            sw = new Stopwatch();
            sw.Reset();
        }
        public void BeginTracing(string ExternalTraceKey = "", object startObject = null)
        {
            if (this._AppConfigure.EnableTrace)
            {
                var inx = ++TraceIndex;
                _TracingUID = Guid.NewGuid();
                _ExternalTraceKey = ExternalTraceKey;
                var url = System.Web.HttpContext.Current.Request.Url;
                jaegerHelper = JaegerHelper.InitActivitySource(_AppConfigure);
                jaegerHelper.RelateActivity(System.Diagnostics.Activity.Current);
                sw.Start();
                HostingEnvironment.QueueBackgroundWorkItem(ct =>
                {

                    var actionName = url.Segments[url.Segments.Length - 1];
                    string requestData = "";
                    if (startObject != null)
                    {
                        try
                        {
                            requestData = Newtonsoft.Json.JsonConvert.SerializeObject(startObject);
                        }
                        catch
                        {
                            requestData = startObject.ToString();
                        }
                    }
                    var sizeofmessage = System.Text.ASCIIEncoding.Unicode.GetByteCount(requestData);
                    if (sizeofmessage > 1 * 1024 * 1024)
                    {
                        requestData = "content too large";
                    }
                    var message = $"Tracing#{_TracingUID} begin trace";
                    var auth = this._AuthenticationProvider.GetAuthenticationInfo();
                    this._LogInfiltrator.Log(message, actionName, auth.Account, "Info", (int)YAEP.Constants.BelongToTypes.WMSCommon,
                        jsonBefore: requestData, belongToUID: _TracingUID.ToString(),
                        application: WMSAPIParameters.TRACE_LOG_NAME, subApplication: _ExternalTraceKey, belongToRemark: $"{inx}");
                    if (this._AppConfigure.EnableLocalLogging)
                    {
                        using (var activity = jaegerHelper.GetActivity(actionName))
                        {
                            activity.AddTag("data", requestData);
                        }
                        this._LocallogInfiltrator.Log(message, actionName, auth.Account, "trace", (int)YAEP.Constants.BelongToTypes.WMSCommon,
                              jsonBefore: requestData, belongToUID: _TracingUID.ToString(),
                              application: WMSAPIParameters.TRACE_LOG_NAME, subApplication: _ExternalTraceKey, belongToRemark: $"{inx}");
                    }

                });


            }
        }
        public void Trace(string message, object beforeObject = null, object afterObject = null, [CallerMemberName] string callerName = "", bool useCallername = false)
        {
            if (this._AppConfigure.EnableTrace)
            {
                var inx = ++TraceIndex;
                var url = System.Web.HttpContext.Current?.Request?.Url;
                HostingEnvironment.QueueBackgroundWorkItem(ct =>
                {
                    var beforeData = beforeObject?.ToString();// Newtonsoft.Json.JsonConvert.SerializeObject(beforeObject);
                    var afterData = afterObject?.ToString();// Newtonsoft.Json.JsonConvert.SerializeObject(afterObject);
                    try
                    {

                        beforeData = Newtonsoft.Json.JsonConvert.SerializeObject(beforeObject);
                        afterData = Newtonsoft.Json.JsonConvert.SerializeObject(afterObject);
                    }
                    catch { }
                    var actionName = "";
                    if (url != null)
                    {
                        actionName = url.Segments[url.Segments.Length - 1];
                    }
                    var auth = this._AuthenticationProvider.GetAuthenticationInfo();
                    this._LogInfiltrator.Log(message, actionName, auth.Account, "Info", (int)YAEP.Constants.BelongToTypes.WMSCommon,
                        jsonBefore: beforeData, jsonAfter: afterData, belongToUID: _TracingUID.ToString(),
                        application: WMSAPIParameters.TRACE_LOG_NAME, subApplication: _ExternalTraceKey, belongToRemark: $"{inx}");
                    if (this._AppConfigure.EnableLocalLogging)
                    {
                        var activityName = useCallername ? callerName : message;
                        //using (var activity = jaegerHelper?.GetActivity(activityName))
                        //{
                        //    activity?.AddTag("CallerName", callerName);
                        //    activity?.AddTag("message", message);
                        //    activity?.AddTag("jsonBefore", beforeData);
                        //    activity?.AddTag("jsonAfter", afterData);

                        //}
                        this._LocallogInfiltrator.Log(message, actionName, auth.Account, "trace", (int)YAEP.Constants.BelongToTypes.WMSCommon,
                          jsonBefore: beforeData, jsonAfter: afterData, belongToUID: _TracingUID.ToString(),
                          application: WMSAPIParameters.TRACE_LOG_NAME, subApplication: _ExternalTraceKey, belongToRemark: $"{inx}");
                    }
                });
            }
        }
        public void EndTracing(object endObject = null)
        {
            if (this._AppConfigure.EnableTrace)
            {
                var inx = ++TraceIndex;
                if (endObject != null)
                {
                    var url = System.Web.HttpContext.Current.Request.Url;
                    sw.Stop();
                    HostingEnvironment.QueueBackgroundWorkItem(ct =>
                    {

                        var requestData = "";
                        if (endObject != null)
                        {
                            try
                            {
                                requestData = Newtonsoft.Json.JsonConvert.SerializeObject(endObject);
                            }
                            catch
                            {
                                requestData = endObject.ToString();
                            }

                        }
                        var message = $"Tracing#{_TracingUID} execute time:{sw.ElapsedMilliseconds} ms end trace";
                        var sizeofmessage = System.Text.ASCIIEncoding.Unicode.GetByteCount(requestData);
                        if (sizeofmessage > 1 * 1024 * 1024)
                        {
                            requestData = "content too large";
                        }
                        var actionName = url.Segments[url.Segments.Length - 1];
                        var auth = this._AuthenticationProvider.GetAuthenticationInfo();
                        this._LogInfiltrator.Log(message, actionName, auth.Account, "Info", (int)YAEP.Constants.BelongToTypes.WMSCommon,
                            jsonBefore: requestData, belongToUID: _TracingUID.ToString(),
                            application: WMSAPIParameters.TRACE_LOG_NAME, subApplication: _ExternalTraceKey, belongToRemark: $"{inx}");
                        if (this._AppConfigure.EnableLocalLogging)
                        {
                            using (var activity = jaegerHelper.GetActivity(actionName))
                            {
                                activity?.AddTag("jsonBefore", requestData);

                            }

                            this._LocallogInfiltrator.Log(message, actionName, auth.Account, "trace", (int)YAEP.Constants.BelongToTypes.WMSCommon,
                               jsonBefore: requestData, belongToUID: _TracingUID.ToString(),
                               application: WMSAPIParameters.TRACE_LOG_NAME, subApplication: _ExternalTraceKey, belongToRemark: $"{inx}");
                        }
                    });
                }

            }
        }
        public PayloadTransactionLogTypes GetTransactionLogType()
        {
            var transactionInfo = this.TransactionInfo;
            var _sub = "";
            var _ext = "";
            var _action = "";
            switch (transactionInfo.Externalfunction)
            {
                case TransactionlogExternalfunction.APP:
                    _ext = "01";
                    break;
                case TransactionlogExternalfunction.Web:
                    _ext = "02";
                    break;
                case TransactionlogExternalfunction.PackingStation:
                    _ext = "03";
                    break;
                case TransactionlogExternalfunction.ExternalService:
                    _ext = "04";
                    break;
                default:
                    break;
            }
            switch (transactionInfo.Subfunction)
            {
                case TransactionlogSubfunction.General:
                    _sub = "11";
                    break;
                case TransactionlogSubfunction.Transfer:
                    _sub = "12";
                    break;
                case TransactionlogSubfunction.Adjust:
                    _sub = "13";
                    break;
                case TransactionlogSubfunction.InventoryCounting:
                    _sub = "14";
                    break;
                default:
                    break;
            }
            switch (transactionInfo.Action)
            {
                case TransactionlogAction.Receiving:
                    _action = "100";
                    break;
                case TransactionlogAction.Move:
                    _action = "200";
                    break;
                case TransactionlogAction.Pack:
                    _action = "300";
                    break;
                case TransactionlogAction.PickAll:
                    _action = "400";
                    break;
                case TransactionlogAction.AddInventory:
                    _action = "500";
                    break;
                case TransactionlogAction.ModifiedInventory:
                    _action = "510";
                    break;
                case TransactionlogAction.ModifiedInventoryMoveTicket:
                    _action = "520";
                    break;
                case TransactionlogAction.ModifiedInventoryChangeSlot:
                    _action = "530";
                    break;
                case TransactionlogAction.ModifiedInventoryChangePackage:
                    _action = "540";
                    break;
                case TransactionlogAction.ChangePackageSlot:
                    _action = "550";
                    break;
                case TransactionlogAction.ModifiedInventoryChangePackageSlot:
                    _action = "560";
                    break;
                case TransactionlogAction.DeleteInventory:
                    _action = "570";
                    break;
                case TransactionlogAction.ChangeSlot:
                    _action = "580";
                    break;
                case TransactionlogAction.ChangePackage:
                    _action = "590";
                    break;

                case TransactionlogAction.ReturnTicketToOpen:
                    _action = "600";
                    break;
                case TransactionlogAction.AddInventorySetType:
                    _action = "610";
                    break;
                case TransactionlogAction.ModifiedInventorySetType:
                    _action = "620";
                    break;
                default:
                    break;
            }
            // TransactionInfo 三段(Subfunction/Externalfunction/Action)未設妥時 concat 會是 ""/非數字,
            // 原本 Convert.ToInt32 會丟 FormatException 並把整筆業務交易 rollback。
            // log 類別算不出來不該拖垮交易 → 解析失敗則退預設(0)。
            int type;
            if (!int.TryParse(string.Concat(_sub, _ext, _action), out type))
            {
                return default(PayloadTransactionLogTypes);
            }
            return (PayloadTransactionLogTypes)type;
        }
        public void Init(IAppConfigure appConfigure, ILogInfiltrator locallogInfiltrator, ILogInfiltrator logInfiltrator, IAuthenticationProvider authenticationProvider)
        {
            this._AppConfigure = appConfigure;
            this._LogInfiltrator = logInfiltrator;
            this._LocallogInfiltrator = locallogInfiltrator;
            this._AuthenticationProvider = authenticationProvider;
        }

        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public System.Diagnostics.Activity StartActivity(string activityname)
        {
            return jaegerHelper.GetActivity(activityname);
        }
    }
}
