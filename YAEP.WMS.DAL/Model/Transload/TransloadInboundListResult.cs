using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    /// <summary>
    /// <see cref="ITransloadInboundListResult"/> 的實作：包裝 GetTransloadInboundList 的三個結果集。
    /// </summary>
    internal class TransloadInboundListResult : ITransloadInboundListResult
    {
        public IEnumerable<ITransloadInboundRowViewModel> Rows { get; set; }
        public IEnumerable<ITransloadInboundPackageViewModel> OnhandPackages { get; set; }
        public IEnumerable<ITransloadInboundPackageViewModel> ReceivedPackages { get; set; }
    }
}
