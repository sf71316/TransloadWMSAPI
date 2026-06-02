using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class ReplicationlogRepository<T> : AbstractRepository<T>, IReplicationlogRepository where T : class, IReplicationlogModel
    {
        public ReplicationlogRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> Add(IReplicationlogModel model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(model);
                rs.Success = rs.Content;
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

        public IActionResult<bool> BatchAdd(IEnumerable<IReplicationlogModel> models)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                #region regular method
                //     var query = @"
                //    INSERT INTO [dbo].[WMS_Replicationlog]
                //    ([UID]
                //    ,[ReplicateUID]
                //    ,[Action]
                //    ,[Operate]
                //    ,[ItemUID]
                //    ,[Quantity]
                //    ,[OriginalData]
                //    ,[BelongToUID]
                //    ,[IsComplete]
                //    ,[CreatedOn]
                //    ,[CreatedBy])
                //VALUES
                //    (@UID
                //    ,@ReplicateUID
                //    ,@Action
                //    ,@Operate
                //    ,@ItemUID
                //    ,@Quantity
                //    ,@OriginalData
                //    ,@BelongToUID
                //    ,@IsComplete
                //    ,@CreatedOn
                //    ,@CreatedBy)
                //    ";
                //    rs.Content = true;
                //    var index = 0;
                //    var grp = models.GroupBy(g => index++ / 2000);
                //    //if(this._Handler.Instance.Connection.State==System.Data.ConnectionState.Closed)
                //    //this._Handler.Instance.Connection.Open();
                //    //bool success = this._Handler.BatchCreateByDynamic(models, this._Handler.GetTableName());
                //    //if (this._Handler.Instance.Connection.State == System.Data.ConnectionState.Open)
                //    //    this._Handler.Instance.Connection.Close();
                //    foreach (var items in grp)
                //    {
                //        rs.Content &= this._Handler.Instance.Execute(query,
                //            items) > 0;
                //    }
                #endregion

                rs.Content = true;
                var dt = ToDataTable<IReplicationlogModel>(models);
                if (this._Handler.Instance.Connection.State == System.Data.ConnectionState.Closed)
                    this._Handler.Instance.Connection.Open();
                //bool success = this._Handler.BatchCreateByDynamic(models, this._Handler.GetTableName());
                
                using (var sqlBulkCopy = new SqlBulkCopy(this._Handler.Instance.Connection as SqlConnection))
                {
                    sqlBulkCopy.BulkCopyTimeout = 3600;
                    sqlBulkCopy.BatchSize = 10000;
                    foreach (DataColumn column in dt.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    sqlBulkCopy.DestinationTableName = "WMS_Replicationlog";
                    sqlBulkCopy.WriteToServer(dt);
                }
                if (this._Handler.Instance.Connection.State == System.Data.ConnectionState.Open)
                    this._Handler.Instance.Connection.Close();
                rs.Success = rs.Content;
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
