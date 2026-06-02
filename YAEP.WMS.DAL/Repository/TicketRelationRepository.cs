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
    public class TicketRelationRepository<T> : AbstractRepository<T>, ITicketRelationRepository
        where T : class, ITicketRelationModel
    {
        public TicketRelationRepository(IRepositoryHandler<T> handler) : base(handler)
        {
        }

        public IActionResult<bool> Add(IEnumerable<ITicketRelationModel> Collection)
        {
            var query = @"INSERT INTO WMS_TIcket_Relation (UID,ParentUID,TicketUID,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn) 
                        VALUES(@UID,@ParentUID,@TicketUID,@Status,@CreatedBy,@CreatedOn,@ModifiedBy,@ModifiedOn)";
            var rs = ActionResultTemplates.Result<bool>();
            var dt = this.ToDataTable<ITicketRelationModel>(Collection);
            SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
            cmd.Parameters.Add("@ParentUID", SqlDbType.UniqueIdentifier, 16, "ParentUID");
            cmd.Parameters.Add("@TicketUID", SqlDbType.UniqueIdentifier, 16, "TicketUID");
            cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
            cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
            cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
            cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
            cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");
            rs = this.BatchInsertTable(dt, cmd);

            return rs;
        }
        public IActionResult<bool> Delete(Guid uid)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(new { UID = uid });
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
        public IActionResult<IEnumerable<ITicketRelationModel>> GetTicketRelationList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketRelationModel>>();
            try
            {
                var result = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > 0);

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
        public IActionResult<int?> GetParentTicketServiceType(Guid ticketUID)
        {

            var rs = ActionResultTemplates.Result<int?>();
            try
            {
                var query = @"SELECT [WT].* FROM WMS_TIcket_Relation AS [WTR] 
                            INNER JOIN WMS_Ticket AS [WT] ON [WTR].ParentUID=[WT].UID
                             where TicketUID=@TicketUID";
                var result = this._Handler.Instance.QueryFirst<dynamic>(query, new { TicketUID = ticketUID });
                if (result != null)
                    rs.Content = result.Type;
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
    }
}
