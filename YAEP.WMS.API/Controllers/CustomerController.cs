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
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// Customer API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Customer")]
    public class CustomerController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public CustomerController()
        {
            this._PartyFactory = new Lazy<PartyFactory>(() => FactoryUtils.GetPartyFactory(base.GetAuthenticationInfo()));
        }

        #region Factories


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
        /// get customer list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerList")]
        public IHttpActionResult GetCustomerList([FromUri]CustomerSearchRequestModel requestModel)
        {
            try
            {
                // 無所屬 Group 直接回傳找不到
                var groupUIDs = this.getGroupsByUser();
                if ((groupUIDs?.Count() ?? 0) == 0)
                {
                    return base.GetDataNotFoundResult();
                }

                var manager = this.GetPartyFactory().CreatePartyManager();
                var parameters = this.GetPartyFactory().CreatePartyParameter();
                parameters.PartyTypeCategory = PartyTypeCategories.Customer;
                parameters.ListOfGroupUID.AddRange(groupUIDs);

                if (requestModel != null)
                {
                    parameters.UID = requestModel.UID;
                    parameters.ID = requestModel.ID;
                    parameters.Name = requestModel.Company;
                }

                var result = manager.GetParties(parameters);

                if (result.Success)
                {
                    var collection = result.Content;
                    var apiResult = base.GetSuccessResult(collection);
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
        /// get customer Info
        /// </summary>
        /// <param name="cid">unique identifier of customer</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerInfo")]
        public IHttpActionResult GetCustomerInfo([FromUri]Guid cid)
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
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddCustomer")]
        public IHttpActionResult AddCustomer([FromBody]CustomerRequestModel customer)
        {
            if (customer == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (String.IsNullOrWhiteSpace(customer.ID))
            {
                return base.GetFailureResult(-1, Resource.CUSTOMER_ID_EMPTY);
            }
            if (String.IsNullOrWhiteSpace(customer.Company))
            {
                return base.GetFailureResult(-1, Resource.CUSTOMER_COMPANY_EMPTY);
            }

            var defaultTypeUID = this.getDefaultCustomerTypeUID();

            if (defaultTypeUID == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_ERROR_OCCURED);
            }

            try
            {
                var groupUID = (customer.GroupUID == Guid.Empty ? this.getDefaultGroupUID() : customer.GroupUID);

                var manager = this.GetPartyFactory().CreatePartyManager();
                customer = this.AntiXSSEncode<CustomerRequestModel>(customer);
                var party = new PartyModel()
                {
                    UID = Guid.NewGuid(),
                    GroupUID = groupUID,
                    ID = customer.ID,
                    Name = customer.Company,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    PhoneExtension = customer.Ext,
                    Fax = customer.Ext,
                    Country = customer.Country,
                    State = customer.State,
                    City = customer.City,
                    Zip = customer.Zip,
                    Address = customer.Address,
                    Description = customer.Description,
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
                    return base.GetFailureResult(-1, Resource.CUSTOMER_ADD_CUSTOMER_FAIL);
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
        /// <param name="customer"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateCustomer")]
        public IHttpActionResult UpdateCustomer([FromBody]CustomerUpdateRequestModel customer)
        {
            if (customer == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (String.IsNullOrWhiteSpace(customer.ID))
            {
                return base.GetFailureResult(-1, Resource.CUSTOMER_ID_EMPTY);
            }
            if (String.IsNullOrWhiteSpace(customer.Company))
            {
                return base.GetFailureResult(-1, Resource.CUSTOMER_COMPANY_EMPTY);
            }
            if (customer.UID == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var groupUID = customer.GroupUID == Guid.Empty ? this.getDefaultGroupUID() : customer.GroupUID;

                var manager = this.GetPartyFactory().CreatePartyManager();
                customer = this.AntiXSSEncode(customer);

                var party = new PartyModel()
                {
                    UID = customer.UID,
                    ID = customer.ID,
                    Name = customer.Company,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    PhoneExtension = customer.Ext,
                    Fax = customer.Fax,
                    Country = customer.Country,
                    State = customer.State,
                    City = customer.City,
                    Zip = customer.Zip,
                    Address = customer.Address,
                    Description = customer.Description,

                    GroupUID = groupUID,
                    Status = (int)PartyStatus.Active,
                };

                var result = manager.Update(party);

                if (result.Success)
                {
                    var apiResult = base.GetSuccessResult(party);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.CUSTOMER_ADD_CUSTOMER_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        /// <summary>
        /// get customer address list
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerAddressList")]
        public IHttpActionResult GetCustomerAddressList([FromUri]Guid cid)
        {
            if (cid == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();

                var result = manager.GetPartyAddresses(cid);

                if (result.Success)
                {
                    var collection = result.Content.Select(o => new
                    {
                        o.UID,
                        o.PartyUID,
                        o.ID,
                        o.Name,
                        o.Description,
                        o.Type,
                        o.Status,
                        o.Country,
                        o.State,
                        o.City,
                        o.Zip,
                        o.Address1,
                        o.Address2,
                        o.Address3,
                        o.Email,
                        o.PhoneHome,
                        o.PhoneCell,
                        o.PhoneOffice,
                        o.FaxHome,
                        o.FaxOffice,
                        o.IsDefault,
                        TypeName = EnumerableData.GetName<PartyAddressTypes>(o.Type ?? 0),
                        StatusName = EnumerableData.GetName<PartyAddressStatus>(o.Status),
                    });
                    var apiResult = base.GetSuccessResult(collection);
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
        /// add customer address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddCustomerAddress")]
        public IHttpActionResult AddCustomerAddress([FromBody]CustomerAddressRequestModel address)
        {
            if (address == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (address.PartyUID == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            address = base.AntiXSSEncode(address);
            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();

                var model = new PartyAddressModel()
                {
                    UID = Guid.NewGuid(),
                    PartyUID = address.PartyUID,
                    ID = address.ID,
                    Name = address.Name,
                    Email = address.Email,
                    PhoneCell = address.PhoneCell,
                    PhoneHome = address.PhoneHome,
                    PhoneOffice = address.PhoneOffice,
                    FaxHome = address.FaxHome,
                    FaxOffice = address.FaxOffice,
                    Country = address.Country,
                    State = address.State,
                    City = address.City,
                    Zip = address.Zip,
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    Address3 = address.Address3,
                    Description = address.Description,
                    IsDefault = address.IsDefault ?? false,
                    Type = address.Type ?? 0,
                    Status = (int)PartyAddressStatus.Active,
                };

                var result = manager.CreateAddress(model);

                if (result.Success)
                {
                    var apiResult = base.GetSuccessResult(model);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.CUSTOMER_ADD_CUSTOMER_ADDRESS_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// update customer address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateCustomerAddress")]
        public IHttpActionResult UpdateCustomerAddress([FromBody]CustomerAddressRequestModel address)
        {
            if (address == null)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }
            if (address.UID == Guid.Empty)
            {
                return base.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();
                address = this.AntiXSSEncode(address);
                var model = new PartyAddressModel()
                {
                    UID = address.UID,
                    ID = address.ID,
                    Name = address.Name,
                    Email = address.Email,
                    PhoneCell = address.PhoneCell,
                    PhoneHome = address.PhoneHome,
                    PhoneOffice = address.PhoneOffice,
                    FaxHome = address.FaxHome,
                    FaxOffice = address.FaxOffice,
                    Country = address.Country,
                    State = address.State,
                    City = address.City,
                    Zip = address.Zip,
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    Address3 = address.Address3,
                    Description = address.Description,
                    IsDefault = address.IsDefault ?? false,
                    Type = address.Type ?? 0,
                    Status = (int)PartyAddressStatus.Active,
                };

                var result = manager.UpdateAddress(model);

                if (result.Success)
                {
                    var apiResult = base.GetSuccessResult(model);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.CUSTOMER_UPDATE_CUSTOMER_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// delete customer address
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteCustomerAddress")]
        public IHttpActionResult DeleteCustomerAddress([FromUri]Guid cid)
        {
            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();

                var result = manager.DeleteAddress(cid);

                if (result.Success)
                {
                    var apiResult = base.GetSuccessResult(result.Content);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, Resource.CUSTOMER_DELETE_CUSTOMER_FAIL);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// get customer address types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCustomerAddressTypes")]
        public IHttpActionResult GetCustomerAddressTypes()
        {
            try
            {
                var collection = YAEP.Utilities.EnumerableData.GetDataForGeneric(typeof(PartyAddressTypes));

                collection = collection?.Where(o => !(o.Value ?? String.Empty).ToString().Equals(((int)PartyAddressTypes.Unavailable).ToString()));

                var apiResult = base.GetSuccessResult(collection);

                return base.Json(apiResult);
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        // 暫時
        private Guid getDefaultCustomerTypeUID()
        {
            try
            {
                var manager = this.GetPartyFactory().CreatePartyManager();
                var result = manager.GetPartyTypes(this.getDefaultGroupUID(), PartyTypeCategories.Customer, "Customer");
                return result.Content?.FirstOrDefault()?.UID ?? Guid.Empty;
            }
            catch (Exception ex)
            {

            }

            return Guid.Empty;
        }
        private Guid getDefaultGroupUID()
        {
            var groups = this.getGroupsByUser();
            return groups?.FirstOrDefault() ?? Guid.Empty;
        } 
        private IEnumerable<Guid> getGroupsByUser()
        {
            var authInfo = base.GetAuthenticationInfo();
            var manager = this.GetIdentityFactory().CreateGroupManager();
            var result = manager.GetGroupKeysByUser(authInfo.UID);
            if (result.Success)
            {
                return result.Content;
            }
            return null;
        }
    }
}
