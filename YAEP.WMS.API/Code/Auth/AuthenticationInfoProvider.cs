using System;
using YAEP.Interfaces;


namespace YAEP.WMS.Controllers.Api
{
    internal class AuthenticationInfoProvider : IAuthenticationProvider
    {
        private readonly IAuthenticationInfo _AuthenticationInfo;

        public AuthenticationInfoProvider()
        {
        }

        public AuthenticationInfoProvider(IAuthenticationInfo authenticationInfo)
        {
            this._AuthenticationInfo = authenticationInfo;
        }
        public IAuthenticationInfo GetAuthenticationInfo()
        {
            return this._AuthenticationInfo;
        }
    }
}
