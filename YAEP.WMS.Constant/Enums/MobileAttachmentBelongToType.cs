using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities.Attributes;

namespace YAEP.WMS.Constant.Enums
{
    public enum MobileAttachmentBelongToType
    {
        [EnumFieldInfo(Sort = 1)]
        Ticket =1,
        [EnumFieldInfo(Sort = 2)]
        TicketInfo =2
    }
}
