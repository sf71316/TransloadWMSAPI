using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MergePayloadInventoryParameters : IMergePayloadInventoryParameters
    {
        public IList<IMergeInfo> MergePayloadInfoList { get; set; } = new List<MergeInfo>().ToArray();
    }

    public class MergeInfo : IMergeInfo
    {
        public Guid MainPayload { get; set; }

        /// <summary>
        /// 要拆分的Payload來源
        /// </summary>
        public Guid PayloadUID { get; set; }


    }
}