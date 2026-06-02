using System;
using YAEP.Interfaces;


namespace YAEP.WMS.Cache
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
