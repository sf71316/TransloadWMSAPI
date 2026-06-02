using System;
using System.Net.Http;
using System.Web;
using System.Web.Http.Description;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.API.Models.Request;
using YAEP.Identities.DI;
using YAEP.Identities.Models;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// 
    /// </summary>
    [Authentication]
    [ApiExplorerSettings(IgnoreApi = true)]
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [RoutePrefix("api/Identity")]
    public class IdentityController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public IdentityController()
        {
            this._IdentityFactory = new Lazy<IdentityFactory>(() => FactoryUtils.GetIdentityFactory(base.GetAuthenticationInfo()));
        }


        /// <summary>
        /// Test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Test()
        {
            return $"{this.GetType().Name.Replace("Controller", " - ")}{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddGroup([FromBody]GroupModel model)
        {
            if (String.IsNullOrWhiteSpace(model.ID))
            {
                return this.GetFailureResult(-1, "incorrect parameters");
            }
            if (String.IsNullOrWhiteSpace(model.Name))
            {
                return this.GetFailureResult(-1, "incorrect parameters");
            }


            var factory = this.GetIdentityFactory();
            var manager = factory.CreateGroupManager();

            var result = manager.CreateGroup(model);

            if (result.Success)
            {
                var apiResult = base.GetSuccessResult(model);
                return base.Json(apiResult);
            }
            else
            {
                return this.GetFailureResult(-1, result.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public string ParseJwtToken(string token)
        {
            string parsedToken = JwtHelper.Encode(token);
            return parsedToken;
        }


        #region Factories

        private readonly Lazy<IdentityFactory> _IdentityFactory;

        private IdentityFactory GetIdentityFactory()
        {
            return this._IdentityFactory.Value;
        }

        #endregion
    }
}
