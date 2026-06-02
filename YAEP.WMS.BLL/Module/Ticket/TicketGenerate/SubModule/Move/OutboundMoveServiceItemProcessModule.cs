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
    internal class OutboundMoveServiceItemProcessModule : AbstractMoveServiceItemProcessModule
    {
        public OutboundMoveServiceItemProcessModule(ServiceItemProcessModuleParameters parameters) : base(parameters)
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
            List<ITicketModel> _atickets = new List<ITicketModel>();
            List<OutboundMoveTicketMappingModel> _ticketMapping = new List<OutboundMoveTicketMappingModel>();
            List<ITicketInfoModel> _ticketInfos = new List<ITicketInfoModel>();
            //List<ITicketInfoModel> _mticketInfos = new List<ITicketInfoModel>();
            List<ITicketRelationModel> _ticketRelation = new List<ITicketRelationModel>();
            var result = Tuple.Create<IEnumerable<ITicketModel>, IEnumerable<ITicketInfoModel>, IEnumerable<ITicketRelationModel>>(_tickets, _ticketInfos, _ticketRelation);

            #region 產生原本Move Ticket 資料 
            var itemGroup = collection.GroupBy(g => new { ItemUID = g.ItemUID, PackageUID = g.PackageUID, SlotUID = g.SlotUID });
            var podGroup = collection.GroupBy(g => g.WorkOrderPodUID);
            var ticketInfoCount = podGroup.SelectMany(g => g.Select(x => x.WorkOrderPayloadUID)).Count();
            var tckSeq = this.SequenceAgent.GetTicketSeqenceByTimeSerial(TicketType.Move,
                                                            ticketInfoCount);
            foreach (var pod in podGroup)
            {

                //process Ticket
                var _sequence = tckSeq.Dequeue();
                var _ticket = new TicketInnerModel();
                _ticket.ManifestType = (int)Type;
                _ticket.WarehouseUID = warehouseUID;
                _ticket.ServiceItemUID = this.ServiceItemID;
                _ticket.WorkOrderUID = pod.First().WorkOrderUID;
                _ticket.Type = (int)TicketInfoType.Move;
                _ticket.Status = _ticketStatus;
                _ticket.UID = Guid.NewGuid();
                _ticket.TicketSequence = DateTime.Now.Ticks;
                _ticket.ID = _sequence;
                _ticket.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                _ticket.CreatedOn = DateTime.UtcNow;
                _tickets.Add(_ticket);
                var tck2Seq = this.SequenceAgent.GetTicketInfoSeqenceByTimeSerial(TicketType.Move, ticketInfoCount);
                foreach (var payload in pod)
                {
                    var _ticketUID = _ticket.UID;
                    var _hasSameItem = itemGroup.Where(g => g.Key.ItemUID == payload.ItemUID
                    && g.Key.PackageUID == payload.PackageUID && g.Key.SlotUID == payload.SlotUID);
                    #region 同產品同包裝同位置的項目不放在同個Ticket內 (目前不使用)
                    //if (_hasSameItem.First().Count() > 1)
                    //{
                    //    if (_ticketMapping.Count > 0)
                    //    {
                    //        var sameticket = _ticketMapping.Where(p =>
                    //            p.ItemUID == payload.ItemUID &&
                    //            p.PackageUID == payload.PackageUID &&
                    //            p.SlotUID == payload.SlotUID);
                    //        var notinticket = _atickets.Where(o => sameticket.Select(r => r.TicketUID).Any(y => y != o.UID));
                    //        if (sameticket.Count() > 0) //是否有相同的item 在同個pod 上
                    //        {
                    //            if (notinticket.Count() == 0)//排除相同的pod 資料是否還有其它pod 可以使用
                    //            {
                    //                _sequence = tckSeq.Dequeue();
                    //                var _oticket = new TicketInnerModel();
                    //                _oticket.WarehouseUID = warehouseUID;
                    //                _oticket.ManifestType = (int)Type;
                    //                _oticket.ServiceItemUID = this.ServiceItemID;
                    //                _oticket.WorkOrderUID = pod.First().WorkOrderUID;
                    //                _oticket.Type = (int)TicketInfoType.Move;
                    //                _oticket.Status = _ticketStatus;
                    //                _oticket.UID = Guid.NewGuid();
                    //                _oticket.TicketSequence = DateTime.Now.Ticks;
                    //                _oticket.ID = _sequence;
                    //                _atickets.Add(_oticket);
                    //                _ticketUID = _oticket.UID;
                    //            }
                    //            else
                    //            {
                    //                _ticketUID = notinticket.First().UID;
                    //            }
                    //        }
                    //    }
                    //    _ticketMapping.Add(new OutboundMoveTicketMappingModel
                    //    {
                    //        TicketUID = _ticketUID,
                    //        ItemUID = payload.ItemUID,
                    //        PackageUID = payload.PackageUID,
                    //        SlotUID = payload.SlotUID
                    //    });
                    //}
                    #endregion


                    TicketInfoInnerModel _info = new TicketInfoInnerModel();
                    var _itemSequence = tck2Seq.Dequeue();
                    _info.UID = Guid.NewGuid();
                    _info.TicketUID = _ticketUID;
                    _info.Status = _ticketInfoStatus;
                    _info.Type = (int)TicketInfoType.Move;
                    _info.ID = _info.Name = _itemSequence;
                    _info.EstQty = payload.Qty;
                    _info.WorkOrderPayloadUID = payload.WorkOrderPayloadUID;
                    _info.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                    _info.CreatedOn = DateTime.UtcNow;
                    _ticketInfos.Add(_info);
                }
                _tickets.AddRange(_atickets);
                _atickets = new List<ITicketModel>();
                _ticketMapping = new List<OutboundMoveTicketMappingModel>();
            }

            #endregion
            //Process Ticket Relation 需判斷上一個Move manifest Ticket 關係 (目前不會執行)
            if (parentTicket.Count > 0)
            {
                foreach (var item in parentTicket)
                {

                    var belongToTicket = _tickets.Where(y =>
                    {
                        var xx = _ticketInfos
                          .Where(x => item.WorkOrderPayloadUID.Any(p => p == x.WorkOrderPayloadUID) ||
                        item.WorkOrderPodUID.Any(p => p == x.WorkOrderPodUID));
                        return xx.Any(r => r.TicketUID == y.UID);
                    });
                    foreach (var item2 in belongToTicket)
                    {
                        if (!_ticketRelation.Any(p => p.ParentUID == item.Ticket.UID && p.TicketUID == item2.UID))
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
