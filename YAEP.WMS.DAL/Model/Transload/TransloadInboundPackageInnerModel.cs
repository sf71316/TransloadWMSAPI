using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    /// <summary>
    /// <see cref="ITransloadInboundPackageViewModel"/> 的 Dapper 對應實作（在倉/收貨的 (Manifest, PackageUID, Qty) 明細）。
    /// </summary>
    internal class TransloadInboundPackageInnerModel : ITransloadInboundPackageViewModel
    {
        public Guid ManifestUID { get; set; }
        public Guid PackageUID { get; set; }
        public int Qty { get; set; }
    }
}
