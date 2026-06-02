using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Identities.Models;

namespace YAEP.WMS.API.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AddUserWithGroupsRequestModel : UserModel
    { 
        /// <summary>
        /// 
        /// </summary>
        public Guid[] GroupUID { get; set; }
    }

}