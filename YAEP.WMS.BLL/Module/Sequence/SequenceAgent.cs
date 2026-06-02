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
    public class SequenceAgent : ISequenceAgent
    {
        int _Sequence_Length = 7;
        string _Link = "-";
        string _Year;
        public SequenceAgent(ISequenceRepository sequenceRepository)
        {
            this.Repository = sequenceRepository;
            _Year = DateTime.Now.ToString("yy");
        }
        public string GetManinfestSequence(Guid BelongtoUID, ManifestType manifestType)
        {
            var Prefix = $"{SequenceTag.MANIFEST_TAG}{(int)manifestType}{_Year}";
            return this.GetSeqence(_Year, SequenceBelongTag.MANIFEST_TAG, Prefix, fillzerolength: _Sequence_Length);
        }
        public string GetMainfestItemListSequence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.MANIFEST_ITEM_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.MANIFEST_ITEM_TAG, Prefix);
        }
        public Queue<string> GetMainfestItemListSequence(Guid BelongtoUID, int BatchCount)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.MANIFEST_ITEM_TAG;
            return this.BatchGetSeqence(_Year, SequenceBelongTag.MANIFEST_ITEM_TAG, BatchCount, Prefix);
        }
        public string GetBOLSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.BOL_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.BOL_TAG, Prefix);
        }
        public Queue<string> GetBOLSeqence(Guid BelongtoUID, int BatchCount)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.BOL_TAG;
            return this.BatchGetSeqence(_Year, SequenceBelongTag.BOL_TAG, BatchCount, Prefix);
        }
        public string GetVesselSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.VESSEL_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.VESSEL_TAG, Prefix);
        }
        public Queue<string> GetVesselSeqence(Guid BelongtoUID, int BatchCount)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.VESSEL_TAG;
            return this.BatchGetSeqence(_Year, SequenceBelongTag.VESSEL_TAG, BatchCount, Prefix);
        }
        public string GetVesselManifestSequence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.VESSEL_MANIFEST_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.VESSEL_MANIFEST_TAG, Prefix);
        }
        public Queue<string> GetVesselManifestSequence(Guid BelongtoUID, int BatchCount)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.VESSEL_MANIFEST_TAG;
            return this.BatchGetSeqence(_Year, SequenceBelongTag.VESSEL_MANIFEST_TAG, BatchCount, Prefix);
        }
        #region Work order 
        public string GetWorkOrderSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.WORKORDER_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.WORKORDER_TAG, Prefix);
        }
        public string GetWorkOrderPodSeqence(Guid BelongtoUID)
        {
            BelongtoUID = GetWorkOrderPodRootUID();
            var Prefix = SequenceTag.WORKORDER_POD_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.WORKORDER_POD_TAG, Prefix);
        }
        public Queue<string> GetWorkOrderPodSeqence(Guid BelongtoUID, int BatchCount)
        {
            BelongtoUID = GetWorkOrderPodRootUID();
            var Prefix = SequenceTag.WORKORDER_POD_TAG;
            return this.BatchGetSeqence(_Year, SequenceBelongTag.WORKORDER_POD_TAG, BatchCount, Prefix);
        }
        public string GetWorkOrderPayloadSeqence(Guid BelongtoUID)
        {
            BelongtoUID = GetWorkOrderPayloadRootUID();
            var Prefix = SequenceTag.WORKORDER_PAYLOAD_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.WORKORDER_PAYLOAD_TAG, Prefix);
        }
        public Queue<string> GetWorkOrderPayloadSeqence(Guid BelongtoUID, int BatchCount)
        {
            BelongtoUID = GetWorkOrderPayloadRootUID();
            var Prefix = SequenceTag.WORKORDER_PAYLOAD_TAG;
            return this.BatchGetSeqence(_Year, SequenceBelongTag.WORKORDER_PAYLOAD_TAG, BatchCount, Prefix);
        }
        public string GetWorkOrderSeqenceByTimeSerial(ManifestType manifestType)
        {
            _Link = "";
            var Prefix = SequenceTag.WORKORDER_TAG;
            return this.GetSeqenceByTimeSerial($"{Prefix}{(int)manifestType}");
        }

        public string GetWorkOrderPodSeqenceByTimeSerial(ManifestType manifestType)
        {
            _Link = "";
            var Prefix = SequenceTag.WORKORDER_POD_TAG;
            return this.GetSeqenceByTimeSerial($"{Prefix}{(int)manifestType}");
        }

        public string GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType manifestType)
        {
            _Link = "";
            var Prefix = SequenceTag.WORKORDER_PAYLOAD_TAG;
            return this.GetSeqenceByTimeSerial($"{Prefix}{(int)manifestType}");
        }

        public Queue<string> GetWorkOrderPodSeqenceByTimeSerial(ManifestType manifestType, int BatchCount)
        {
            _Link = "";
            var Prefix = SequenceTag.WORKORDER_POD_TAG;
            return this.GetBatchSeqenceByTimeSerial(BatchCount, $"{Prefix}{(int)manifestType}");
        }

        public Queue<string> GetWorkOrderPayloadSeqenceByTimeSerial(ManifestType manifestType, int BatchCount)
        {
            _Link = "";
            var Prefix = SequenceTag.WORKORDER_PAYLOAD_TAG;
            return this.GetBatchSeqenceByTimeSerial(BatchCount, $"{Prefix}{(int)manifestType}");
        }
        #endregion

        #region Ticket
        public string GetTicketSeqence(Guid BelongtoUID, TicketType serviceItem)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = $"{SequenceTag.TICKET_TAG}{(int)serviceItem}{_Year}";
            return this.GetSeqence(_Year, SequenceBelongTag.TICKET_TAG, Prefix, fillzerolength: _Sequence_Length);
        }
        public Queue<string> GetTicketSeqence(Guid BelongtoUID, TicketType serviceItem, int BatchCount)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = $"{SequenceTag.TICKET_TAG}{(int)serviceItem}{_Year}";
            return this.BatchGetSeqence(_Year, SequenceBelongTag.TICKET_TAG, BatchCount, Prefix, fillzerolength: _Sequence_Length);
        }
        public string GetTicketInfoSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.TICKET_INFO_TAG;
            return this.GetSeqence(_Year, SequenceBelongTag.TICKET_INFO_TAG, Prefix, fillzerolength: _Sequence_Length);
        }
        public Queue<string> GetTicketInfoSeqence(Guid BelongtoUID, int BatchCount)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.TICKET_INFO_TAG;
            return this.BatchGetSeqence(_Year, SequenceBelongTag.TICKET_INFO_TAG,
                                            BatchCount, Prefix, fillzerolength: _Sequence_Length);
        }
        public string GetTicketInfoSeqenceByTimeSerial(TicketType ticketType)
        {
            var Prefix = $"{SequenceTag.TICKET_INFO_TAG}{(int)ticketType}";
            _Link = "";
            return this.GetSeqenceByTimeSerial(Prefix);
        }

        public string GetTicketSeqenceByTimeSerial(TicketType ticketType)
        {
            var Prefix = $"{SequenceTag.TICKET_TAG}{(int)ticketType}";
            _Link = "";
            return this.GetSeqenceByTimeSerial(Prefix);
        }

        public Queue<string> GetTicketInfoSeqenceByTimeSerial(TicketType ticketType, int BatchCount)
        {
            var Prefix = $"{SequenceTag.TICKET_INFO_TAG}{(int)ticketType}";
            _Link = "";
            return this.GetBatchSeqenceByTimeSerial(BatchCount, Prefix);
        }

        public Queue<string> GetTicketSeqenceByTimeSerial(TicketType ticketType, int BatchCount)
        {
            var Prefix = $"{SequenceTag.TICKET_TAG}{(int)ticketType}";
            _Link = "";
            return this.GetBatchSeqenceByTimeSerial(BatchCount, Prefix);
        }
        #endregion

        #region Payload/Pod
        public string GetPayloadSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.PAYLOAD_TAG;
            return this.GetSeqence(BelongtoUID.ToString(), SequenceBelongTag.PAYLOAD_TAG, Prefix);
        }
        public string GetPodSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.POD_TAG;
            return this.GetSeqence(BelongtoUID.ToString(), SequenceBelongTag.POD_TAG, Prefix);
        }
        public string GetPayloadSeqenceByTimeSerial(PayloadType paylaodType)
        {
            var Prefix = SequenceTag.PAYLOAD_TAG;
            _Link = "-";
            return this.GetSeqenceByTimeSerial(Prefix + _Link);
        }

        public string GetPodSeqenceByTimeSerial(PayloadType paylaodType)
        {
            var Prefix = SequenceTag.POD_TAG;
            _Link = "-";
            return this.GetSeqenceByTimeSerial(Prefix + _Link);
        }
        #endregion

        public string GetSlotSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.SLOT_TAG;
            return this.GetSeqence(BelongtoUID.ToString(), SequenceBelongTag.SLOT_TAG, Prefix);
        }
        public string GetBinlSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.BIN_TAG;
            return this.GetSeqence(BelongtoUID.ToString(), SequenceBelongTag.BIN_TAG, Prefix);
        }
        public string GetAreaSeqence(Guid BelongtoUID)
        {
            BelongtoUID = Guid.Empty;
            var Prefix = SequenceTag.AREA_TAG;
            return this.GetSeqence(BelongtoUID.ToString(), SequenceBelongTag.AREA_TAG, Prefix);
        }
        public string GetBulkPickSeqence()
        {
            var belongToUID = GetBulkPickRootUID();
            var belongToTag = SequenceBelongTag.BULKPICK_TAG;
            var prefix = SequenceTag.BULKPICK_TAG;
            return this.GetSeqence(belongToUID.ToString(), belongToTag, Prefix: prefix, fillzerolength: _Sequence_Length);
        }
        public string GetBulkPickInfoSeqence(Guid belongToUID)
        {
            var belongToTag = SequenceBelongTag.BULKPICK_INFO_TAG;
            var prefix = SequenceTag.BULKPICK_INFO_TAG;
            return this.GetSeqence(belongToUID.ToString(), belongToTag, Prefix: prefix, fillzerolength: 4);
        }
        public string GetSeqence(string BelongtoUID, string BelongtoTag,
            string Prefix = "", string suffix = "", int fillzerolength = 0)
        {
            var _sequence = this.Repository.GetSeqeuce(BelongtoUID, BelongtoTag);
            var _seq = "";
            if (fillzerolength == 0)
            {
                _seq = _sequence.Content.SequenceValue.ToString();
            }
            else
            {
                _seq = _sequence.Content.SequenceValue.ToString().PadLeft(fillzerolength, '0');
                _Link = "";
            }
            return Prefix + _Link + _seq + (suffix != "" ? _Link + suffix : "");
        }
        private string GetSeqenceByTimeSerial(
          string Prefix, string suffix = "", int fillzerolength = 0)
        {
            var _seq = DateTime.Now.ToString("yyMMddHHmmssfff");
            return Prefix +
                   _Link +
                   ((fillzerolength == 0) ? _seq : _seq.PadLeft(fillzerolength, '0')) +
                   (suffix != "" ? _Link + suffix : "");
        }
        private Queue<string> GetBatchSeqenceByTimeSerial(int BatchCount,
           string Prefix, string suffix = "", int fillzerolength = 0)
        {
            Queue<string> queue = new Queue<string>();
            for (int i = 0; i < BatchCount; i++)
            {
                queue.Enqueue(this.GetSeqenceByTimeSerial(Prefix, suffix, fillzerolength));
            }

            return queue;
        }
        private Queue<string> BatchGetSeqence(string BelongtoUID, string BelongtoTag, int BatchCount,
            string Prefix = "", string suffix = "", int fillzerolength = 0)
        {
            Queue<string> queue = new Queue<string>();
            var _sequence = this.Repository.GetSeqeuceByBatch(BelongtoUID, BelongtoTag, BatchCount);
            _sequence.Content.OrderBy(o => o.SequenceValue).ToList().ForEach(
                p =>
                {
                    var _seq = "";
                    if (fillzerolength == 0)
                    {
                        _seq = p.SequenceValue.ToString();
                    }
                    else
                    {
                        _seq = p.SequenceValue.ToString().PadLeft(fillzerolength, '0');
                        _Link = "";
                    }
                    queue.Enqueue(Prefix + _Link + _seq + (suffix != "" ? _Link + suffix : ""));
                }
                );
            return queue;
        }
        public int GetSeqenceIndex(Guid BelongtoUID, string BelongtoTag)
        {
            var _sequence = this.Repository.GetSeqeuce(BelongtoUID.ToString(), BelongtoTag);
            return _sequence.Content.SequenceValue;
        }

        public Guid GetManifestRootUID()
        {
            return new Guid("841bffcc-3c73-43d4-95bc-33299ff63160");
        }
        public Guid GetWorkOrderPodRootUID()
        {
            return new Guid("0a7e03bb-9461-4c01-9e4f-bc24c2e82b9c");
        }
        public Guid GetWorkOrderPayloadRootUID()
        {
            return new Guid("a71155a3-c227-47bc-ba7d-153cb809a611");
        }
        public Guid GetBulkPickRootUID()
        {
            return new Guid("ce437ae5-f560-11e9-979b-00155de60a79");
        }



        private ISequenceRepository Repository { get; set; }
    }
}
