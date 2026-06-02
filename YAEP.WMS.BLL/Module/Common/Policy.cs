using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class Policy
    {
        public const int DEFAULT_RETRY_COUNT = 3;
        private int _MaxRetryCount = 2;
        private int _RetryIntervalMillisecond = 0;
        ITracingAgent _TracingAgent;
        public static Policy Create()
        {
            return new Policy();
        }
        public Policy AddTracingAgent(ITracingAgent tracingAgent)
        {
            _TracingAgent = tracingAgent;
            return this;
        }
        public Policy Retry(int retryCount, int retryIntervalmillisecond = 0, EventHandler<ExceptionArgs> eventHandler = null)
        {
            _RetryIntervalMillisecond = retryIntervalmillisecond;
            _MaxRetryCount = retryCount;
            if (eventHandler != null)
                this.OnException += eventHandler;
            return this;
        }
        public IActionResult<T> Execute<T>(Func<IActionResult<T>> p)
        {
            Exception _ex = null;
            int current = 0;
            while (_MaxRetryCount >= current)
            {
                try
                {
                    var rs = p.Invoke();
                    if (rs.Success)
                    {
                        if (this._TracingAgent != null)
                            this._TracingAgent.Trace($"Invoke method {p.Method.Name} successfully", rs.Content);
                        return rs;
                    }
                    else
                    {
                        this.onError(rs.InnerException);
                        if (this._TracingAgent != null)
                            this._TracingAgent.Trace($"Invoke method {p.Method.Name} failure", rs.Message, rs.InnerException);
                        current++;
                    }
                    if (_RetryIntervalMillisecond > 0)
                    {
                        System.Threading.Thread.Sleep(_RetryIntervalMillisecond);
                    }
                }
                catch (Exception ex)
                {
                    this.onError(ex);
                    current++;
                    _ex = ex;
                }
            }
            return default(IActionResult<T>);

        }
        public T Execute<T>(Func<T> p)
        {
            Exception _ex = null;
            int current = 0;
            while (_MaxRetryCount >= current)
            {
                try
                {
                    var rs = p.Invoke();

                    if (this._TracingAgent != null)
                        this._TracingAgent.Trace($"Invoke method {p.Method.Name} successfully");
                    return rs;

                }
                catch (Exception ex)
                {
                    this.onError(ex);
                    current++;
                    _ex = ex;
                }
                if (_RetryIntervalMillisecond > 0)
                {
                    System.Threading.Thread.Sleep(_RetryIntervalMillisecond);
                }
            }
            return default(T);


        }
        public void Execute(Action p)
        {
            Exception _ex = null;
            int current = 0;
            while (_MaxRetryCount >= current)
            {
                try
                {
                    p.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    this.onError(ex);
                    current++;
                    _ex = ex;
                }
            }

        }

        private void onError(Exception ex)
        {
            if (this.OnException != null)
                this.OnException(this, new ExceptionArgs()
                {
                    Exception = ex
                });
        }

        private event EventHandler<ExceptionArgs> OnException;

    }
}
