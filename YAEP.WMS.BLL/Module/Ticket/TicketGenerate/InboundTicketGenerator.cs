using System;
using System.Collections.Generic;
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
    internal class InboundTicketGenerator : AbstractTicketGenerator
    {
        public InboundTicketGenerator(TicketGeneratorParameters parameters)
            : base(parameters)
        {

        }

        public override ManifestType Type
        {
            get
            {
                return ManifestType.Inbound;
            }
        }
        protected override ServiceProcessItem[] ServiceItems
        {
            get
            {
                return new ServiceProcessItem[] { ServiceProcessItem.Receiving, ServiceProcessItem.InboundMove };
            }
        }
        public override IActionResult<bool> Execute(ITicketGenerateParameter parameter)
        {
            
            var rs = ActionResultTemplates.Result<bool>();
            //try
            //{
                List<string> Error = new List<string>();
                var collection = this.GetData(parameter);
                var Errors = new List<string>();
                if (collection != null && collection.Count() > 0)
                {
                    rs = CheckData(parameter, collection);
                    if (rs.Success)
                    {
                        

                        //group by workorder
                        var workOrderGroup = collection.GroupBy(p => p.WorkOrderUID);
                        foreach (var group in workOrderGroup)
                        {
                            var parentTicket = new List<ParentTicketGenerateItem>();
                            foreach (var service in ServiceItems)
                            {

                                var _serviceModule = AbstractServiceItemProcessModule.GetSubModule(service, this.GetServiceParameters());
                                var result = _serviceModule.Execute(group, parentTicket, Type,parameter.WarehouseUID,parameter.ForceOpen);
                                var rs1 = this.TicketRepository.AddTicket(result.Item1);
                                var rs2 = this.TicketInfoRepository.AddTickInfos(result.Item2);
                                var rs3 = this.TicketRelationRepository.Add(result.Item3);

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


                            }


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

                }
                else
                {
                    rs.Content = rs.Success = false;
                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_GENERATE_TICKET;
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
        protected override IActionResult<bool> CheckData(ITicketGenerateParameter parameter, IEnumerable<ITicketGeneratoreDataModel> collection)
        {
            //TODO Inbound 檢查資料數量是否完全分配

            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            if (collection.Any(p => p.LoadingZoneSlotUID == Guid.Empty))
            {
                rs.Success = false;
                rs.Message += $"LoadingZoneSlotUID not empty";
            }
            if (collection.Any(p => p.SlotUID == Guid.Empty))
            {
                rs.Success = false;
                rs.Message += $"SlotUID not empty";
            }
            return rs;
        }
    }
}
