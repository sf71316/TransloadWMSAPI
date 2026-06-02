using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class MoveTicketGenerator : AbstractTicketGenerator
    {
        public MoveTicketGenerator(TicketGeneratorParameters parameters)
            : base(parameters)
        {

        }

        public override ManifestType Type
        {
            get
            {
                return ManifestType.Move;
            }
        }

        protected override ServiceProcessItem[] ServiceItems
        {
            get
            {
                return new ServiceProcessItem[] { ServiceProcessItem.WarehouseMove };
            }
        }

        public override IActionResult<bool> Execute(ITicketGenerateParameter parameter)
        {

            var rs = ActionResultTemplates.Result<bool>();
            //try
            //{
                var collection = this.GetDataByMoveManifest(parameter);
                var Errors = new List<string>();
                if (collection != null && collection.Count() > 0)
                {

                    //using (TransactionScope scope = GetTransactionScope())
                    //{
                        var parentTicket = new List<ParentTicketGenerateItem>();
                        foreach (var service in ServiceItems)
                        {
                            var _serviceModule = AbstractServiceItemProcessModule.GetSubModule(service, this.GetServiceParameters());
                            var result = _serviceModule.Execute(collection, parentTicket, this.Type, parameter.WarehouseUID, parameter.ForceOpen);
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
                        if (Errors.Count == 0)
                        {
                            //scope.Complete();
                            rs.Content = true;
                            rs.Success = true;
                        }
                        else
                        {
                            rs.Content = rs.Success = false;
                            rs.Message = string.Join("\r\n", Errors);
                        }
                    //}
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
    }
}
