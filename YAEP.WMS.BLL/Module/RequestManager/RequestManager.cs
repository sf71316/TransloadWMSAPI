using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL
{
    public class RequestManager
    {
        public static string CACHE_NAME = "RequestManager";
        private ConcurrentDictionary<string, ConcurrentBag<RequestidentifierBase>> _currentReqeustCollection;
        public RequestManager()
        {
            this._currentReqeustCollection = new ConcurrentDictionary<string, ConcurrentBag<RequestidentifierBase>>();
        }
        public ConcurrentDictionary<string, ConcurrentBag<RequestidentifierBase>> CurrentRequests
        {
            get
            {
                return this._currentReqeustCollection;

            }

        }
        public string GetObjectKey(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        public bool IsRequestProcessing(string ActionKey, string requestkey)
        {
            if (this._currentReqeustCollection.ContainsKey(ActionKey))
            {
                var request = this._currentReqeustCollection.FirstOrDefault(p => p.Key == ActionKey);
                return request.Value.Any(p => p.Key == requestkey);
            }
            return false;
        }
        public bool IsRequestProcessing(string ActionKey, Func<ConcurrentBag<RequestidentifierBase>, bool> compareFunc)
        {
            if (this._currentReqeustCollection.ContainsKey(ActionKey))
            {
                var request = this._currentReqeustCollection.FirstOrDefault(p => p.Key == ActionKey);
                return compareFunc(request.Value);
            }
            return false;
        }
        public void AddRequest(string ActionKey, string requestkey)
        {

            if (this._currentReqeustCollection.ContainsKey(ActionKey))
            {
                var request = this._currentReqeustCollection.FirstOrDefault(p => p.Key == ActionKey);
                request.Value.Add(new RequestidentifierBase
                {
                    Key = requestkey
                });
            }
            else
            {
                var requestidentifiers = new ConcurrentBag<RequestidentifierBase>();
                requestidentifiers.Add(new RequestidentifierBase
                {
                    Key = requestkey
                });
                this._currentReqeustCollection.TryAdd(ActionKey, requestidentifiers);
            }
        }
        public void AddRequest(string ActionKey, RequestidentifierBase requestobject)
        {

            if (this._currentReqeustCollection.ContainsKey(ActionKey))
            {
                var request = this._currentReqeustCollection.FirstOrDefault(p => p.Key == ActionKey);
                request.Value.Add(requestobject);
            }
            else
            {
                var requestidentifiers = new ConcurrentBag<RequestidentifierBase>();
                requestidentifiers.Add(requestobject);
                this._currentReqeustCollection.TryAdd(ActionKey, requestidentifiers);
            }
        }
        public void RemoveRequest(string ActionKey, string requestkey)
        {

            if (this._currentReqeustCollection.ContainsKey(ActionKey))
            {
                var request = this._currentReqeustCollection.FirstOrDefault(p => p.Key == ActionKey);
                var obj = request.Value.FirstOrDefault(x => x.Key == requestkey);
                request.Value.TryTake(out obj);

            }

        }
        public void RemoveRequest(string ActionKey, Action<ConcurrentBag<RequestidentifierBase>> removeAction)
        {

            if (this._currentReqeustCollection.ContainsKey(ActionKey))
            {
                var request = this._currentReqeustCollection.FirstOrDefault(p => p.Key == ActionKey);
                removeAction(request.Value);
            }

        }
        public dynamic GetCurrentRequest()
        {
            return this.CurrentRequests;
        }
    }
}
