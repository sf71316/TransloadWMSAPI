using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class ProductThirdPartySearchRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string[] ItemIDs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid[] ItemUID { get; set; }

        public Guid? CustomerUID { get; set; }

    }
}