using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.Core.Party.DI;
using YAEP.Core.Party.Models;
using YAEP.Core.Party.Constants;
using YAEP.Utilities;
using YAEP.WMS.API.Models.Request;
using YAEP.Common.DI;
using YAEP.Identities.DI;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// Customer API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Vendor")]
    public class VendorController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public VendorController()
        {
            this._PartyFactory = new Lazy<PartyFactory>(() => FactoryUtils.GetPartyFactory(base.GetAuthenticationInfo()));
            this._IdentityFactory = new Lazy<IdentityFactory>(() => FactoryUtils.GetIdentityFactory(base.GetAuthenticationInfo()));
        }

        #region Factories

        private readonly Lazy<IdentityFactory> _IdentityFactory;

        private IdentityFactory GetIdentityFactory()
        {
            return this._IdentityFactory.Value;
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

        #endregion

        /// <summary>
        /// get vendor list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetVendorList")]
        public IHttpActionResult GetVendorList()
        {
            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();

                var result = manager.GetParties(PartyTypeCategories.Vendor);

                if (result.Success)
                {
                    var collection = result.Content;
                    var apiResult = base.GetSuccessResult(collection);
                    return base.Json(apiResult);
                }
                else
                {
                    List<IPartyModel> empty = new List<IPartyModel>();
                    return base.Json(empty);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }

        }
        /// <summary>
        /// get vendor Info
        /// </summary>
        /// <param name="cid">unique identifier of vendor</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetVendorInfo")]
        public IHttpActionResult GetVendorInfo([FromUri]Guid cid)
        {
            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();

                var result = manager.GetParty(cid);

                if (result.Success && result.Content != null)
                {
                    var apiResult = base.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetDataNotFoundResult();
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddVendor")]
        public IHttpActionResult AddVendor([FromBody]VendorRequestModel vendor)
        {
            if (vendor == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (String.IsNullOrWhiteSpace(vendor.ID))
            {
                return base.GetFailureResult(-1, Resource.VENDOR_ID_EMPTY);
            }
            if (String.IsNullOrWhiteSpace(vendor.Company))
            {
                return base.GetFailureResult(-1, Resource.VENDOR_COMPANY_EMPTY);
            }

            var defaultTypeUID = this.getDefaultVendorTypeUID();

            if (defaultTypeUID == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_ERROR_OCCURED);
            }

            try
            {
                var groupUID = vendor.GroupUID == Guid.Empty ? this.getDefaultGroupUID() : vendor.GroupUID;

                var manager = this.GetPartyFactory().CreatePartyManager();

                var party = new PartyModel()
                {
                    UID = Guid.NewGuid(),
                    GroupUID = groupUID,
                    ID = vendor.ID,
                    Name = vendor.Company,
                    Email = vendor.Email,
                    Phone = vendor.Phone,
                    PhoneExtension = vendor.Ext,
                    Fax = vendor.Ext,
                    Country = vendor.Country,
                    State = vendor.State,
                    City = vendor.City,
                    Zip = vendor.Zip,
                    Address = vendor.Address,
                    Description = vendor.Description,
                    Status = (int)PartyStatus.Active,
                };

                var result = manager.Create(party, defaultTypeUID, null);

                if (result.Success)
                {
                    var apiResult = base.GetSuccessResult(party);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.VENDOR_ADD_VENDOR_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        // 暫時
        private Guid getDefaultVendorTypeUID()
        {
            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();
                var result = manager.GetPartyTypes(this.getDefaultGroupUID(), PartyTypeCategories.Vendor, "Vendor");
                return result.Content?.FirstOrDefault()?.UID ?? Guid.Empty;
            }
            catch (Exception ex)
            {

            }

            return Guid.Empty;
        }
        private Guid getDefaultGroupUID()
        {
            var authInfo = base.GetAuthenticationInfo();
            var manager = this.GetIdentityFactory().CreateGroupManager();
            var result = manager.GetGroupsByUser(authInfo.UID);
            if (result.Success)
            {
                return (result.Content?.OrderBy(o => o.Sort).FirstOrDefault()?.UID ?? Guid.Empty);
            }
            return Guid.Empty;
        }

        // 暫時
        private string defaultGroupUID = "231BCF53-AC0E-4311-AC00-76EC7CCB2C61";
    }
}
