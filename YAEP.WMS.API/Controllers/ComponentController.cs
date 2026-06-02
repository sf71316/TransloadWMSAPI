using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Common.DI;
using YAEP.Core.Party.DI;
using YAEP.Identities.Constants;
using YAEP.Identities.DI;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Code;
using YAEP.WMS.API.Models;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.DI.Agent;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// 共用套件相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/General")]
    public class ComponentController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public ComponentController()
        {
            this._CommonFactory = new Lazy<CommonFactory>(() => FactoryUtils.GetCommonFactory(base.GetAuthenticationInfo()));
            this._PartyFactory = new Lazy<PartyFactory>(() => FactoryUtils.GetPartyFactory(base.GetAuthenticationInfo()));
            this._IdentityFactory = new Lazy<IdentityFactory>(() => FactoryUtils.GetIdentityFactory(base.GetAuthenticationInfo()));

        }

        #region Factories

        private readonly Lazy<CommonFactory> _CommonFactory;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private CommonFactory GetCommonFactory()
        {
            return this._CommonFactory.Value;
        }

        private readonly Lazy<PartyFactory> _PartyFactory;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private PartyFactory GetPartyFactory()
        {
            return this._PartyFactory.Value;
        }

        private readonly Lazy<IdentityFactory> _IdentityFactory;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IdentityFactory GetIdentityFactory()
        {
            return this._IdentityFactory.Value;
        }

        #endregion
        #region Mainifest

        #endregion
        #region Inventory

        #endregion
        #region Warehouse

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCountry")]
        public IHttpActionResult GetCountry()
        {
            try
            {
                var collection = DrKnowAll.GetCountry().Select(o => new { o.UID, o.ID, o.Name, o.EnglishName });
                var apiResult = base.GetSuccessResult(collection);
                return base.Json(apiResult);
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetState")]
        public IHttpActionResult GetState(string countryId)
        {
            try
            {
                countryId = countryId.GetFilterXSSstring();
                var collection = DrKnowAll.GetState().Where(o =>
                {
                    return o.Country.Equals(countryId, StringComparison.OrdinalIgnoreCase) ||
                                            (
                                                o.Country.Equals(DrKnowAll.GetCountry().FirstOrDefault(c =>
                                                                        c.EnglishName.Equals(countryId, StringComparison.OrdinalIgnoreCase))?.ID, StringComparison.OrdinalIgnoreCase)
                                            );
                }).Select(o => new { o.UID, o.ID, o.Name });
                var apiResult = base.GetSuccessResult(collection);
                return base.Json(apiResult);
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="stateId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCity")]
        public IHttpActionResult GetCity(string countryId, string stateId)
        {
            try
            {
                countryId = countryId.GetFilterXSSstring();
                stateId = stateId.GetFilterXSSstring();
                var collection = DrKnowAll.GetCity().Where(o =>
                {
                    return o.Country.Equals(countryId, StringComparison.OrdinalIgnoreCase) &&
                                o.State.Equals(stateId, StringComparison.OrdinalIgnoreCase);
                }).Select(o => new { o.UID, o.ID, o.Name });
                var apiResult = base.GetSuccessResult(collection);
                return base.Json(apiResult);
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="stateId"></param>
        /// <param name="cityIdOrName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetZip")]
        public IHttpActionResult GetZip(string countryId, string stateId, string cityIdOrName)
        {
            try
            {
                countryId = countryId.GetFilterXSSstring();
                stateId = stateId.GetFilterXSSstring();
                cityIdOrName = cityIdOrName.GetFilterXSSstring();
                var collection = DrKnowAll.GetZip().Where(o =>
                {
                    return o.Country.Equals(countryId, StringComparison.OrdinalIgnoreCase) &&
                                o.State.Equals(stateId, StringComparison.OrdinalIgnoreCase) &&
                                (
                                    o.City.Equals(cityIdOrName, StringComparison.OrdinalIgnoreCase) ||
                                    o.City.Equals($"{stateId}-{cityIdOrName}", StringComparison.OrdinalIgnoreCase)
                                );
                }).Select(o => new { o.UID, o.ID, Name = o.ID });
                var apiResult = base.GetSuccessResult(collection);
                return base.Json(apiResult);
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// 回傳Warehouse Name 清單
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetWarehouseNameList")]
        public IHttpActionResult GetWarehouseNameList()
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            var rs = _instance.GetWarehouseNameList();

            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAreaNameList")]
        public IHttpActionResult GetAreaNameList(Guid wuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().AreaManager;
            var _parameters = this.DIContainer.WarehouseFactory.GenerateModel<IWarehouseComponentParameters>();
            _parameters.ConditionUID = wuid;
            var rs = _instance.GetAreaNameList(_parameters);

            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSlotNameList")]
        public IHttpActionResult GetSlotNameList(Guid buid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;
            var _parameters = this.DIContainer.WarehouseFactory.GenerateModel<IWarehouseComponentParameters>();
            _parameters.ConditionUID = buid;
            var rs = _instance.GetSlotNameList(_parameters);

            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="auid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBinNameList")]
        public IHttpActionResult GetBinNameList(Guid auid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;
            var _parameters = this.DIContainer.WarehouseFactory.GenerateModel<IWarehouseComponentParameters>();
            _parameters.ConditionUID = auid;
            var rs = _instance.GetBinNameList(_parameters);

            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }

        }
        #endregion
        #region Ticket

        #endregion
        #region Product

        #endregion
        #region Party
        /// <summary>
        /// get customer name list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerNameList")]
        public IHttpActionResult GetCustomerNameList()
        {
            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();

                var result = manager.GetParties(YAEP.Core.Party.Constants.PartyTypeCategories.Customer);

                if (result.Success)
                {
                    var collection = result.Content.Select(o => new { o.UID, o.ID, o.Name });
                    var apiResult = base.GetSuccessResult(collection);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, result.Message);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }

        }

        #endregion
        #region Identities

        /// <summary>
        /// get group name list
        /// </summary>
        /// <param name="groupType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetGroupNameList")]
        public IHttpActionResult GetGroupNameList(GroupTypes? groupType = null)
        {
            var authInfo = base.GetAuthenticationInfo();

            try
            {
                var arrGroupUID = authInfo.GetGroupKeys(groupType);

                if (arrGroupUID != null)
                {
                    var groups = DrKnowAll.GetGroup(arrGroupUID);

                    if (groups?.Count() > 0)
                    {
                        var collection = groups.Select(o => new { o.UID, o.ID, o.Name });
                        var apiResult = base.GetSuccessResult(collection);
                        return base.Json(apiResult);
                    }
                }

                var failResult = base.GetSuccessResult(new object[] { });
                failResult.IsComplete = false;
                return base.Json(failResult);
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        #endregion
    }
}
