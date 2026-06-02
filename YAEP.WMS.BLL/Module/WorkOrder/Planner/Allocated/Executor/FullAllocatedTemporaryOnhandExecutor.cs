using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class FullAllocatedTemporaryOnhandExecutor
    {
        IAllocateExecutorParameters _Managers;
        public FullAllocatedTemporaryOnhandExecutor(IAllocateExecutorParameters parameters)
        {
            _Managers = parameters;
        }
        public IActionResult<List<ILocationItemViewModel>> ExecuteAllocated(IEnumerable<AllocatedItem> allocatedItems)
        {
            List<IPayloadModel> UpdatepayloadModels = new List<IPayloadModel>();
            List<IPayloadModel> InsertpayloadModels = new List<IPayloadModel>();
            List<Guid> UnpackPods = new List<Guid>();
            List<ICloneLabelModel> cloneLabelModels = new List<ICloneLabelModel>();

            var temporaryPayloadlist = new List<ILocationItemViewModel>();
            var Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<List<ILocationItemViewModel>>();
            var _canAllocated = true;
            //try
            //{
            rs.Content = temporaryPayloadlist;
            rs.Success = true;
            var _payloads = this._Managers.InventoryManager.GetPayload(allocatedItems.Select(p => p.PayloadUID));
            List<IPayloadModel> newpayloads = new List<IPayloadModel>();
            if (_payloads.Success) //TODO 慮掃描方式決定判斷方式
            {

                var _pod = this._Managers.InventoryManager.GetPod(_payloads.Content.Select(p => p.PODUID).ToArray());
                foreach (var item in allocatedItems)
                {
                    if (item.AllocateType == AllocateType.GeneralAllocate)
                    {
                        var _pl = _payloads.Content.FirstOrDefault(p => p.UID == item.PayloadUID);
                        IPodModel _pd = null;
                        if (_pod.Content != null && _pod.Content.Count() > 0)
                            _pd = _pod.Content.FirstOrDefault(p => p.UID == _pl.PODUID);

                        if (item.AllocatedPackageUID == _pl.PackageUID)
                        {
                            #region 當要求包裝跟Payload 相同時處理方式
                            if (item.AllocatedQty <= _pl.Quantity)
                            {
                                _pl.Quantity -= item.AllocatedQty;
                                if (_pl.Quantity == 0)
                                    _pl.Status = (int)PayloadStatus.Inactive;
                                //alocated data
                                var _clone = _pl.Clone<IPayloadModel>();//IPayloadModel
                                                                        //var _cpkg = this._Managers.PackageManager.GetPackage(_clone.PackageUID).Content;
                                var _cpkg = this._Managers.PackageMappingCache.GetPackage(_clone.PackageUID);
                                _clone.UID = Guid.NewGuid();
                                _clone.Type = (int)PayloadType.TemporaryOnhand;
                                _clone.Status = (int)PayloadStatus.Active;
                                _clone.ID = _clone.Name = "Temmporay onhand";
                                _clone.Quantity = item.AllocatedQty;
                                _clone.OriginalPayloadUID = _pl.UID;
                                _clone.VolumeLimit = this._Managers.ProductUtility.CalculateCUFT(_cpkg, _clone.Quantity);
                                _clone.WeightLimit = this._Managers.ProductUtility.CaculateTTLWeight(_cpkg, _clone.Quantity);
                                _clone.CreatedOn = DateTime.UtcNow;
                                _pl.VolumeLimit = this._Managers.ProductUtility.CalculateCUFT(_cpkg, _pl.Quantity);
                                _pl.WeightLimit = this._Managers.ProductUtility.CaculateTTLWeight(_cpkg, _pl.Quantity);
                                newpayloads.Add(_clone);

                                //clone source label to new payload 
                                //Result.Add(this._Managers.LabelManager.CloneLabel(_pl.UID, _clone.UID));
                                //var rsl = this._Managers.LabelManager.ReturnCloneLabel(_pl.UID, _clone.UID);
                                cloneLabelModels.Add(new CloneLabelModel
                                {
                                    CreatedBy = this._Managers.TracingAgent.AuthenticationProvider.GetAuthenticationInfo().Account,
                                    SourceBelongToUID = _pl.UID,
                                    TargetBelongToUID = _clone.UID
                                });
                                temporaryPayloadlist.Add(PayloadConverttoOnhandEntity(_clone, _pl.Type.Value));
                                UpdatepayloadModels.Add(_pl);
                                InsertpayloadModels.Add(_clone);
                                //Result.Add(this._Managers.InventoryManager.UpdatePayload(_pl));
                                //Result.Add(this._Managers.InventoryManager.AddPayload(_clone));

                            }
                            else
                            {
                                rs.Success = false;
                                rs.Message = "Onhand has use,can't allocated.";
                                _canAllocated = false;
                                this._Managers.TracingAgent.Trace("Onhand has use,can't allocated.", item);
                            }
                            #endregion
                        }
                        else
                        {
                            #region 當要求包裝跟Payload包裝 不同時處理方式
                            // if payload have pod => unpack 
                            if (_pd != null && _pd.IsPack)
                            {
                                _pd.IsPack = false;
                                //Result.Add(this._Managers.InventoryManager.UnPack(_pd.UID));
                                UnpackPods.Add(_pd.UID);
                            }

                            IActionResult<int> _onhand = null;
                            bool _issameVer = true;
                            //down to W.payload package
                            //判斷package 是否為同版本
                            var plpkg = this._Managers.PackageMappingCache.GetPackage(_pl.PackageUID);
                            var alpkg = this._Managers.PackageMappingCache.GetPackage(item.AllocatedPackageUID);
                            if (plpkg.VersionUID == alpkg.VersionUID)
                            {
                                _onhand = this._Managers.PackageMappingCache.GetReceivePackageUomQuantity(
                                    _pl.PackageUID, item.AllocatedPackageUID, _pl.Quantity);
                            }
                            else
                            {
                                _issameVer = false;
                            }
                            if (_issameVer)
                            {
                                #region 當要求包裝跟Payload包裝不同但版本相同

                                if (_onhand.Success && _onhand.Content > 0)
                                {

                                    _onhand.Content -= item.AllocatedQty;
                                    var _clone = _pl.Clone<IPayloadModel>();
                                    _clone.UID = Guid.NewGuid();
                                    _clone.Status = (int)PayloadStatus.Active;
                                    _clone.ID = _clone.Name = "Temmporay onhand";
                                    _clone.Type = (int)PayloadType.TemporaryOnhand;
                                    _clone.Quantity = item.AllocatedQty;

                                    _clone.PackageUID = item.AllocatedPackageUID;
                                    _clone.OriginalPayloadUID = _pl.UID;

                                    //原payload 降至與allocated 同階包裝
                                    if (_onhand.Content > 0)
                                    {
                                        _pl.PackageUID = item.AllocatedPackageUID;
                                        _pl.Quantity = _onhand.Content;
                                    }
                                    else
                                    {
                                        _pl.Quantity = 0;
                                        _pl.Status = (int)PayloadStatus.Inactive;
                                    }
                                    newpayloads.Add(_clone);
                                    //clone source label to new payload 
                                    //Result.Add(this._Managers.LabelManager.CloneLabel(_pl.UID, _clone.UID));
                                    //var rsl = this._Managers.LabelManager.ReturnCloneLabel(_pl.UID, _clone.UID);
                                    cloneLabelModels.Add(new CloneLabelModel
                                    {
                                        CreatedBy = this._Managers.TracingAgent.AuthenticationProvider.GetAuthenticationInfo().Account,
                                        SourceBelongToUID = _pl.UID,
                                        TargetBelongToUID = _clone.UID
                                    });
                                    temporaryPayloadlist.Add(PayloadConverttoOnhandEntity(_clone, _pl.Type.Value));
                                    //allocated data
                                    //Result.Add(this._Managers.InventoryManager.UpdatePayload(_pl));
                                    //Result.Add(this._Managers.InventoryManager.AddPayload(_clone));
                                    UpdatepayloadModels.Add(_pl);
                                    InsertpayloadModels.Add(_clone);



                                }
                                else
                                {
                                    //目前可能不會被執行，當Allocated 執行的階段(已啟用Snapshot scope)當目前Payload 資料被其它程序修改時會有Exception
                                    _canAllocated = false;
                                    rs.Success = false;
                                    rs.Message = "Onhand has use,can't allocated.";
                                    this._Managers.TracingAgent.Trace("Onhand has use,can't allocated.", item);
                                }
                                #endregion
                            }
                            else
                            {
                                #region 當要求包裝跟Payload包裝不同但版本不相同

                                var plRatio = this._Managers.PackageMappingCache
                                   .GetReceivePackageUomQuantity(plpkg.UID,
                                   this._Managers.PackageMappingCache.GetMinPackage(plpkg.UID).UID, 1);
                                var plminqty = this._Managers.PackageMappingCache
                                   .GetReceivePackageUomQuantity(plpkg.UID,
                                   this._Managers.PackageMappingCache.GetMinPackage(plpkg.UID).UID,
                                   _pl.Quantity);
                                var apminqty = this._Managers.PackageMappingCache
                                  .GetReceivePackageUomQuantity(item.AllocatedPackageUID,
                                  this._Managers.PackageMappingCache.GetMinPackage(item.AllocatedPackageUID).UID,
                                  item.AllocatedQty);
                                _pl.Quantity = (plminqty.Content - apminqty.Content) / plRatio.Content;
                                var _clone = _pl.Clone<IPayloadModel>();
                                _clone.UID = Guid.NewGuid();
                                _clone.Status = (int)PayloadStatus.Active;
                                _clone.ID = _clone.Name = "Temmporay onhand";
                                _clone.Type = (int)PayloadType.TemporaryOnhand;
                                _clone.Quantity = item.AllocatedQty;
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
                                    // Result.Add(this._Managers.LabelManager.CloneLabel(_pl.UID, _clone.UID));
                                    var rsl = this._Managers.LabelManager.ReturnCloneLabel(_pl.UID, _clone.UID);
                                    temporaryPayloadlist.Add(PayloadConverttoOnhandEntity(_clone, _pl.Type.Value));
                                    cloneLabelModels.Add(new CloneLabelModel
                                    {
                                        CreatedBy = this._Managers.TracingAgent.AuthenticationProvider.GetAuthenticationInfo().Account,
                                        SourceBelongToUID = _pl.UID,
                                        TargetBelongToUID = _clone.UID
                                    });
                                    //allocated data
                                    //Result.Add(this._Managers.InventoryManager.UpdatePayload(_pl));
                                    //Result.Add(this._Managers.InventoryManager.AddPayload(_clone));
                                    UpdatepayloadModels.Add(_pl);
                                    InsertpayloadModels.Add(_clone);
                                    rs.Success = true;

                                }
                                else
                                {
                                    rs.Success = false;
                                    _canAllocated = false;
                                }
                                #endregion
                            }
                            #endregion
                        }

                    }
                    else if (item.AllocateType == AllocateType.FutureAllocate)
                    {
                        var futurePayload = new PayloadInnerModel();
                        var _cpkg = this._Managers.PackageMappingCache.GetPackage(item.AllocatedPackageUID);
                        futurePayload.UID = Guid.NewGuid();
                        futurePayload.Type = (int)PayloadType.FutureAllocated;
                        futurePayload.Status = (int)PayloadStatus.Active;
                        futurePayload.Quantity = item.AllocatedQty;
                        futurePayload.OriginalPayloadUID = null;
                        //如果是FutureAllocated 不是真的庫存，所以不計算重量與體積
                        futurePayload.VolumeLimit = 0;
                        futurePayload.WeightLimit = 0;
                        //Result.Add(this._Managers.InventoryManager.AddPayload(futurePayload));
                        InsertpayloadModels.Add(futurePayload);
                        rs.Success = true;
                    }
                }


                //批次更新

                if (_canAllocated)
                {
                    Result.Add(this._Managers.InventoryManager.BatchUnPack(UnpackPods));
                    Result.Add(this._Managers.InventoryManager.BatchUpdatePayload(UpdatepayloadModels));
                    Result.Add(this._Managers.InventoryManager.BatchAddPayload(InsertpayloadModels));
                    if (Result.All(p => p.Success))
                    {
                        var labels = this._Managers.LabelManager.BatchReturnCloneLabel(cloneLabelModels);
                        if (labels.Success)
                        {
                            //mapping label
                            foreach (var item in temporaryPayloadlist)
                            {
                                var onhandlables = labels.Content.Where(p => p.BelongToUID == item.PayloadUID);
                                if (onhandlables != null)
                                {
                                    item.Labels = onhandlables.Select(p =>
                                    {
                                        return new LabelInnerViewModel
                                        {
                                            Barcode = p.Content,
                                            BarcodeType = (int)p.Type,
                                            BelongToUID = p.UID,
                                            FileUID = p.FileUID,
                                            BelongToType = (int)p.BelongToType,
                                            Status = p.Status
                                        };
                                    });
                                }
                            }
                        }
                        else
                        {
                            rs.Success = false;
                            rs.Message = labels.Message;
                        }
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = string.Join(",", Result.Select(p => p.Message));
                    }
                }
                else
                {
                    rs.Success = false;
                    rs.Message = "Onhand has use,can't allocated.";
                    rs.Content = new List<ILocationItemViewModel>();
                }


            }
            else
            {
                rs.Success = false;
                rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_PAYLOAD;

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
        /// <summary>
        /// 沒檢查資料是否為Temponhand
        /// </summary>
        /// <param name="payloadlist"></param>
        /// <returns></returns>
        public IActionResult<bool> RecoveryTemporaryOnhand(IEnumerable<DeallocatedParameters> payloadlist)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {

                var prs = this._Managers.InventoryManager.DeallocatedPayload(payloadlist);
                rs = prs;
            }
            catch (Exception ex)
            {
                rs.Success = false;
                rs.Message = ex.Message;
                rs.InnerException = ex;
            }
            //}
            //else
            //{
            //    rs.Success = false;
            //    rs.Message = $"Payload not belong to temporary onhand";
            //}
            return rs;
        }
        public IActionResult<bool> ClearTemporaryOnhand(IEnumerable<Guid> payloadlist)
        {
            var rs = ActionResultTemplates.Result<bool>();
            //var _payloadlist = this._Managers.InventoryManager.GetPayloadList(new
            //{
            //    UID = payloadlist
            //});
            //if (_payloadlist.Content.All(p => p.Type == (int)PayloadType.TemporaryOnhand))
            //{
            try
            {
                var prs = this._Managers.InventoryManager.ChangePayloadStauts(payloadlist, PayloadStatus.Inactive);
                rs = prs;
            }
            catch (Exception ex)
            {
                rs.Success = false;
                rs.Message = ex.Message;
                rs.InnerException = ex;
            }
            //}
            //else
            //{
            //    rs.Success = false;
            //    rs.Message = $"Payload not belong to temporary onhand";
            //}
            return rs;
        }
        private ILocationItemViewModel PayloadConverttoOnhandEntity(IPayloadModel payloadModel, int originalPayloadType, IEnumerable<ILabelModel> labels = null)
        {
            var onhand = new LocationItemInnerViewModel();
            onhand.SlotUID = payloadModel.SlotUID;
            onhand.Sequence = 1;
            onhand.PackageSerialNumber = 1;
            onhand.PackageUID = onhand.OriginalPackageUID = payloadModel.PackageUID;
            onhand.UID = onhand.PayloadUID = payloadModel.UID;
            onhand.Quantity = payloadModel.Quantity;
            onhand.ItemUID = payloadModel.ItemUID;
            onhand.VesselUID = payloadModel.VesselUID;
            onhand.OriginalPayloadUID = payloadModel.OriginalPayloadUID;
            onhand.Type = originalPayloadType;
            return onhand;
        }
    }
}
