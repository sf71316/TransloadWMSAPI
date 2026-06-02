using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.Constant;
using YAEP.WMS.BLL.Model;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.BLL.Model.Parameters;
using System.Transactions;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IVesselManager
    {
        public IActionResult<IEnumerable<IVesselModel>> GetVesselList(IVesselSearchParameters Parameters)
        {
            var collection = this.VesselRepository.GetList(Parameters);
            if (collection.Success)
            {
                foreach (var item in collection.Content)
                {
                    item.StatusName = ((VesselStatus)item.Status).ToString();
                }
            }
            return collection;
        }
        public IActionResult<bool> AddVessel(IVesselModel Model)
        {
            var _seq = this.SequenceAgent.GetVesselSeqence(Model.BolUID);
            Model.ID = _seq;
            return this.VesselRepository.AddVessel(Model);
        }
        public IActionResult<bool> EditVessel(dynamic Model)
        {
            return this.VesselRepository.EditVessel(Model);
        }
        public IActionResult<bool> DeleteVesselAPI(IVesselDeleteParamters Parameters)
        {

            var vesselInfos = this.VesselRepository.GetList(new { UID = Parameters.UID });
            var bolInfos = this.BolRepository.GetList(new { UID = vesselInfos.Content.Select(p => p.BolUID) });
            if (bolInfos.Content.Count() == 0 ||
                bolInfos.Content.All(p => (int)p.Status > (int)BolStatus.Open))
            {
                var rs = ActionResultTemplates.OK();
                rs.Success = false;
                rs.Message = string.Format(Resource.MANIFEST_BOL_STATUS_MUST_LESS_THAN, BolStatus.Open.ToString());
                return rs;
            }
            using (var db = this.DbEntities.DbAdapter)
            {
                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                var rs = this.DeleteVessel(Parameters);
                if (rs.Success)
                {
                    db.Commit();
                }
                else
                {
                    db.Rollback();
                }
                return rs;
            }

        }
        public IActionResult<bool> DeleteVessel(IVesselDeleteParamters Parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var vesselinfo = this.VesselRepository.GetList(new { UID = Parameters.UID });
                var wkinfo = this.WorkOrderRepository.GetList(new { VesselUID = vesselinfo.Content.Select(p => p.UID) });

                IActionResult<bool> rs1 = ActionResultTemplates.Result<bool>();
                IActionResult<bool> rs2 = ActionResultTemplates.Result<bool>();
                IActionResult<bool> rs3 = ActionResultTemplates.Result<bool>();
                rs1 = this.WorkOrderManager.RemoveWorkOrder(wkinfo.Content.Select(x => x.UID).ToArray());
                if (rs1.Success)
                {
                    rs2 = this.VesselRepository.DeleteVessel(Parameters);
                    if (rs2.Success)
                    {
                        rs3 = this.VesselManifestRepository.DeleteVesselManifest(new { VesselUID = Parameters.UID });
                    }
                }
                if (rs1.Success && rs2.Success && rs3.Success)
                {
                    rs.Success = true;

                }
                else
                {
                    rs.Success = false;
                    rs.Message = rs1.Message + " " + rs2.Message + " " + rs3.Message;
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
        public IActionResult<bool> AddVesselManifest(IVesselManifestModel Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<string> Errors = new List<string>();
                if (Model.ItemUID == Guid.Empty || Model.ItemUID == null)
                {
                    Errors.Add($"{nameof(Model.ItemUID)} cannot empty.");
                }
                if (Model.ManifestItemUID == Guid.Empty || Model.ManifestItemUID == null)
                {
                    Errors.Add($"{nameof(Model.ManifestItemUID)} cannot empty.");
                }
                if (Model.PackageUID == Guid.Empty || Model.PackageUID == null)
                {
                    Errors.Add($"ReceivePackage cannot empty.");
                }
                if (Model.VesselUID == Guid.Empty || Model.VesselUID == null)
                {
                    Errors.Add($"{nameof(Model.VesselUID)} cannot empty.");
                }
                if (Model.Qty <= 0)
                {
                    Errors.Add($"{nameof(Model.Qty)} must more than zero.");
                }
                //TODO 檢查qty 是否超過ManifestItem 設定的qty
                var manifestItemInfo = this.ManifestItemListRepository.GetManifestItemInfo(new { UID = Model.ManifestItemUID });
                if (manifestItemInfo.Success)
                {
                    var vesselManifestInfo = this.VesselManifestRepository
                                        .GetList(new { manifestitemUID = manifestItemInfo.Content.ManifestUID });
                    if (vesselManifestInfo.Success)
                    {
                        //var pkgTree = this.PackageManager.GetPackageTree(manifestItemInfo.Content.PackageUID);
                        var miniPkg = this.PackageCacheManager.GetMinPackage(manifestItemInfo.Content.PackageUID);
                        var ttlQty = this.PackageCacheManager.GetReceivePackageUomQuantity(
                            manifestItemInfo.Content.PackageUID, miniPkg.UID, manifestItemInfo.Content.PackageQty.Value).Content;
                        var _addItemQty = vesselManifestInfo.Content.Sum(p => this.PackageCacheManager.GetReceivePackageUomQuantity(p.PackageUID, miniPkg.UID, p.Qty).Content);
                        var assignedQty = this.PackageCacheManager.GetReceivePackageUomQuantity(Model.PackageUID, miniPkg.UID, Model.Qty).Content;
                        if (ttlQty < _addItemQty + assignedQty)
                        {
                            Errors.Add("this item qty is exceed manifest item qty");
                        }
                    }
                    else
                    {
                        Errors.Add(vesselManifestInfo.Message);
                    }
                }
                else
                {
                    Errors.Add(manifestItemInfo.Message);
                }
                if (Errors.Count == 0)
                {
                    // party uid, bol uid 
                    var info = this.VesselManifestRepository.GetPartyBolInfo(Model.VesselUID);
                    if (info.Success)
                    {
                        if (info.Content != null)
                        {
                            Model.PartyUID = info.Content.PartyUID;
                            Model.BolUID = info.Content.BolUID;
                        }
                    }
                    var pkgInfo = this.PackageCacheManager.GetPackage(Model.PackageUID);
                    if (pkgInfo != null)
                    {
                        var _seqence = this.SequenceAgent.GetVesselManifestSequence(Model.VesselUID);
                        Model.ID = _seqence;
                        Model.Weight = this.ProductUtility.CalculateCUFT(pkgInfo, Model.Qty);
                        Model.Volume = this.CalculateVolume(pkgInfo, Model.Qty);
                        rs = this.VesselManifestRepository.AddVesselManifest(Model);
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = Resource.TICKET_LOST_PACKAGE;
                    }
                }
                else
                {
                    rs.Content = rs.Success = false;
                    rs.Message = string.Join("\r\n", Errors);
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
        public IActionResult<bool> DeleteVesselManifestFromUI(IVesselManifestDeleteParameters Parameters)
        {
            var vesselManifestInfos = this.VesselManifestRepository.GetList(new { UID = Parameters.UID });
            var vesselInfos = this.VesselRepository.GetList(new { UID = vesselManifestInfos.Content.Select(p => p.VesselUID) });
            if (vesselInfos.Content.Count() == 0 || vesselInfos.Content.All(x => x.Status > (int)VesselStatus.Open))
            {
                var rs = ActionResultTemplates.OK();
                rs.Success = false;
                rs.Message = string.Format(Resource.MANIFEST_VESSEL_STATUS_FAILURE, VesselStatus.Open.ToString());
                return rs;
            }
            return this.DeleteVesselManifest(Parameters);
        }
        public IActionResult<bool> DeleteVesselManifest(IVesselManifestDeleteParameters Parameters)
        {
            var wpayloadInfo = this.WorkOrderPayloadRepository.GetList(new { VesselManifestUID = Parameters.UID });
            if (wpayloadInfo.Content == null || (wpayloadInfo.Content != null && wpayloadInfo.Content.Count() == 0))
            {
                return this.VesselManifestRepository.DeleteVesselManifest(Parameters);
            }
            else
            {

                var rs = ActionResultTemplates.Result<bool>();
                rs.Success = false;
                rs.Message = Resource.MANIFEST_VESSEL_DELETE_EXIST_ASSIGNED_WPAYLOAD;
                return rs;
            }
        }
        public IActionResult<IEnumerable<IVesselManifestItemListViewModel>> GetVesselManifestItemList(IVesselManifestSearchParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IVesselManifestItemListViewModel>>();
            try
            {
                var collection = this.VesselManifestRepository.GetVesselManifestItemList(Parameters).Content;
                var pitems = this.ProductCacheManager.GetItems(collection.Select(p => p.ItemUID));
                foreach (var item in collection)
                {
                    var pitem = pitems.FirstOrDefault(p => p.UID == item.ItemUID);
                    var pkg = this.PackageCacheManager.GetPackage(item.PackageUID);
                    if (pitem != null)
                    {
                        item.ItemName = pitem.Name;
                        item.ItemID = pitem.ID;
                    }
                    if (pkg != null)
                        item.PackageName = pkg.Name;
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
            }
            return rs;
        }
        public IActionResult<IEnumerable<IUnAssignedListViewModel>> GetUnAssignedList(IVesselManifestSearchParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IUnAssignedListViewModel>>();
            try
            {
                List<IUnAssignedListViewModel> Result = new List<IUnAssignedListViewModel>();
                // get vessel item list
                var _vesselItemList = this.VesselManifestRepository.GetList(parameters).Content;
                // get workorder assinged item list
                var _assginedItemList = this.WorkOrderPayloadRepository.GetAssignWorkOrderItemList(parameters.VesselUID.Value).Content;
                // each of item  calculate remain qty
                var pitems = this.ProductCacheManager.GetItems(_vesselItemList.Select(p => p.ItemUID));
                foreach (var item in _vesselItemList)
                {
                    var r = new UnAssignedListViewInnerModel();
                    var _itemInfo = pitems.FirstOrDefault(p => p.UID == item.ItemUID);
                    var _packagetree = this.PackageCacheManager.GetPackageTree(item.PackageUID);
                    var _m_minipkg = this.PackageCacheManager.GetMinPackage(item.PackageUID);
                    var _assignedItem = _assginedItemList.Where(p => p.VesselManifestUID == item.UID);
                    var _a_minipkgQtySum = _assignedItem.Sum(p =>
                       this.PackageCacheManager.GetReceivePackageUomQuantity(p.PackageUID, _m_minipkg.UID, p.PackageQty).Content);
                    var _pkg = this.PackageCacheManager.GetPackage(item.PackageUID);
                    if (_assignedItem != null && _assignedItem.Count() > 0)
                    {
                        var _apkg = this.PackageCacheManager.GetPackage(_assignedItem.FirstOrDefault().PackageUID);
                        if (_apkg != null)
                        {
                            //    r.ReceivePackageName = _apkg.Name;
                            r.Weight = this.ProductUtility.CalculateCUFT(_apkg, _assignedItem.Sum(p => p.PackageQty));
                        }
                        r.ReceivePackageQty = _assignedItem.Sum(p => p.PackageQty);
                    }
                    r.Volume = this.ProductUtility.CalculateCUFT(_pkg, item.Qty);
                    r.Weight = this.ProductUtility.CaculateTTLWeight(_pkg, item.Qty);
                    r.ItemUID = item.ItemUID;
                    //TODO ???
                    r.VesselMainifestUID = item.UID;
                    r.ItemID = _itemInfo.ID;
                    r.PackPackageUID = item.PackageUID;
                    r.ManifestType = item.ManifestType;

                    r.PackageQty = item.Qty;
                    if (_pkg != null)
                    {
                        r.PackPackageName = _pkg.Name;
                        r.ReceivePackageName = _pkg.Name;
                    }

                    r.ReceivePackageUID = r.PackPackageUID;// _assignedItem.FirstOrDefault().PackageUID;
                    //r.ReceivePackageQty = _v_minipkgQtySum - _a_minipkgQtySum;
                    r.Volume = this.CalculateVolume(_pkg, r.PackageQty);

                    if (_packagetree != null)
                        ProcessPackageEstimateQty(r.EstimateQtyList, r.PackPackageUID, r.PackageQty, _packagetree.Root, _m_minipkg, _a_minipkgQtySum);
                    var allocatedinfo = r.EstimateQtyList.FirstOrDefault(x => x.PackPackageUID == r.PackPackageUID);
                    if (allocatedinfo != null)
                    {
                        r.AllocatedQty = allocatedinfo.AllocatedQty;
                        r.FreeQty = allocatedinfo.TTLQty - allocatedinfo.AllocatedQty;
                    }
                    Result.Add(r);
                }

                rs.Content = Result;
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
        public IActionResult<IEnumerable<IVesselAddItemListVewModel>> GetAddItemList(IGetAddItemListparameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IVesselAddItemListVewModel>>();
            try
            {
                if (!parameters.manifestuid.HasValue)
                {
                    var manifestInfo = this.WorkOrderRepository.GetManifestInfo(parameters.vesseluid.Value);
                    if (manifestInfo.Success)
                    {
                        parameters.manifestuid = manifestInfo.Content.UID;
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA;
                    }
                }
                List<VesselAddItemListInnerVewModel> Result = new List<VesselAddItemListInnerVewModel>();
                // get manifest item list
                var _manifestItemList = this.GetManifestItemListByGroupItem(parameters).Content;
                // get vessel assinged item list
                var _assginedVesslItemList = this.VesselManifestRepository.GetVesselAssignItemList(parameters).Content;
                // each of item  calculate remain qty
                var pitems = this.ProductCacheManager.GetItems(_manifestItemList.Select(p => p.ItemUID));
                foreach (var item in _manifestItemList)
                {
                    var r = new VesselAddItemListInnerVewModel();
                    //var _itemgrossmweight = this.ItemManager.GetProperty(item.ItemUID, ItemPropertyFields.PRODUCT_GROSS_WEIGHT_FIELDNAME);
                    var _itemInfo = pitems.FirstOrDefault(p => p.UID == item.ItemUID);
                    var _pkg = this.PackageCacheManager.GetPackage(item.PackageUID);
                    var _packagetree = this.PackageCacheManager.GetPackageTree(item.PackageUID);
                    //var _m_minipkg = _packagetree.Content.GetMinPackage();
                    var _m_minipkg = this.PackageCacheManager.GetMinPackage(item.PackageUID);
                    //var _m_minipkgQtySum = this.PackageManager.GetReceivePackageUomQuantity(
                    //    item.PackageUID, _m_minipkg.UID, item.PackageQty).Content;

                    var _assignedItem = _assginedVesslItemList.Where(p => p.ManifestItemUID == item.ManifestItemUID);
                    var _a_minipkgQtySum = _assignedItem.Sum(p =>
                        this.PackageCacheManager.GetReceivePackageUomQuantity(p.PackageUID, _m_minipkg.UID, p.PackageQty).Content);
                    r.ItemUID = item.ItemUID;
                    r.ItemID = _itemInfo.ID;
                    r.ManifestItemUID = item.ManifestItemUID;
                    r.PackPackageUID = item.PackageUID;
                    r.PackageQty = item.PackageQty;
                    var pkg = this.PackageCacheManager.GetPackage(r.PackPackageUID);
                    if (pkg != null)
                        r.PackPackageName = pkg.Name;
                    if (_packagetree != null)
                        ProcessPackageEstimateQty(r.EstimateQtyList, r.PackPackageUID, r.PackageQty, _packagetree.Root, _m_minipkg, _a_minipkgQtySum);
                    r.Volume = this.CalculateVolume(_pkg, r.PackageQty);
                    var allocatedinfo = r.EstimateQtyList.FirstOrDefault(x => x.PackPackageUID == r.PackPackageUID);
                    if (allocatedinfo != null)
                    {
                        r.AllocatedQty = allocatedinfo.AllocatedQty;
                        r.FreeQty = allocatedinfo.TTLQty - allocatedinfo.AllocatedQty;
                    }
                    if (r.EstimateQtyList.Count() > 0)
                        Result.Add(r);
                }


                rs.Content = Result;
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
        public IActionResult<bool> ChangeVesselStatus(Guid VesselUID, VesselStatus status, VesselManifestStatus vesselManifestStatus)
        {
            var rs1 = this.VesselRepository.ChangeVesselStatus(VesselUID, status);
            var rs2 = this.VesselManifestRepository.ChangeVesselManifestStatus(VesselUID, vesselManifestStatus);
            rs1.Content &= rs2.Content;
            rs1.Success &= rs2.Success;
            return rs1;
        }
        private void ProcessPackageEstimateQty(ICollection<IPackageEstimateQtyList> estimateQtyList,
            Guid PackPackageUID, int PackageQty,
            IPackageNode node, IPackageNode _m_minipkg, int a_minipkgQtySum)
        {
            //TODO不考慮平行包裝的問題
            PackageEstimateQtyListInnerModel model = new PackageEstimateQtyListInnerModel();
            model.PackPackageUID = node.UID;
            model.PackPackageName = node.Name;
            var ttlqty = this.PackageCacheManager.GetReceivePackageUomQuantity(PackPackageUID, node.UID, PackageQty);
            if (ttlqty.Success)
                model.TTLQty = ttlqty.Content;
            if (node.UID == _m_minipkg.UID)
            {
                model.AllocatedQty = a_minipkgQtySum;
            }
            else
            {
                if (a_minipkgQtySum > 0)
                {
                    var uomqty = this.PackageCacheManager.GetReceivePackageUomQuantity(node.UID, _m_minipkg.UID, 1).Content;
                    model.AllocatedQty = (int)Math.Ceiling((decimal)a_minipkgQtySum / uomqty);
                }
            }
            model.FreeQty = model.TTLQty - model.AllocatedQty;
            if (model.TTLQty > 0 && model.FreeQty > 0)
                estimateQtyList.Add(model);
            if (node.Children.Count > 0)
            {
                ProcessPackageEstimateQty(estimateQtyList, PackPackageUID, PackageQty, node.Children.First(), _m_minipkg, a_minipkgQtySum);
            }
        }

        public IActionResult<IEnumerable<dynamic>> GetOutboundUnAssignedList(Guid vesselUID)
        {
            //PackageEstimateQtyListInnerModel model = new PackageEstimateQtyListInnerModel();
            var rs = ActionResultTemplates.Result<IEnumerable<dynamic>>();
            IActionResult<IEnumerable<IOutboundAllocatedItemModel>> allocatedCollection =
                this.WorkOrderRepository.GetOutboundAllocatedList(vesselUID);
            IActionResult<IEnumerable<IOutboundUnAssignedListModel>> vesselCollection =
                this.WorkOrderRepository.GetOutboundUnAssignedList(vesselUID);
            if (vesselCollection.Success)
            {
                //OutboundUnAssignedListInnerModel
                var group = vesselCollection.Content.GroupBy(g => new
                {
                    ItemUID = g.ItemUID,
                    ItemID = g.ItemID,
                    PickPackageUID = g.PickPackageUID,
                    PickQty = g.PickQty,
                    VesselManifestUID = g.UID
                });
                var itemlist = vesselCollection.Content.GroupBy(p => p.ItemUID).Select(p => p.Key).ToList();
                //ItemInnerParameterize _parameters = new ItemInnerParameterize();
                //_parameters.ListOfItemUID = itemlist;
                var itemInfos = this.ProductCacheManager.GetItems(itemlist);

                if (allocatedCollection.Success && allocatedCollection.Content.Count() > 0)
                {
                    foreach (var item in allocatedCollection.Content)
                    {
                        var _pkg = this.PackageCacheManager.GetPackage(item.PickPackageUID);
                        if (itemInfos != null)
                        {
                            var itemInfo = itemInfos.FirstOrDefault(p => p.UID == item.ItemUID);
                            if (itemInfo != null)
                            {
                                item.ItemID = itemInfo.ID;
                            }
                        }
                        if (_pkg != null)
                        {
                            item.PickPackageName = _pkg.Name;
                        }
                    }
                }
                foreach (var grp in group)
                {
                    var _pkg = this.PackageCacheManager.GetPackage(grp.Key.PickPackageUID);

                    foreach (var item in grp)
                    {
                        var _itemInfo = this.ProductCacheManager.GetItem(item.ItemUID);
                        // var _packagetree = this.PackageManager.GetPackageTree(item.PickPackageUID);
                        //var _m_minipkg = _packagetree.Content.MiniPackage();
                        //var _a_minipkgQtySum = grp.Sum(p =>
                        //   this.PackageManager.GetReceivePackageUomQuantity(p.PickPackageUID, _m_minipkg.UID, allocatedQty).Content);
                        if (_pkg != null)
                        {
                            item.PickPackageName = _pkg.Name;
                            item.Weight = this.ProductUtility.CalculateCUFT(_pkg, item.PickQty);
                        }
                        if (_itemInfo != null)
                            item.ItemID = _itemInfo.Name;
                    }
                }
                rs.Success = true;
                rs.Content = group.Select(
                    gp =>
                    {
                        var e = new OutboundUnAssignedGroupModel
                        {
                            UID = Guid.NewGuid(),
                            VesselManifestUID = gp.Key.VesselManifestUID,
                            ItemID = gp.Key.ItemID,
                            ItemUID = gp.Key.ItemUID,
                            PickQty = gp.Key.PickQty,
                            PickPackageUID = gp.Key.PickPackageUID,
                            PickPackagename = gp.First().PickPackageName,
                            Items = allocatedCollection.Content.Where(p => p.VesselManifestUID == gp.Key.VesselManifestUID)
                        };
                        e.AllocatedQty = e.Items.Sum(p => p.PickQty);
                        e.FreeQty = e.PickQty - e.AllocatedQty;
                        return e;
                    }
                );
            }
            else
            {
                rs.Success = false;
                rs.Message = vesselCollection.Message;
            }

            return rs;
        }
        public IActionResult<IEnumerable<ILocationItemViewModel>>
            GetAvailableInventoryData(IGetAvailableInventoryDataInnerListParameters request)
        {
            var rs = this.WarehouseRepository.GetAvailableInventoryList(request);
            if (request.IsincludeReceivingQty)
            {
                if (rs.Content.Count() > 0)
                {
                    var checkparam = new CheckPodBarcodeInfoParameters();
                    checkparam.BelongToUID = rs.Content.Select(p => p.PodUID);
                    checkparam.LabelType = (int)LabelType.Pallet_OrginalTracking;
                    var belonglabels = this.GetPodBarcodeInfo(checkparam);
                    if (belonglabels.Content != null)
                    {
                        foreach (var item in rs.Content)
                        {
                            var belonglabel = belonglabels.Content
                                .Where(p => p.BelongToUID == item.PodUID);
                            if (belonglabel != null)
                            {
                                item.Labels = belonglabel.Select(p => new TicketLabelInnerModel
                                {
                                    Barcode = p.Barcode,
                                    AttachmentUID = Guid.Empty,
                                    BarcodeType = (int)p.Type,
                                    BarcodeTypeName = p.Type.ToString(),
                                    BelongToType = (int)p.BelongToType,
                                    BelongToUID = p.BelongToUID,
                                    Status = p.Status,
                                    StatusName = ((LabelStatus)p.Status).ToString(),
                                    AddQty = p.Qty
                                } as ITicketLabelViewModel);
                            }
                        }
                    }
                }
            }
            return rs;
        }

        public IActionResult<IEnumerable<ILocationItemViewModel>> GetAvailableInventoryList(IGetAvailableInventoryListParameters request)
        {


            GetAvailableInventoryInnerListParameters param = new GetAvailableInventoryInnerListParameters();
            param.AreaUID = request.AreaUID;
            param.BinUID = request.BinUID;
            param.SlotUID = request.SlotUID;
            param.Items.Add((int)PayloadType.Stock, new Guid[] { request.ItemUID });
            param.OptionText = request.OptionText;
            param.OptionValue = request.OptionValue;
            param.WarehouseUID = request.WarehouseUID;
            var rs = ActionResultTemplates.Result<IEnumerable<ILocationItemViewModel>>();
            List<ILocationItemViewModel> resultCollection = new List<ILocationItemViewModel>();
            try
            {
                var targetPkg = this.PackageCacheManager.GetPackage(request.PackageUID);
                if (targetPkg != null)
                {
                    if (request.VesselManifestUID.HasValue && !param.WarehouseUID.HasValue)
                    {
                        IActionResult<IManifestModel> mInfo = this.VesselManifestRepository.GetManifestInfo(request.VesselManifestUID.Value);
                        if (mInfo.Content != null)
                        {
                            param.WarehouseUID = mInfo.Content.WarehouseUID;
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = Resource.MANIFEST_NOT_FIND_VESSEL_DATA;
                        }
                    }
                    var collection = this.WarehouseRepository.GetAvailableInventoryList(param);
                    if (collection.Success)
                    {

                        foreach (var source in collection.Content)
                        {

                            var o_pkgInfo = this.PackageCacheManager.GetPackage(source.OriginalPackageUID);
                            if (o_pkgInfo.VersionUID == targetPkg.VersionUID)
                            {
                                LocationItemInnerViewModel item = new LocationItemInnerViewModel(source);
                                item.PackageUID = request.PackageUID;
                                item.PayloadUID = item.UID;
                                item.VesselManifestUID = request.VesselManifestUID.Value;
                                var itemInfo = this.ProductCacheManager.GetItem(item.ItemUID);
                                //var pkgTree = this.PackageManager.GetPackageTree(item.PackageUID);

                                if (itemInfo != null)
                                {
                                    item.ItemID = itemInfo.ID;
                                }
                                if (targetPkg != null)
                                {
                                    item.WeightLimit = this.ProductUtility.CalculateCUFT(targetPkg, item.Quantity);
                                    item.PackageName = targetPkg.Name;
                                }
                                if (o_pkgInfo != null)
                                {
                                    item.OriginalPackageName = o_pkgInfo.Name;
                                }
                                if (targetPkg.UID != item.OriginalPackageUID)
                                {
                                    var packageQty = this.PackageCacheManager.GetReceivePackageUomQuantity(
                                                     item.OriginalPackageUID, targetPkg.UID, item.Quantity);

                                    if (packageQty.Success)
                                    {
                                        item.PackageQty = packageQty.Content;
                                    }
                                    else
                                    {
                                        item.PackageQty = 0;
                                        collection.Message += $"PayloadUID:{item.UID} insufficient quantity";
                                    }
                                }
                                else
                                {
                                    item.PackageQty = item.Quantity;
                                }
                                if (item.PackageQty > 0)
                                    resultCollection.Add(item);
                            }
                        }

                    }
                    collection.Content = resultCollection;
                    return collection;
                }
                else
                {


                    rs.Message = Resource.MANIFEST_VESSEL_NOT_FIND_BELONGTO_PKG;
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    return rs;
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message + "\n" + ex.StackTrace;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                return rs;
            }

        }

        public IActionResult<IVesselModel> GetVessel(Guid vesselUID)
        {
            if (vesselUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult<IVesselModel>(nameof(vesselUID));
            }

            var result = this.VesselRepository.GetData(new { UID = vesselUID });

            return result;
        }

        public IActionResult<int> GetUnassignedVesslManifestCount(Guid vesselUID)
        {
            var rs = ActionResultTemplates.Result<int>();
            try
            {
                var param = new VesselManifestSearchInnerParameters();
                param.VesselUID = vesselUID;
                var vidata = this.VesselManifestRepository.GetList(param);
                if (vidata.Success && vidata.Content.Count() > 0)
                {
                    var pdata = this.WorkOrderPayloadRepository.GetList(new { VesselManifestUID = vidata.Content.Select(p => p.UID) });
                    var pitems = this.ProductCacheManager.GetItems(vidata.Content.Select(p => p.ItemUID));
                    foreach (var item in vidata.Content)
                    {
                        //總計Vessel Manifest 總數
                        var vpkg = this.PackageCacheManager.GetPackageTree(item.PackageUID);
                        var itemInfo = pitems.First(p => p.UID == item.ItemUID);
                        //var vptree = vpkg.Content.GetMinPackage();
                        var vpminpkg = this.PackageCacheManager.GetMinPackage(item.PackageUID);
                        var ttlQty = this.PackageCacheManager.GetReceivePackageUomQuantity(item.PackageUID,
                            vpminpkg.UID, item.Qty).Content;
                        //計算被assigned 總數
                        var assignedpl = pdata.Content.Where(p => p.VesselManifestUID == item.UID);
                        var assignedTtlQty = assignedpl.Sum(s =>
                        {
                            //var apkg = this.PackageManager.GetPackageTree(s.PackageUID);
                            var aptree = this.PackageCacheManager.GetMinPackage(s.PackageUID);
                            return this.PackageCacheManager.GetReceivePackageUomQuantity(s.PackageUID, aptree.UID, s.Qty).Content;
                        });
                        if (ttlQty - assignedTtlQty != 0)
                        {
                            rs.Content += 1;
                        }
                    }
                    rs.Success = true;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_VESSEL;
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

        public IActionResult<IEnumerable<IVesselManifestModel>> GetVesselManifestByBol(IEnumerable<Guid> condition)
        {
            return this.VesselManifestRepository.GetListByBol(condition);
        }
        public IActionResult<IEnumerable<IVesselManifestModel>> GetVesselManifest(object condition)
        {
            return this.VesselManifestRepository.GetList(condition);
        }

        IActionResult<IManifestModel> IVesselManager.GetManifestInfo(Guid vesselUID)
        {
            return this.WorkOrderRepository.GetManifestInfo(vesselUID);
        }
        public IActionResult<IEnumerable<IPodBarcodeInfo>> GetPodBarcodeInfo(ICheckPodBarcodeInfoParameters parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPodBarcodeInfo>>();
            try
            {
                parameters.LabelType = (int)LabelType.Pallet_OrginalTracking;
                rs = this.WorkOrderPayloadRepository.GetPodBarcodeInfo(parameters);
                foreach (var item in rs.Content)
                {
                    var minPkg = this.PackageCacheManager.GetMinPackage(item.PackageUID);
                    if (minPkg != null)
                    {
                        var minPkgQty = this.PackageCacheManager.GetReceivePackageUomQuantity(item.PackageUID, minPkg.UID, item.Qty);
                        if (minPkgQty.Success)
                        {
                            item.Qty = minPkgQty.Content;
                        }
                    }
                    else
                    {
                        item.Qty = 0;
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
    }
}
