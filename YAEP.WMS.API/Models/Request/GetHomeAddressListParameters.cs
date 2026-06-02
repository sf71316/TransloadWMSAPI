using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class GetHomeAddressListParameters : IGetHomeAddressListParameters
    {
        public Guid ItemUID { get; set; }
    }
}