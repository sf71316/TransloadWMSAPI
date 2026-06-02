using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace YAEP.WMS.BLL.Interfaces
{
    internal interface ITransacationScope
    {
        /// <summary>
        /// 註記TransactionScope 已經啟用的旗標
        /// </summary>
        bool ExistTransactionScope { get; set; }
        TransactionScope GetNewTransactionScope(TransactionScopeOption scope = TransactionScopeOption.Required);
        TransactionScope GetNewTransactionScope(IsolationLevel isolationLevel, TransactionScopeOption scope = TransactionScopeOption.Required);
        /// <summary>
        /// 取得系統預設Transaction Scope
        /// 預設隔離層級:Snapshot
        /// 預設操作逾期時間:5min
        /// </summary>
        /// <param name="scope">隔離層級</param>
        /// <returns></returns>
        TransactionScope GetDefaultTransactionScope(TransactionScopeOption scope = TransactionScopeOption.Required);
        /// <summary>
        /// 取得系統預設Transaction Scope
        /// 預設隔離層級:Snapshot
        /// 預設操作逾期時間:5min
        /// </summary>
        /// <param name="timeSpan">逾期時間</param>
        /// <param name="scope">隔離層級</param>
        /// <returns></returns>
        TransactionScope GetDefaultTransactionScope(TimeSpan timeSpan, TransactionScopeOption scope = TransactionScopeOption.Required);
        /// <summary>
        /// 取得系統預設Transaction Scope
        /// 預設隔離層級:Snapshot
        /// 預設操作逾期時間:5min
        /// </summary>
        /// <param name="timeout">逾期時間(秒)</param>
        /// <param name="scope">隔離層級</param>
        /// <returns></returns>
        TransactionScope GetDefaultTransactionScope(int timeout, TransactionScopeOption scope = TransactionScopeOption.Required);
        /// <summary>
        /// 取得系統預設Transaction Scope
        /// 預設隔離層級:Snapshot
        /// 預設操作逾期時間:5min
        /// </summary>
        /// <param name="isolationLevel">隔離層級</param>
        /// <param name="scope"></param>
        /// <returns></returns>
        TransactionScope GetDefaultTransactionScope(IsolationLevel isolationLevel, TransactionScopeOption scope = TransactionScopeOption.Required);
        /// <summary>
        /// 取得系統預設Transaction Scope
        /// 預設隔離層級:Snapshot
        /// 預設操作逾期時間:5min
        /// </summary>
        /// <param name="isolationLevel">隔離層級</param>
        /// <param name="timeout">逾期時間(秒)</param>
        /// <param name="scope"></param>
        /// <returns></returns>
        TransactionScope GetDefaultTransactionScope(IsolationLevel isolationLevel, int timeout, TransactionScopeOption scope = TransactionScopeOption.Required);
        /// <summary>
        /// 自定 Transaction Scope
        /// </summary>
        /// <param name="scope">隔離層級</param>
        /// <param name="options">交易區間設定</param>
        /// <returns></returns>
        TransactionScope GetNewTransactionScope(TransactionScopeOption scope, TransactionOptions options);
        TransactionOptions GetDefaultTransactionScopeOption(IsolationLevel? isolationLevel = null, TimeSpan? timeSpan = null);
        void Complete(TransactionScope scope);
    }
}
