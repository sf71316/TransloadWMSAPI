using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Core.Item.Models;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class BatchSyncProductResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ItemModel Data { get; set; }
    }
}