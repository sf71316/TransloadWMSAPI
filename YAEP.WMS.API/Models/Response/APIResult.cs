using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.Api.Models
{
    /// <summary>
    /// Web API 回傳結果集
    /// </summary>
    public class APIResult<T> 
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsComplete { get; set; }
        /// <summary>
        /// Error Code
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 回應時間
        /// </summary>
        public DateTime ResponseTime { get; set; }
        /// <summary>
        /// 資料
        /// </summary>
        public T Data { get; set; }
    }
}