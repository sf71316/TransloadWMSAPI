using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Identities.Constants;
using YAEP.Identities.DI;
using YAEP.Utilities;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// 工單相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Ticket")]
    public class TicketController : AbstractApiController
    {

        public TicketController()
        {

        }

        /// <summary>
        /// 取得Service Item  清單
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetServiceItemNameList")]
        public IHttpActionResult GetServiceItemNameList()
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            
            var rs = _instance.GetServiceItemNameList();
            var result = this.GetSuccessResult<List<IEnumFieldInfo>>(rs.ToList());
            return this.Json<APIResult<List<IEnumFieldInfo>>>(result);
        }
        /// <summary>
        /// 刪除Ticket
        /// </summary>
        /// <param name="uid">Manifest UID</param>
        /// <param name="wuid">Workder UID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("VoidTicket")]
        public IHttpActionResult VoidTicket(Guid uid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var _parameters = this.DIContainer.ManifestFactory.GenerateVoidTicketParameters();
            _parameters.ManifestUID = uid;
            var rs = _instance.VoidTicket(_parameters);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();

                return this.Json<APIResult<string>>(result);
            }
            else
            {

                return this.GetFailureResult(-1, rs.Message);
            }

        }
        /// <summary>
        /// 產生 Ticket 
        /// </summary>
        /// <param name="muid"></param>
        /// <param name="boluid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateTicket")]
        public IHttpActionResult GenerateTicket([FromUri]Guid? muid, [FromUri]Guid? boluid)
        {
            InitDIRoot();
            TicketGenerateParameter parameter = new TicketGenerateParameter();
            parameter.BolUID = boluid;
            parameter.ManifestUID = muid;
            if (boluid.HasValue || muid.HasValue)
            {
                var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
                var rs = _instance.GeneratreTicket(parameter);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {

                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, YAEP.WMS.Language.Resources.Resource.TICKET_GENERATETICKET_PARAMETER_NULL);
            }

        }
        /// <summary>
        /// 取得BOL Name 清單 by Manifest
        /// </summary>
        /// <param name="ManifestUID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBolNameList")]
        public IHttpActionResult GetBolNameList(Guid ManifestUID)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetBolNameList(ManifestUID);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<IComponentViewModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<IComponentViewModel>>>(result);
            }
            else
            {

                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 取得BOL Info 
        /// </summary>
        /// <param name="BolUID">Bol UID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBolInfo")]
        public IHttpActionResult GetBolInfo(Guid BolUID)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetBolInfo(BolUID);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<IBolInfoViewModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<IBolInfoViewModel>>>(result);
            }
            else
            {

                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 取得Ticket 清單
        /// </summary>
        /// <param name="BolUID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTicketIDList")]
        public IHttpActionResult GetTicketIDList(Guid BolUID)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetTicketIDList(BolUID);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<IComponentViewModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<IComponentViewModel>>>(result);
            }
            else
            {

                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 取得 Vessel RefNo 清單
        /// </summary>
        /// <param name="BolUID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetVesselRefNoList")]
        public IHttpActionResult GetVesselRefNoList(Guid BolUID)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetVesselRefNoList(BolUID);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<IComponentViewModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<IComponentViewModel>>>(result);
            }
            else
            {

                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetGroupUserList")]
        public IHttpActionResult GetGroupUserList()
        {
            var authInfo = base.GetAuthenticationInfo();

            var factory = FactoryUtils.GetIdentityFactory(authInfo);
            var manager = factory.CreateGroupManager();
            var getGroupUserViewResult = manager.GetGroupUserViewByUser(authInfo.UID);

            if (getGroupUserViewResult.Success)
            {
                if (getGroupUserViewResult.Content?.Count() > 0)
                {
                    var result = this.GetSuccessResult(getGroupUserViewResult.Content);
                    return this.Json(result);
                }
                else
                {
                    return this.GetFailureResult(-1, "Not Found.");
                }
            }

            return this.GetFailureResult(-1, getGroupUserViewResult.Message);
        }

        /// <summary>
        ///  Ticket 指派 Worker
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddWorker")]
        public IHttpActionResult AddWorker(MaintainWorkderParameters parameters)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.AddWorkder(parameters);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {

                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// Ticket 刪除 Worker
        /// </summary>
        /// <param name="tauid"></param>
        /// <param name="ticketInfoUID"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("RemoveWorker")]
        public IHttpActionResult RemoveWorker([FromUri]Guid[] tauid, Guid ticketInfoUID)
        {
            InitDIRoot();
            if (tauid != null && tauid.Length > 0)
            {
                if (ticketInfoUID != Guid.Empty)
                {
                    var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
                    var rs = _instance.RemoveWorkderAPI(tauid, ticketInfoUID);
                    if (rs.Success)
                    {
                        var result = this.GetSuccessResult();
                        return this.Json<APIResult<string>>(result);
                    }
                    else
                    {
                        return this.GetFailureResult(-1, rs.Message);
                    }
                }
                else
                {
                    return this.GetFailureResult(-1, "must have ticket info UID.");
                }
            }
            else
            {
                return this.GetFailureResult(-1, "must have ticket assigned info UID.");
            }
        }
        /// <summary>
        ///  批次加入 Worker 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BatchAssignWorker")]
        public IHttpActionResult BatchAssignWorker(MaintainWorkderParameters parameters)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.BatchAssignWorkerAPI(parameters);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {

                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetTicketAssignedList")]
        public IHttpActionResult GetTicketAssignedList(TicketAssignedListParameters parameters)
        {
            InitDIRoot();
            var authInfo = base.GetAuthenticationInfo();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var factory = FactoryUtils.GetIdentityFactory(authInfo);
            var manager = factory.CreateGroupManager();
            var rs = _instance.GetTicketAssignedList(parameters, manager);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<ITicketAssignedListViewModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<ITicketAssignedListViewModel>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketinfouid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetTicketGroupAssignedList")]
        public IHttpActionResult GetTicketGroupAssignedList(Guid ticketinfouid)
        {
            InitDIRoot();
            var authInfo = base.GetAuthenticationInfo();
            var factory = FactoryUtils.GetIdentityFactory(authInfo);
            var manager = factory.CreateGroupManager();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetTicketGroupAssignedList(ticketinfouid, manager);

            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<ITicketGroupAssignedModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<ITicketGroupAssignedModel>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        ///  get group name list
        /// </summary>
        /// <param name="warehousegroupUID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAssignedTicketGroupNameList")]
        public IHttpActionResult GetAssignedTicketGroupNameList(Guid warehousegroupUID)
        {
            var authInfo = base.GetAuthenticationInfo();
            GroupTypes[] groupType = new GroupTypes[] {
                GroupTypes.CorporateStore,
                GroupTypes.Department,
                GroupTypes.Group,
                GroupTypes.Team };
            var factory = this.GetIdentityFactory();
            var manager = factory.CreateGroupManager();

            try
            {
                var resultToGetAllSubGroups = manager.GetAllSubGroups(warehousegroupUID);

                if (resultToGetAllSubGroups.Success && resultToGetAllSubGroups.Content?.Count() > 0)
                {
                    if (groupType?.Count() > 0)
                    {
                        var matchTypeCollection = resultToGetAllSubGroups.Content.Where(o => groupType.Any(t => (int)t == o.Type)).ToArray();

                        if (matchTypeCollection.Count() > 0)
                        {
                            var collection = matchTypeCollection.Select(o => new { o.UID, o.ID, o.Name });
                            var apiResult = base.GetSuccessResult(collection);
                            return base.Json(apiResult);
                        }
                    }
                    else
                    {
                        var collection = resultToGetAllSubGroups.Content.Select(o => new { o.UID, o.ID, o.Name });
                        var apiResult = base.GetSuccessResult(resultToGetAllSubGroups.Content);
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetTicketTypeList")]
        public IHttpActionResult GetTicketTypeList()
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetTicketTypeList();
            var result = this.GetSuccessResult<List<IEnumFieldInfo>>(rs.ToList());
            return this.Json<APIResult<List<IEnumFieldInfo>>>(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetTicketStatusList")]
        public IHttpActionResult GetTicketStatusList()
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetTicketStatusList();
            var result = this.GetSuccessResult<List<IEnumFieldInfo>>(rs.ToList());
            return this.Json<APIResult<List<IEnumFieldInfo>>>(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetSearchTicketList")]
        public IHttpActionResult GetSearchTicketList(TicketSearchListParameters parameters)
        {
            InitDIRoot();
            // 收集正確的Item UID
            //parameters.PHierarchy = this.getArrayOfItemUID(parameters.PHierarchy).ToArray();

            var authInfo = base.GetAuthenticationInfo();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            var factory = FactoryUtils.GetIdentityFactory(authInfo);
            var manager = factory.CreateGroupManager();
            var rs = _instance.GetTicketSearchList(parameters, manager);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<ITicketSearchListViewModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<ITicketSearchListViewModel>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }

        private IEnumerable<Guid> getArrayOfItemUID(IEnumerable<Guid> itemUID)
        {
            var groupUIDs = this.GroupManager.GetGroupKeysByUser(base.GetAuthenticationInfo().UID).Content;
            var result = DrKnowAll.GetProduct().Where(o =>
            {
                if (itemUID.Any(u => u == o.UID))
                {
                    if (groupUIDs != null)
                    {
                        return groupUIDs.Any(g => g == o.GroupUID);
                    }
                }

                return false;
            }).Select(o => o.UID);

            return result;
        }

    }
}
