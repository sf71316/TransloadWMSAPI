using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.NotificationSender.Client
{
    public abstract class AbstractSenderAPI : ISenderAPI
    {
        protected const string NOTIFICATION_COMMAND = "api/Message/Notify";
        protected ISenderConfigure _ISenderConfigure;
        protected RestClient _Client;
        public IRestResponse OriginalResposne { get; protected set; }
        public virtual int Timeout { get; set; }
        public virtual int RetryCount { get; set; }
        public virtual int RetryInterval { get; set; }
        public AbstractSenderAPI(ISenderConfigure senderConfigure)
        {
            _ISenderConfigure = senderConfigure;

        }
        public static ISenderAPI GetSenderAPI(ISenderConfigure Configure)
        {
            return new SenderAPI(Configure);
        }
        protected Tuple<T, IRestResponse> InnerActionPostMethod<T>(string cmd, object requestdata
            , Method method, DataFormat dataFormat) where T : class, new()
        {
            IRestResponse apiresponse = null;
            try
            {
                _Client.Timeout = Timeout;
                int _currentRetryCount = 1;
                while (_currentRetryCount <= this.RetryCount)//retry policy
                {
                    if (_currentRetryCount > 1)
                        System.Threading.Thread.Sleep(this.RetryInterval);
                    var request = new RestRequest(cmd, method);
                    request.RequestFormat = dataFormat;
                    request.AddHeader("Content-Type", "application/json");
                    if (dataFormat == DataFormat.Xml)
                        request.AddObject(requestdata);
                    else
                        request.AddJsonBody(requestdata);
                    AddAuthorizeHeader(request, _ISenderConfigure.ReceiverSecret);
                    apiresponse = _Client.Post<T>(request);
                    if ((int)apiresponse.StatusCode > 0)
                    {
                        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(apiresponse.Content);
                        Tuple<T, IRestResponse> result = new Tuple<T, IRestResponse>(obj, apiresponse);
                        this.OriginalResposne = apiresponse;
                        return result;
                    }
                    else
                    {
                        _currentRetryCount++;
                    }
                }
                if (_currentRetryCount >= this.RetryCount)
                {
                    ResponseExceptionArg arg = new ResponseExceptionArg();
                    arg.Command = cmd;
                    arg.Response = apiresponse;
                    arg.Type = ExceptionType.ExecuteExpire;
                    this.OnError(arg);
                }
                return new Tuple<T, IRestResponse>(null, apiresponse);
            }
            catch (Exception ex)
            {
                ResponseExceptionArg arg = new ResponseExceptionArg();
                arg.Command = cmd;
                arg.Response = apiresponse;
                arg.Type = ExceptionType.ResponseException;
                this.OnError(arg);
                return new Tuple<T, IRestResponse>(null, null);
            }
        }
        private void AddAuthorizeHeader(RestRequest request, string secretString)
        {
            request.AddHeader("Authorization", secretString);

        }
        public event EventHandler<ResponseExceptionArg> Error;
        protected void OnError(ResponseExceptionArg arg)
        {
            if (this.Error != null)
            {
                this.Error(this, arg);
            }
        }
        public abstract IAPIResult<bool> SendNotify(INotificationRequest request);
    }
}
