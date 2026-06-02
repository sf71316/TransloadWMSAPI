using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using YAEP.Interfaces;
using YAEP.WMS.Api.Models;
using YAEP.WMS.Controllers.Api;
using YAEP.WMS.DI.Agent;
using Unity;
using Unity.Injection;
using Unity.Resolution;
using System.Net;
using System.Net.Http;
using YAEP.Package.Interfaces;
using Microsoft.Security.Application;
using YAEP.WMS.API.Code;
using YAEP.WMS.Language.Resources;
using YAEP.Identities.DI;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Text;
using System.Runtime.Caching;
using YAEP.Identities.Interfaces;
using YAEP.Identities.Constants;
using YAEP.Core.Party.Interfaces;
using YAEP.Core.Party.DI;

namespace YAEP.WMS.Api.Code
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractApiController : ApiController
    {
        private string CACHE_KEY = "DI_CONTAINER";
        protected readonly Lazy<IdentityFactory> _IdentityFactory;
        protected readonly Lazy<PartyFactory> _PartyFactory;
        /// <summary>
        /// 
        /// </summary>
        public AbstractApiController()
        {
            this._IdentityFactory = new Lazy<IdentityFactory>(() => FactoryUtils.GetIdentityFactory(GetAuthenticationInfo()));
            this._PartyFactory = new Lazy<PartyFactory>(() => FactoryUtils.GetPartyFactory(GetAuthenticationInfo()));

        }
        public IGroupManager GroupManager { get; set; }
        public IPartyManager PartyManager { get; set; }
        protected void InitDIRoot()
        {

            var authInfo = GetAuthenticationInfo();
            var factory = this.GetIdentityFactory();
            var partyfactory = this.GetPartyFactory();
            this.GroupManager = factory.CreateGroupManager();
            this.PartyManager = partyfactory.CreatePartyManager();
            this.DIContainer = GetDIInstance();
        }
        private DIRoot GetDIInstance()
        {
            DIRoot root = new DIRoot();
            root = root.InitRoot(
                new DefaultAppSettings(),
                 new ConnectionSettings(),
                this.GetAuthenticationInfo(),
                this.GetAuthenticationInfoProvider(),
                this.GroupManager,
                this.PartyManager);
            root.Container.RegisterInstance<Func<YAEP.Core.Item.Interfaces.IItemManager>>(() =>
            {
                return FactoryUtils.GetItemFactory(this.GetAuthenticationInfo()).CreateItemManager();
            });
            root.Container.RegisterInstance<YAEP.Core.Item.Interfaces.IItemManager>(FactoryUtils.GetItemFactory(this.GetAuthenticationInfo()).CreateItemManager());
            root.Container.RegisterInstance<YAEP.Package.Interfaces.IPackageManager>(FactoryUtils.GetPackageFactory(this.GetAuthenticationInfo()).CreatePackageManager());
            root.Container.RegisterInstance<YAEP.Package.Interfaces.IPackageUomManager>(FactoryUtils.GetPackageFactory(this.GetAuthenticationInfo()).CreatePackageUomManager());
            root.Container.RegisterInstance<YAEP.Core.Party.Interfaces.IPartyManager>(FactoryUtils.GetPartyFactory(this.GetAuthenticationInfo()).CreatePartyManager());
            root.Container.RegisterInstance<IPackageVersionManager>(FactoryUtils.GetPackageFactory(this.GetAuthenticationInfo())
                .CreatePackageVersionManager(new VersionSerialNumberGenerator(root.GetSequenceAgent())));
            root.Container.RegisterInstance<YAEP.WMS.Interfaces.IRefreshDrKnowAll>(new RefreshDrKnowAll());
            return root;
        }
        protected APIResult<T> GetSuccessResult<T>(T ResponseObject, string message = "", bool iscomplete = true)
        {
            APIResult<T> result = new APIResult<T>();
            result.IsComplete = iscomplete;
            result.ResponseTime = DateTime.Now;
            result.Message = message;
            result.Data = ResponseObject;
            return result;
        }
        protected APIResult<string> GetSuccessResult()
        {
            APIResult<string> result = new APIResult<string>();
            result.IsComplete = true;
            result.ResponseTime = DateTime.Now;
            result.Data = "";
            return result;
        }
        protected IHttpActionResult GetFailureResult(int ErrorCode, string ErrorMessage)
        {
            APIResult<string> result = new APIResult<string>();
            result.IsComplete = false;
            result.Code = ErrorCode;
            result.Message = ErrorMessage;
            result.ResponseTime = DateTime.Now;
            return Content(HttpStatusCode.BadRequest, result);
        }
        protected IHttpActionResult GetFailureResult<T>(T ResponseObject, string message = "", bool iscomplete = true)
        {
            APIResult<T> result = new APIResult<T>();
            result.IsComplete = iscomplete;
            result.ResponseTime = DateTime.Now;
            result.Message = message;
            result.Data = ResponseObject;
            return Content(HttpStatusCode.BadRequest, result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        protected IHttpActionResult GetDataNotFoundResult(int errorCode = -1)
        {
            APIResult<string> result = new APIResult<string>();
            result.IsComplete = false;
            result.Code = errorCode;
            result.Message = Resource.COMMON_DATA_NOT_FOUND;
            result.ResponseTime = DateTime.Now;
            return Content(HttpStatusCode.OK, result);
        }
        protected IHttpActionResult GetFailureResult<T>(int ErrorCode, string ErrorMessage, T obj)
        {
            APIResult<T> result = new APIResult<T>();
            result.IsComplete = false;
            result.Code = ErrorCode;
            result.Message = ErrorMessage;
            result.Data = obj;
            result.ResponseTime = DateTime.Now;
            return Content(HttpStatusCode.OK, result);
        }
        protected IdentityFactory GetIdentityFactory()
        {
            return this._IdentityFactory.Value;
        }
        protected PartyFactory GetPartyFactory()
        {
            return this._PartyFactory.Value;
        }
        protected DIRoot DIContainer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected IAuthenticationInfo GetAuthenticationInfo()
        {
            return this.Request?.GetAuthenticationInfo();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected IAuthenticationProvider GetAuthenticationInfoProvider()
        {
            return new AuthenticationInfoProvider(this.GetAuthenticationInfo());
        }
        protected bool IsLegalGuid(Guid? guid)
        {
            return guid.HasValue || (guid.HasValue && guid.Value != Guid.Empty);
        }
        protected string AntiXSSEncode(string content)
        {
            return Sanitizer.GetSafeHtmlFragment(content);
        }
        protected T AntiXSSEncode<T>(T obj) where T : class
        {
            try
            {
                if (obj != null)
                {
                    var array = obj.GetType()
                            .GetInterfaces()
                            .Where(t => t.IsGenericType
                            && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                    if (array.Count() == 0)
                    {
                        var props = obj.GetType().GetProperties();
                        foreach (var item in props)
                        {
                            if (item.PropertyType == typeof(string))
                            {

                                var getter = item.GetValueGetter<T>();
                                var setter = item.GetValueSetter<T>();
                                var value = Convert.ToString(getter(obj));
                                value = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(value));
                                setter(obj, this.AntiXSSEncode(value));
                            }
                        }
                    }
                    else
                    {
                        var _array = obj as IEnumerable<dynamic>;
                        foreach (var item in _array)
                        {
                            this.AntiXSSEncode(item);
                        }

                    }

                }
                return obj as T;
            }
            catch
            {
                return obj;
            }
        }

    }
}