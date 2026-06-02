using System;
using YAEP.Interfaces;

namespace YAEP.WMS.Controllers.Api
{
    internal class AuthenticationInfo : IAuthenticationInfo
    {
        public AuthenticationInfo()
        {
            // fake
            //this.UID = Guid.Empty;
            //this.Account = "unknow";
            //this.MemberName = "unknow";
        }

        public Guid UID { get; set; } = Guid.Empty;
        public string Account { get; set; } = "Tester";
        public string Identification { get; set; }  
        public DateTime LoginedTime { get; set; } = DateTime.UtcNow;
        public string MemberName { get; set; } = "Tester";
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddHours(1);
        public string Token { get; set; }
    }
}
