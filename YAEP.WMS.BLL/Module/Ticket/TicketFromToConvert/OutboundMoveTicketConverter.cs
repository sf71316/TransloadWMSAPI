using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundMoveTicketConverter : MoveTicketToConverter
    {
        public override void Convert(ITicketInfoCommonViewModel ticketInfo)
        {
            ticketInfo.OriginalSlotUID = ticketInfo.SourceSlotUID;
            ticketInfo.TargetSlotUID = ticketInfo.SourceLoadingZoneSlotUID;

            ticketInfo.OriginalPackage = ticketInfo.PayloadPackageUID;
            //ticketInfo.TargetPackage = ticketInfo.SourcePackageUID;
            //20200707 Targetpackage已調整成以payload 上的包裝為主，並非當初Request package
            ticketInfo.TargetPackage = ticketInfo.PayloadPackageUID;
        }
    }
}
