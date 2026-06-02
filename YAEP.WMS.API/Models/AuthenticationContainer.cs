using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Controllers.Api;

namespace YAEP.WMS.Api.Models
{
    internal class AuthenticationContainer
    {
        public bool success { get; set; }
        public AuthenticationInfo Data { get; set; }
    }
}