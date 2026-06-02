using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Attachment.ClientAPI;
using YAEP.Constants;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Identities.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.Utilities.Model;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, ITicketManager
    {
        #region Properties
        private IInventoryRepository InventoryRepository { get; set; }
        private ILabelRepository LabelRepository { get; set; }
        private ITicketInfoAssigneeRelationRepository TicketInfoAssigneeRelationRepository { get; set; }
        private ITicketRelationRepository TicketRelationRepository { get; set; }
        private IWorkOrderManager WorkOrderManager { get; set; }
        #endregion
        public IActionResult<bool> AddWorkderAPI(IMaintainWorkderParameters Parametes, bool isIgnoreCheck = false)
        {
            //return this.TicketInfoAssigneeRelationRepository.AddWorkder(Parametes);

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    rs = this.AddWorkder(Parametes, isIgnoreCheck);
                    if (rs.Success)
                    {
                        db.Commit();
                    }
                    else
                    {
                        db.Rollback();
                    }
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }

            return rs;
        }
        public IActionResult<bool> AddWorkder(IMaintainWorkderParameters Parametes, bool isIgnoreCheck = false)
        {
            //return this.TicketInfoAssigneeRelationRepository.AddWorkder(Parametes);

            var rs = ActionResultTemplates.Result<bool>();
            try
            {

                var ticketInfos = this.TicketInfoRepository.GetList(Parametes.TicketInfoUID);
                if (ticketInfos.Success && ticketInfos.Content.Count() > 0)
                {
                    var result2 = this.TicketInfoAssigneeRelationRepository.AddWorkder(Parametes, isIgnoreCheck);
                    if (result2.Success)
                    {
                        var isComplete = false;
                        var resultMessage = "";
                        if (ticketInfos.Content.All(p => p.Status == (int)TicketStatus.Assigned))
                        {
                            isComplete = true;
                        }
                        else
                        {
                            if (!ticketInfos.Content.Any(x => x.Status > (int)TicketStatus.Assigned))
                            {
                                var ticketUIDs = ticketInfos.Content.GroupBy(p => p.TicketUID).Select(g => g.Key).ToArray();
                                var rs1 = this.StatusAgent.Ticket.ChangeAllTicketStatus(ticketUIDs, TicketStatus.Assigned, TicketInfoStatus.Assigned);
                                resultMessage = rs.Message;
                                isComplete = rs1.Success;
                            }
                            else
                            {
                                isComplete = true;
                            }
                        }
                        if (isComplete)
                        {
                            rs.Content =
                            rs.Success = true;

                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = resultMessage;
                        }
                    }
                    else
                    {
                        rs.Content =
                        rs.Success = false;
                        rs.Message = Resource.TICKET_ADD_WORKER_FAILURE;
                    }
                }
                else
                {
                    rs.Content =
                    rs.Success = false;
                    rs.Message = Resource.TICKET_NOT_FIND_TICKETINFO;
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }

            return rs;
        }
        public IActionResult<bool> BatchAssignWorkerAPI(IMaintainWorkderParameters Parametes)
        {

            var rs = ActionResultTemplates.Result<bool>();
            using (var db = this.DbEntities.DbAdapter)
            {
                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                rs = this.BatchAssignWorker(Parametes);
                if (rs.Success)
                {
                    db.Commit();
                }
                else
                {
                    db.Rollback();
                }
            }
            return rs;
        }
        public IActionResult<bool> BatchAssignWorker(IMaintainWorkderParameters Parametes)
        {

            var rs = ActionResultTemplates.Result<bool>();


            try
            {

                //Clear Worker
                var result = this.TicketInfoAssigneeRelationRepository.ClearAllWorkder(Parametes.TicketInfoUID);


                //Add New Worker
                if (result.Success)
                {
                    var ticketInfos = this.TicketInfoRepository.GetList(Parametes.TicketInfoUID);
                    if (ticketInfos.Success && ticketInfos.Content.Count() > 0)
                    {
                        var result2 = this.TicketInfoAssigneeRelationRepository.AddWorkder(Parametes);
                        if (result2.Success)
                        {
                            var isComplete = false;
                            var resultMessage = "";
                            if (ticketInfos.Content.All(p => p.Status == (int)TicketStatus.Assigned))
                            {
                                isComplete = true;
                            }
                            else
                            {
                                if (!ticketInfos.Content.Any(x => x.Status > (int)TicketStatus.Assigned))
                                {
                                    var ticketUIDs = ticketInfos.Content.GroupBy(p => p.TicketUID).Select(g => g.Key).ToArray();
                                    var rs1 = this.StatusAgent.Ticket.ChangeAllTicketStatus(ticketUIDs, TicketStatus.Assigned, TicketInfoStatus.Assigned);
                                    resultMessage = rs.Message;
                                    isComplete = rs1.Success;

                                }
                                else
                                {
                                    isComplete = true;
                                }

                            }
                            if (isComplete)
                            {
                                rs.Content =
                                rs.Success = true;
                                //scope.Complete();
                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = resultMessage;
                            }
                        }
                        else
                        {
                            rs.Content =
                            rs.Success = false;
                            rs.Message = Resource.TICKET_ADD_WORKER_FAILURE;
                        }
                    }
                    else
                    {
                        rs.Content =
                        rs.Success = false;
                        rs.Message = Resource.TICKET_NOT_FIND_TICKETINFO;
                    }
                }
                else
                {
                    rs.Content =
                    rs.Success = false;
                    rs.Message = Resource.TICKET_ADD_WORKER_FAILURE;
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }

            return rs;
        }
        public IActionResult<bool> ChangeTicketStatus(Guid ticketUID, TicketStatus status)
        {
            return this.TicketRepository.UpdateTicketStatus(new Guid[] { ticketUID }, status);
        }
        public IActionResult<bool> GeneratreTicket(ITicketGenerateParameter parameter)
        {
            var _isAllowGenerate = false;
            //IActionResult<IManifestViewModel> mInfo = null;
            int _manifestType = parameter.ManifestType;
            if (parameter.WarehouseUID != Guid.Empty)
            {
                if (parameter.ManifestUID.HasValue)
                {
                    _isAllowGenerate = true;
                }
                if (parameter.BolUID.HasValue)//Allocated By Cap
                {
                    _isAllowGenerate = true;
                }
                if (parameter.BolUIDs != null && parameter.BolUIDs.Count() > 0)//Allocated By Cap
                {
                    _isAllowGenerate = true;
                }
                if (parameter.WorkOrderUID.HasValue)//目前只有Move Manifest /BulkPick 使用
                {
                    if (parameter.IsBulkPick)
                    {
                        _manifestType = (int)ManifestType.BlukPick;
                    }
                    else
                    {
                        _manifestType = (int)ManifestType.Move;
                    }
                    _isAllowGenerate = true;
                }
            }
            else
            {
                _isAllowGenerate = false;
                var rs = ActionResultTemplates.Result<bool>();
                rs.Success =
                rs.Content = false;
                rs.Message = Resource.MANIFEST_TICKET_GENERATE_MISS_WAREHOUSE;
                return rs;
            }
            //this.ManifestAgent.BolManager
            if (_isAllowGenerate)
            {
                TicketGeneratorParameters parameters = new TicketGeneratorParameters();
                parameters.PackageManager = this.PackageCacheManager;
                parameters.PackageUomManager = this.PackageUomManager;
                parameters.SequenceAgent = this.SequenceAgent;
                parameters.TicketInfoRepository = this.TicketInfoRepository;
                parameters.TicketRelationRepository = this.TicketRelationRepository;
                parameters.TicketRepository = this.TicketRepository;
                parameters.WorkOrderManager = this.WorkOrderManager;
                parameters.LabelRepository = this.LabelRepository;
                parameters.AuthenticationProvider = this.AuthProvider;
                parameters.TracingAgent = this.TracingAgent;
                var TicketGenerator = AbstractTicketGenerator.GetInstance((ManifestType)_manifestType, parameters);
                return TicketGenerator.Execute(parameter);
            }
            else
            {

                var rs = ActionResultTemplates.Result<bool>();
                rs.Success =
                rs.Content = false;
                rs.Message = Resource.MANIFEST_GENERATE_TICKET_ERROR;
                return rs;
            }
        }
        public IActionResult<IEnumerable<IBolInfoViewModel>> GetBolInfo(Guid BolUID)
        {
            return this.TicketRepository.GetBolInfo(BolUID);
        }
        public IActionResult<IEnumerable<IComponentViewModel>> GetBolNameList(Guid ManifestUID)
        {
            return this.TicketRepository.GetBolNameList(ManifestUID);
        }
        public IActionResult<IEnumerable<IStatusCheckModel>> GetManifestStatusCollection(IEnumerable<Guid> TicketUIDs)
        {
            return this.TicketRepository.GetManifestStatusCollection(TicketUIDs);
        }
        public IActionResult<IEnumerable<IStatusCheckModel>> GetManifestStatusCollection(Guid TicketUID)
        {
            return this.TicketRepository.GetManifestStatusCollection(new Guid[] { TicketUID });
        }
        public IActionResult<IEnumerable<IStatusCheckModel>> GetBatchManifestStatusCollection(IEnumerable<Guid> TicketUIDCollection)
        {
            return this.TicketRepository.GetManifestStatusCollection(TicketUIDCollection);
        }
        public IActionResult<IEnumerable<ITicketInfoModel>> GetPodBelongTicket(IEnumerable<string> enumerable)
        {
            return this.TicketInfoRepository.GetPodBelongTicket(enumerable);
        }
        public IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoList(object condition)
        {
            return this.TicketInfoRepository.GetList(condition);
        }
        public IEnumerable<IEnumFieldInfo> GetServiceItemNameList()
        {
            return EnumerableData.GetDataForGeneric(typeof(TicketType));
        }
        public IActionResult<IEnumerable<ITicketAssignedListViewModel>> GetTicketAssignedList(ITicketAssignedListParameters Parameters, IGroupManager groupManager)
        {
            var source = this.TicketRepository.GetTicketAssignedList(Parameters);
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketAssignedListViewModel>>();
            if (source.Success)
            {
                try
                {
                    var collection = source.Content.PayloadData.ToList();
                    var pldata = source.Content.PodData
                        .GroupBy(g => new { WorkOrderPodUID = g.WorkOrderPodUID, Type = g.ServiceType })
                       .Select(p => converAssignListViewModel(p));
                    collection.AddRange(pldata);
                    rs.Content = collection;
                    rs.Success = true;
                    ItemInnerParameterize param = new ItemInnerParameterize();
                    param.ListOfItemUID.AddRange(collection.Select(p => p.ItemUID));
                    var itemids = this.ItemManager.GetItems(param);
                    var lg = collection.GroupBy(p => p.SourceLoadingZoneSlotUID).Select(p => p.Key).ToList();
                    lg.AddRange(collection.GroupBy(p => p.SourceSlotUID).Select(p => p.Key));
                    var locationGroup = this.WarehouseAgent.SlotManager.GetLocations(lg.ToArray());
                    var assignedgroupList = this.TicketInfoAssigneeRelationRepository.GetAssignedList(collection.Select(p => p.TicketInfoUID).ToArray());
                    var groupList = groupManager.GetGroupUserView(assignedgroupList.Content.Select(p => p.GroupUID));
                    foreach (var item in collection)
                    {
                        var converter = AbstractTicketConverter.GetInstance(item.ManifestType, item.ServiceType);
                        if (itemids.Success)
                        {
                            var itemInfo = itemids.Content.FirstOrDefault(p => p.UID == item.ItemUID);
                            if (itemInfo != null)
                            {
                                item.ItemID = itemInfo.ID;
                            }
                        }
                        converter.Convert(item);
                        var assignedGroup = assignedgroupList.Content.Where(p => p.TicketInfoUID == item.TicketInfoUID);
                        if (assignedGroup != null)
                        {
                            if (groupList.Success)
                            {
                                var agl = groupList.Content.Where(p => assignedGroup.Any(x => x.GroupUID == p.GroupUID));
                                if (agl != null)
                                {
                                    var members = string.Join(",", agl.Select(p => p.Members)).Split(',').GroupBy(g => g);
                                    item.AssignedGroup = string.Join(",", members.Select(p => p.Key));
                                }
                            }
                        }
                        if (item.OriginalPackage.HasValue && item.OriginalPackage != Guid.Empty)
                        {

                            var pkg = this.PackageManager.GetPackage(item.OriginalPackage.Value);
                            if (pkg.Success)
                            {
                                item.OriginalPackageName = pkg.Content.Name;
                            }
                        }
                        if (item.TargetPackage.HasValue && item.TargetPackage != Guid.Empty)
                        {
                            var pkg = this.PackageManager.GetPackage(item.TargetPackage.Value);
                            if (pkg.Success)
                            {
                                item.TargetPackageName = pkg.Content.Name;
                            }
                        }
                        if (item.OriginalSlotUID.HasValue && item.OriginalSlotUID != Guid.Empty && locationGroup.Success)
                        {
                            var l = locationGroup.Content.FirstOrDefault(g => g.SlotUID == item.OriginalSlotUID);
                            item.OriginalLocation = l;
                            item.OriginalSlotName = l.SlotName;
                        }
                        if (item.TargetSlotUID.HasValue && item.TargetSlotUID != Guid.Empty && locationGroup.Success)
                        {
                            var l = locationGroup.Content.FirstOrDefault(g => g.SlotUID == item.TargetSlotUID);
                            item.TargetLocation = l;
                            item.TargetSlotName = l.SlotName;
                        }
                        item.ServiceTypeName = ((TicketType)item.ServiceType).ToString();
                    }
                }
                catch (Exception ex)
                {
                    rs.Message = ex.Message;
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    rs.InnerException = ex;
                    this.Log("Get ticket assigned list failure " + ex.StackTrace, "GetTicketAssignedList",
                        this.AuthProvider.GetAuthenticationInfo().Account, Logger.ERROR, (int)BelongToTypes.Ticket,
                        exception: ex);
                }
            }
            return rs;
        }
        public IActionResult<IEnumerable<ITicketGroupAssignedModel>> GetTicketGroupAssignedList(Guid ticketinfouid
             , IGroupManager groupManager)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketGroupAssignedModel>>();
            try
            {
                List<ITicketGroupAssignedModel> groups = new List<ITicketGroupAssignedModel>();
                var collection = this.TicketInfoAssigneeRelationRepository.GetAssignedList(new Guid[] { ticketinfouid });
                foreach (var item in collection.Content)
                {
                    TicketGroupAssignedInnerModel e = new TicketGroupAssignedInnerModel();
                    e.UID = item.UID;
                    var group = groupManager.GetGroupUserView(new Guid[] { item.GroupUID });
                    if (group.Success)
                    {
                        var g = group.Content.FirstOrDefault(p => p.GroupUID == item.GroupUID);
                        e.GroupName = g.GroupName;
                        e.Members = g.Members;
                    }
                    //var g = groupUsers.FirstOrDefault(p => p.GroupUID == item.GroupUID);
                    //if (g != null)
                    //{
                    //    e.GroupName = g.GroupName;
                    //    e.Members = g.Members;
                    //}
                    groups.Add(e);
                }
                rs.Content = groups;
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<IEnumerable<IComponentViewModel>> GetTicketIDList(Guid BolUID)
        {
            return this.TicketRepository.GetTicketIDList(BolUID);
        }
        public IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoList(Guid BolUID)
        {
            return this.TicketRepository.GetTicketInfoList(BolUID);
        }
        public IActionResult<ITicketModel> GetTicketModel(object condition)
        {

            var rs = ActionResultTemplates.Result<ITicketModel>();
            try
            {
                var result = this.TicketRepository.GetList(condition);
                rs.Content = result.Content.FirstOrDefault();
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<IEnumerable<ITicketRelationModel>> GetTicketRelationList(object condition)
        {
            return this.TicketRelationRepository.GetTicketRelationList(condition);
        }
        public IActionResult<IEnumerable<IComponentViewModel>> GetVesselRefNoList(Guid BolUID)
        {
            return this.TicketRepository.GetVesselRefNoList(BolUID);
        }
        public IActionResult<bool> IsTicketComplete(Guid TicketUID, Guid[] notContainsTicketInfoUID = null)
        {
            return this.TicketInfoRepository.IsTicketComplete(TicketUID, notContainsTicketInfoUID);
        }
        public IActionResult<bool> RemoveWorkderAPI(Guid[] tauid, Guid TicketInfoUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            using (var db = this.DbEntities.DbAdapter)
            {
                try
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    var relationTicketInfos = this.TicketInfoAssigneeRelationRepository.GetRelationTicketInfo(TicketInfoUID);
                    var assignedList = this.TicketInfoAssigneeRelationRepository.GetAssignedList(new Guid[] { TicketInfoUID });
                    rs = this.TicketInfoAssigneeRelationRepository.RemoveWorkder(tauid);
                    if (relationTicketInfos.Success)
                    {
                        if (relationTicketInfos.Content.Count() > 0)
                        {
                            if (relationTicketInfos.Content.Any(p => p.Status == (int)TicketInfoStatus.Draft))
                            {
                                this.StatusAgent.Ticket.ChangeTicketStatus(
                                                    relationTicketInfos.Content.First().TicketUID, TicketStatus.Draft,
                                                    this.AuthProvider.GetAuthenticationInfo().Account);
                            }
                        }
                        if (assignedList.Content.Count() == 0)
                        {
                            this.StatusAgent.Ticket.ChangeTicketInfoStatus(TicketInfoUID, TicketInfoStatus.Draft);
                        }
                        db.Commit();
                        rs.Content = true;
                        rs.Success = true;
                    }
                    else
                    {
                        db.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    db.Rollback();
                    rs.Message = ex.Message;
                }
            }

            return rs;
        }
        public IActionResult<bool> UpdateTicketInfo(ITicketInfoModel model)
        {
            return this.TicketInfoRepository.UpdateTickInfo(model);
        }
        public IActionResult<bool> VoidTicket(IVoidTicketParameters Parameters)
        {
            if (Parameters.WorkOrderUID.HasValue)
            {
                return this.TicketRepository.VoidTicketByWorkOrder(Parameters);
            }
            else
            {
                return this.TicketRepository.VoidTicket(Parameters);
            }
        }
        private ITicketAssignedListViewModel converAssignListViewModel(IGrouping<dynamic, ITicketAssignedListViewModel> group)
        {
            ItemInnerParameterize param = new ItemInnerParameterize();
            param.ListOfItemUID.AddRange(group.GroupBy(g => g.ItemUID).Select(x => x.Key));
            var itemid = this.ItemManager.GetItems(param);
            ITicketAssignedListViewModel e = new TicketAssignedListViewInnerModel();
            e.ActQty = group.First().ActQty;
            e.EstQty = group.First().EstQty;
            if (itemid.Success)
                e.ItemID = string.Join(",", itemid.Content.Select(p => p.ID));
            e.ManifestType = group.First().ManifestType;
            e.MappingType = group.First().MappingType;
            e.OriginalPackageName = WMSAPIParameters.PALLET_UOM_KEYNAME;
            e.OriginalSlotUID = group.First().OriginalSlotUID;
            e.SavQty = group.First().SavQty;
            e.ShtQty = group.First().ShtQty;
            e.SourceLoadingZoneSlotUID = group.First().SourceLoadingZoneSlotUID;
            e.SourceSlotUID = group.First().SourceSlotUID;
            e.TargetPackageName = WMSAPIParameters.PALLET_UOM_KEYNAME;
            e.TargetSlotUID = group.First().TargetSlotUID;
            e.WorkOrderPodUID = group.Key.WorkOrderPodUID;
            e.TicketNo = group.First().TicketNo;
            e.VesselName = group.First().VesselName;
            e.Status = group.First().Status;
            e.ServiceType = group.First().ServiceType;
            e.TicketInfoUID = group.First().TicketInfoUID;
            return e;
        }
        public IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoByPickAll(IEnumerable<Guid> vesselUID = null, IEnumerable<Guid> workorderPayloadUID = null)
        {
            return this.TicketInfoRepository.GetTicketInfoByPickAll(vesselUID, workorderPayloadUID);
        }
        public IEnumerable<IEnumFieldInfo> GetTicketTypeList()
        {
            return EnumerableData.GetDataForGeneric(typeof(TicketType));
        }
        public IEnumerable<IEnumFieldInfo> GetTicketStatusList()
        {
            return EnumerableData.GetDataForGeneric(typeof(TicketStatus));
        }
        public IActionResult<IEnumerable<ITicketSearchListViewModel>> GetTicketSearchList(ITicketSearchListParameters Parameters, IGroupManager groupManager)
        {
            var source = this.TicketRepository.GetTicketSearchList(Parameters);
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketSearchListViewModel>>();
            if (source.Success)
            {
                try
                {
                    var collection = source.Content.ToList();
                    if (collection.Count > 0)
                    {
                        foreach (var item in collection)
                        {
                            item.TicketStatusName = ((TicketStatus)item.TicketStatus).ToString();
                            item.TicketTypeName = ((TicketType)item.TicketType).ToString();
                        }
                    }
                    rs.Content = collection;
                    rs.Success = true;
                }
                catch (Exception ex)
                {
                    rs.Message = ex.Message;
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    rs.InnerException = ex;
                    this.Log("Get ticket search list failure " + ex.StackTrace, "GetTicketSearchList",
                        this.AuthProvider.GetAuthenticationInfo().Account, Logger.ERROR, (int)BelongToTypes.Ticket,
                        exception: ex);
                }
            }
            return rs;
        }
        public IActionResult<Guid> GenerateBulkPickTicket(IEnumerable<Guid> TicketInfoUID)
        {

            var rs = ActionResultTemplates.Result<Guid>();
            List<IActionResult<bool>> result = new List<IActionResult<bool>>();
            try
            {
                //get original ticketinfo & w.payload data
                var originalData = this.TicketRepository.GetBulkPickOrignalData(TicketInfoUID);
                if (originalData.Success && originalData.Content.TicketInfoCollection.Count() > 0 &&
                    originalData.Content.WorderPayloadCollection.Count() > 0 &&
                    originalData.Content.TicketInfoCollection.All(x => x.Type == (int)TicketInfoType.Move))
                {
                    var orgWPayloadCollection = originalData.Content.WorderPayloadCollection;
                    var orgTicketInfoCollection = originalData.Content.TicketInfoCollection;
                    //change w.payload from location to Parcel slot(targetslot)
                    result.Add(this.WorkOrderPayloadRepository.BulkPickChangeFromSlot(
                        orgWPayloadCollection.Select(x => x.UID)));
                    //change ticket status to AssignedBulkPick
                    result.Add(this.TicketInfoRepository.UpdateTicketInfoStatus(
                            orgTicketInfoCollection.Select(x => x.UID),
                            TicketInfoStatus.AssignedBulkPick));
                    result.Add(this.TicketRepository.UpdateTicketStatus(
                         orgTicketInfoCollection.Select(x => x.TicketUID),
                         TicketStatus.AssignedBulkPick
                        ));
                    //group original w.payload data by same item,package,fromslot, targetslot
                    var wpayloadGrp = orgWPayloadCollection.GroupBy(g => new
                    {
                        ItemUID = g.ItemUID,
                        PackageUID = g.PackageUID,
                        FromSlotUID = g.SlotUID,
                        TargetSlotUID = g.LoadingZoneSlotUID
                    });
                    var bulkPickWorkOrder = new AssignedOutboundWorkOrderCollection();
                    bulkPickWorkOrder.ServiceType = ManifestType.BlukPick;
                    var witems = new List<AssignedBulkPickWorkOrderPayload>();
                    foreach (var witem in wpayloadGrp)
                    {
                        var e = new AssignedBulkPickWorkOrderPayload();

                        if (witem.All(p => p.ItemGroupUID.HasValue))
                        {
                            e.ItemGroupUID = Guid.NewGuid();
                        }
                        e.PayloadUID = Guid.NewGuid();
                        e.ItemUID = witem.Key.ItemUID;
                        e.AllocatedQty = witem.Sum(x => x.Qty);
                        e.PickPackageUID = witem.Key.PackageUID;
                        e.SlotUID = witem.Key.FromSlotUID;
                        e.TargetSlotUID = witem.Key.TargetSlotUID;
                        e.OriginalPayloadUID = witem.Select(p => p.PayloadUID);
                        e.OriginalWordPayloadUID = witem.Select(p => p.UID);
                        witems.Add(e);
                    }
                    bulkPickWorkOrder.Items = witems.Select(p => p as IAssignedOutboundWorkOrderPayload).ToList();
                    var converter = new AssignedParameterConverter();
                    var workOrder = converter.BulkPickParameterConvert(bulkPickWorkOrder);
                    //create workorder data 
                    var agent = AbstractWorkOrderAssignAgent.GetAgent(ManifestType.BlukPick,
                        this.GetWorkOrderAgentParameters());
                    var wresult = agent.Execute(workOrder);
                    //create ticket data 
                    if (wresult.Success)
                    {
                        var workOrderpayload = this
                               .WorkOrderPayloadRepository.GetList(new { workOrderUID = wresult.Content });
                        //created work pod
                        var workorderPod = new WorkOrderPodInnerModel();
                        workorderPod.PodUID = Guid.NewGuid();
                        workorderPod.UID = Guid.NewGuid();
                        workorderPod.ID = this.SequenceAgent.GetWorkOrderPodSeqenceByTimeSerial(ManifestType.BlukPick);
                        //workorderPod.OperationSuggestion = workOrder.OperationSuggestion;
                        workorderPod.Type = workOrder.StorageMethod;
                        workorderPod.WorkOrderUID = wresult.Content.WorkOrderUID;
                        workorderPod.Status = (int)WorkOrderPodStatus.Open;
                        workorderPod.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                        workorderPod.Weight = workOrderpayload.Content.Sum(p => p.Weight);
                        workorderPod.Volume = workOrderpayload.Content.Sum(p => p.Volume);
                        var addPodResult = this.WorkOrderPodRepository.AddWorkOrderPod(workorderPod);
                        if (addPodResult.Success)
                        {
                            var assignpodResult = this.WorkOrderManager
                                         .AssignedPayloadtoPod(workorderPod.UID, workOrderpayload.Content.Select(p => p.UID));
                            if (assignpodResult.Success)
                            {
                                var param = new TicketGenerateInnerParameter();
                                param.WorkOrderUID = wresult.Content.WorkOrderUID;
                                param.WarehouseUID = orgTicketInfoCollection.First().WarehouseUID;
                                param.IsBulkPick = true;
                                var rsGenerateTicket = this.GeneratreTicket(param);
                                if (rsGenerateTicket.Content)
                                {
                                    var ticketInfo = this.TicketManager.GetTicketModel(new { WorkOrderUID = wresult.Content });
                                    if (ticketInfo.Success)
                                    {
                                        //get bluk pick ticket UID
                                        rs.Content = ticketInfo.Content.UID;
                                    }

                                }
                            }
                            else
                            {
                                result.Add(assignpodResult);
                            }
                        }
                        else
                        {
                            result.Add(addPodResult);
                        }
                    }
                    else
                    {
                        var allocatedResult = ActionResultTemplates.OK();
                        allocatedResult.Success = false;
                        allocatedResult.Message = wresult.Message;
                        result.Add(allocatedResult);
                    }


                    rs.Success = true;
                    if (result.All(x => x.Success))
                    {

                    }
                    else
                    {

                        rs.Content = Guid.Empty;
                        rs.Success = false;
                        rs.Message = string.Join(",", result.Select(x => x.Message));
                    }
                }
                else
                {

                    rs.Success = false;
                    rs.Message = "Not find w.payload data or ticket item data.";
                }

            }
            catch (Exception ex)
            {

                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;

        }
        public IActionResult<bool> RemoveBulkPickTicket(IEnumerable<IBulkPickTicketInfoRelationModel> blukpickInfos)
        {

            var rs = ActionResultTemplates.Result<bool>();
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();

            try
            {
                var ticketInfo = this.TicketRepository.GetTicketByBulkPick(blukpickInfos.First().BulkPickUID);
                if (ticketInfo.Content != null)
                {
                    //void blukpick's ticket data
                    //delete blukpick's workorder data
                    //delete blukpick's payload
                    var voidResult = this.WorkOrderManager.RemoveWorkOrder(new Guid[] { ticketInfo.Content.WorkOrderUID });
                    Result.Add(voidResult);
                    if (voidResult.Content)
                    {
                        var originalWpayload = this.WorkOrderPayloadRepository
                                                .GetWorkOrderPayloadByTicketInfo(blukpickInfos.Select(p => p.TicketInfoUID));
                        var ticketInfos = this.TicketInfoRepository.GetList(new
                        {
                            UID =
                                                                originalWpayload.Content.Select(x => x.TicketInfoUID)
                        });
                        var bulkpickgrp = blukpickInfos.GroupBy(g => new { FromSlotUID = g.FromSlotUID });
                        //recovery original w.payload from slot uid

                        foreach (var grp in bulkpickgrp)
                        {
                            var oriwpayloadgrp = originalWpayload.Content.Where(p => grp.Any(o => o.TicketInfoUID == p.TicketInfoUID));
                            Result.Add(this.WorkOrderPayloadRepository
                                        .BulkPickChangebackFromSlot(oriwpayloadgrp.Select(a => a.UID), grp.Key.FromSlotUID));
                        }
                        //recovery original ticket info status "assigned bulkpick ->Open"
                        Result.Add(this.TicketRepository
                            .UpdateTicketStatus(ticketInfos.Content.Select(y => y.TicketUID), TicketStatus.Open));
                        Result.Add(this.TicketInfoRepository.UpdateTicketInfoStatus(
                            originalWpayload.Content.Select(r => r.TicketInfoUID), TicketInfoStatus.Open));
                        //recovery original allocated payload status "bulkpick pending->Allocated"
                        Result.Add(this.PayloadRepository
                            .ChangePayloadType(
                            originalWpayload.Content.Select(p => p.PayloadUID), (int)PayloadType.Allocated));
                        if (Result.All(p => p.Success))
                        {
                            rs.Success = true;
                        }
                        else
                        {
                            rs.Content = false;
                            rs.Success = false;
                            rs.Message = string.Join(",", Result.Select(x => x.Message));
                        }
                    }
                    else
                    {
                        rs.Success = rs.Content = false;
                        rs.Message = voidResult.Message;
                    }
                }
                else
                {
                    rs.Success = false;
                    rs.Message = "not find belong tickets.";
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }

            return rs;
        }

        public IActionResult<bool> CheckTicketStatus()
        {
            var _parameter = this.GetTicketProcessAgentParameter();
            var agent = AbstractProcessAgent.GetAgent(Constant.ProcessKind.TicketProcess, _parameter);
            var aa = agent.CheckStatus(new Guid("24F6883E-42AF-4E5C-AEEA-EF8C9C875889"));
            return ActionResultTemplates.OK();
        }
    }
}
