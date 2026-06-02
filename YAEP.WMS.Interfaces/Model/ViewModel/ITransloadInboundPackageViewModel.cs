using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// Transload 入庫「每 (Manifest, PackageUID) 一列」的包裝明細，供 API 層解析該 manifest 的 UOM 名稱用。
    /// 在倉明細：Qty = SUM(PayLoad.Quantity)（Type=Stock 1、Status&gt;0）。
    /// 收貨明細：僅取 DISTINCT (Manifest, PackageUID)（Qty 不具意義、固定 0），用來判斷實到包裝單位。
    /// </summary>
    public interface ITransloadInboundPackageViewModel
    {
        Guid ManifestUID { get; set; }
        Guid PackageUID { get; set; }
        int Qty { get; set; }
    }
}
