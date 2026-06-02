using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using YAEP.WMS.BLL.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class TransactionScopeAgent : ITransacationScope
    {
        TransactionOptions _Options;
        public TransactionScopeAgent()
        {
            _Options = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.Snapshot,
                Timeout = new TimeSpan(0, 5, 0)
            };
        }
        public TransactionScopeAgent(IsolationLevel isolationLevel, TimeSpan timeSpan)
        {
            _Options = new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = timeSpan
            };
        }
        public bool ExistTransactionScope { get; set; }
        private TransactionScope _TransactionScope { get; set; }
        public void Complete(TransactionScope scope)
        {
            //ExistTransactionScope = false;
            if (scope != null)
                scope.Complete();
        }
        /// <summary>
        /// 取得全新Transactionscope 不受單一scope限制
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public TransactionScope GetNewTransactionScope(TransactionScopeOption scope = TransactionScopeOption.Required)
        {
            return new TransactionScope(scope, GetTransactionOptions());

        }
        /// <summary>
        /// 取得全新Transactionscope 不受單一scope限制
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public TransactionScope GetNewTransactionScope(IsolationLevel isolationLevel, TransactionScopeOption scope = TransactionScopeOption.Required)
        {
            return new TransactionScope(scope, GetTransactionOptions(isolationLevel: isolationLevel));

        }
        public TransactionScope GetDefaultTransactionScope(TransactionScopeOption scope = TransactionScopeOption.Required)
        {
            ExistTransactionScope = true;
            _TransactionScope = new TransactionScope(scope, GetTransactionOptions());
            return _TransactionScope;
        }
        public TransactionScope GetDefaultTransactionScope(IsolationLevel isolationLevel, TransactionScopeOption scope = TransactionScopeOption.Required)
        {
            ExistTransactionScope = true;
            _TransactionScope = new TransactionScope(scope, GetTransactionOptions(isolationLevel: isolationLevel));
            return _TransactionScope;
        }
        public TransactionScope GetDefaultTransactionScope(IsolationLevel isolationLevel, int timeout, TransactionScopeOption scope = TransactionScopeOption.Required)
        {
            ExistTransactionScope = true;
            _TransactionScope = new TransactionScope(scope, GetTransactionOptions(isolationLevel: isolationLevel, timeSpan: new TimeSpan(0, 0, timeout)));
            return _TransactionScope;
        }
        public TransactionScope GetDefaultTransactionScope(TimeSpan timeSpan, TransactionScopeOption scope = TransactionScopeOption.Required)
        {
            ExistTransactionScope = true;
            _TransactionScope = new TransactionScope(scope, GetTransactionOptions(timeSpan: timeSpan));
            return _TransactionScope;
        }

        public TransactionScope GetDefaultTransactionScope(int timeout, TransactionScopeOption scope = TransactionScopeOption.Required)
        {
            ExistTransactionScope = true;
            _TransactionScope = new TransactionScope(scope, GetTransactionOptions(timeSpan: TimeSpan.FromSeconds(timeout)));
            return _TransactionScope;
        }

        public TransactionScope GetNewTransactionScope(TransactionScopeOption scope, TransactionOptions options)
        {
            ExistTransactionScope = true;
            _TransactionScope = new TransactionScope(scope, options);
            return _TransactionScope;
        }
        protected TransactionOptions GetTransactionOptions(IsolationLevel? isolationLevel = null, TimeSpan? timeSpan = null)
        {
            if (timeSpan.HasValue)
            {
                _Options.Timeout = timeSpan.Value;
            }
            if (isolationLevel.HasValue)
            {
                _Options.IsolationLevel = isolationLevel.Value;
            }
            return _Options;
        }

        public TransactionOptions GetDefaultTransactionScopeOption(IsolationLevel? isolationLevel = null, TimeSpan? timeSpan = null)
        {
            return this.GetTransactionOptions(isolationLevel, timeSpan);
        }
    }
}
