using System;
using YAEP.Interfaces;

namespace YAEP.WMS.Api.Code
{
    internal class DrKnowAllAuthenticationInfo : IAuthenticationInfo
    {
        public DrKnowAllAuthenticationInfo()
        {

        }

        public Guid UID { get; set; } = Guid.Empty;
        public string Account { get; set; } = "System-DrKnowAll";
        public string Identification { get; set; } = "D3737521-E6BC-40B8-98EC-043E158FE596";
        public DateTime LoginedTime { get; set; } = DateTime.UtcNow;
        public string MemberName { get; set; } = "Dr Know All";
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddHours(240000);
        public string Token { get; set; }
    }
}
