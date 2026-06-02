using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    /// <summary>
    /// 判斷產品是否有儲存和是否使用過(Inbound/Outbound)
    /// </summary>
    public enum ItemStorageStatus
    {
        Noneonahnd_Unused = 1,
        HadOnahnd_Used=2,
        Noneonahnd_Used = 3,
        HadOnahnd_Unused = 4
    }
}
