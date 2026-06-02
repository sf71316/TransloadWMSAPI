using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundServiceItemProcessModule : AbstractServiceItemProcessModule
    {
        public override Guid ServiceItemID => new Guid("3fa73155-f46a-4116-9e37-da24028d1f77");
        public OutboundServiceItemProcessModule(ServiceItemProcessModuleParameters parameters) : base(parameters)
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
            var _sequence = "";
            //process Ticket
            //group by workorder pod 
            var groupPod = collection.GroupBy(g => g.WorkOrderPodUID);
            var ticketInfoCount = groupPod.SelectMany(g => g.Select(x => x.WorkOrderPayloadUID)).Count();
            var tckSeq = this.SequenceAgent.GetTicketSeqenceByTimeSerial(
                                                                TicketType.Outbound, groupPod.Count());
            var tck2Seq = this.SequenceAgent.GetTicketInfoSeqenceByTimeSerial(TicketType.Outbound, ticketInfoCount);
            foreach (var pod in groupPod)
            {
                _sequence = tckSeq.Dequeue();
                TicketInnerModel _ticket = new TicketInnerModel();
                _ticket.ManifestType = (int)Type;
                _ticket.ServiceItemUID = this.ServiceItemID;
                _ticket.WorkOrderUID = pod.First().WorkOrderUID;
                _ticket.WarehouseUID = warehouseUID;
                _ticket.Type = (int)TicketType.Outbound;
                _ticket.Status = _ticketStatus;
                _ticket.TicketSequence = DateTime.Now.Ticks;
                _ticket.UID = Guid.NewGuid();
                _ticket.ID = _sequence;
                _ticket.OperationSuggestion = pod.First().OperationSuggestion;
                _ticket.OperationInstruction = this.InstructionBuilder.GetInstruction(collection.First());
                _ticket.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                _ticket.CreatedOn = DateTime.UtcNow;
                //_ticket.ParentTicketUID = parentTicketUID;
                _tickets.Add(_ticket);

                //process Ticket Info
                foreach (var payload in pod)
                {
                    TicketInfoInnerModel _info = new TicketInfoInnerModel();
                    var _itemSequence = tck2Seq.Dequeue();
                    _info.Type = (int)TicketInfoType.Outbound;
                    _info.UID = Guid.NewGuid();
                    _info.Status = _ticketInfoStatus;
                    _info.TicketUID = _ticket.UID;
                    _info.ID = _info.Name = _itemSequence;
                    _info.EstQty = payload.Qty;
                    _info.WorkOrderPayloadUID = payload.WorkOrderPayloadUID;
                    _info.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                    _info.CreatedOn = DateTime.UtcNow;
                    _ticketInfos.Add(_info);
                }
            }
            //process Ticket Relation
            if (parentTicket.Count > 0)
            {
                foreach (var item in parentTicket)
                {
                    var _belongToTickets = _tickets.Where(o =>
                    {
                        var xx = _ticketInfos.Where(p =>
                        {
                            return item.WorkOrderPayloadUID.Exists(x => x == p.WorkOrderPayloadUID) ||
                           item.WorkOrderPodUID.Exists(y => y == p.WorkOrderPodUID);
                        }
                        );
                        return xx.Any(i => i.TicketUID == o.UID);
                    }).ToList();
                    foreach (var item2 in _belongToTickets)
                    {
                        TicketRelationInnerModel rm = new TicketRelationInnerModel();
                        rm.UID = Guid.NewGuid();
                        rm.Status = (int)TicketRelationStatus.Active;
                        rm.ParentUID = item.Ticket.UID;
                        rm.TicketUID = item2.UID;
                        rm.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                        rm.CreatedOn = DateTime.UtcNow;
                        _ticketRelation.Add(rm);
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
