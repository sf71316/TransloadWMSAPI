using System;
using System.Collections.Generic;

namespace YAEP.WMS.API.Models.Response
{
    /// <summary>
    /// Transload 建立收貨回應（對應主文件 §C3）：建立的 Manifest 與每櫃 Vessel。
    /// </summary>
    public class TransloadReceivingResponseModel
    {
        public Guid ManifestUID { get; set; }
        public List<TransloadReceivingVesselResult> Vessels { get; set; }
    }

    /// <summary>單一櫃建立結果。</summary>
    public class TransloadReceivingVesselResult
    {
        public Guid VesselUID { get; set; }
        /// <summary>櫃號（= Vessel.RefNo）。</summary>
        public string ConNo { get; set; }
        public int Status { get; set; }
    }
}
