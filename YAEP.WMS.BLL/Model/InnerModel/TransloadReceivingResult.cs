using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    /// <summary><see cref="ITransloadReceivingResult"/> 的實作。</summary>
    internal class TransloadReceivingResult : ITransloadReceivingResult
    {
        public Guid ManifestUID { get; set; }
        public IEnumerable<IVesselModel> Vessels { get; set; }
    }
}
