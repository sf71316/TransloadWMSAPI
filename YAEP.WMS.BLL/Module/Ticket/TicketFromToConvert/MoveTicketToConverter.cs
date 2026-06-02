using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class MoveTicketToConverter : AbstractTicketConverter
    {
        public MoveTicketToConverter()
        {

        }

        public override void Convert(ITicketInfoCommonViewModel ticketInfo)
        {
            ticketInfo.OriginalSlotUID = ticketInfo.SourceSlotUID;
            ticketInfo.TargetSlotUID = ticketInfo.SourceLoadingZoneSlotUID;
            ticketInfo.OriginalPackage = ticketInfo.PayloadPackageUID;
            ticketInfo.TargetPackage = ticketInfo.SourcePackageUID;
        }
    }
}
