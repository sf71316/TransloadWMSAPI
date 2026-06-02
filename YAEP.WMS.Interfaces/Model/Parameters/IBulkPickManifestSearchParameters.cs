using System;
using System.Collections.Generic;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickManifestSearchParameters
    {
        /// <summary>
        /// 
        /// </summary>
        Guid? CustomerUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string RefNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string DateBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DateTime? StartDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DateTime? EndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string OptionText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string OptionValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<int> TicketInfoStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<int> TicketInfoType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<int> ManifestType { get; set; }
    }
}
