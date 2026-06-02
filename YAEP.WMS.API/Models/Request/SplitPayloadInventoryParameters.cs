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
    public class SplitPayloadInventoryParameters : ISplitPayloadInventoryParameters
    {
        public IList<ISplitInfo> SplitPayloadList { get; set; } = new List<SplitInfo>().ToArray();
    }
    public class SplitInfo : ISplitInfo
    {
        public Guid PayloadUID { get; set; }
        public int SplitQuantity { get; set; }
    }
}