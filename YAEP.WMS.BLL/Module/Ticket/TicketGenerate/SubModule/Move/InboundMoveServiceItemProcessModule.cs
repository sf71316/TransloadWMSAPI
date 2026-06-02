using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundMoveServiceItemProcessModule : AbstractMoveServiceItemProcessModule
    {
        public InboundMoveServiceItemProcessModule(ServiceItemProcessModuleParameters parameters) : base(parameters)
        {

        }
        public override Tuple<IEnumerable<ITicketModel>, IEnumerable<ITicketInfoModel>, IEnumerable<ITicketRelationModel>>
            Execute(IEnumerable<ITicketGeneratoreDataModel> collection, List<ParentTicketGenerateItem> parentTicket, ManifestType Type
            , Guid warehouseUID, bool ForceOpen = false)
        {
            var _ticketStatus = (int)TicketStatus.Draft;
            var _ticketInfoStatus = (int)TicketInfoStatus.Open;
            if (ForceOpen)
            {
                _ticketStatus = (int)TicketStatus.Open;
                _ticketInfoStatus = (int)TicketInfoStatus.Open;
            }
            List<ITicketModel> _tickets = new List<ITicketModel>();
            List<ITicketInfoModel> _ticketInfos = new List<ITicketInfoModel>();
            List<ITicketRelationModel> _ticketRelation = new List<ITicketRelationModel>();
            var result = Tuple.Create<IEnumerable<ITicketModel>, IEnumerable<ITicketInfoModel>, IEnumerable<ITicketRelationModel>>(_tickets, _ticketInfos, _ticketRelation);
            var _sequence = this.SequenceAgent.GetTicketSeqenceByTimeSerial(TicketType.Move);
            //process Ticket
            TicketInnerModel _ticket = new TicketInnerModel();
            _ticket.ServiceItemUID = this.ServiceItemID;
            _ticket.WarehouseUID = warehouseUID;
            _ticket.WorkOrderUID = collection.First().WorkOrderUID;
            _ticket.ManifestType = (int)Type;
            _ticket.Type = (int)TicketInfoType.Move;
            _ticket.Status = _ticketStatus;
            _ticket.TicketSequence = DateTime.Now.Ticks;
            _ticket.UID = Guid.NewGuid();
            _ticket.ID = _sequence;
            _ticket.OperationSuggestion = collection.First().OperationSuggestion;
            _ticket.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
            _ticket.CreatedOn = DateTime.UtcNow;
            _ticket.OperationInstruction = this.InstructionBuilder.GetInstruction(collection.First());

            _tickets.Add(_ticket);

            var palletGroup = collection.GroupBy(p => p.WorkOrderPodUID);
            var ticketInfoCount = palletGroup.SelectMany(g => g.Select(x => x.WorkOrderPayloadUID)).Count();
            var tckSeq = this.SequenceAgent.GetTicketInfoSeqenceByTimeSerial(TicketType.Move, ticketInfoCount);
            foreach (var grp in palletGroup)
            {
                if (grp.First().StoreageMethod == (int)StorageMethod.NewPallet)
                {
                    //TODO 修改Ticket 產生方式
                    var itemInfo = grp.First();
                    TicketInfoInnerModel _info = new TicketInfoInnerModel();
                    var _itemSequence = tckSeq.Dequeue();
                    _info.UID = Guid.NewGuid();
                    _info.TicketUID = _ticket.UID;
                    _info.Status = _ticketInfoStatus;
                    _info.Type = (int)TicketInfoType.Move;
                    _info.EstQty = 1;
                    _info.WorkOrderPodUID = itemInfo.WorkOrderPodUID;
                    _info.ID = _info.Name = _itemSequence;
                    _info.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                    _info.CreatedOn = DateTime.UtcNow;
                    _ticketInfos.Add(_info);
                }
                else
                {
                    foreach (var item in grp)
                    {
                        TicketInfoInnerModel _info = new TicketInfoInnerModel();
                        var _itemSequence = tckSeq.Dequeue();
                        _info.UID = Guid.NewGuid();
                        _info.TicketUID = _ticket.UID;
                        _info.Status = _ticketInfoStatus;
                        _info.Type = (int)TicketInfoType.Move;
                        _info.EstQty = item.Qty;
                        _info.WorkOrderPayloadUID = item.WorkOrderPayloadUID;
                        _info.ID = _info.Name = _itemSequence;
                        _info.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                        _info.CreatedOn = DateTime.UtcNow;
                        _ticketInfos.Add(_info);
                    }
                }
            }
            //process ticket relation
            if (parentTicket.Count > 0)
            {
                foreach (var item in parentTicket)
                {
                    var belongTicket = _ticketInfos.Where(p =>
                     (p.WorkOrderPayloadUID.HasValue && item.WorkOrderPayloadUID.Any(x => x == p.WorkOrderPayloadUID)) ||
                     (p.WorkOrderPodUID.HasValue && item.WorkOrderPodUID.Any(x => x == p.WorkOrderPodUID))
                    );
                    foreach (var item2 in belongTicket)
                    {
                        if (!_ticketRelation.Any(p => p.ParentUID == item.Ticket.UID && p.TicketUID == item2.TicketUID))
                        {
                            TicketRelationInnerModel rm = new TicketRelationInnerModel();
                            rm.UID = Guid.NewGuid();
                            rm.Status = (int)TicketRelationStatus.Active;
                            rm.ParentUID = item.Ticket.UID;
                            rm.TicketUID = item2.TicketUID;
                            rm.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                            rm.CreatedOn = DateTime.UtcNow;
                            _ticketRelation.Add(rm);
                        }
                    }

                }
            }
            //assingned parent data
            parentTicket.Clear();
            foreach (var item in _tickets)
            {
                ParentTicketGenerateItem e = new ParentTicketGenerateItem();
                e.Ticket = item;
                var belongticketinfos = _ticketInfos.Where(p => p.TicketUID == item.UID);
                e.WorkOrderPayloadUID.AddRange(belongticketinfos
                    .Where(p => p.WorkOrderPayloadUID.HasValue).Select(p => p.WorkOrderPayloadUID.Value));
                e.WorkOrderPodUID.AddRange(belongticketinfos
                    .Where(p => p.WorkOrderPodUID.HasValue).Select(p => p.WorkOrderPodUID.Value));
                parentTicket.Add(e);
            }
            return result;
        }
    }
}
