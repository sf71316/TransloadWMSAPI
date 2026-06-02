using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Extension;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundTicketExtraProcessModule
    {
        TicketExtraProcessParameters Provider { get; set; }
        public OutboundTicketExtraProcessModule(TicketExtraProcessParameters ticketExtraProcessParameters)
        {
            Provider = ticketExtraProcessParameters;
        }

        public IActionResult<bool> GenerateUnAllocatedItem(ITicketInfoModel ticketInfoModel)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var Result = new List<IActionResult<bool>>();
                //List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
                var orignalwpayloadinfo = this.Provider.WorkOrderPayloadRepository
                     .GetList(new { UID = ticketInfoModel.WorkOrderPayloadUID }).Content.First();
                WorkOrderPayloadInnerModel clonewpayload = new WorkOrderPayloadInnerModel(orignalwpayloadinfo);
                //1-1 產生未allocate的w.payload 
                clonewpayload.UID = Guid.NewGuid();
                clonewpayload.PayloadPackageUID = Guid.Empty;
                clonewpayload.PayloadUID = Guid.Empty;
                clonewpayload.SlotUID = Guid.Empty;
                clonewpayload.Qty = ticketInfoModel.ShtQty + ticketInfoModel.SavQty;
                // TODO: clone workpayload + ShtGroupUID

                clonewpayload.SeparateByUID = orignalwpayloadinfo.UID;
                Result.Add(this.Provider.WorkOrderPayloadRepository.AddPayload(clonewpayload));
                //1-2 修改原生w.payload qty
                orignalwpayloadinfo.Qty -= ticketInfoModel.ShtQty + ticketInfoModel.SavQty;
                // TODO: original workpayload + ShtGroupUID

                Result.Add(this.Provider.WorkOrderPayloadRepository.EditPayload(new { UID = orignalwpayloadinfo.UID }, orignalwpayloadinfo));
                //1-2-1 是否為BulkPick產生的WorkOrderpayload，需將新產生的w.payload 加入關聯表內
                //if (this.Provider.BulkPickManager.IsBulkPickWorkOrderPayload(orignalwpayloadinfo.UID).AllComplete())
                //{
                //var brmodel = new BulkPickWorkOrderPayloadRelationModel();
                //brmodel.OriginalWorkOrderPayloadUID = clonewpayload.UID;
                //brmodel.BulkPickWorkOrderPayloadUID = orignalwpayloadinfo.UID;
                //brmodel.Status = (int)BulkPickWorkOrderPayloadStatus.Active;
                //Result.Add(this.Provider.BulkPickManager.AddBlukPickWorkOrderPayloadRelation(
                //    new IBulkPickWorkOrderPayloadRelationModel[] { brmodel }));
                //}
                //1-3 修改原生payload qty
                var originalpayload = this.Provider.InventoryManager.GetPayload(orignalwpayloadinfo.PayloadUID);
                originalpayload.Content.Quantity -= ticketInfoModel.ShtQty + ticketInfoModel.SavQty;
                if (originalpayload.Content.Quantity == 0)  //沒有可用的數量
                {
                    originalpayload.Content.Status = (int)PayloadStatus.Inactive;
                }
                Result.Add(this.Provider.InventoryManager.UpdatePayload(originalpayload.Content));
                //1-3-1 將損壞數量還回原本payload
                if (originalpayload.Content.OriginalPayloadUID.HasValue &&
                    originalpayload.Content.OriginalPayloadUID.Value != Guid.Empty)
                {
                    var rawpayload = this.Provider.InventoryManager.GetPayload(originalpayload.Content.OriginalPayloadUID.Value);
                    rawpayload.Content.Quantity += ticketInfoModel.ShtQty + ticketInfoModel.SavQty;
                    Result.Add(this.Provider.InventoryManager.UpdatePayload(rawpayload.Content));
                }
                else //找不到原始payload 就重建一筆新的
                {
                    var rawpayload = originalpayload.Content.Clone<IPayloadModel>();
                    rawpayload.CreatedOn = DateTime.UtcNow;
                    rawpayload.UID = Guid.NewGuid();
                    rawpayload.Quantity = ticketInfoModel.ShtQty + ticketInfoModel.SavQty;
                    rawpayload.Type = (int)PayloadType.Stock;
                    rawpayload.Status = (int)PayloadStatus.Active;
                    Result.Add(this.Provider.InventoryManager.AddPayload(rawpayload));
                }


                //1-4 修改原生outbound ticket qty
                var outboundticketInfos = this.Provider.TicketInfoRepository
                                    .GetList(new
                                    {
                                        WorkOrderPayloadUID = ticketInfoModel.WorkOrderPayloadUID,
                                        Type = (int)TicketInfoType.Outbound
                                    });
                if (outboundticketInfos.Content != null && outboundticketInfos.Content.Count() == 1)
                {
                    var originalOutboundTicketInfo = outboundticketInfos.Content.First();
                    originalOutboundTicketInfo.EstQty -= ticketInfoModel.ShtQty + ticketInfoModel.SavQty;
                    if (originalOutboundTicketInfo.EstQty == 0)
                    {
                        originalOutboundTicketInfo.Status = (int)TicketInfoStatus.Complete;
                    }
                    Result.Add(this.Provider.TicketInfoRepository.UpdateTickInfo(originalOutboundTicketInfo));
                    if (Result.All(p => p.Success))
                    {
                        //1-3-2 通知pbsc 將該筆allocated 刪除
                        var parameter = new ReplicateDataParameter();
                        parameter.TicketInfoUID = new Guid[] { ticketInfoModel.UID };
                        if (orignalwpayloadinfo.Qty == 0)
                        {
                            this.Provider.CompleteUnexecutedMethod.Enqueue(() => this.Provider.ReplicationManager.Deallocated(parameter));
                            //Result.Add(this.Provider.ReplicationManager.Deallocated(parameter));
                        }
                        else
                        {
                            //allocated 要用outbound TicketInfo uid
                            parameter.TicketInfoUID = new Guid[] { originalOutboundTicketInfo.UID };
                            this.Provider.CompleteUnexecutedMethod.Enqueue(() => this.Provider.ReplicationManager.Allcoated(parameter));
                            //Result.Add(this.Provider.ReplicationManager.Allcoated(parameter));
                        }
                    }
                    if (Result.All(p => p.Success))
                    {
                        //Result.Add(rs1);
                        //2-1 產生Ticketinfo
                        TicketInfoInnerModel cloneticketInfo = new TicketInfoInnerModel(ticketInfoModel);
                        cloneticketInfo.UID = Guid.NewGuid();
                        cloneticketInfo.EstQty = clonewpayload.Qty;
                        cloneticketInfo.WorkOrderPayloadUID = clonewpayload.UID;
                        cloneticketInfo.ActQty = cloneticketInfo.ShtQty = cloneticketInfo.SavQty = 0;
                        cloneticketInfo.Status = (int)TicketInfoStatus.Open;

                        TicketInfoInnerModel cloneoutboundTicket = new TicketInfoInnerModel(ticketInfoModel);
                        cloneoutboundTicket.UID = Guid.NewGuid();
                        cloneoutboundTicket.TicketUID = originalOutboundTicketInfo.TicketUID;
                        cloneoutboundTicket.Type = (int)TicketInfoType.Outbound;
                        cloneoutboundTicket.EstQty = clonewpayload.Qty;
                        cloneoutboundTicket.WorkOrderPayloadUID = clonewpayload.UID;
                        cloneoutboundTicket.ActQty = cloneoutboundTicket.ShtQty = cloneoutboundTicket.SavQty = 0;
                        cloneoutboundTicket.Status = (int)TicketInfoStatus.Open;
                        var rs2 = this.Provider.TicketInfoRepository
                            .AddTickInfos(new TicketInfoInnerModel[] { cloneticketInfo, cloneoutboundTicket });
                        //2-2 產生新的outbound ticketInfo
                        if (rs2.Success)
                        {

                            //Result.Add(rs2);
                            //3 assigned worker
                            var ticketInforelation = this.Provider.TicketInfoAssigneeRelationRepository
                                .GetAssignedList(new Guid[] { ticketInfoModel.UID });
                            var param = new MaintainWorkderInnerParameters();
                            param.TicketInfoUID = new Guid[] { cloneticketInfo.UID, cloneoutboundTicket.UID };
                            param.GroupUID = ticketInforelation.Content.Select(x => x.GroupUID).ToArray();
                            var rs3 = this.Provider.TicketManager.BatchAssignWorker(param);

                            if (rs3.Success)
                            {
                                //2-2-1 通知PBSC 修改allocated資料
                                var parameter2 = new ReplicateDataParameter();
                                parameter2.TicketInfoUID = new Guid[] { cloneoutboundTicket.UID };
                                //var rs4 = this.Provider.ReplicationManager.Allcoated(parameter2);
                                this.Provider.CompleteUnexecutedMethod.Enqueue(() => this.Provider.ReplicationManager.Allcoated(parameter2));
                                //if (rs4.Success)
                                //{
                                rs.Success = rs.Content = true;
                                //}
                                //else
                                //{
                                //    rs.Content = rs4.Content;
                                //    rs.Success = rs4.Success;
                                //    rs.Message = rs4.Message;
                                //}

                            }
                            else
                            {
                                rs.Content = rs3.Content;
                                rs.Success = rs3.Success;
                                rs.Message = rs3.Message;
                            }
                        }
                        else
                        {
                            rs.Content = rs2.Content;
                            rs.Success = rs2.Success;
                            rs.Message = rs2.Message;

                        }
                    }
                    else
                    {
                        rs.Content = Result.All(p => p.Success);
                        rs.Success = Result.All(p => p.Success);
                        rs.Message = string.Join(",", Result.Select(x => x.Message));
                    }
                }
                else
                {
                    rs.Success = rs.Content = false;
                    rs.Message = "outbound ticket must have only one.";
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
