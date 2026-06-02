using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.NotificationSender.Client
{
    public interface IAPIResult<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        bool IsComplete { get; set; }
        /// <summary>
        /// Error Code
        /// </summary>
        int Code { get; set; }
        /// <summary>
        /// 訊息
        /// </summary>
        string Message { get; set; }
        /// <summary>
        /// 回應時間
        /// </summary>
        DateTime ResponseTime { get; set; }
        /// <summary>
        /// 資料
        /// </summary>
        [JsonProperty("Data")]
        T Data { get; set; }
    }
}
