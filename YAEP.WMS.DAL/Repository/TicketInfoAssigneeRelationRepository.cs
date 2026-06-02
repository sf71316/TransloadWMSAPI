using Dapper;
using DapperParameters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class TicketInfoAssigneeRelationRepository<T> : AbstractRepository<T>, ITicketInfoAssigneeRelationRepository
         where T : class, ITicketInfoAssigneeRelationModel
    {
        public TicketInfoAssigneeRelationRepository(IRepositoryHandler<T> handler, IAuthenticationProvider authenticationProvider) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;
            this.AuthProvider = authenticationProvider;
        }
        private IAuthenticationProvider AuthProvider { get; set; }
        /// <summary>
        /// Connection 不會關
        /// </summary>
        /// <param name="Parametes"></param>
        /// <param name="isIgnoreCheck"></param>
        /// <returns></returns>
        public IActionResult<bool> AddWorkder(IMaintainWorkderParameters Parametes, bool isIgnoreCheck = false)
        {
            var rs = ActionResultTemplates.Result<bool>();

            List<TicketInfoAssigneeRelationInnerModel> collection = new List<TicketInfoAssigneeRelationInnerModel>();

            foreach (var item in Parametes.TicketInfoUID)
            {
                foreach (var item2 in Parametes.GroupUID)
                {
                    TicketInfoAssigneeRelationInnerModel e = new TicketInfoAssigneeRelationInnerModel();
                    if (!isIgnoreCheck && CheckHasWorkder(item, item2).Success)
                    {
                        continue;
                    }
                    e.UID = Guid.NewGuid();
                    e.TicketInfoUID = item;
                    e.Status = (int)TicketInfoAssigneeRelationStatus.Active;
                    e.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                    e.CreatedOn = DateTime.Now;
                    e.GroupUID = item2;
                    collection.Add(e);
                }
            }

            bool _isAllComplete = true;
            rs.Success = rs.Content = _isAllComplete;
            #region old code

            //var query = @"INSERT INTO [dbo].[WMS_TicketInfo_Assignee_Relation]
            //            ([UID]
            //            ,[GroupUID]
            //            ,[TicketInfoUID]
            //            ,[Status]
            //            ,[Description]
            //            ,[CreatedBy]
            //            ,[CreatedOn]
            //            ,[ModifiedBy]
            //            ,[ModifiedOn])
            //      VALUES
            //            (@UID, 
            //            @GroupUID,
            //            @TicketInfoUID,
            //            @Status, 
            //            @Description,
            //            @CreatedBy,
            //            @CreatedOn, 
            //            @ModifiedBy, 
            //            @ModifiedOn)";
            //_isAllComplete = this._Handler.BatchCreateByDynamic(collection, null);
            //var collectiongrp = collection.Select((itemUID, index) => new { itemUID, index })
            //                                .GroupBy(g => g.index / 2000, i => i.itemUID);
            //foreach (var grp in collectiongrp)
            //{
            //    _isAllComplete &= this._Handler.Instance.Execute(query, grp)>0;
            //}
            //_isAllComplete = this._Handler.Instance.Execute(query, collection) > 0;
            //var dt = ToDataTable<TicketInfoAssigneeRelationInnerModel>(collection);
            //if (this._Handler.Instance.Connection.State == System.Data.ConnectionState.Closed)
            //    this._Handler.Instance.Connection.Open();
            //bool success = this._Handler.BatchCreateByDynamic(models, this._Handler.GetTableName());

            //using (var sqlBulkCopy = new SqlBulkCopy(this._Handler.Instance.Connection as SqlConnection,
            //    SqlBulkCopyOptions.Default, this._Handler.Instance.Transaction as SqlTransaction))
            //{
            //    sqlBulkCopy.BulkCopyTimeout = 3600;
            //    sqlBulkCopy.BatchSize = 10000;
            //    foreach (DataColumn column in dt.Columns)
            //    {
            //        sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            //    }
            //    sqlBulkCopy.DestinationTableName = "WMS_TicketInfo_Assignee_Relation";
            //    sqlBulkCopy.WriteToServer(dt);
            //}
            //if (this._Handler.Instance.Connection.State == System.Data.ConnectionState.Open)
            //    this._Handler.Instance.Connection.Close();
            #endregion
            var query = "sp_InsertTicketInfoAssigneeRelationWithTvp";

            var parameters = new DynamicParameters();

            //parameters.Add("DataList", collection);
            parameters.AddTable("@DataList", "TVP_TicketInfoAssigneeRelation", collection);
            this._Handler.Instance.Execute(query, parameters, commandType: CommandType.StoredProcedure);
            rs.Success = rs.Content = true;

            // rs.Success = rs.Content = _isAllComplete;


            return rs;
        }
        public IActionResult<bool> CheckHasWorkder(Guid TicketInfoUID, Guid GroupUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var _worker = this._Handler.RetrieveCollectionByDynamicConditions(new
                {
                    TicketInfoUID = TicketInfoUID,
                    GroupUID = GroupUID
                }).Where(p => p.Status > 0);
                rs.Success = rs.Content = _worker.Count() > 0;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<bool> ClearAllWorkder(Guid[] TicketInfoUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                bool _isAllComplete = true;
                rs.Success = rs.Content = _isAllComplete;
                List<TicketInfoAssigneeRelationInnerModel> collection = new List<TicketInfoAssigneeRelationInnerModel>();
                foreach (var item in TicketInfoUID)
                {
                    this._Handler.DeleteByDynamicConditions(new { TicketInfoUID = item });
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<IEnumerable<ITicketInfoAssigneeRelationModel>> GetAssignedList(Guid[] TicketInfoUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoAssigneeRelationModel>>();
            try
            {
                var query = "SELECT * FROM WMS_TicketInfo_Assignee_Relation WHERE TicketInfoUID IN @TicketInfoUID AND Status>0";
                rs.Content = this._Handler.Instance.Query<TicketInfoAssigneeRelationModel>(query, new { TicketInfoUID = TicketInfoUID });
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<IEnumerable<ITicketInfoModel>> GetRelationTicketInfo(Guid TicketInfoUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                //修改關聯取得相關TickInfo
                var query = @"SELECT [WTI2].* FROM  WMS_TicketInfo AS [WTI] 
                INNER JOIN WMS_Ticket AS [WT] ON [WT].UID=[WTI].TicketUID
                INNER JOIN WMS_TicketInfo AS [WTI2] ON [WT].UID=[WTI2].TicketUID
                WHERE [WTI].UID in @UID";
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query, new { UID = TicketInfoUID });
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<bool> RemoveWorkder(Guid[] tauids)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                bool _isAllComplete = true;
                rs.Success = rs.Content = _isAllComplete;
                List<TicketInfoAssigneeRelationInnerModel> collection = new List<TicketInfoAssigneeRelationInnerModel>();
                _isAllComplete &= this._Handler.DeleteByDynamicConditions(new
                {
                    UID = tauids
                });
                rs.Success = rs.Content = _isAllComplete;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
    }
}
