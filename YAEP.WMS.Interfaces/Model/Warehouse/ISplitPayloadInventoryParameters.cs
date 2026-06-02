using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ISplitPayloadInventoryParameters
    {
        /// <summary>
        /// 預計分割的數量
        /// </summary>
        IList<ISplitInfo> SplitPayloadList { get; set; } 
    }

    public interface ISplitInfo
    {

        /// <summary>
        /// 要拆分的Payload來源
        /// </summary>
        Guid PayloadUID { get; set; }

        /// <summary>
        /// 要拿的的數量
        /// </summary>
        int SplitQuantity { get; set; }
    }
    
}
