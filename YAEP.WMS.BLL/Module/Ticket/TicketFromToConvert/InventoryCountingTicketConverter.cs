using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InventoryCountingTicketConverter : AbstractTicketConverter
    {
        public override void Convert(ITicketInfoCommonViewModel ticketInfo)
        {
            ticketInfo.TargetSlotUID = ticketInfo.SourceLoadingZoneSlotUID;
            ticketInfo.TargetPackage = ticketInfo.SourcePackageUID;
        }
    }
}
