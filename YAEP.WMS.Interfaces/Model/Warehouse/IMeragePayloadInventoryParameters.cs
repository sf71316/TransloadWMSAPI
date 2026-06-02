using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IMergePayloadInventoryParameters
    {
        /// <summary>
        /// 預計合並的資訊
        /// </summary>
        IList<IMergeInfo> MergePayloadInfoList { get; set; }
    }

    public interface IMergeInfo
    {
        /// <summary>
        /// 如果PUOM 需要傳入完整要合並的主PAYLOAD UID 所以開LIST
        /// </summary>
        Guid MainPayload { get; set; }

        /// <summary>
        /// 要拆分的Payload來源
        /// </summary>
        Guid PayloadUID { get; set; }

    }
}
