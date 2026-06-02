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

namespace YAEP.WMS.BLL.Module
{
    internal class InboundRollbackProcesser : RollbackProcesserAbstract
    {
        public InboundRollbackProcesser(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }
        public override IActionResult<bool> Execute(Guid warehouseUID, IEnumerable<ITicketModel> ticketModels)
        {
            var Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //group by inbound ticket
                //if move ticket status=complete 不管inbound Ticket 完成才需要改onhand和刪除payload
                //real delete payload
                //decrease onhand
                //else
                //nothing to do


                // if ticket complete(ticket type = inbound)
                var inboundTickets = ticketModels.Where(p =>
                p.Type == (int)TicketType.Receiving &&
                p.Status >= (int)TicketStatus.Processing);
                if (inboundTickets.Count() > 0)
                {
                    var wpods = this._parameters.WorkOrderPodRepository
                        .GetWorkOrderPodList(ticketModels.Select(p => p.WorkOrderUID));
                    var wpayload = this._parameters.WorkOrderPayloadRepository
                        .GetList(new { WorkOrderUID = ticketModels.Select(x => x.WorkOrderUID) });
                    //real delete pod
                    if (wpods != null && wpods.Content.Where(p => p.PodUID.HasValue).Count() > 0)
                        Result.Add(this._parameters.InventoryManager.DeletePodFromDb(
                            new { UID = wpods.Content.Select(p => p.PodUID) }));
                    //real delete payload
                    if (wpayload != null && wpayload.Content.Count() > 0)
                    {
                        Result.Add(this._parameters.InventoryManager.DeletePayloadFromDb(
                            new { UID = wpayload.Content.Select(p => p.PayloadUID) }));
                        //deduct onhand
                        foreach (var item in wpayload.Content)
                        {
                            //var param = this.DbContext.InventoryManager.CreateEditInventoryParameters();
                            //param.ItemUID = item.ItemUID;
                            //param.Onhand = item.Qty * -1;
                            //param.SlotUID = item.SlotUID.Value;
                            //param.TargetPackageUID = item.PackageUID;
                            //param.WarehouseUID = warehouseUID;
                            //Result.Add(this.DbContext.InventoryManager.UpdateInventory(param));

                            InsertInventoryParameter iparam = new InsertInventoryParameter();
                            iparam.ItemUID = item.ItemUID;
                            iparam.Qty = item.Qty * -1;
                            iparam.SlotUID = item.SlotUID.Value;
                            iparam.TargetPackageUID = item.PackageUID;
                            iparam.Type = InventoryType.Stock;
                            iparam.WarehouseUID = warehouseUID;

                            Result.Add(this._parameters.InventoryManager.InsertInventory(new InsertInventoryParameter[] { iparam }));
                        }

                    }
                }


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
