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
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundTicketBatchGenerator : AbstractTicketGenerator
    {
        public OutboundTicketBatchGenerator(
            TicketGeneratorParameters parameters)
            : base(parameters)
        {

        }
        public override ManifestType Type
        {
            get
            {
                return ManifestType.Outbound;
            }
        }
        protected override ServiceProcessItem[] ServiceItems
        {
            get
            {
                return new ServiceProcessItem[] { ServiceProcessItem.OutboundMove, ServiceProcessItem.Outbound };
            }
        }

        public override IActionResult<bool> Execute(ITicketGenerateParameter parameter)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var collection = this.GetData(parameter);
            var Errors = new List<string>();
            var checkResult = CheckData(parameter, collection);
            if (checkResult.Success)
            {
                if (collection != null && collection.Count() > 0)
                {
                    var ticketgrp = collection.GroupBy(g => g.WorkOrderUID);
                    List<ITicketModel> ticketModels = new List<ITicketModel>();
                    List<ITicketInfoModel> ticketInfoModels = new List<ITicketInfoModel>();
                    List<ITicketRelationModel> ticketRelationModels = new List<ITicketRelationModel>();
                    foreach (var titem in ticketgrp)
                    {
                        var parentTicketUID = new List<ParentTicketGenerateItem>();
                        foreach (var service in ServiceItems)
                        {
                            var _serviceModule = AbstractServiceItemProcessModule.GetSubModule(service, this.GetServiceParameters());
                            var result = _serviceModule.Execute(titem, parentTicketUID, this.Type,
                                            parameter.WarehouseUID, parameter.ForceOpen);
                            ticketModels.AddRange(result.Item1);
                            ticketInfoModels.AddRange(result.Item2);
                            ticketRelationModels.AddRange(result.Item3);



                        }


                    }
                    //execute sql
                    var rs1 = this.TicketRepository.AddTicket(ticketModels);
                    var rs2 = this.TicketInfoRepository.AddTickInfos(ticketInfoModels);
                    var rs3 = this.TicketRelationRepository.Add(ticketRelationModels);
                    if (!rs1.Success)
                    {
                        Errors.Add(rs1.Message);
                    }
                    if (!rs2.Success)
                    {
                        Errors.Add(rs2.Message);
                    }
                    if (!rs3.Success)
                    {
                        Errors.Add(rs3.Message);
                    }
                    if (Errors.Count == 0)
                    {

                        rs.Content = true;
                        rs.Success = true;
                    }
                    else
                    {
                        rs.Content = rs.Success = false;
                        rs.Message = string.Join("\r\n", Errors);
                    }

                }
                else
                {
                    rs.Content = rs.Success = false;
                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_GENERATE_TICKET;
                }

            }
            else
            {
                rs = checkResult;
            }

            return rs;
        }
        protected override IActionResult<bool> CheckData(ITicketGenerateParameter parameter, IEnumerable<ITicketGeneratoreDataModel> collection)
        {
            //TODO Outbound 檢查資料數量是否完全分配
            var rs = ActionResultTemplates.Result<bool>();
            var check1 = this.WorkOrderManager.CheckHaveUnAssingedPodPayload(parameter);
            rs = check1;
            return rs;
        }
    }
}
