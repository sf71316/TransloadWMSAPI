using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CompentResponse : IComponentViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
    }
}