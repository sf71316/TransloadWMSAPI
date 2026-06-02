using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Model;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundRollbackProcesser : RollbackProcesserAbstract
    {
        public OutboundRollbackProcesser(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }

        public override IActionResult<bool> Execute(Guid warehouseUID, IEnumerable<ITicketModel> ticketModels)
        {
            var Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //group by outbound ticket
                var _outboundTicketGrp = ticketModels.Where(p => p.Type == (int)TicketType.Outbound);
                //get relation list
                var _ticketRelation = this._parameters.TicketManager
                    .GetTicketRelationList(new { ParentUID = _outboundTicketGrp.Select(x => x.UID) });
                //get outbound ticketinfos
                var _outboundTicketInfos = this._parameters.TicketInfoRepository
                    .GetList(new { TicketUID = _outboundTicketGrp.Select(x => x.UID) });
                // get outbound ticket work payload
                var _workpayloads = this._parameters.WorkOrderPayloadRepository
                    .GetRollbackWorkPayload(_outboundTicketInfos.Content.Select(x => x.TicketUID));
                // 取列表資料不會把刪掉的資料找回來
                //var _payloads = this._parameters.InventoryManager.GetPayloadList(
                //    new { UID = _workpayloads.Content.Select(p => p.PayloadUID) }
                //    );
                //var _opayloads = this._parameters.InventoryManager.GetPayloadList(
                //   new { UID = _payloads.Content.Select(p => p.OriginalPayloadUID) }
                //   );
                List<PayloadTransactionLogInnerModel> logs = new List<PayloadTransactionLogInnerModel>();
                List<InsertInventoryParameter> inventoryParameters = new List<InsertInventoryParameter>();
                foreach (var item in _outboundTicketGrp)
                {
                    //if move ticket status=complete
                    if (item.Status >= (int)TicketStatus.Glitch)
                    {
                        this._parameters.TracingAgent.TransactionInfo.Action = TransactionlogAction.ReturnTicketToOpen;
                        //find associate outbound ticket
                        //var _relation = _ticketRelation.Content.FirstOrDefault(y => y.ParentUID == item.UID);
                        //get ticketinfo
                        var _ticketinfos = _outboundTicketInfos.Content.Where(p => p.TicketUID == item.UID);
                        foreach (var ticketInfo in _ticketinfos)
                        {
                            //if outbound ticket info status =complete (outbound ticket 已完成) onhand 補正 payload 資料還原
                            //先找inventory，再看需不需要加或是改onhand
                            //2020.9.23 因Pick All 改變Inventory 寫入方式(寫入onhand 增減值)
                            var _wpayload = _workpayloads.Content.FirstOrDefault(p => p.TicketInfoUID == ticketInfo.UID);
                            var _payload = this._parameters.InventoryManager.GetPayload(_wpayload.PayloadUID).Content;
                            var _opayload = this._parameters.InventoryManager.GetPayload(_payload.OriginalPayloadUID.Value).Content;
                            //var _payload = _payloads.Content.FirstOrDefault(p => p.UID == _wpayload.PayloadUID);
                            //var _opayload = _opayloads.Content.FirstOrDefault(p => p.UID == _payload.OriginalPayloadUID.Value);
                            //_payloads.Content.FirstOrDefault(p => p.UID == _wpayload.PayloadUID);
                            #region old
                            //if (this.DbContext.InventoryManager.IsItemInSlot(warehouseUID,
                            //            _wpayload.ItemUID, _wpayload.PackageUID, _wpayload.LoadingZoneSlotUID).Success)
                            //{
                            //    EditOnhandInnerParameters parameters = new EditOnhandInnerParameters();
                            //    parameters.ItemUID = _wpayload.ItemUID;
                            //    parameters.Onhand = _wpayload.Qty;
                            //    parameters.TargetPackageUID = _wpayload.PackageUID;
                            //    parameters.SlotUID = _wpayload.LoadingZoneSlotUID;
                            //    parameters.WarehouseUID = warehouseUID;
                            //    Result.Add(this.DbContext.InventoryManager.UpdateInventory(parameters));
                            //}
                            //else
                            //{
                            //    var targetparam = this.DbContext.InventoryManager.CreateAddInventoryParameters();
                            //    targetparam.WarehouseUID = warehouseUID;
                            //    targetparam.ItemUID = _wpayload.ItemUID;
                            //    targetparam.TargetPackageUID = _wpayload.PackageUID;
                            //    //increase target onhand
                            //    targetparam.SlotUID = _wpayload.LoadingZoneSlotUID;
                            //    targetparam.Onhand = _wpayload.Qty;
                            //    targetparam.isPauseSync = true;
                            //    //increase original onhand
                            //    //incparam.SlotUID = _wpayload.SlotUID.Value;
                            //    //incparam.Onhand = _wpayload.Qty;
                            //    Result.Add(this.DbContext.InventoryManager.ProcessAddInventory(targetparam));
                            //}
                            #endregion
                            InsertInventoryParameter iparam = new InsertInventoryParameter();
                            iparam.ItemUID = _payload.ItemUID;
                            iparam.Qty = _payload.Quantity;
                            iparam.SlotUID = _payload.SlotUID;
                            iparam.TargetPackageUID = _payload.PackageUID;
                            iparam.Type = (InventoryType)_opayload.Type;
                            iparam.WarehouseUID = warehouseUID;
                            //transaction log
                            var logModel = new PayloadTransactionLogInnerModel();
                            logModel.UID = Guid.NewGuid();
                            logModel.WarehouseUID = warehouseUID;
                            logModel.ItemUID = _payload.ItemUID;
                            logModel.PayloadUID = _payload.UID;
                            logModel.WorkOrderPayloadUID = _wpayload.UID;
                            logModel.WorkOrderPodUID = _wpayload.WorkOrderPodUID;
                            logModel.TargetPackage = _payload.PackageUID;
                            logModel.QtyBeforeTX = 0;
                            logModel.QtyAfterTX = _payload.Quantity;
                            logModel.TargetSlotUID = _payload.SlotUID;
                            logModel.Status = (int)PayloadTransactionLogStatus.Active;
                            logModel.Type = (int)this._parameters.TracingAgent.GetTransactionLogType();
                            logModel.CreatedBy = this._parameters.AuthProvider.GetAuthenticationInfo().Account;
                            logModel.CreatedOn = DateTime.UtcNow;
                            logs.Add(logModel);
                            inventoryParameters.Add(iparam);


                            if (_payload != null)
                            {
                                if (ticketInfo.Status >= (int)TicketInfoStatus.Glitch
                                    && _payload.Status == (int)PayloadStatus.Inactive)
                                {
                                    //(outbound ticket 完成) onhand 補正 rollback payload & 修改slotUID
                                    _payload.Status = (int)PayloadStatus.Active;
                                    if (_payload.Type != (int)PayloadType.BulkPickPending)
                                        _payload.Type = (int)PayloadType.Allocated;
                                }
                                //(outbound ticket 未完成) onhand 補正 payload 修改slotUID
                                //2019.8.27 討論後rollback 後 不回原本的Slot
                                //_payload.Content.SlotUID = _wpayload.SlotUID.Value;
                            }

                            //Result.Add(this.DbContext.InventoryManger.EditInventory(incparam));
                            Result.Add(this._parameters.InventoryManager.UpdatePayload(_payload));
                        }
                    }

                    //else
                    //nothing to do
                }
                if (inventoryParameters.Count > 0)
                    Result.Add(this._parameters.InventoryManager.InsertInventory(inventoryParameters));
                if (logs.Count > 0)
                    Result.Add(this._parameters.InventoryManager.BatchAddLog(logs));
                //rollback ticket status
                Result.Add(this._parameters.TicketRepository
                    .UpdateTicketStatus(ticketModels.Select(x => x.UID), TicketStatus.Open));
                ////rollback ticketinfo qty & status
                Result.Add(this._parameters.TicketInfoRepository
                    .RollbackTicketInfo(ticketModels.Select(x => x.UID), TicketInfoStatus.Open));
                //rollback label status 200->100
                Result.Add(this._parameters.LabelManager.RollbackLabel(ticketModels.Select(x => x.UID)));

                rs.Content = rs.Success = Result.All(x => x.Success);
                if (!rs.Success)
                {
                    rs.Message = string.Join(",", Result.Select(x => x.Message));
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
