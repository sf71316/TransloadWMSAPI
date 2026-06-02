using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Utilities.Attributes;

namespace YAEP.WMS.Constant.Enums
{
    public enum MobileAttachmentType
    {
        [EnumFieldInfo(Sort = 1)]
        General = 100,

    }
    public enum MobileBelongToType
    {
        [EnumFieldInfo(Sort = 1)]
        Ticket = 100,
        TicketInfo = 200,

    }
}
