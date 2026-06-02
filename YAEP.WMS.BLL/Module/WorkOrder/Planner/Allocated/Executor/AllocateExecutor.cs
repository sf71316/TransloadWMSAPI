using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;
using System.Collections.Concurrent;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class AllocateExecutor
    {
        IAllocateExecutorParameters _Managers;
        public AllocateExecutor(IAllocateExecutorParameters parameters)
        {
            _Managers = parameters;
        }
        public IActionResult<ConcurrentStack<Func<IActionResult<bool>>>> GetAllcoateFunc(
            IEnumerable<IWorkOrderPayloadModel> workOrderpayloads, bool IsRegular = true)
        {

            var rs = ActionResultTemplates.Result<ConcurrentStack<Func<IActionResult<bool>>>>();
            try
            {
                rs.Success = true;
                ConcurrentStack<Func<IActionResult<bool>>> funcs = new ConcurrentStack<Func<IActionResult<bool>>>();
                var _payloads = this._Managers.InventoryManager.GetPayload(
                                                workOrderpayloads.Select(p => p.PayloadUID).ToArray());
                List<IPayloadModel> newpayloads = new List<IPayloadModel>();
                if (_payloads.Success) //TODO 慮掃描方式決定判斷方式
                {
                    var _pod = this._Managers.InventoryManager.GetPod(_payloads.Content.Select(p => p.PODUID).ToArray());
                    foreach (var item in workOrderpayloads)
                    {
                        if (IsRegular && item.Type == (int)WorkOrderPayloadType.Allocated)
                        {
                            var _pl = _payloads.Content.FirstOrDefault(p => p.UID == item.PayloadUID);
                            IPodModel _pd = null;
                            if (_pod.Content != null && _pod.Content.Count() > 0)
                                _pd = _pod.Content.FirstOrDefault(p => p.UID == _pl.PODUID);
                            //check pkg  & qty
                            if (item.PackageUID == _pl.PackageUID)
                            {
                                if (item.Qty <= _pl.Quantity)
                                {
                                    _pl.Quantity -= item.Qty;
                                    if (_pl.Quantity == 0)
                                        _pl.Status = (int)PayloadStatus.Inactive;
                                    //alocated data
                                    var _clone = _pl.Clone<IPayloadModel>();//IPayloadModel
                                                                            //var _cpkg = this._Managers.PackageManager.GetPackage(_clone.PackageUID).Content;
                                    var _cpkg = this._Managers.PackageMappingCache.GetPackage(_clone.PackageUID);
                                    _clone.UID = Guid.NewGuid();
                                    _clone.Type = (int)PayloadType.Allocated;
                                    _clone.Status = (int)PayloadStatus.Active;
                                    _clone.Quantity = item.Qty;
                                    _clone.OriginalPayloadUID = _pl.UID;
                                    _clone.VolumeLimit = this._Managers.ProductUtility.CalculateCUFT(_cpkg, _clone.Quantity);
                                    _clone.WeightLimit = this._Managers.ProductUtility.CaculateTTLWeight(_cpkg, _clone.Quantity);

                                    _pl.VolumeLimit = this._Managers.ProductUtility.CalculateCUFT(_cpkg, _pl.Quantity);
                                    _pl.WeightLimit = this._Managers.ProductUtility.CaculateTTLWeight(_cpkg, _pl.Quantity);
                                    newpayloads.Add(_clone);

                                    //clone source label to new payload 
                                    funcs.Push(() => this._Managers.LabelManager.CloneLabel(_pl.UID, _clone.UID));
                                    funcs.Push(() => this._Managers.InventoryManager.UpdatePayload(_pl));
                                    funcs.Push(() => this._Managers.InventoryManager.AddPayload(_clone));
                                    // modified workorder payload payloaduid
                                    funcs.Push(() => this._Managers.WorkOrderPayloadRepository.ChangePayload(item.UID, _clone.UID));

                                }
                                else
                                {
                                    rs.Success = false;
                                    rs.Message = "Onhand has use,can't allocated.";
                                    this._Managers.TracingAgent.Trace("Onhand has use,can't allocated.", item);
                                }
                            }
                            else
                            {
                                // if payload have pod => unpack 
                                if (_pd != null && _pd.IsPack)
                                {
                                    _pd.IsPack = false;
                                    funcs.Push(() => this._Managers.InventoryManager.UnPack(_pd.UID));
                                }

                                IActionResult<int> _onhand = null;
                                bool _issameVer = true;
                                //down to W.payload package
                                //判斷package 是否為同版本
                                var plpkg = this._Managers.PackageMappingCache.GetPackage(_pl.PackageUID);
                                var alpkg = this._Managers.PackageMappingCache.GetPackage(item.PackageUID);
                                if (plpkg.VersionUID == alpkg.VersionUID)
                                {
                                    _onhand = this._Managers.PackageMappingCache.GetReceivePackageUomQuantity(_pl.PackageUID, item.PackageUID, _pl.Quantity);
                                }
                                else
                                {
                                    _issameVer = false;
                                }
                                if (_issameVer)
                                {
                                    if (_onhand.Success && _onhand.Content > 0)
                                    {

                                        _onhand.Content -= item.Qty;
                                        var _clone = _pl.Clone<IPayloadModel>();
                                        _clone.UID = Guid.NewGuid();
                                        _clone.Status = (int)PayloadStatus.Active;
                                        _clone.Type = (int)PayloadType.Allocated;
                                        _clone.Quantity = item.Qty;

                                        _clone.PackageUID = item.PackageUID;
                                        _clone.OriginalPayloadUID = _pl.UID;

                                        //原payload 降至與allocated 同階包裝
                                        if (_onhand.Content > 0)
                                        {
                                            _pl.PackageUID = item.PackageUID;
                                            _pl.Quantity = _onhand.Content;
                                        }
                                        else
                                        {
                                            _pl.Quantity = 0;
                                            _pl.Status = (int)PayloadStatus.Inactive;
                                        }
                                        newpayloads.Add(_clone);
                                        //clone source label to new payload 
                                        funcs.Push(() => this._Managers.LabelManager.CloneLabel(_pl.UID, _clone.UID));
                                        //allocated data
                                        funcs.Push(() => this._Managers.InventoryManager.UpdatePayload(_pl));
                                        funcs.Push(() => this._Managers.InventoryManager.AddPayload(_clone));
                                        // modified workorder payload payloaduid
                                        funcs.Push(() => this._Managers.WorkOrderPayloadRepository.ChangePayload(item.UID, _clone.UID));



                                    }
                                    else
                                    {
                                        //目前可能不會被執行，當Allocated 執行的階段(已啟用Snapshot scope)當目前Payload 資料被其它程序修改時會有Exception
                                        rs.Success = false;
                                        rs.Message = "Onhand has use,can't allocated.";
                                        this._Managers.TracingAgent.Trace("Onhand has use,can't allocated.", item);
                                    }
                                }
                                else
                                {

                                    var plRatio = this._Managers.PackageMappingCache
                                       .GetReceivePackageUomQuantity(plpkg.UID,
                                       this._Managers.PackageMappingCache.GetMinPackage(plpkg.UID).UID, 1);
                                    var plminqty = this._Managers.PackageMappingCache
                                       .GetReceivePackageUomQuantity(plpkg.UID,
                                       this._Managers.PackageMappingCache.GetMinPackage(plpkg.UID).UID,
                                       _pl.Quantity);
                                    var apminqty = this._Managers.PackageMappingCache
                                      .GetReceivePackageUomQuantity(item.PackageUID,
                                      this._Managers.PackageMappingCache.GetMinPackage(item.PackageUID).UID,
                                      item.Qty);
                                    _pl.Quantity = (plminqty.Content - apminqty.Content) / plRatio.Content;
                                    var _clone = _pl.Clone<IPayloadModel>();
                                    _clone.UID = Guid.NewGuid();
                                    _clone.Status = (int)PayloadStatus.Active;
                                    _clone.Type = (int)PayloadType.Allocated;
                                    _clone.Quantity = item.Qty;
                                    //2020-6-10 討論後不修改原本payload上的包裝
                                    //_clone.PackageUID = item.PackageUID;
                                    _clone.OriginalPayloadUID = _pl.UID;
                                    if (_pl.Quantity == 0)
                                    {
                                        _pl.Quantity = 0;
                                        _pl.Status = (int)PayloadStatus.Inactive;
                                    }
                                    if (_pl.Quantity >= 0)
                                    {
                                        newpayloads.Add(_clone);
                                        //clone source label to new payload 
                                        funcs.Push(() => this._Managers.LabelManager.CloneLabel(_pl.UID, _clone.UID));
                                        //allocated data
                                        funcs.Push(() => this._Managers.InventoryManager.UpdatePayload(_pl));
                                        funcs.Push(() => this._Managers.InventoryManager.AddPayload(_clone));
                                        // modified workorder payload payloaduid
                                        funcs.Push(() => this._Managers.WorkOrderPayloadRepository.ChangePayload(item.UID, _clone.UID));
                                        rs.Success = true;
                                    }
                                    else
                                    {
                                        rs.Success = false;
                                    }

                                }

                            }
                        }
                        else if (item.Type == (int)WorkOrderPayloadType.FutureAllocated)
                        {
                            //如果是FutureAllocated 則另外處理Payload
                            var futurePayload = new PayloadInnerModel();
                            var _cpkg = this._Managers.PackageMappingCache.GetPackage(item.PackageUID);
                            futurePayload.UID = Guid.NewGuid();
                            futurePayload.Type = (int)PayloadType.FutureAllocated;
                            futurePayload.Status = (int)PayloadStatus.Active;
                            futurePayload.Quantity = item.Qty;
                            futurePayload.OriginalPayloadUID = null;
                            //如果是FutureAllocated 不是真的庫存，所以不計算重量與體積
                            futurePayload.VolumeLimit = 0;
                            futurePayload.WeightLimit = 0;
                            funcs.Push(() => this._Managers.InventoryManager.AddPayload(futurePayload));
                            // modified workorder payload payloaduid
                            funcs.Push(() => this._Managers.WorkOrderPayloadRepository
                            .ChangePayload(item.UID, futurePayload.UID));
                            rs.Success = true;
                        }
                    }

                    //funcs.Push(() =>
                    //{
                    //    this._Managers.TracingAgent.Trace($"Allocated payload", newpayloads);
                    //    return ActionResultTemplates.OK();
                    //});
                    rs.Content = funcs;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_PAYLOAD;

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

        internal IActionResult<ConcurrentStack<Func<IActionResult<bool>>>>
            GetForecAllcoateFunc(List<IWorkOrderPayloadModel> workOrderpayloads)
        {
            var rs = ActionResultTemplates.Result<ConcurrentStack<Func<IActionResult<bool>>>>();
            //try
            //{
            ConcurrentStack<Func<IActionResult<bool>>> funcs = new ConcurrentStack<Func<IActionResult<bool>>>();
            var blukpickWk = workOrderpayloads.Select(p => p as IBulkPickWorkOrderPayloadRelateModel);
            var labels = this._Managers.LabelManager.GetBelongtoBarcode(blukpickWk.SelectMany(x => x.OriginalPayloadUID).ToArray());
            foreach (var item in blukpickWk)
            {
                PayloadInnerModel pm = new PayloadInnerModel();
                var _cpkg = this._Managers.PackageMappingCache.GetPackage(item.PackageUID);
                pm.UID = item.PayloadUID;
                pm.ItemUID = item.ItemUID;
                pm.PackageUID = item.PackageUID;
                pm.Quantity = item.Qty;
                pm.SlotUID = item.SlotUID.Value;
                pm.Status = (int)PayloadStatus.Active;
                pm.Type = (int)PayloadType.Allocated;
                pm.VesselUID = Guid.Empty;
                pm.PODUID = Guid.Empty;
                pm.VolumeLimit = this._Managers.ProductUtility.CalculateCUFT(_cpkg, pm.Quantity);
                pm.WeightLimit = this._Managers.ProductUtility.CaculateTTLWeight(_cpkg, pm.Quantity);
                funcs.Push(() => this._Managers.InventoryManager.AddPayload(pm));
                //copy label
                var belongLabels = labels.Content.Where(x => item.OriginalPayloadUID.Any(a => a == x.BelongToUID));
                var labelGrp = belongLabels.GroupBy(g => new
                {
                    BarcodeType = g.BarcodeType,
                    Barcode = g.Barcode,
                    BelongToType = g.BelongToType,
                    FileUID = g.AttachmentUID
                }); ;
                List<LabelInnerModel> lcols = new List<LabelInnerModel>();
                foreach (var label in labelGrp)
                {
                    LabelInnerModel l = new LabelInnerModel();
                    l.UID = Guid.NewGuid();
                    l.BelongToType = (LabelBelongType)label.Key.BelongToType;
                    l.Type = (LabelType)label.Key.BarcodeType;
                    l.BelongToUID = pm.UID;
                    l.Content = label.Key.Barcode;
                    l.FileUID = label.Key.FileUID;
                    l.Status = (int)LabelStatus.Active;
                    lcols.Add(l);
                }

                funcs.Push(() => this._Managers.InventoryManager.ChangePayloadType(item.OriginalPayloadUID,
                    (int)PayloadType.BulkPickPending));
                funcs.Push(() => this._Managers.LabelManager.AddLabels(lcols.ToArray()));
            }

            rs.Content = funcs;
            rs.Success = true;
            //}
            //catch (Exception ex)
            //{
            //    rs.Message = ex.Message;
            //    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
            //    rs.Success = false;
            //    rs.InnerException = ex;
            //}
            return rs;
        }

        public IActionResult<bool> BatchExecuteAllocated(IEnumerable<IWorkOrderPayloadModel> workOrderpayloads, bool IsRegular = true)
        {
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            //try
            //{
            var actions = this.GetAllcoateFunc(workOrderpayloads, IsRegular);
            var resultAction = actions.Content.Reverse();
            foreach (var item in resultAction)
            {
                Result.Add(item.Invoke());
            }
            rs.Content = Result.All(p => p.Success);
            if (rs.Content)
            {
                rs.Success = true;
            }
            else
            {
                rs.Success = false;
                rs.Message = string.Join(",", Result.Select(x => x.Message));
            }

            //}
            //catch (Exception ex)
            //{
            //    rs.Message = ex.Message;
            //    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
            //    rs.Success = false;
            //    rs.InnerException = ex;
            //}
            return rs;
        }
        public int CompareOnhand(Guid onhandPkg, Guid comparePkg, int onhandQty, int CompareQty)
        {
            var plpkg = this._Managers.PackageMappingCache.GetPackage(onhandPkg);
            var alpkg = this._Managers.PackageMappingCache.GetPackage(comparePkg);
            var plpkgMinpkg = this._Managers.PackageMappingCache.GetMinPackage(onhandPkg);
            var alpkgMinpkg = this._Managers.PackageMappingCache.GetMinPackage(comparePkg);
            var _onhand = this._Managers.PackageMappingCache.GetReceivePackageUomQuantity(plpkg.UID, plpkgMinpkg.UID, onhandQty);
            var _allocatd = this._Managers.PackageMappingCache.GetReceivePackageUomQuantity(alpkg.UID, alpkgMinpkg.UID, CompareQty);
            return _onhand.Content - _allocatd.Content;
        }
        public IActionResult<Guid> ForceAllocated(Guid packageUID, Guid itemUID, int allocatedQty, Guid slotUID
            , IEnumerable<Guid> originalPayload, Guid allocatedPayloadUID, Guid ticketInfoUID)
        {
            var rs = ActionResultTemplates.OK<Guid>();
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            foreach (var payloadUID in originalPayload)
            {
                var pl = this._Managers.InventoryManager.GetPayload(payloadUID);

                if ((pl?.Content.Quantity ?? 0) == 0)
                {
                    Result.Add(this._Managers.InventoryManager.ChangePayloadStauts(payloadUID, PayloadStatus.Inactive));
                }
            }
            var _cpkg = this._Managers.PackageMappingCache.GetPackage(packageUID);
            var newPayload = new PayloadInnerModel();
            newPayload.ID = this._Managers.SequenceAgent.GetPayloadSeqenceByTimeSerial(PayloadType.Stock);
            newPayload.UID = Guid.NewGuid();
            newPayload.Type = (int)PayloadType.Stock;
            newPayload.Status = (int)PayloadStatus.Active;
            newPayload.SlotUID = slotUID;
            newPayload.ItemUID = itemUID;
            newPayload.Quantity = allocatedQty * -1;
            newPayload.OriginalPayloadUID = allocatedPayloadUID;
            newPayload.PackageUID = packageUID;
            newPayload.VolumeLimit = this._Managers.ProductUtility.CalculateCUFT(_cpkg, allocatedQty);
            newPayload.WeightLimit = this._Managers.ProductUtility.CaculateTTLWeight(_cpkg, allocatedQty);
            newPayload.Description = $"Ticketinfo# {ticketInfoUID} Change from slot generate negative payload ";
            var rs2 = this._Managers.InventoryManager.AddPayloadWithModifedSpecialLogical(packageUID, itemUID,
                allocatedQty, slotUID, newPayload.UID);

            Result.Add(this._Managers.InventoryManager.AddPayload(newPayload));
            if (Result.All(x => x.Success) && rs2.Success)
            {
                rs.Content = rs2.Content;
                rs.Success = true;
            }
            else
            {
                rs.Success = false;
                rs.Message = string.Join(",", Result.Where(p => !p.Success).Select(x => x.Message)) + rs.Message;
            }
            return rs;
        }
    }
}
