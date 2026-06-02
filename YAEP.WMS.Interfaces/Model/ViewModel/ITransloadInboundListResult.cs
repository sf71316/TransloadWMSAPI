using System.Collections.Generic;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// GetTransloadInboundList 的整批回傳：一次 QueryMultiple 往返取回三個結果集。
    /// Rows = 每 Manifest 一列（含 DB 端彙總）；
    /// OnhandPackages = 在倉 (Manifest, PackageUID, Qty) 明細（解析在倉 UOM 用）；
    /// ReceivedPackages = 收貨 DISTINCT (Manifest, PackageUID) 明細（解析實到 UOM 用）。
    /// </summary>
    public interface ITransloadInboundListResult
    {
        IEnumerable<ITransloadInboundRowViewModel> Rows { get; set; }
        IEnumerable<ITransloadInboundPackageViewModel> OnhandPackages { get; set; }
        IEnumerable<ITransloadInboundPackageViewModel> ReceivedPackages { get; set; }
    }
}
