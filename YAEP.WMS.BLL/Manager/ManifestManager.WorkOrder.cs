using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.Utilities;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IWorkOrderManager
    {
        public IActionResult<IWorkOrderPodModel> AddWorkOrderPodAPI(IWorkOrderPodParameter parameter)
        {

            var rs = ActionResultTemplates.Result<IWorkOrderPodModel>();
            var Model = new WorkOrderPodInnerModel();

            var workorders = this.WorkOrderRepository.GetList(new { VesselUID = parameter.VesselUID });
            using (var db = this.DbEntities.DbAdapter)
            {
                try
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    IWorkOrderModel _workorderInfo = new WorkOrderInnerModel();
                    if (!workorders.Success || workorders.Content.Count() == 0)
                    {

                        var vesselInfo = this.VesselRepository.GetData(new { UID = parameter.VesselUID });

                        if (vesselInfo.Success)
                        {
                            var manifestInfo = this.WorkOrderRepository.GetManifestInfo(vesselInfo.Content.UID);
                            if (manifestInfo.Success && manifestInfo.Content != null)
                            {
                                //WMS UI 使用
                                var orderSeq = SequenceAgent.GetWorkOrderSeqence(parameter.VesselUID);

                                _workorderInfo.UID = Guid.NewGuid();
                                _workorderInfo.ID = orderSeq;
                                _workorderInfo.ManifestUID = manifestInfo.Content.UID;
                                _workorderInfo.VesselUID = vesselInfo.Content.UID;
                                _workorderInfo.Status = (int)WorkOrderStatus.Draft;
                                _workorderInfo.Type = 1;
                                _workorderInfo.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                                var rsOrder = this.WorkOrderRepository.AddWorkOrder(_workorderInfo);
                                if (!rsOrder.Success)
                                {
                                    rs.Success = false;
                                    rs.Message = rsOrder.Message;
                                    return rs;
                                }

                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;
                                return rs;
                            }
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_VESSEL;
                            return rs;
                        }
                    }
                    else
                    {
                        _workorderInfo = workorders.Content.FirstOrDefault();
                    }
                    //WMS UI
                    var sequence = SequenceAgent.GetWorkOrderPodSeqence(_workorderInfo.UID);
                    if (!parameter.UID.HasValue || (parameter.UID.HasValue && parameter.UID.Value == Guid.Empty))
                        Model.UID = Guid.NewGuid();
                    else
                        Model.UID = parameter.UID.Value;
                    Model.Name = parameter.Name;
                    Model.Status = (int)WorkOrderPodStatus.Draft;
                    Model.StartDate = parameter.StartDate;
                    Model.EndDate = parameter.EndDate;
                    Model.ContainerType = parameter.ContainerType;
                    Model.OperationSuggestion = parameter.OperationSuggestion;
                    Model.ID = sequence;
                    Model.WorkOrderUID = _workorderInfo.UID;
                    Model.Type = parameter.Type;
                    var result = this.WorkOrderPodRepository.AddWorkOrderPod(Model);
                    if (result.Success)
                    {
                        rs.Success = true;
                        rs.Content = Model;
                        //scope.Complete();
                        db.Commit();
                        return rs;
                    }
                    else
                    {
                        db.Rollback();
                        rs.Message = result.Message;
                    }

                }
                catch (Exception ex)
                {
                    db.Rollback();
                    rs.Message = ex.Message;
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    rs.InnerException = ex;
                }
            }
            return rs;
        }

        public IActionResult<bool> AssignedPayloadtoPod(Guid workOrderPodUID, IEnumerable<Guid> workOrderPayloadUID)
        {
            return this.WorkOrderPayloadRepository.AssignedPayloadtoPod(workOrderPodUID, workOrderPayloadUID);
        }

        public IActionResult<bool> ChangeWorkOrderPayloadStatus(Guid guid, WorkOrderPayloadStatus status)
        {
            return this.WorkOrderPayloadRepository.ChangeStatus(guid, status);
        }

        public IActionResult<bool> ChangeWorkOrderPodStatus(Guid guid, WorkOrderPodStatus status)
        {
            return this.WorkOrderPodRepository.ChangeStatus(guid, status);
        }

        public IActionResult<bool> ChangeWorkOrderStatus(Guid guid, WorkOrderStatus status)
        {
            return this.WorkOrderRepository.ChangeStatus(guid, status);
        }

        public IActionResult<bool> CheckHaveUnAssingedPodPayload(ITicketGenerateParameter parameter)
        {

            IActionResult<bool> rs = ActionResultTemplates.Result<bool>();
            var result = this.WorkOrderRepository.GetUnAssingedPayload(parameter);
            if (result.Success)
            {
                rs.Success = rs.Content = result.Content.Count() == 0;
                if (!rs.Success)
                {
                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_ASSINGED_POD;
                }
            }
            else
            {
                rs.Success = false;
                rs.Message = Resource.MANIFEST_WORKORDER_NOT_ASSINGED_POD;
            }
            return rs;
        }

        public IActionResult<ICheckOutboundAvailabilityResponse> CheckOutboundAvailabilityQty(ICheckAllocatedParameters parameters)
        {
            var rs = ActionResultTemplates.Result<ICheckOutboundAvailabilityResponse>();
            CheckOutboundAvailabilityResponse Response = new CheckOutboundAvailabilityResponse();
            VesselManifestSearchInnerParameters vparam = new VesselManifestSearchInnerParameters();
            vparam.VesselManifestUID = parameters.VesselMainifestUID;
            var vesselManifestInfo = this.VesselManifestRepository.GetList(vparam);
            if (vesselManifestInfo.Success && vesselManifestInfo.Content.Count() == 1)
            {
                //assinged qty
                var vinfo = vesselManifestInfo.Content.FirstOrDefault();
                var itemInfo = this.ProductCacheManager.GetItem(vinfo.ItemUID);
                var pkgTree = this.PackageCacheManager.GetPackageTree(vinfo.PackageUID);
                var assignedPkg = this.PackageCacheManager.GetPackage(vinfo.PackageUID);
                //var minPkg = pkgTree.Content.GetMinPackage();
                var minPkg = this.PackageCacheManager.GetMinPackage(vinfo.PackageUID);
                var minAssignedQty = this.PackageCacheManager.GetReceivePackageUomQuantity(vinfo.PackageUID, minPkg.UID, vinfo.Qty).Content;
                //allocated qty
                var allocatedInfo = this.WorkOrderPayloadRepository.GetList(new { VesselManifestUID = vinfo.UID });
                var minAllocatedQty = 0;
                if (allocatedInfo.Success)
                {
                    minAllocatedQty = allocatedInfo.Content.Sum(
                        p => this.PackageCacheManager.GetReceivePackageUomQuantity(p.PackageUID, minPkg.UID, p.Qty).Content);
                }
                //not yet allocated qty
                var unassignedInfo = this.InventoryManager.GetPayload(parameters.Items.Select(p => p.PayloadUID).ToArray());
                var minUnassignedQty = parameters.Items.Sum(x =>
                {
                    var plInfo = unassignedInfo.Content.FirstOrDefault(p => p.UID == x.PayloadUID);
                    if (plInfo != null)
                    {
                        return this.PackageCacheManager.GetReceivePackageUomQuantity(plInfo.PackageUID, minPkg.UID, x.AllocatedQty).Content;
                    }
                    return 0;
                });
                var freeQty = minAssignedQty - (minAllocatedQty + minUnassignedQty);
                Response.ItemID = itemInfo.ID;
                Response.ItemUID = itemInfo.UID;
                Response.RequestPackageUID = vinfo.PackageUID;
                Response.RequestPackageName = assignedPkg.Name;
                Response.RequestPackageQty = vinfo.Qty;
                Response.AllocatedQty = this.PackageCacheManager.GetReceivePackageUomQuantity(
                                        minPkg.UID, Response.RequestPackageUID, minAllocatedQty + minUnassignedQty).Content;
                if (freeQty < 0)
                {
                    freeQty = Math.Abs(freeQty);
                    Response.FreeQty = this.PackageCacheManager.GetReceivePackageUomQuantity(
                                           minPkg.UID, Response.RequestPackageUID, freeQty).Content;
                    Response.FreeQty = Response.FreeQty * -1;
                }
                else
                {
                    Response.FreeQty = this.PackageCacheManager.GetReceivePackageUomQuantity(
                                            minPkg.UID, Response.RequestPackageUID, freeQty).Content;
                }
                ProcessOutboundEstimateQty(Response.EstimateList, minPkg, pkgTree.Root, freeQty);
                rs.Content = Response;
                rs.Success = true;
            }
            else
            {
                rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_VESSELMANIFST;
            }
            return rs;
        }

        public IActionResult<bool> EditWorkOrderPod(dynamic parameter)
        {
            return this.WorkOrderPodRepository.EditWorkOrderPod(new { UID = parameter.UID },
                parameter);
        }

        public IActionResult<bool> ExecuteInboundAutoAssign(Guid vesselUID)
        {
            if (vesselUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(vesselUID));
            }

            var provider = new AutoAssignAgentProviders()
            {
                PartyManager = this.PartyManager,
                //PackageManager = this.PackageManager,
                //ItemManager = this.ItemManager,
                PackageCacheManager = this.PackageCacheManager,
                ProductCacheManager = this.ProductCacheManager,
                PackageUomManager = this.PackageUomManager,
                VesselManifestRepository = this.VesselManifestRepository,
                WarehouseManager = this.WarehouseManager,
                BolManager = this,
                ManifestManager = this,
                VesselManager = this,
                WorkOrderAssignAgentParameters = this.GetWorkOrderAgentParameters(),
                TicketRepository = this.TicketRepository,
                TicketRelationRepository = this.TicketRelationRepository,
                TicketInfoRepository = this.TicketInfoRepository,
                LabelRepository = this.LabelRepository
            };

            var autoAssignAgent = new InboundAutoAssignAgent(provider);
            var parameters = new AutoAssignParameters();
            parameters.VesselUID = vesselUID;
            return autoAssignAgent.Execute(parameters);
        }

        public IActionResult<bool> ExecuteOutboundAutoAssign(Guid vesselUID)
        {
            if (vesselUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(vesselUID));
            }
            //TODO 尚未移除Item/Package Manger 相依性
            var provider = new AutoAssignAgentProviders()
            {
                PartyManager = this.PartyManager,
                PackageManager = this.PackageManager,
                ItemManager = this.ItemManager,
                PackageCacheManager = this.PackageCacheManager,
                ProductCacheManager = this.ProductCacheManager,
                PackageUomManager = this.PackageUomManager,
                VesselManifestRepository = this.VesselManifestRepository,
                WarehouseManager = this.WarehouseManager,
                BolManager = this,
                ManifestManager = this,
                VesselManager = this,
                WorkOrderAssignAgentParameters = this.GetWorkOrderAgentParameters(),
                TicketRepository = this.TicketRepository,
                TicketRelationRepository = this.TicketRelationRepository,
                TicketInfoRepository = this.TicketInfoRepository,
                LabelRepository = this.LabelRepository,
            };

            var autoAssignAgent = new OutboundAutoAssignAgent(provider);
            var parameters = new AutoAssignParameters();
            parameters.VesselUID = vesselUID;
            return autoAssignAgent.Execute(parameters);
        }

        public IActionResult<IEnumerable<IAvailableInventoryModel>> GeteAvailableInventoryList(IGetAvailableInventoryParameters parameters)
        {
            return this.InventoryManager.GeteAvailableInventoryList(parameters);
        }

        public IActionResult<IEnumerable<ILoadingZoneSelectModel>> GetLandingZoneList(Guid value)
        {
            return this.WarehouseRepository.GetLoadingZoneList(value);
        }

        public IActionResult<IEnumerable<IWorkOrderPayloadViewModel>> GetWorkOrderPayload(Guid VesselUID)
        {
            var rs = this.WorkOrderPayloadRepository.GetWorkOrderPayload(VesselUID);

            if (rs.Success)
            {
                var labels = this.LabelManager.GetBelongtoBarcode(rs.Content.Select(p => p.PayloadUID).ToArray());
                var pItems = this.ProductCacheManager.GetItems(rs.Content.Select(p => p.ItemUID));
                foreach (var item in rs.Content)
                {
                    item.PackageName = this.PackageCacheManager.GetPackage(item.PackageUID).Name;
                    item.ItemID = pItems.FirstOrDefault(p => p.UID == item.ItemUID).Name;
                    item.StatusName = ((WorkOrderPayloadStatus)item.Status).ToString();
                    if (labels.Success)
                    {
                        var selfLabels = labels.Content.Where(p => p.BelongToUID == item.PayloadUID);
                        item.Labels = selfLabels;
                    }
                }
                rs.Content = rs.Content.OrderBy(o => o.Name);
            }
            return rs;
        }

        public IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetWorkOrderPayload(object condition)
        {
            return this.WorkOrderPayloadRepository.GetList(condition);
        }

        public IActionResult<IEnumerable<IWorkOrderPodViewModel>> GetWorkOrderPod(Guid VesselUID)
        {
            var rs = this.WorkOrderPodRepository.GetWorkOrderPod(VesselUID);
            if (rs.Success)
            {
                var pkgUOMList = this.PackageUomManager.GetPackageUomList();
                if (pkgUOMList.Success)
                {
                    foreach (var item in rs.Content)
                    {
                        var pkgUOM = pkgUOMList.Content.FirstOrDefault(p => p.UID == item.ContainerType);
                        if (pkgUOM != null)
                        {
                            item.ContainerTypeName = pkgUOM.Name;
                        }

                    }
                }
                rs.Content = rs.Content.OrderBy(o => o.Name);
            }
            return rs;
        }

        public IActionResult<IWorkOrderPodModel> GetWorkOrderPod(object condition)
        {
            return this.WorkOrderPodRepository.GetWorkOrderPod(condition);
        }

        public IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(Guid vesselUID)
        {
            return this.WorkOrderPodRepository.GetWorkOrderPodList(vesselUID);
        }

        public IActionResult<bool> HaveTicket(Guid[] workOrderPodGuids = null, Guid[] workOrderpayloadGuids = null)
        {
            HaveTicketParameters p = new HaveTicketParameters();
            p.workOrderPayloadGuids = workOrderpayloadGuids;
            p.workOrderPodGuids = workOrderPodGuids;
            return this.WorkOrderRepository.HaveTicket(p);
        }

        public IActionResult<bool> MergePalletAPI(IWorkOrderMergePalletParameter parameter)
        {
            var rs = ActionResultTemplates.Result<bool>();
            if (parameter.Mergefrom.Count() == 0)
            {
                rs.Message = Resource.MANIFEST_WORKORDER_MERAGE_POD_ERROR_FROM;
            }
            else if (parameter.Mergeto == Guid.Empty)
            {
                rs.Message = Resource.MANIFEST_WORKORDER_MERAGE_POD_ERROR_TO;
            }
            else
            {
                rs.Success = true;
            }
            if (rs.Success)
            {
                using (var db = this.DbEntities.DbAdapter)
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    //change wop.uid include  mergefrom  workorderpayload to mergeto
                    var rs1 = this.WorkOrderPodRepository.MergePod(parameter);
                    //after merged pod then arrange payload data (ex. same itemuid & package merge to one record)?
                    //delete mergeform wop.uid

                    var rs2 = this.WorkOrderPodRepository.DeleteWorkOrderPod(new { UID = parameter.Mergefrom.ToArray() });
                    if (!(rs1.AllComplete() && rs2.AllComplete()))
                    {
                        rs1.Message += " " + rs2.Message;
                        db.Rollback();
                    }
                    else
                    {
                        //scope.Complete();
                        db.Commit();
                    }
                    return rs1;
                }
            }
            else
            {
                return rs;
            }

        }

        public IActionResult<bool> RemoveWorkOrder(Guid[] workorderguids)
        {


            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //var deallocatedInfo = this.TicketRepository.GetDeallocatedInfoList(workorderguids);
                var tickets = this.TicketRepository.GetList(new { WorkOrderUID = workorderguids });
                var ticketInfo = this.TicketInfoRepository.GetListByTicket(tickets.Content.Select(p => p.UID).ToArray()).Content;
                var workorders = this.WorkOrderRepository.GetList(new { UID = workorderguids }).Content;
                var workorderpods = this.WorkOrderPodRepository.GetWorkOrderPodList(workorderguids).Content;
                var workorderpayload = this.WorkOrderPayloadRepository.GetList(new { WorkOrderUID = workorderguids }).Content;
                var manifestInfos = this.WorkOrderRepository.GetManifestInfoByWorkOrder(workorders.Select(p => p.UID).ToArray());
                //var tickets = deallocatedInfo.Content.Select(p => p.TicketUID);
                //var ticketInfo = deallocatedInfo.Content.Select(p => p.TicketInfoUID);
                //var vesseluid = deallocatedInfo.Content.Select(p => p.VesselUID);
                //var workorders = deallocatedInfo.Content.Select(p => p.WorkOrderUID);
                //var workorderpods = deallocatedInfo.Content.Where(x => x.WorkOrderPodUID.HasValue)
                //    .Select(p => p.WorkOrderPodUID.Value);
                //var workorderpayload = deallocatedInfo.Content
                //    .Where(x => x.WorkOrderPayloadUID.HasValue)
                //    .Select(p => p.WorkOrderPayloadUID.Value);
                //目前只允許outbound 能夠在ticket 進行中退單
                if ((ticketInfo.All(x => x.Status <= (int)TicketInfoStatus.Open) &&
                    tickets.Content.FirstOrDefault().ManifestType == (int)ManifestType.Inbound)
                    ||
                    (tickets.Content.FirstOrDefault().ManifestType == (int)ManifestType.Outbound)
                    ||
                    (ticketInfo.All(x => x.Status <= (int)TicketInfoStatus.Open) &&
                    tickets.Content.FirstOrDefault().ManifestType == (int)ManifestType.BlukPick))
                {

                    var rs1 = ActionResultTemplates.Result<bool>();
                    var rs2 = ActionResultTemplates.Result<bool>();
                    var rs3 = ActionResultTemplates.Result<bool>();
                    rs1.Success = true;
                    //void ticket
                    if (tickets.Content.Count() > 0)
                    {
                        var parm = new VoidTicketInnerParameters();
                        if (tickets.Content.FirstOrDefault().ManifestType == (int)ManifestType.BlukPick)
                        {
                            parm.WorkOrderUID = workorders.FirstOrDefault().UID;
                        }
                        else
                        {
                            parm.VesselUID = workorders.Select(a => a.VesselUID).ToArray();
                        }
                        //parm.VesselUID = vesseluid.ToArray();

                        rs1 = this.TicketManager.VoidTicket(parm);
                    }
                    //delete workorder pod & payload  if outbound deallocated
                    if (rs1.Success)
                    {
                        if (workorderpods != null && workorderpods.Count() > 0)
                        {
                            rs2 = this.RemoveWorkOrderPod(tickets.Content.First().WarehouseUID,
                                workorderpods.Select(p => p.UID).ToArray(), workorderpayload, tickets.Content, ticketInfo);
                        }
                        //else if (workorderpayload != null && workorderpayload.Count() > 0)
                        //{
                        //    rs2 = this.RemoveWorkOrderPayload(workorderpayload.Select(p => p.UID).ToArray());
                        //}
                        else
                        {
                            rs2 = ActionResultTemplates.Result<bool>();
                            rs2.Success = true;
                        }
                        if (rs2.Success)
                        {
                            //delete workorder
                            rs3 = this.WorkOrderRepository.DeleteWorkOrder(new { UID = workorderguids });
                        }
                    }
                    if (rs1.Success && rs2.Success && rs3.Success)
                    {
                        rs.Success = true;
                        rs.Content = true;

                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = rs1.Message + "\n" + rs2.Message + "\n" + rs3.Message;
                    }

                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_BOL_REJECT_FAILURE_TICKET_WORKING;
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
        public IActionResult<bool> RemoveWorkOrderPayloadFromUI(Guid[] workorderpayloadUID)
        {
            var wpayload = this.WorkOrderPayloadRepository.GetList(new { UID = workorderpayloadUID });
            var workorders = this.WorkOrderRepository.GetList(new { UID = wpayload.Content.Select(p => p.WorkOrderUID) });
            if (workorders.Content.Count() == 0 || workorders.Content.All(p => p.Status > (int)WorkOrderStatus.Open))
            {
                var rs = ActionResultTemplates.OK();
                rs.Success = false;
                rs.Message = string.Format(Resource.MANIFEST_WORKORDER_FAILURE_STATUS, WorkOrderStatus.Open.ToString());
                return rs;
            }
            return this.RemoveWorkOrderPayload(workorderpayloadUID);
        }
        public IActionResult<bool> RemoveWorkOrderPayload(Guid[] workorderpayloadUID)
        {
            //RETEST 改寫deallcated
            if (this.HaveTicket(null, workorderpayloadUID).Success)
            {
                var rs = ActionResultTemplates.Result<bool>();
                rs.Success = rs.Content = false;
                rs.Message = Resource.MANIFEST_WORKORDER_REMOVEPOD_ERROR;
                return rs;
            }
            var rs1 = ActionResultTemplates.Result<bool>();
            var wpayloadInfo = this.WorkOrderPayloadRepository.GetListByUID(workorderpayloadUID);
            if (wpayloadInfo != null && wpayloadInfo.Content.Count() > 0)
            {
                //限制同時只能刪同一個workorder 資料
                if (wpayloadInfo.Content.GroupBy(p => p.WorkOrderUID).Count() == 1)
                {
                    var workorders = this.WorkOrderRepository.GetList(new { UID = wpayloadInfo.Content.First().WorkOrderUID }).Content;
                    var tickets = this.TicketRepository.GetList(new { WorkOrderUID = wpayloadInfo.Content.First().WorkOrderUID });
                    var ticketInfo = this.TicketInfoRepository.GetList(new { TicketUID = tickets.Content.Select(p => p.UID) }).Content;
                    var manifestInfos = this.WorkOrderRepository.GetManifestInfoByWorkOrder(workorders.Select(p => p.UID).ToArray());

                    var rs2 = ActionResultTemplates.Result<bool>();
                    var rs3 = ActionResultTemplates.Result<bool>();
                    var rs4 = ActionResultTemplates.Result<bool>(success: true);
                    rs2 = DeallocatedByWorkOrderPayload(manifestInfos.Content.First().WarehouseUID, wpayloadInfo.Content, tickets.Content, ticketInfo);
                    if (rs2.Success)
                    {
                        rs1 = this.WorkOrderPayloadRepository.DeletePayload(new { UID = workorderpayloadUID });
                        if (rs1.Success)
                        {
                            rs3 = this.LabelRepository.DeleteLabel(wpayloadInfo.Content.Select(p => p.PayloadUID).ToArray());
                            var workorderUID = wpayloadInfo.Content.First().WorkOrderUID;
                            var remainwpl = this.WorkOrderPayloadRepository.GetList(new { workorderUID = workorderUID });
                            var remainwp = this.WorkOrderPodRepository.GetWorkOrderPodList(new Guid[] { workorderUID });
                            if (remainwpl.Content.Count() == 0 && remainwp.Content.Count() == 0)
                            {
                                rs4 = this.WorkOrderRepository.DeleteWorkOrder(new { UID = workorderUID });
                            }
                        }
                    }
                    if (rs1.Success && rs2.Success && rs3.Success && rs4.Success)
                    {

                    }
                    else
                    {
                        rs1.Message += " " + rs2.Message + " " + rs3.Message + " " + rs4.Success;

                    }
                }
                else
                {
                    rs1.Success = false;
                    rs1.Message = "Remove wokrorder not support different workorder at the same time.";
                }
            }
            else
            {
                rs1.Success = false;
                rs1.Message = "Not find workorder Payload data.";
            }

            return rs1;
        }
        public IActionResult<bool> RemoveWorkOrderPodFromUI(Guid[] workorderpodguids)
        {
            var wororderpods = this.WorkOrderPodRepository.GetWorkOrderPodList(new { UID = workorderpodguids });
            var workorderInfos = this.WorkOrderRepository.GetList(new { UID = wororderpods.Content.Select(x => x.WorkOrderUID) });
            if (workorderInfos.Content.Count() == 0 || workorderInfos.Content.All(x => x.Status > (int)WorkOrderStatus.Open))
            {
                var rs = ActionResultTemplates.OK();
                rs.Success = false;
                rs.Message = string.Format(Resource.MANIFEST_WORKORDER_FAILURE_STATUS, WorkOrderStatus.Open.ToString());
                return rs;
            }
            return this.RemoveWorkOrderPod(workorderpodguids);
        }
        public IActionResult<bool> RemoveWorkOrderPod(Guid[] workorderpodguids)
        {
            var rs1 = ActionResultTemplates.Result<bool>();
            if (this.HaveTicket(workorderpodguids).Success)
            {
                var rs = ActionResultTemplates.Result<bool>();
                rs.Success = rs.Content = false;
                rs.Message = Resource.MANIFEST_WORKORDER_REMOVEPOD_ERROR;
                return rs;
            }
            var wpodInfo = this.WorkOrderPodRepository.GetWorkOrderPodList(workorderpodguids);
            var wpayloadInfo = this.WorkOrderPayloadRepository.GetList(new { workorderpodUID = wpodInfo.Content.Select(p => p.UID) });
            var workorders = this.WorkOrderRepository.GetList(new { UID = wpayloadInfo.Content.First().WorkOrderUID }).Content;
            var tickets = this.TicketRepository.GetList(new { WorkOrderUID = wpayloadInfo.Content.First().WorkOrderUID });
            var ticketInfo = this.TicketInfoRepository.GetList(new { TicketUID = tickets.Content.Select(p => p.UID) }).Content;
            var manifestInfos = this.WorkOrderRepository.GetManifestInfoByWorkOrder(workorders.Select(p => p.UID).ToArray());
            if (wpodInfo.Content != null && wpodInfo.Content.Count() > 0)
            {
                if (wpodInfo.Content.GroupBy(p => p.WorkOrderUID).Count() == 1)
                {
                    rs1 = this.RemoveWorkOrderPod(manifestInfos.Content.FirstOrDefault().WarehouseUID,
                       workorderpodguids,
                       wpayloadInfo.Content,
                       tickets.Content,
                       ticketInfo);
                }
                else
                {
                    rs1.Success = false;
                    rs1.Message = "Remove wokrorder not support different workorder at the same time.";
                }
            }
            else
            {
                rs1.Success = false;
                rs1.Message = "Not find workorder pod data.";
            }
            return rs1;

        }

        public IActionResult<bool> RemoveWorkOrderPod(Guid warehouseUID, Guid[] workorderpodguids,
            IEnumerable<IWorkOrderPayloadModel> wpayloadInfo, IEnumerable<ITicketModel> ticketModels
            , IEnumerable<ITicketInfoModel> ticketInfoModels)
        {

            var rs1 = ActionResultTemplates.Result<bool>();


            //var wpayloadUID = this.WorkOrderPayloadRepository.GetList(new { WorkOrderPodUID = workorderpodguids });
            var rs2 = ActionResultTemplates.Result<bool>();
            var rs3 = ActionResultTemplates.Result<bool>(success: true);
            var rs4 = ActionResultTemplates.Result<bool>();
            if (wpayloadInfo.Count() > 0)
                rs3 = DeallocatedByWorkOrderPayload(warehouseUID, wpayloadInfo, ticketModels, ticketInfoModels);
            if (rs3.Success)
            {
                rs1 = this.WorkOrderPodRepository.DeleteWorkOrderPod(workorderpodguids);
                if (rs1.Success)
                {
                    rs2 = this.WorkOrderPayloadRepository.DeletePayloadByUID(wpayloadInfo.Select(p => p.UID));
                    //deallocated clear label ?
                    //if (rs2.Success)
                    //    rs4 = this.LabelRepository.DeleteLabel(wpayloadInfo.Select(p => p.PayloadUID).ToArray());
                }
            }
            if (rs1.Success && rs2.Success && rs3.Success)
            {

            }
            else
            {
                rs1.Message += " " + rs2.Message + " " + rs3.Message + " " + rs4.Message;
                rs1.Success = false;
            }


            return rs1;
        }

        public IActionResult<Guid> SaveAssignedWorkItmes(IAssignedWorkOrderCollection Parameters)
        {
            var rs = ActionResultTemplates.Result<Guid>();
            try
            {
                rs.Success = true;
                if (new int[] { 3 }.Contains(Parameters.StorageMethod) && !Parameters.PodUID.HasValue)
                {
                    rs.Message += Resource.MANIFEST_WORKORDER_MUST_HAVE_POD;
                    rs.Success = false;
                }
                //由payload UID 推斷
                //if (new int[] { 2 }.Contains(Parameters.StorageMethod) && !Parameters.Items.All(p => p.SlotUID.HasValue))
                //{
                //    rs.Message += "must have Slot UID ";
                //    rs.Success = false;
                //}
                if (Parameters.Items.Count == 0)
                {
                    rs.Message += Resource.MANIFEST_WORKORDER_MUST_HAVE_ITEM;
                    rs.Success = false;
                }
                if (Parameters.Items.All(p => p.ReceivePackageQty == 0))
                {
                    //var itemsInfo = this.ProductCacheManager.GetProducts(
                    //    Parameters.Items.Where(x => x.ReceivePackageQty == 0).Select(y => y.ItemUID));
                    rs.Message = string.Format(Resource.MANIFEST_QTY_MORE_THAN_ZERO, "");
                    rs.Success = false;
                }
                if (rs.Success)
                {
                    rs = this.InnerSaveAssignedWorkItmes(Parameters);
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

        public IActionResult<Guid> SaveOutboundAssignedWorkItems(IAssignedOutboundWorkOrderCollection Parameters)
        {
            if (Parameters != null)
            {
                var converter = new AssignedParameterConverter();
                return this.SaveAssignedWorkItmes(converter.OutboundParameterConvert(Parameters));
            }
            else
            {
                var rs = ActionResultTemplates.Result<Guid>();
                rs.Message = Resource.MANIFEST_SAVEOUTBOUNDASSIGNEDITEM_LOST_PARAMETERS;
                return rs;
            }
        }
        public IActionResult<bool> SetLoadingZoneSlot(ISetSlotParameters Parameters)
        {
            if (Parameters.WorkOrderPayloadUID.HasValue && Parameters.WorkOrderPodUID.HasValue)
            {
                var rs2 = ActionResultTemplates.Result<bool>();
                rs2.Message = Resource.MANIFEST_WORKORDER_SETLANDINGSLOT_LOST_PARAMETER;
                return rs2;
            }
            if (Parameters.WorkOrderPayloadUID.HasValue)
            {
                return this.WorkOrderRepository.SetLoadingZoneSlotByWorkOrderPayloadUID(Parameters);
            }
            if (Parameters.WorkOrderPodUID.HasValue)
            {
                return this.WorkOrderRepository.SetLoadingZoneSlotByWorkOrderPodUID(Parameters);
            }

            var rs = ActionResultTemplates.Result<bool>();
            rs.Message = Resource.MANIFEST_WORKORDER_SETLANDINGSLOT_LOST_PARAMETER;
            return rs;
        }

        public IActionResult<bool> SetSlot(ISetSlotParameters Parameters)
        {
            return this.WorkOrderRepository.SetSlot(Parameters);
        }

        public IActionResult<bool> SetWorkOrderPodBarcode(ISetWorkOrderPodBarcodeParameters Parameters)
        {
            return this.WorkOrderRepository.SetWorkOrderPodBarcode(Parameters);
        }
        public IActionResult<bool> SetWorkOrderPodBarcode(IWorkOrderPodModel workOrderPod, string customerBarcode, bool isInbound = true)
        {
            var rs = ActionResultTemplates.Result<bool>();
            if ((workOrderPod.PodUID.HasValue && workOrderPod.Type == (int)StorageMethod.NewPallet))
            {
                SetWorkOrderPodBarcodeInnerParameters parameter = new SetWorkOrderPodBarcodeInnerParameters();
                var model = new GeneratePalletLabelModel();

                parameter.PodUID = workOrderPod.PodUID.Value;
                if (string.IsNullOrEmpty(customerBarcode))
                {
                    var Infos = this.WorkOrderRepository.GetManifestVesselInfo(workOrderPod.UID);
                    if (Infos.Success)
                    {
                        model.SysPon = Infos.Content.Item1.RefNo;
                        model.Notes = workOrderPod.OperationSuggestion;
                        model.ContainerNo = Infos.Content.Item2.RefNo;
                        var _barcode = this.LabelManager.GeneratePodLabel(model, workOrderPod.PodUID.Value);
                        if (_barcode.Success)
                        {
                            parameter.BarcodeUID = _barcode.Content.BarcodeUID;
                            var rs2 = this.WorkOrderRepository.SetWorkOrderPodBarcode(parameter);
                            if (rs2.Success)
                                return rs2;
                            else
                            {
                                this.LabelManager.DeleteLabel(new Guid[] { workOrderPod.PodUID.Value });
                                this.LabelManager.DeleteAttachment(new Guid[] { _barcode.Content.FileUID });
                                return rs2;
                            }
                        }
                        else
                        {
                            rs.Message = Resource.MANIFEST_WORKORDER_SET_POD_LABEL_ERROR;
                            rs.TypeCode = FlowStatusCode.DATA_IS_EXIST;
                            rs.Success = false;

                        }
                    }
                    else
                    {
                        rs.Message = Resource.MANIFEST_WORKORDER_SET_POD_LABEL_ERROR;
                        rs.TypeCode = FlowStatusCode.DATA_IS_EXIST;
                        rs.Success = false;
                    }
                }
                else
                {
                    List<LabelInnerModel> labelInnerModels = new List<LabelInnerModel>();
                    LabelInnerModel idlabel = new LabelInnerModel();
                    idlabel.Type = LabelType.Pallet_Self;
                    idlabel.UID = Guid.NewGuid();
                    idlabel.BelongToType = LabelBelongType.Pod;
                    idlabel.BelongToUID = workOrderPod.PodUID.Value;
                    idlabel.Content = customerBarcode;
                    idlabel.Status = (int)LabelStatus.Active;
                    idlabel.FileUID = Guid.Empty;
                    labelInnerModels.Add(idlabel);
                    if (isInbound)
                    {
                        LabelInnerModel podqtylabel = new LabelInnerModel();
                        podqtylabel.Type = LabelType.Pallet_OrginalTracking;
                        podqtylabel.UID = Guid.NewGuid();
                        podqtylabel.BelongToType = LabelBelongType.Pod;
                        podqtylabel.BelongToUID = workOrderPod.PodUID.Value;
                        podqtylabel.Content = customerBarcode;
                        podqtylabel.Status = (int)LabelStatus.Active;
                        podqtylabel.FileUID = Guid.Empty;
                        labelInnerModels.Add(podqtylabel);
                    }
                    var result = this.LabelManager.AddLabels(labelInnerModels.ToArray());
                    if (result.AllComplete())
                    {
                        parameter.BarcodeUID = idlabel.UID;
                        return this.WorkOrderRepository.SetWorkOrderPodBarcode(parameter);
                    }
                    else
                    {
                        this.LabelManager.DeleteLabel(new Guid[] { idlabel.BelongToUID });

                        return result;
                    }
                }

            }
            else
            {
                rs.Message = Resource.MANIFEST_OPERTAE_FAILURE;
                rs.TypeCode = FlowStatusCode.DATA_NOT_FIND;
                rs.Success = false;
            }
            return rs;
        }
        public IActionResult<bool> SetWorkOrderPodBarcode(Guid WorkOrderPodUID, string customerBarcode)
        {
            var palletLabelModel = new GeneratePalletLabelModel();
            var rs = ActionResultTemplates.Result<bool>();
            var _workOrderPod = this.WorkOrderPodRepository.GetWorkOrderPod(new { UID = WorkOrderPodUID });
            if (_workOrderPod.Success && _workOrderPod.Content != null)
            {
                return this.SetWorkOrderPodBarcode(_workOrderPod.Content, customerBarcode);
            }
            else
            {
                rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER_POD;
                rs.TypeCode = FlowStatusCode.DATA_NOT_FIND;
                rs.Success = false;
            }
            return rs;
        }

        public IActionResult<bool> SetWorkOrderPodBarcode(Dictionary<IWorkOrderPodModel, string> collection)
        {
            var rs = ActionResultTemplates.OK();
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            List<LabelInnerModel> labels = new List<LabelInnerModel>();
            foreach (KeyValuePair<IWorkOrderPodModel, string> item in collection)
            {
                LabelInnerModel label = new LabelInnerModel();
                label.Type = LabelType.Pallet_Self;
                label.UID = Guid.NewGuid();
                label.BelongToType = LabelBelongType.Pod;
                label.BelongToUID = item.Key.PodUID.Value;
                label.Content = item.Value;
                label.Status = (int)LabelStatus.Active;
                label.FileUID = Guid.Empty;
            }

            var result = this.LabelManager.AddLabels(labels.ToArray());
            if (result.AllComplete())
            {
                foreach (KeyValuePair<IWorkOrderPodModel, string> item in collection)
                {
                    var label = labels.FirstOrDefault(p => p.BelongToUID == item.Key.PodUID.Value);
                    SetWorkOrderPodBarcodeInnerParameters parameter = new SetWorkOrderPodBarcodeInnerParameters();
                    parameter.PodUID = item.Key.PodUID.Value;
                    parameter.BarcodeUID = label.UID;
                    Result.Add(this.WorkOrderRepository.SetWorkOrderPodBarcode(parameter));
                }
                if (Result.All(x => x.Success))
                {
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = string.Join(",", Result.Select(x => x.Message));
                }
            }
            else
            {
                result.Success = false;
                result.Message = result.Message;
            }
            return rs;
        }

        internal IWorkOrderAssignAgentParameters GetWorkOrderAgentParameters()
        {
            //TODO 尚未移除Item/Package Manger 相依性
            var parameters = new WorkOrderAssignAgentParameters();
            parameters.AuthenticationInfo = this.AuthProvider.GetAuthenticationInfo();
            parameters.SequenceAgent = this.SequenceAgent;
            parameters.warehouseManger = this.WarehouseManager;
            parameters.WorkOrderPayloadRepository = this.WorkOrderPayloadRepository;
            parameters.WorkOrderPodRepository = this.WorkOrderPodRepository;
            parameters.WorkOrderRepository = this.WorkOrderRepository;
            parameters.ItemManager = this.ItemManager;
            parameters.PackageManager = this.PackageManager;
            parameters.ProductCacheManager = this.ProductCacheManager;
            parameters.PackageCacheManager = this.PackageCacheManager;
            parameters.InventoryManager = this.InventoryManager;
            parameters.WorkOrderManager = this as IWorkOrderManager;
            parameters.BulkPickWorkOrdrPayloadRelationRepository = this.BulkPickWorkOrdrPayloadRelationRepository;
            parameters.VesselManifestRepository = this.VesselManifestRepository;
            parameters.LabelManager = this.LabelManager;
            parameters.PackageUomManager = this.PackageUomManager;
            parameters.VesselRepository = this.VesselRepository;
            //parameters.TransacationScope = this.TransacationScopeModel.Value;
            parameters.TracingAgent = this.TracingAgent;
            return parameters;
        }

        protected IActionResult<bool> DeallocatedByWorkOrderPayload(Guid warehouseUID, IEnumerable<IWorkOrderPayloadModel> wplcollection,
            IEnumerable<ITicketModel> ticketModels, IEnumerable<ITicketInfoModel> ticketInfoModels)
        {

            List<Guid> notrecoveryLabelpayload = new List<Guid>();

            var rs = ActionResultTemplates.Result<bool>();
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            var workorderUIDs = wplcollection.GroupBy(p => p.WorkOrderUID);
            if (ticketModels.All(p => p.ManifestType == (int)ManifestType.Outbound))
            {
                var ticketInBulkPick = GetTickItemInBulkPickList(ticketInfoModels.Select(x => x.UID));
                if (ticketInBulkPick.Content != null && ticketInBulkPick.Content.Count() == 0)
                {
                    var deallocatedpayloadInfo = this.WarehouseManager.FindDeallocatedRelatedPayloadCollection(
                                                                        wplcollection.Select(g => g.PayloadUID));
                    //找出wms_payload 被配置的資料
                    var allocatedplInfos = deallocatedpayloadInfo.Content.AllocatedPayload;
                    var orignalplInfos = deallocatedpayloadInfo.Content.OriginalPayload;
                    //因Change from 多產生的payload集合
                    List<IPayloadModel> orginalplwithextraInfo = new List<IPayloadModel>();
                    var index = 0;
                    var grp = allocatedplInfos.Select(x => x.UID).GroupBy(g => index++ / 2000);
                    foreach (var items in grp)
                    {
                        orginalplwithextraInfo.AddRange(this.PayloadRepository.GetList(new
                        {
                            OriginalPayloadUID = items
                        }).Content);
                    }
                    //var orginalplwithextraInfo = this.PayloadRepository.GetList(new
                    //{
                    //    OriginalPayloadUID = allocatedplInfos.Select(x => x.UID)
                    //});
                    if (allocatedplInfos != null)
                    {

                        List<IDeallocatedParameters> recoverypayload = new List<IDeallocatedParameters>();
                        this.TracingAgent.Debug($"Process in workorder payload TTL:{wplcollection.Count()}");
                        var pindex = 1;
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        foreach (var wpl in wplcollection)
                        {

                            this.TracingAgent.Debug($"process {pindex} data");
                            //當w.payload.originalpayload屬於Change from集合則略過不處理
                            if (orginalplwithextraInfo.Any(p => p.UID == wpl.PayloadUID))
                                continue;
                            var apl = allocatedplInfos.FirstOrDefault(p => p.UID == wpl.PayloadUID);
                            if (apl != null)
                            {
                                //Future Allocated 
                                if (apl.Type == (int)PayloadType.FutureAllocated)
                                {
                                    apl.Status = (int)PayloadStatus.Inactive;
                                    Result.Add(this.WarehouseManager.EditPayload(apl));
                                    continue;
                                }

                                //取得oringalpayload附屬的payload (change from data )
                                var belongorginal = orginalplwithextraInfo.Where(p => apl.UID == p.OriginalPayloadUID);
                                var recoveryqtypayload = belongorginal.Where(p => p.Type == (int)PayloadType.Allocated);
                                var clearpayload = belongorginal.Where(p => p.Type == (int)PayloadType.Stock);
                                foreach (var item in clearpayload)
                                {
                                    this.TracingAgent.Debug($"clear oringalpayload  data");
                                    item.Status = (int)PayloadStatus.Inactive;
                                    Result.Add(this.WarehouseManager.EditPayload(item));
                                }

                                // 如果outbound ticket status >=Glitch 500  需要回補onhand
                                var ticketInfo = ticketInfoModels.FirstOrDefault(p =>
                                p.Type == (int)TicketInfoType.Outbound && wpl.UID == p.WorkOrderPayloadUID);
                                if (ticketInfo != null && ticketInfo.Status >= (int)TicketInfoStatus.Glitch)
                                {
                                    var pl = orignalplInfos.FirstOrDefault(p => p.UID == apl.OriginalPayloadUID.Value);
                                    InsertInventoryParameter iparam = new InsertInventoryParameter();
                                    iparam.ItemUID = apl.ItemUID;
                                    iparam.Qty = apl.Quantity;
                                    iparam.SlotUID = apl.SlotUID;
                                    iparam.TargetPackageUID = apl.PackageUID;
                                    if (pl != null)
                                    {
                                        iparam.Type = (InventoryType)pl.Type;
                                    }
                                    else //如果找不到Originalpayload 只能還stock (特殊狀況)
                                    {
                                        iparam.Type = InventoryType.Stock;
                                    }
                                    iparam.WarehouseUID = warehouseUID;
                                    iparam.UseMiniPackage = true;
                                    Result.Add(this.InventoryManager.InsertInventory(new InsertInventoryParameter[] { iparam }));
                                    this.TracingAgent.Debug($"add onhand  data");



                                }
                                if (apl.OriginalPayloadUID.HasValue)
                                {

                                    var pl = orignalplInfos.FirstOrDefault(p => p.UID == apl.OriginalPayloadUID.Value);
                                    if (pl != null)
                                    {
                                        //找得到原payload 且包裝Slot一致，Type 是onhand
                                        //還原原本的payload
                                        //將allocated payload void
                                        if (pl.PackageUID == wpl.PayloadPackageUID && pl.SlotUID == apl.SlotUID)
                                        {
                                            this.TracingAgent.Debug($"process payload  data");
                                            var pkginfo = this.PackageCacheManager.GetPackage(pl.PackageUID);
                                            pl.Quantity += wpl.Qty + recoveryqtypayload.Sum(p => p.Quantity);
                                            pl.VolumeLimit = this.ProductUtility.CalculateCUFT(pkginfo, pl.Quantity);
                                            pl.WeightLimit = this.ProductUtility.CaculateTTLWeight(pkginfo, pl.Quantity);
                                            pl.Status = (int)PayloadStatus.Active;//原本payload 未必是Active 所以一律改成Active
                                            apl.Status = (int)PayloadStatus.Inactive;
                                            Result.Add(this.LabelManager.ChangeLabelStatusByBelongtoUID(new Guid[] { pl.UID }
                                            , LabelStatus.Active));
                                            Result.Add(this.WarehouseManager.EditPayload(pl));
                                            Result.Add(this.WarehouseManager.EditPayload(apl));
                                            notrecoveryLabelpayload.Add(apl.UID);
                                        }
                                        else //包裝不一致或已經被allocated只能直接還原
                                        {
                                            var aparam = new DeallocatedParameters();
                                            aparam.PayloadUID = wpl.PayloadUID;
                                            aparam.RecoveryPayloadType = pl.Type.Value;
                                            recoverypayload.Add(aparam);

                                        }
                                    }
                                    else //oringal payload 找不到  (特殊狀況)
                                    {
                                        var aparam = new DeallocatedParameters();
                                        aparam.PayloadUID = wpl.PayloadUID;
                                        aparam.RecoveryPayloadType = 999;
                                        recoverypayload.Add(aparam);
                                    }

                                }
                                else //來源payload UID 沒設定 (特殊狀況)
                                {
                                    var aparam = new DeallocatedParameters();
                                    aparam.PayloadUID = wpl.PayloadUID;
                                    aparam.RecoveryPayloadType = 999;
                                    recoverypayload.Add(aparam);
                                }

                            }
                            else //找不到allocated payload info
                            {
                                if (wpl.PayloadUID != Guid.Empty)
                                {
                                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_ALLOCATD_PAYLOAD;
                                    rs.Success = false;
                                }
                                else
                                {
                                    rs.Success = true;
                                }
                                return rs;
                            }
                            pindex++;
                        }
                        sw.Stop();
                        this.TracingAgent.Debug($"Processed complete elapsed {sw.ElapsedMilliseconds} ms");
                        if (recoverypayload.Count > 0)
                        {
                            Result.Add(this.WorkOrderPayloadRepository.DeallcatedByWorkOrderPayload(recoverypayload.ToArray()));
                        }
                        if (Result.All(p => p.Success))//需還原Label
                        {
                            //排除還原回原本onhand payload 的allocated payload UID
                            var recoverylabel = allocatedplInfos.Where(p => !notrecoveryLabelpayload.Contains(p.UID));
                            Result.Add(this.LabelManager.ChangeLabelStatusByBelongtoUID(recoverylabel.Select(p => p.UID)
                                , LabelStatus.Active));
                        }
                        rs.Success = Result.All(r => r.Success);
                        if (!rs.Success)
                        {
                            rs.Message = string.Join(",", Result.Select(p => p.Message));
                        }
                        else
                        {

                        }

                        return rs;

                    }
                    else
                    {
                        rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_PAYLOAD;
                        return rs;
                    }
                }
                else
                {
                    rs.Success = false;
                    if (ticketInBulkPick.Content != null)
                    {
                        rs.Message = string.Format(Resource.TICKET_TICKET_IN_BULKPICK,
                            string.Join(",", ticketInBulkPick.Content));
                    }
                    return rs;
                }
            }
            else if (ticketModels.All(p => p.ManifestType == (int)ManifestType.BlukPick)) //bulkpick 處理
            {
                if (ticketModels.All(p => p.Status <= (int)TicketStatus.Open))
                {
                    //2.刪除payload(真刪)
                    //3.扣除onhand
                    foreach (var item in wplcollection)
                    {
                        Result.Add(this.PayloadRepository.ChangePayloadStauts(item.PayloadUID, PayloadStatus.Inactive));
                    }

                    //4.clear label status 100->0
                    Result.Add(this.LabelManager.ClearLabelByTickets(ticketModels.Select(x => x.UID)));
                    rs.Content = rs.Success = Result.All(x => x.Success);
                    if (!rs.Success)
                    {
                        rs.Message = string.Join(",", Result.Select(x => x.Message));
                    }
                    rs.Success = true;
                    return rs;
                }
                else
                {
                    rs.Message = Resource.MANIFEST_ORDER_BULKPICK_PROCESSING;
                    rs.Content = false;
                    return rs;
                }
            }
            else if (ticketModels.All(p => p.ManifestType == (int)ManifestType.Inbound)) //inbound 處理
            {
                //1.檢查是否已收貨完成，且沒被allocated
                var wPayload = this.WorkOrderPayloadRepository
                                .GetListByUID(wplcollection.Select(x => x.UID));
                var payloadInfo = this.InventoryManager
                                .GetPayload(wPayload.Content.Select(x => x.PayloadUID));
                //比對w.payload & payload count 
                if (wPayload.Content.Count() == payloadInfo.Content.Count())
                {
                    rs.Success = wPayload.Content.Sum(x => x.Qty) == payloadInfo.Content.Sum(p => p.Quantity);
                    if (!rs.Success)
                    {
                        rs.Message = Resource.MANIFEST_ORDER_INBOUND_ROLLBACKTICKET_USED;
                    }
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_ORDER_INBOUND_ROLLBACKTICKET_USED;
                }


                //2.刪除payload(真刪)
                //3.扣除onhand
                var inboundTickets = ticketModels.Where(p =>
               p.Type == (int)TicketType.Receiving &&
               p.Status >= (int)TicketStatus.Processing);
                if (inboundTickets.Count() > 0)
                {
                    var wpods = this.WorkOrderPodRepository
                        .GetWorkOrderPodList(ticketModels.Select(p => p.WorkOrderUID));
                    var wpayload = this.WorkOrderPayloadRepository
                        .GetList(new { WorkOrderUID = ticketModels.Select(x => x.WorkOrderUID) });
                    //real delete pod
                    if (wpods != null && wpods.Content.Where(p => p.PodUID.HasValue).Count() > 0)
                        Result.Add(this.InventoryManager.DeletePodFromDb(
                            new { UID = wpods.Content.Select(p => p.PodUID) }));
                    //real delete payload
                    if (wpayload != null && wpayload.Content.Count() > 0)
                    {
                        Result.Add(this.InventoryManager.DeletePayloadFromDb(
                            new { UID = wpayload.Content.Select(p => p.PayloadUID) }));
                        //deduct onhand
                        foreach (var item in wpayload.Content)
                        {
                            var param = this.InventoryManager.CreateEditInventoryParameters();
                            param.ItemUID = item.ItemUID;
                            param.Onhand = item.Qty * -1;
                            param.SlotUID = item.SlotUID.Value;
                            param.TargetPackageUID = item.PackageUID;
                            param.WarehouseUID = warehouseUID;
                            Result.Add(this.InventoryManager.UpdateInventory(param));
                        }

                    }
                }
                //clear label status 100->0
                Result.Add(this.LabelManager.ClearLabelByTickets(ticketModels.Select(x => x.UID)));
                rs.Content = rs.Success = Result.All(x => x.Success);
                if (!rs.Success)
                {
                    rs.Message = string.Join(",", Result.Select(x => x.Message));
                }
                rs.Success = true;
                return rs;
            }
            else
            {
                rs.Message = "Not support this manifest type deallocated.";
                return rs;
            }
        }

        private IActionResult<Guid> InnerSaveAssignedWorkItmes(IAssignedWorkOrderCollection parameters)
        {

            var rs = ActionResultTemplates.Result<Guid>();
            try
            {
                var _parameters = this.GetWorkOrderAgentParameters();
                var _agent = AbstractWorkOrderAssignAgent.GetAgent(parameters.ServiceType, _parameters);
                var crs = _agent.Execute(parameters);
                rs.Content = crs.Content.WorkOrderUID;
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
        private void ProcessOutboundEstimateQty(IList<ICheckOutboundEstimateItem> estimateList,
            IPackageNode minPkg, IPackageNode node, int freeQty)
        {
            CheckOutboundEstimateItem item = new CheckOutboundEstimateItem();
            item.FreeQty = this.PackageCacheManager.GetReceivePackageUomQuantity(minPkg.UID, node.UID, freeQty).Content;
            item.PackageUID = node.UID;
            item.PackageName = node.Name;
            if (item.FreeQty > 0)
                estimateList.Add(item);
            if (node.Children.Count > 0)
                ProcessOutboundEstimateQty(estimateList, minPkg, node.Children.First(), freeQty);
        }
        private IActionResult<IEnumerable<string>> GetTickItemInBulkPickList(IEnumerable<Guid> ticketInfoUIDs)
        {
            return this.GetBulkPickIDByTicketInfo(ticketInfoUIDs);
        }


    }
}

