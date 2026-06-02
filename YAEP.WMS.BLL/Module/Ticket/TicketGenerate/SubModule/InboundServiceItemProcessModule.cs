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
    internal class InboundServiceItemProcessModule : AbstractServiceItemProcessModule
    {
        public override Guid ServiceItemID => new Guid("b0cbc36c-c875-457d-aabf-f3ce19f71e13");
        public InboundServiceItemProcessModule(ServiceItemProcessModuleParameters parameters) : base(parameters)
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
            var _sequence = this.SequenceAgent.GetTicketSeqenceByTimeSerial(TicketType.Receiving);
            //process Ticket
            TicketInnerModel _ticket = new TicketInnerModel();
            _ticket.ServiceItemUID = this.ServiceItemID;
            _ticket.WorkOrderUID = collection.First().WorkOrderUID;
            _ticket.WarehouseUID = warehouseUID;
            _ticket.Type = (int)TicketInfoType.Receiving;
            _ticket.Status = _ticketStatus;
            _ticket.ManifestType = (int)Type;
            _ticket.UID = Guid.NewGuid();
            _ticket.ID = _sequence;
            _ticket.TicketSequence = DateTime.Now.Ticks;
            _ticket.OperationSuggestion = collection.First().OperationSuggestion;
            _ticket.OperationInstruction = this.InstructionBuilder.GetInstruction(collection.First());
            _ticket.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
            _ticket.CreatedOn = DateTime.UtcNow;
            //_ticket.ParentTicketUID = parentTicketUID;
            _tickets.Add(_ticket);

            var uomList = this.PackageUomManager.GetPackageUomList();
            var palletGroup = collection.GroupBy(p => new { WorkOrderPodUID = p.WorkOrderPodUID, StorageMethod = p.StoreageMethod });
            var itemsPkgInfo = collection.GroupBy(g => g.OriginalPackageUID)
                .Select(p => this.PackageManager.GetPackage(p.Key));
            var tckSeq = this.SequenceAgent.GetTicketInfoSeqenceByTimeSerial(TicketType.Receiving,
                        palletGroup.SelectMany(x => x.Select(a => a.WorkOrderPayloadUID)).Count());
            //process Ticket Info
            foreach (var grp in palletGroup)
            {
                var palletUOM = uomList.Content.FirstOrDefault(p => p.Name.Equals(WMSAPIParameters.PALLET_UOM_KEYNAME, StringComparison.OrdinalIgnoreCase));
                var originalPkgInfos = grp.Select(x =>
                                                this.PackageUomManager.GetPackageUom(
                                                    itemsPkgInfo.FirstOrDefault(p => p.UID == x.OriginalPackageUID).UOM));
                //用Pod 去關聯資料 當item UOM=Pallet &StorageMethod=NewPallet  & item count=1
                //https://docs.google.com/spreadsheets/d/1w3oMMN78QzOgF6YmXWwPhlZV1WMYIu2UrdmOVnj593c/edit#gid=1448835445
                if (originalPkgInfos.All(p => p.Content.UID == palletUOM.UID)
                    && grp.Key.StorageMethod == (int)StorageMethod.NewPallet && grp.Count() == 1)
                {
                    TicketInfoInnerModel _info = new TicketInfoInnerModel();
                    var _itemSequence = tckSeq.Dequeue();
                    _info.UID = Guid.NewGuid();
                    _info.TicketUID = _ticket.UID;
                    _info.Status = _ticketInfoStatus;
                    _info.Type = (int)TicketInfoType.Receiving;
                    _info.EstQty = grp.First().Qty;
                    _info.WorkOrderPodUID = grp.Key.WorkOrderPodUID;
                    _info.ID = _info.Name = _itemSequence;
                    _info.CreatedBy = this.AuthenticationProvider.GetAuthenticationInfo().Account;
                    _info.CreatedOn = DateTime.UtcNow;
                    _ticketInfos.Add(_info);
                }
                else //用Payload 關聯資料
                {
                    foreach (var item in grp)
                    {

                        TicketInfoInnerModel _info = new TicketInfoInnerModel();
                        var _itemSequence = tckSeq.Dequeue();
                        //this.SequenceAgent.GetTicketInfoSeqence(_ticket.UID, SequenceTag.TICKET_INFO_TAG);
                        _info.UID = Guid.NewGuid();
                        _info.TicketUID = _ticket.UID;
                        _info.Status = _ticketInfoStatus;
                        //_info.ItemUID = item.ItemUID;
                        _info.Type = (int)TicketInfoType.Receiving;
                        _info.EstQty = item.Qty;
                        _info.WorkOrderPayloadUID = item.WorkOrderPayloadUID;
                        //_info.WorkOrderPodUID = item.WorkOrderPodUID;
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
                //取出是wplayload mapping 且有pod barcode 加入wpod guid ，以便後續move ticket 做pod 關聯
                var havepodbarcode = collection.Where(p => belongticketinfos.Where(a => a.WorkOrderPodUID == null).Select(a => a.WorkOrderPayloadUID).Any(x => x == p.WorkOrderPayloadUID))
                    .Where(x => x.Labels.Any(l =>
                        new int[] { (int)LabelType.Pallet_Self, (int)LabelType.Pallet_Other }
                    .Any(o => o == l.BarcodeType))).Select(y => y.WorkOrderPodUID);
                if (havepodbarcode.Count() > 0)
                    e.WorkOrderPodUID.AddRange(havepodbarcode);
                parentTicket.Add(e);
            }
            return result;
        }
    }
}
