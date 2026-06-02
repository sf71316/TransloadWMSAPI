using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IPickAllParameters
    {
        /// <summary>
        /// 最多支援1000筆
        /// </summary>
        IEnumerable<Guid> BolUID { get; set; }

        /// <summary>
        /// 最多支援1000筆
        /// </summary>
        IEnumerable<Guid> VesselUID { get; set; }

        /// <summary>
        /// 最多支援1000筆
        /// </summary>
        IEnumerable<Guid> WorkPayloadUID { get; set; }

        int[] TicketInfoStatus { get; set; }
    }
}
