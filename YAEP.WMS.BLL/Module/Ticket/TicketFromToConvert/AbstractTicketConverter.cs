using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractTicketConverter
    {
        public AbstractTicketConverter()
        {

        }
        public static AbstractTicketConverter GetInstance(int manifestType, int selftServiceItem)
        {
            var manifestTypeEnum = (ManifestType)manifestType;
            var selftServiceItemEnum = (TicketType)selftServiceItem;
            if (manifestTypeEnum == ManifestType.InventoryCounting)
                return new InventoryCountingTicketConverter();
            else if (manifestTypeEnum == ManifestType.Move || manifestTypeEnum== ManifestType.BlukPick)
                return new MoveTicketToConverter();
            else if (manifestTypeEnum == ManifestType.Inbound)
            {
                if (selftServiceItemEnum == TicketType.Receiving)
                {
                    return new ReceivingTicketConverter();
                }
                return new ReceivingMoveTicketConverter();
            }
            else if (manifestTypeEnum == ManifestType.Outbound)
            {
                if (selftServiceItemEnum == TicketType.Outbound)
                {
                    return new OutboundTicketConverter();
                }
                return new OutboundMoveTicketConverter();
            }
            return null;
        }
        public abstract void Convert(ITicketInfoCommonViewModel ticketInfo);

    }
}
