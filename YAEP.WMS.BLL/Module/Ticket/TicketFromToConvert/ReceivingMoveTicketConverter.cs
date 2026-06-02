using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class ReceivingMoveTicketConverter : MoveTicketToConverter
    {
        public ReceivingMoveTicketConverter()
        {

        }
        public override void Convert(ITicketInfoCommonViewModel ticketInfo)
        {
            ticketInfo.OriginalSlotUID = ticketInfo.SourceLoadingZoneSlotUID;
            ticketInfo.TargetSlotUID = ticketInfo.SourceSlotUID;
            if ((TicketInfoMappingType)ticketInfo.MappingType ==
                TicketInfoMappingType.WorkOrderPod)
            {
                //  ticketInfo.OriginalPackageName=
                // ticketInfo.TargetPackageName =
                ticketInfo.TargetUOMName = WMSAPIParameters.PALLET_UOM_KEYNAME;
            }
            ticketInfo.OriginalPackage = ticketInfo.SourcePackageUID;
            ticketInfo.TargetPackage = ticketInfo.SourcePackageUID;
        }
    }
}
