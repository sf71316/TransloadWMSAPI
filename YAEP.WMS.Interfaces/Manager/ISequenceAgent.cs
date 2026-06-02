using System;
using System.Collections.Generic;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ISequenceAgent
    {
        Guid GetManifestRootUID();
        Guid GetWorkOrderPodRootUID();
        Guid GetWorkOrderPayloadRootUID();

        string GetManinfestSequence(Guid BelongtoUID, ManifestType manifestType);
        string GetMainfestItemListSequence(Guid BelongtoUID);
        string GetBOLSeqence(Guid BelongtoUID);
        Queue<string> GetBOLSeqence(Guid BelongtoUID, int BatchCount);
        Queue<string> GetMainfestItemListSequence(Guid BelongtoUID, int BatchCount);

        string GetVesselManifestSequence(Guid BelongtoUID);
        Queue<string> GetVesselManifestSequence(Guid BelongtoUID, int BatchCount);
        string GetVesselSeqence(Guid BelongtoUID);
        Queue<string> GetVesselSeqence(Guid BelongtoUID, int BatchCount);

        #region Work order
        string GetWorkOrderSeqence(Guid BelongtoUID);
        string GetWorkOrderPodSeqence(Guid BelongtoUID);
        string GetWorkOrderPayloadSeqence(Guid BelongtoUID);
        Queue<string> GetWorkOrderPodSeqence(Guid BelongtoUID, int BatchCount);
        Queue<string> GetWorkOrderPayloadSeqence(Guid BelongtoUID, int BatchCount);

        string GetWorkOrderSeqenceByTimeSerial(ManifestType manifestType);
        string GetWorkOrderPodSeqenceByTimeSerial(ManifestType manifestType);
        string GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType manifestType);
        Queue<string> GetWorkOrderPodSeqenceByTimeSerial(ManifestType manifestType, int BatchCount);
        Queue<string> GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType manifestType, int BatchCount);
        #endregion
        #region Payload
        string GetPayloadSeqence(Guid BelongtoUID);
        string GetPodSeqence(Guid BelongtoUID);
        string GetPayloadSeqenceByTimeSerial(PayloadType paylaodType);
        string GetPodSeqenceByTimeSerial(PayloadType paylaodType);
        #endregion

        string GetAreaSeqence(Guid BelongtoUID);
        string GetSlotSeqence(Guid BelongtoUID);
        string GetBinlSeqence(Guid BelongtoUID);

        string GetBulkPickSeqence();
        string GetBulkPickInfoSeqence(Guid belongToUID);

        #region Ticket
        string GetTicketInfoSeqence(Guid BelongtoUID);
        string GetTicketSeqence(Guid BelongtoUID, TicketType serviceItem);
        Queue<string> GetTicketInfoSeqence(Guid BelongtoUID, int BatchCount);
        Queue<string> GetTicketSeqence(Guid BelongtoUID, TicketType serviceItem, int BatchCount);

        string GetTicketInfoSeqenceByTimeSerial(TicketType ticketType);
        string GetTicketSeqenceByTimeSerial(TicketType ticketType);
        Queue<string> GetTicketInfoSeqenceByTimeSerial(TicketType ticketType, int BatchCount);
        Queue<string> GetTicketSeqenceByTimeSerial(TicketType ticketType, int BatchCount);
        #endregion


        int GetSeqenceIndex(Guid BelongtoUID, string BelongtoTag);
    }
}