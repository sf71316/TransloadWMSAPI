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
    internal class InventoryCountingSubProcessModule : AbstractProcessModule
    {
        public InventoryCountingSubProcessModule(
            ITicketProcessAgentParameter parameters, ILogInfiltrator logInfiltrator)
            : base( parameters, logInfiltrator)
        {

        }


        public override IActionResult<bool> Execute(IEnumerable<IUploadTicketDataParameter> Data,
            NotifySenderConfig sendInfo = null)
        {
            this.sendInfo = sendInfo;
            this.UploadData = Data.Select(p => p.Item);
            TicketInfoStatus _ticketInfoStatus = TicketInfoStatus.Draft;
            // 1-1 get ticket data
            var rs = ActionResultTemplates.Result<bool>();
            rs.Success = true;
            var _ticketInfos = this.ticketManager.GetTicketProcessModel(this.UploadData.Select(p => p.TicketInfoUID)?.ToArray());
            // 1-1-2 log mobile upload scan barcode
            LogScanbarcode(this.UploadData, "Inventory cout");
            foreach (var item in _ticketInfos.Content)
            {
                var converter = AbstractTicketConverter.GetInstance(item.ManifestType, item.Type);
                converter.Convert(item);
                List<ILabelModel> uploadlabelModels = new List<ILabelModel>();
                //find upload data
                var _uploaddata = this.UploadData.FirstOrDefault(p => p.TicketInfoUID == item.UID);
                if (_uploaddata != null)
                {

                    if (_uploaddata.Barcode != null && _uploaddata.Barcode.Count() > 0)//check barcode 
                    {
                        var _usedLabel = item.Barcodes
                            .Where(p => _uploaddata.Barcode.Select(x => x.Barcode)
                            .Contains(p.Content) && p.Status == (int)LabelStatus.Used);
                        if (_usedLabel.Count() == _uploaddata.Barcode.Count())
                        {
                            rs.Success = false;
                            rs.Message = string.Format(Resource.TICKET_BARCODE_HAD_SCANED,
                                string.Join(",", _usedLabel.Select(x => x.Content)));
                        }
                        else
                        {
                            uploadlabelModels.AddRange(item.Barcodes
                            .Where(p => _uploaddata.Barcode.Select(x => x.Barcode)
                            .Contains(p.Content) && !_usedLabel.Any(x => x.Content == x.Content)));
                        }
                    }
                    // 2-1 check ast qty  priority AstQty> barcode code count
                    var _operationQty = (_uploaddata.ScanType == ScanType.NoNeedToScan || _uploaddata.ScanType == ScanType.NoUnique) ?
                                        _uploaddata.ActQty.Value : uploadlabelModels.Count;
                    item.ActQty += _operationQty;
                    if (item.ActQty <= item.EstQty)
                    {
                        if (_uploaddata.SavQty.HasValue)
                        {
                            item.SavQty += _uploaddata.SavQty.Value;
                        }
                        if (_uploaddata.ShtQty.HasValue)
                        {
                            item.ShtQty += _uploaddata.ShtQty.Value;
                        }
                        if (item.EstQty <= item.ActQty + item.SavQty + item.ShtQty) //complete
                        {
                            if (item.ShtQty == 0 || item.SavQty == 0)
                                _ticketInfoStatus = TicketInfoStatus.Complete;
                            else
                                _ticketInfoStatus = TicketInfoStatus.Glitch;
                        }
                        else //not yet
                        {

                            _ticketInfoStatus = TicketInfoStatus.Processing;
                        }
                        item.Status = (int)_ticketInfoStatus;
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = Resource.TICKET_ACTQTY_MUST_EQUAL_ESTQTY;
                    }
                    if (rs.Success)
                    {
                        #region Process



                        //TransactionScope scope = null;
                        //if (!this.IsExistTransaction)
                        //    scope = this.GetTransactionScope();
                        try
                        {
                            List<IActionResult<bool>> _result = new List<IActionResult<bool>>();
                            // 1-1 update ticketinfo 
                            _result.Add(this.ticketManager.UpdateTicketInfo(item));
                            if (_ticketInfoStatus == TicketInfoStatus.Complete)
                            {


                            }
                            if (_result.All(p => p.Success))
                            {
                                //this.transacationScope.Complete(scope);
                                rs.Content = rs.Success = true;
                            }
                            else
                            {
                                rs.Content = rs.Success = false;
                                rs.Message = string.Join("\n\n", _result.Select(p => p.Message));
                            }
                        }
                        catch (Exception ex)
                        {
                            rs.Message = ex.Message;
                            rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                            rs.Success = false;
                            rs.InnerException = ex;
                        }
                        //if (scope != null)
                        //{
                        //    scope.Dispose();
                        //}
                        #endregion
                    }
                }
            }

            return rs;
        }
    }
}
