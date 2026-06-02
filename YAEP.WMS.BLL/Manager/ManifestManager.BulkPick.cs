using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IBulkPickManager
    {
        public IActionResult<IEnumerable<IBulkPickViewModel>> GetBulkPickList(IBulkPickSearchParameters parameters)
        {
            return this._BulkPickRepository.GetList(parameters);
        }

        public IActionResult<IEnumerable<IBulkPickManifestViewModel>> GetManifestList(IBulkPickManifestSearchParameters parameters)
        {
            return this._BulkPickRepository.GetManifestList(parameters);
        }

        public IActionResult<bool> SaveBulkPick(IEnumerable<Guid> ticketInfoUID, string customerName)
        {
            var resultContainer = ActionResultTemplates.Result();

            if ((ticketInfoUID?.Count() ?? 0) == 0)
            {
                resultContainer.Message = $"Incorrect Parameters: {nameof(ticketInfoUID)}";
            }
            if (String.IsNullOrWhiteSpace(customerName))
            {
                resultContainer.Message = $"Incorrect Parameters: {nameof(customerName)}";
            }

            var bulkPickSaveDataResult = this._BulkPickRepository.GetBulkPickSaveDataByTicket(ticketInfoUID);

            if ((bulkPickSaveDataResult?.Content?.Count() ?? 0) > 0)
            {
                var saveData = bulkPickSaveDataResult.Content;

                #region 產生 Model

                string bulkPickID = this.SequenceAgent.GetBulkPickSeqence();
                // 1. Bulk Pick
                var bulkPickModel = new BulkPickInnerModel()
                {
                    UID = Guid.NewGuid(),
                    ID = $"{bulkPickID}",
                    Name = $"{customerName}.{bulkPickID}",
                    PartyName = customerName,
                    TicketUID = Guid.Empty, // 下面流程取得
                    Status = (int)BulkPickStatus.Open,
                    Type = 100, // default value
                };

                var bulkPickTicketRelations = new List<BulkPickTicketInfoRelationInnerModel>();

                // 2. Bulk Pick Ticket Relation
                int index = 1;
                foreach (var item in saveData)
                {
                    bulkPickTicketRelations.Add(new BulkPickTicketInfoRelationInnerModel()
                    {
                        UID = Guid.NewGuid(),
                        Status = (int)BulkPickTicketInfoRelationStatus.Active,
                        ID = $"{bulkPickID}.T{(index++).ToString().PadLeft(5, '0')}",
                        BulkPickUID = bulkPickModel.UID,
                        TicketInfoUID = item.TicketInfoUID,
                        FromSlotUID = item.OriginalSlotUID,
                        ToSlotUID = item.TargetSlotUID,
                        Type = 100, // default value
                    });
                }

                #endregion

                using (var db = this.DbEntities.DbAdapter)
                {
                    try
                    {
                        bool success = false;
                        var actionResult = ActionResultTemplates.Result();
                        this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                        // 1. Bulk Pick 產生 Move Ticket, 並且取得 Ticket UID
                        var generateBulkPickMoveTicketResult = this.TicketManager.GenerateBulkPickTicket(ticketInfoUID);

                        if (!generateBulkPickMoveTicketResult.Success)
                        {
                            actionResult.Message = generateBulkPickMoveTicketResult.Message;
                            actionResult.InnerException = generateBulkPickMoveTicketResult.InnerException;
                            actionResult.TypeCode = generateBulkPickMoveTicketResult.TypeCode;
                            return actionResult;
                        }
                        else if (generateBulkPickMoveTicketResult.Content == Guid.Empty)
                        {
                            actionResult.Message = "Fail to generate move ticket.";
                            return actionResult;
                        }

                        var ticketUID = generateBulkPickMoveTicketResult.Content;

                        bulkPickModel.TicketUID = ticketUID;

                        // 2. Create Bulk Pick
                        actionResult = this._BulkPickRepository.Create(bulkPickModel);
                        if (actionResult.Success)
                        {
                            // 3. Batch Insert Bulk Pick Ticket Info Relation
                            actionResult = this._BulkPickTicketInfoRelationRepository.Create(bulkPickTicketRelations);
                        }

                        if (actionResult?.Success ?? false)
                        {
                            success = true;
                        }
                        else
                        {
                            resultContainer.Message = actionResult?.Message ?? "Some error occured.";
                            resultContainer.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                            resultContainer.InnerException = actionResult?.InnerException;
                        }

                        if (success)
                        {
                            db.Commit();

                            resultContainer.Success = true;
                        }
                        else
                        {
                            db.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        db.Rollback();
                        resultContainer.Message = ex.Message;
                        resultContainer.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                        resultContainer.InnerException = ex;
                    }
                }
            }
            else
            {
                resultContainer.Message = "Not Found.";
            }

            return resultContainer;
        }

        public IActionResult<bool> CreateBulkPick(IBulkPickModel model)
        {
            return this._BulkPickRepository.Create(model);
        }

        public IActionResult<bool> DeleteBulkPick(IEnumerable<Guid> bulkPickUID)
        {
            List<IActionResult<bool>> Result = new List<IActionResult<bool>>();
            // 1. 取得 Ticket Relation 資料
            var getTicketRelationResult = this._BulkPickTicketInfoRelationRepository.GetTicketRelations(bulkPickUID);

            var result = ActionResultTemplates.Result();

            using (var db = this.DbEntities.DbAdapter)
            {
                try
                {
                    this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                    // 2. 刪除 Bulk Pick 資料
                    result = this._BulkPickRepository.Delete(bulkPickUID);
                    Result.Add(result);
                    // 2-1. 刪除 Bulk Pick Ticket Info資料
                    foreach (var item in bulkPickUID)
                    {
                        Result.Add(this._BulkPickTicketInfoRelationRepository.DeleteByBulkPick(item));
                    }

                    if (Result.All(p => p.Success))
                    {
                        if (getTicketRelationResult.Success && (getTicketRelationResult.Content?.Count() ?? 0) > 0)
                        {
                            // 3. 使用 Ticket Manager 執行刪除的後續動作
                            Result.Add(this.TicketManager.RemoveBulkPickTicket(getTicketRelationResult.Content));
                        }
                    }

                    if (Result.All(p => p.Success))
                    {
                        result.Success = true;
                        result.Message = "";
                        db.Commit();
                    }
                    else
                    {
                        db.Rollback();
                        result.Success = false;
                        result.Message = string.Join(",", Result.Select(x => x.Message));
                    }
                }
                catch (Exception ex)
                {
                    db.Rollback();
                    result.Message = ex.Message;
                    result.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    result.InnerException = ex;
                }
            }

            return result;
        }

        public IActionResult<bool> BatchAddWorker(IEnumerable<Guid> bulkPickUID, IEnumerable<Guid> groupUID)
        {
            var result = ActionResultTemplates.Result();

            // 1. 取得 Bulk Pick 資料
            var getCollectionResult = this._BulkPickRepository.GetBulkPickCollection(bulkPickUID);

            if (getCollectionResult.Success && (getCollectionResult.Content?.Count() ?? 0) > 0)
            {
                // 2. 使用 Bulk Pick 資料紀錄的 TicketUID 取得 Move ticket 下的 Ticket Info
                var getTicketInfoListResult = this.TicketInfoRepository.GetList(new { TicketUID = getCollectionResult.Content.Select(o => o.TicketUID).ToArray() });
                if (getTicketInfoListResult.Success && (getTicketInfoListResult.Content?.Count() ?? 0) > 0)
                {
                    // 3. assign worker
                    result = this.TicketManager.BatchAssignWorkerAPI(new MaintainWorkderInnerParameters()
                    {
                        TicketInfoUID = getTicketInfoListResult.Content.Select(o => o.UID).ToArray(),
                        GroupUID = groupUID.ToArray()
                    });
                }
                else
                {
                    result.Message = "Fail to get Ticket Info data.";
                    if (getTicketInfoListResult.InnerException != null)
                    {
                        result.InnerException = getTicketInfoListResult.InnerException;
                    }
                }
            }
            else
            {
                result.Message = "Fail to get Bulk Pick data.";
                if (getCollectionResult.InnerException != null)
                {
                    result.InnerException = getCollectionResult.InnerException;
                }
            }

            return result;
        }

        public IActionResult<IEnumerable<IBulkPickInfoModel>> GetBulkPickInfoList(Guid bulkPickUID)
        {
            return this._BulkPickRepository.GetBulkPickInfoList(bulkPickUID);
        }

        public IActionResult<IEnumerable<IBulkPickInfoViewModel>> GetBulkPickInfoViewList(Guid bulkPickUID)
        {
            return this._BulkPickRepository.GetBulkPickInfoViewList(bulkPickUID);
        }

        public IActionResult<IEnumerable<string>> GetBulkPickIDByTicketInfo(IEnumerable<Guid> ticketInfoUID)
        {
            var result = ActionResultTemplates.Result<IEnumerable<string>>();

            var getBulkPickByTicketInfo = this._BulkPickRepository.GetBulkPickByTicketInfo(ticketInfoUID);

            if (getBulkPickByTicketInfo.Success && (getBulkPickByTicketInfo.Content?.Count() ?? 0) > 0)
            {
                result.Success = true;
                result.Content = getBulkPickByTicketInfo.Content.Select(o => o.ID);
            }
            else
            {
                result.Content = new List<string>();
                result.Message = "Fail to get Bulk Pick data.";
                if (getBulkPickByTicketInfo.InnerException != null)
                {
                    result.InnerException = getBulkPickByTicketInfo.InnerException;

                }
            }

            return result;
        }

        public IActionResult<bool> ChangeBulkPickStatus(IEnumerable<Guid> bulkPickTicketUID, int status)
        {
            return this._BulkPickRepository.ChangeBulkPickStatus(bulkPickTicketUID, status,
                this.AuthProvider.GetAuthenticationInfo().Account);
        }

        public IActionResult<IEnumerable<IBulkPickInfobyOutboundViewModel>> GetBulkPickInfoByTicketInfo(IEnumerable<Guid> ticketInfoUID)
        {
            return this._BulkPickRepository.GetBulkPickInfoByTicketInfo(ticketInfoUID);
        }
        public IActionResult<bool> IsBulkPickWorkOrderPayload(Guid WorkOrderPayloadUID)
        {
            return this.BulkPickWorkOrdrPayloadRelationRepository.Exist(WorkOrderPayloadUID);
        }
        public IActionResult<bool> AddBlukPickWorkOrderPayloadRelation(IEnumerable<IBulkPickWorkOrderPayloadRelationModel> modelCollection)
        {
            return this.BulkPickWorkOrdrPayloadRelationRepository.Create(modelCollection);
        }
        public IActionResult<IEnumerable<IBulkPickNotificationInfoModel>> GetBulkPickOriginalNotificationInfo(IEnumerable<Guid> ticketInfoUID)
        {
            return this.BulkPickWorkOrdrPayloadRelationRepository.GetBulkPickOriginalNotificationInfo(ticketInfoUID);
        }
    }
}

