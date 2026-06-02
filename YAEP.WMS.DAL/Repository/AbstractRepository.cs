using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public abstract class AbstractRepository<T> where T : class
    {
        const string EXCEPTION_CATEGORY = "DAL";
        protected readonly IRepositoryHandler<T> _Handler;
        public AbstractRepository(IRepositoryHandler<T> handler)
        {

            this._Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this._Handler.Instance.InitConnection();
        }

        public DataTable ToDataTable<R>(IEnumerable<R> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(R));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    table.Columns.Add(prop.Name, prop.PropertyType.GetGenericArguments()[0]);
                else
                    table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (R item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var v = props[i].GetValue(item);
                    if (v == null)
                    {
                        values[i] = DBNull.Value;
                    }
                    else
                    {
                        values[i] = v;
                    }
                }
                table.Rows.Add(values);
            }
            return table;
        }

        protected virtual List<string> getConditions(dynamic parameters)
        {
            return null;
        }
        protected IActionResult<bool> BatchInsertTable(DataTable batchdata, SqlCommand sqlCommand, int updateBatchSize = 3000)
        {
            return this.BatchChangeData(batchdata, sqlCommand, BatchChangeType.INSERT, updateBatchSize);
        }
        protected IActionResult<bool> BatchUpdateTable(DataTable batchdata, SqlCommand usqlCommand, int updateBatchSize = 3000)
        {
            batchdata.AcceptChanges();
            foreach (DataRow dr in batchdata.Rows)
            {
                foreach (DataColumn dc in batchdata.Columns)
                {
                    dr[dc.ColumnName] = dr[dc.ColumnName];
                }

            }
            return this.BatchChangeData(batchdata, usqlCommand, BatchChangeType.UPDATE, updateBatchSize);
        }
        protected IActionResult<bool> BatchDeleteTable(DataTable batchdata, SqlCommand sqlCommand, int updateBatchSize = 3000)
        {
            batchdata.AcceptChanges();
            foreach (DataRow dr in batchdata.Rows)
            {
                dr.Delete();
            }
            return this.BatchChangeData(batchdata, sqlCommand, BatchChangeType.DELETE, updateBatchSize);
        }
        protected IActionResult<bool> BatchChangeData(DataTable batchdata, SqlCommand sqlCommand, BatchChangeType changeType, int updateBatchSize = 3000)
        {
            var rs = ActionResultTemplates.Result<bool>();
            rs.Content = true;

            //var adapter = this.GetDbDataAdapter();
            var adapter = new SqlDataAdapter();

            if (changeType == BatchChangeType.INSERT)
            {
                adapter.InsertCommand = sqlCommand;
                adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                if (this._Handler.Instance.Transaction != null)
                {
                    adapter.InsertCommand.Transaction = this._Handler.Instance.Transaction as SqlTransaction;
                }
            }
            else if (changeType == BatchChangeType.UPDATE)
            {

                adapter.UpdateCommand = sqlCommand;
                adapter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;
                if (this._Handler.Instance.Transaction != null)
                {
                    adapter.UpdateCommand.Transaction = (SqlTransaction)this._Handler.Instance.Transaction;
                }
            }
            else if (changeType == BatchChangeType.DELETE)
            {
                adapter.DeleteCommand = sqlCommand;
                adapter.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;
                if (this._Handler.Instance.Transaction != null)
                {
                    adapter.DeleteCommand.Transaction = (SqlTransaction)this._Handler.Instance.Transaction;
                }
            }

            adapter.UpdateBatchSize = updateBatchSize;
            adapter.Update(batchdata);
            rs.Success = rs.Content;

            return rs;
        }
        protected IDbDataAdapter GetDbDataAdapter()
        {
            var factory = System.Data.Common.DbProviderFactories
                   .GetFactory(this._Handler.Instance.Connection as System.Data.Common.DbConnection);
            return factory.CreateDataAdapter();
        }
        private SqlDbType GetDBType(System.Type theType)
        {
            System.Data.SqlClient.SqlParameter p1;
            System.ComponentModel.TypeConverter tc;
            p1 = new System.Data.SqlClient.SqlParameter();
            tc = System.ComponentModel.TypeDescriptor.GetConverter(p1.DbType);
            if (tc.CanConvertFrom(theType))
            {
                p1.DbType = (DbType)tc.ConvertFrom(theType.Name);
            }
            else
            {
                //Try brute force
                try
                {
                    p1.DbType = (DbType)tc.ConvertFrom(theType.Name);
                }
                catch (Exception)
                {
                    //Do Nothing; will return NVarChar as default
                }
            }
            return p1.SqlDbType;
        }
        public virtual IActionResult<bool> Create(T model)
        {
            var resultContainer = ActionResultTemplates.Result<bool>();

            try
            {
                bool success = this._Handler.CreateByDynamic(model);

                resultContainer.Success = success;
                resultContainer.Content = success;
            }
            catch (Exception ex)
            {
                resultContainer.Message = ex.Message;
                resultContainer.InnerException = ex;
            }

            return resultContainer;
        }
        public IActionResult<bool> Delete(object UID)
        {
            var resultContainer = ActionResultTemplates.Result<bool>();

            try
            {
                bool success = this._Handler.Delete(UID);

                resultContainer.Success = success;
                resultContainer.Content = success;
            }
            catch (Exception ex)
            {
                resultContainer.Message = "Error";
                resultContainer.InnerException = ex;
            }

            return resultContainer;
        }
        public IActionResult<bool> Update(T model)
        {
            var resultContainer = ActionResultTemplates.Result<bool>();
            try
            {
                bool success = this._Handler.Update(model);

                resultContainer.Success = success;
                resultContainer.Content = success;
            }
            catch (Exception ex)
            {
                resultContainer.Message = "Error";
                resultContainer.InnerException = ex;
            }

            return resultContainer;
        }
        protected bool IsPropertyExist(dynamic obj, string key)
        {
            return ((IDictionary<String, object>)obj).ContainsKey(key);
        }
        protected void OnExpcetion(Exception exception, string message = "")
        {
            if (this.Tracehandler != null)
            {
                this.Tracehandler.CategoryName = EXCEPTION_CATEGORY;
                if (this.ExceptionNotify != null)
                {
                    this.ExceptionNotify(this, new ExceptionArgs
                    {
                        Exception = exception,
                        Message = message
                    });
                }
                Tracehandler.OnException(exception, message);
            }
            else
            {

            }

        }


        protected IExceptionTraceHandler Tracehandler { get; set; }
        protected event EventHandler<ExceptionArgs> ExceptionNotify;

    }
}
