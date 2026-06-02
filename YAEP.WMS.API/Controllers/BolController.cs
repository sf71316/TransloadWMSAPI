using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// 卡車(BOL)相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Bol")]
    public class BolController : AbstractApiController
    {
        /// <summary>
        /// 刪除 BOL
        /// </summary>
        /// <param name="muid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteBOL")]
        public IHttpActionResult DeleteBOL([FromUri]Guid[] uid)
        {
            InitDIRoot();
            if (uid != null)
            {
                var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
                var _parameters = new BolDeleteParameters();
                _parameters.UID = uid;
                var rs = _instance.DeleteBolAPI(_parameters);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
            else
            {
                return this.BadRequest(Resource.MANIFEST_COMMON_UNIQUE_NULL);
            }

        }

        /// <summary>
        /// 新增 Bol
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddBol")]

        public IHttpActionResult AddBol(BOLRequestModel Model)
        {
            InitDIRoot();
            Model = this.AntiXSSEncode(Model);
            if (!Model.ManifestUID.HasValue && Model.ManifestUID.Value == Guid.Empty)
            {
                var result = this.GetFailureResult(-1, "Manifest UID must have");
                return result;
            }
            if (!Model.WarehouseUID.HasValue && Model.WarehouseUID.Value == Guid.Empty)
            {
                var result = this.GetFailureResult(-1, "Warehouse UID must have");
                return result;
            }
            if (!(new int[] { 1, 2 }.Contains(Model.Type)))

            {
                var result = this.GetFailureResult(-1, "Type fail must 1 (inbound) or 2 (outbound)");
                return result;
            }
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var winstance = this.DIContainer.WarehouseFactory.CreateWarehouseManger();
            var warehouse = winstance.WarehouseManager.GetWarehouse(Model.WarehouseUID.Value);
            var _model = this.DIContainer.ManifestFactory.GenerateModel<IBolModel>();
            if (Model.UID == Guid.Empty)
                _model.UID = Guid.NewGuid();
            else
                _model.UID = Model.UID;
            _model.Contact = Model.Contact;
            _model.Name = Model.Name;
            _model.Status = BolStatus.Draft;
            _model.Type = Model.Type;
            _model.ManifestUID = Model.ManifestUID.Value;
            _model.RefNo = Model.RefNo;
            _model.ShipMethodUID = Model.ShipMethodUID;
            //_model.ShipViaUID = new Guid(Model.ShipviaUID);
            _model.ETA = Model.ETA;
            _model.RevETA = Model.RevETA;
            _model.DeliveryDate = Model.DeliveryDate;
            _model.Phone = Model.Phone;
            _model.RefNo = Model.RefNo;
            _model.Description = Model.Description;
            if (Model.Type == 1) //inbound
            {
                _model.ShipFromAddress = Model.ShipFromAddress;
                _model.ShipFromCity = Model.ShipFromCity;
                _model.ShipFromCountry = Model.ShipFromCountry;
                _model.ShipFromState = Model.ShipFromState;
                _model.ShipFromZip = Model.ShipFromZip;


                if (warehouse.Success)
                {

                    _model.ShipToAddress = warehouse.Content.Address;
                    _model.ShipToCity = warehouse.Content.City;
                    _model.ShipToCountry = warehouse.Content.Country;
                    _model.ShipToState = warehouse.Content.State;
                    _model.ShipToZip = warehouse.Content.Zip;
                }
            }
            else //outbound
            {
                _model.ShipToAddress = Model.ShipToAddress;
                _model.ShipToCity = Model.ShipToCity;
                _model.ShipToCountry = Model.ShipToCountry;
                _model.ShipToState = Model.ShipToState;
                _model.ShipToZip = Model.ShipToZip;

                if (warehouse.Success)
                {
                    _model.ShipFromAddress = warehouse.Content.Address;
                    _model.ShipFromCity = warehouse.Content.City;
                    _model.ShipFromCountry = warehouse.Content.Country;
                    _model.ShipFromState = warehouse.Content.State;
                    _model.ShipFromZip = warehouse.Content.Zip;
                }
            }
            var rs = _instance.AddBol(_model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }

        }
        /// <summary>
        /// 編輯 Bol
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("EditBol")]

        public IHttpActionResult EditBol(BOLRequestModel Model)
        {
            InitDIRoot();
            Model = this.AntiXSSEncode(Model);
            if (Model.ManifestUID == Guid.Empty)
            {
                var result = this.GetFailureResult(-1, "Manifest UID must have");
                return result;
            }
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var winstance = this.DIContainer.WarehouseFactory.CreateWarehouseManger();
            dynamic _model = new ExpandoObject();

            _model.UID = Model.UID;
            _model.Name = Model.Name;
            _model.Type = Model.Type;
            _model.RefNo = Model.RefNo;
            _model.ManifestUID = Model.ManifestUID.Value;
            _model.Contact = Model.Contact;
            _model.Phone = Model.Phone;
            _model.Status = BolStatus.Draft;
            _model.ShipviaUID = Model.ShipviaUID;
            _model.ETA = Model.ETA;
            _model.RevETA = Model.ETA;
            _model.DeliveryDate = Model.DeliveryDate;
            _model.Description = Model.Description;
            _model.RevETA = Model.RevETA;
            _model.ShipMethodUID = Model.ShipMethodUID;
            if (Model.Type == 1) //inbound
            {
                _model.ShipFromAddress = Model.ShipFromAddress;
                _model.ShipFromCity = Model.ShipFromCity;
                _model.ShipFromCountry = Model.ShipFromCountry;
                _model.ShipFromState = Model.ShipFromState;
                _model.ShipFromZip = Model.ShipFromZip;


            }
            else //outbound
            {
                _model.ShipToAddress = Model.ShipToAddress;
                _model.ShipToCity = Model.ShipToCity;
                _model.ShipToCountry = Model.ShipToCountry;
                _model.ShipToState = Model.ShipToState;
                _model.ShipToZip = Model.ShipToZip;
            }
            var rs = _instance.EditBol(_model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }

        }
        /// <summary>
        /// 查詢Bol 資料列表
        /// </summary>
        /// <param name="muid">Manifest UID</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetBOLList")]
        public IHttpActionResult GetBOLList(Guid muid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var _parameters = this.DIContainer.ManifestFactory.GenerateModel<IBolSearchParameters>();
            _parameters.ManifestUID = muid;
            var rs = _instance.GetBolList(_parameters);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IEnumerable<IBolModel>>(rs.Content.ToList());
                return this.Json<APIResult<IEnumerable<IBolModel>>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
        /// <summary>
        /// Submit BOL
        /// </summary>
        /// <param name="boluid">BOL UID</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SubmitBol")]
        public IHttpActionResult SubmitBol(Guid boluid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var rs = _instance.ApproveBol(boluid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IBolModel>(rs.Content);
                return this.Json<APIResult<IBolModel>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }

        }
        /// <summary>
        /// Approve BOL
        /// </summary>
        /// <param name="boluid">BOL UID</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ApproveBol")]
        public IHttpActionResult ApproveBol(Guid boluid)
        {

            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var rs = _instance.ApproveBol(boluid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IBolModel>(rs.Content);
                return this.Json<APIResult<IBolModel>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
        /// <summary>
        /// Reject BOL
        /// </summary>
        /// <param name="boluid">BOL UID</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("RejectBol")]
        public IHttpActionResult RejectBol(Guid boluid)
        {

            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var rs = _instance.RejectBol(boluid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IBolModel>(rs.Content);
                return this.Json<APIResult<IBolModel>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
        /// <summary>
        /// Check have Unassigned Ticket
        /// </summary>
        /// <param name="boluid">BOL UID</param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("CheckHaveUnassignedTicket")]
        public IHttpActionResult CheckHaveUnassignedTicket(Guid boluid)
        {

            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().BolManager;
            var rs = _instance.CheckHaveUnassignedTicket(boluid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult(rs.Content);
                return this.Json<APIResult<bool>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
    }
}
