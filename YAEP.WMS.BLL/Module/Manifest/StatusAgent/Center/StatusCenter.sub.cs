using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal partial class StatusCenter
    {

        protected IActionResult<bool> SubmitManifest(IManifestModel manifestInfo)
        {
            var rs = ActionResultTemplates.Result<bool>();

            try
            {
                var Result = new List<IActionResult<bool>>();
                Result.Add(this._agent.Manifest.ChangeManifestStatus(manifestInfo.UID, ManifestStatus.Open));
                Result.Add(this._agent.Manifest.ChangeManifestItemStatus(manifestInfo.UID, ManifestItemListStatus.Open));
                if (!Result.All(x => x.Success))
                {
                    rs.Message = string.Join(",", Result.Select(x => x.Message));
                }
                else
                {

                    rs.Success = true;
                    rs.Content = true;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
            }

            return rs;
        }
        protected IActionResult<bool> ApproveBol(IBolViewModel bolInfo)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var parm = new VesselSearchInnerParameters();
            parm.BolUID = bolInfo.UID;
            var vesselinfos = this._agent.DataModules.VesselRepository.GetList(parm);
            var workorders = this._agent.DataModules.WorkOrderRepository.GetList(new { VesselUID = vesselinfos.Content.Select(x => x.UID) });
            try
            {
                var Result = new List<IActionResult<bool>>();
                //change bol status
                Result.Add(this._agent.Bol
                    .ChangeBolStatus(bolInfo.UID, BolStatus.Open));
                //change vessel/manifest status
                Result.Add(this._agent.Vessel
                    .ChangeVesselByBol(bolInfo.UID, VesselStatus.Open));
                Result.Add(this._agent.Vessel
                 .ChangeVesselManifestStatusByBOL(bolInfo.UID, VesselManifestStatus.Open));
                //change workorder/pod/payload status
                foreach (var item in workorders.Content)
                {
                    Result.Add(this._agent.WorkOrder
              .ChangeAllWorkOrderStatus(item.UID, WorkOrderStatus.Open,
                WorkOrderPodStatus.Open, WorkOrderPayloadStatus.WaitingForProcessing));
                    //chanage ticket status
                    Result.Add(this._agent.Ticket
                               .ChangeAllTicketStatus(item.UID, TicketStatus.Open, TicketInfoStatus.Open));
                }
                if (!Result.All(x => x.Success))
                {
                    rs.Message = string.Join(",", Result.Select(x => x.Message));
                }
                else
                {
                    rs.Success = true;

                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
            }

            return rs;
        }
        /// <summary>
        /// 不檢查資料強制讓BOL推至Open 狀態
        /// </summary>
        /// <param name="bolUID"></param>
        /// <returns></returns>
        internal IActionResult<bool> ForceApproveBol(Guid bolUID, Guid warehouseUID, int manifestType)
        {

            var rs = ActionResultTemplates.Result<bool>();

            var parm = new VesselSearchInnerParameters();
            parm.BolUID = bolUID;
            var Result = new List<IActionResult<bool>>();
            //generate ticket
            var param = new TicketGenerateInnerParameter();
            param.BolUID = bolUID;
            param.WarehouseUID = warehouseUID;
            param.ForceOpen = true;
            param.ManifestType = manifestType;

            var rsGenerateTicket = this._agent.DataModules.TicketManager.GeneratreTicket(param);
            this.TracingAgent.Trace($"Generate ticket bol:{bolUID} result:{rsGenerateTicket.Success}", rsGenerateTicket, useCallername: true);
            Result.Add(rsGenerateTicket);
            if (!Result.All(x => x.Success))
            {
                rs.Success = false;
                rs.Message = string.Join(",", Result.Select(x => x.Message));
            }
            else
            {
                rs.Success = true;



            }




            return rs;

        }

        internal IActionResult<bool> BatchForceApproveBol(IEnumerable<Guid> bolUIDs, Guid warehouseUID, int manifestType)
        {

            var rs = ActionResultTemplates.Result<bool>();


            var Result = new List<IActionResult<bool>>();
            //generate ticket
            var param = new TicketGenerateInnerParameter();
            param.BolUIDs = bolUIDs;
            param.WarehouseUID = warehouseUID;
            param.ForceOpen = true;
            param.ManifestType = manifestType;

            var rsGenerateTicket = this._agent.DataModules.TicketManager.GeneratreTicket(param);
            this.TracingAgent.Trace($"Generate ticket bol:{string.Join(",", bolUIDs)} result:{rsGenerateTicket.Success}", rsGenerateTicket, useCallername: true);
            Result.Add(rsGenerateTicket);
            if (!Result.All(x => x.Success))
            {
                rs.Success = false;
                rs.Message = string.Join(",", Result.Select(x => x.Message));
            }
            else
            {
                rs.Success = true;



            }




            return rs;

        }

        protected IActionResult<bool> SubmitBol(IBolViewModel bolInfo)
        {
            var rs = ActionResultTemplates.Result<bool>();

            try
            {
                var Result = new List<IActionResult<bool>>();
                //bol status change to review
                Result.Add(this._agent.DataModules.BolRepository
                    .ChangeBolStatus(bolInfo.UID, BolStatus.Review));
                var manifestInfo = this._agent.DataModules.ManifestRepository.GetData(new { UID = bolInfo.ManifestUID });
                //generate ticket
                var param = new TicketGenerateInnerParameter();
                param.BolUID = bolInfo.UID;
                param.WarehouseUID = manifestInfo.Content.WarehouseUID;
                Result.Add(this._agent.DataModules.TicketManager.GeneratreTicket(param));

                if (!Result.All(x => x.Success))
                {
                    rs.Message = string.Join(",", Result.Select(x => x.Message));
                }
                else
                {
                    rs.Success = true;

                }

            }
            catch (Exception ex)
            {
                rs.Success = false;
                rs.Message = ex.Message;
            }


            return rs;
        }
    }




}
