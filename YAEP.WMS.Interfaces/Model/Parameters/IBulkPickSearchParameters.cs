using System;
using System.Collections.Generic;

namespace YAEP.WMS.Interfaces
{
    public interface IBulkPickSearchParameters
    {
        /// <summary>
        /// 
        /// </summary>
        Guid[] UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string PartyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string CustomerPartyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<int> Status { get; set; }
    }
}
