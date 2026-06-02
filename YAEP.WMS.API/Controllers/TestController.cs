using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Identities.Constants;
using YAEP.Identities.Interfaces;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;

namespace YAEP.WMS.API.Controllers
{
    public class TestController : AbstractApiController
    {
        [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
        [Authentication]
        [HttpGet]
        [ActionName("test")]
        public IHttpActionResult Test()
        {
            InitDIRoot();
            //var ticketInfoUIDs = new Guid[] {
            //new Guid("43FF3C1F-8713-4BC8-A6B8-59FE3812CFBE"),
            //new Guid("7A1450CD-CC80-4CFD-A6CE-5EC5A0780E2A"),
            //new Guid("465C68B5-2A0D-4A1F-AC6D-7EB9D78F413A"),
            //new Guid("D8EFC84C-9F1F-438B-96A5-B39EAD9777E2"),
            //new Guid("52C9EB2B-F1C4-4823-8F3A-BE02BADEC202"),
            //new Guid("4A55D61A-7949-48E1-9456-C2A5865B6113")
            //};
            //var manager = this.DIContainer.ManifestFactory.CreateManger().TicketManager;
            ////VoidTicketParameters param = new VoidTicketParameters();
            ////param.BolUID = new Guid("CB30A072-6234-4583-BB0D-82698B5E4AAE");
            //TicketGenerateParameter param = new TicketGenerateParameter();
            //param.BolUID = new Guid("CB30A072-6234-4583-BB0D-82698B5E4AAE");
            //param.WarehouseUID = new Guid("3C336ED8-4A9D-4DF2-94AD-4EBFBC8BCA76");
            //var result = manager.GeneratreTicket(param);
            var _instance = DIContainer.ManifestFactory.CreateManger().BolManager;
            var _parameters = this.DIContainer.ManifestFactory.GenerateModel<IBolSearchParameters>();
            _parameters.RefNo = new string[] { "BOL-13159590-0" };
            var rs = _instance.GetBolList(_parameters);
            //_instance.CheckTicketStatus();
            return this.Json(rs.Content);
        }
        [Authentication]
        [HttpGet]
        [ActionName("GetItemNo")]
        public IHttpActionResult GetItemNo(string itemNO)
        {
            InitDIRoot();

            var _instance = DIContainer.ManifestFactory.CreateManger().OrderManager;
            var rs = _instance.GetItemNo(itemNO);
            //_instance.CheckTicketStatus();
            return this.Json(rs.Content);
        }
        [Authentication]
        [HttpGet]
        [ActionName("GetAllItemNo")]
        public IHttpActionResult GetAllItemNo()
        {
            InitDIRoot();

            var _instance = DIContainer.ManifestFactory.CreateManger().OrderManager;
            var rs = _instance.GetAllItem();
            //_instance.CheckTicketStatus();
            return this.Json(rs.Content);
        }
        [Authentication]
        [HttpPost]
        [ActionName("ImportHomelocation")]
        public IHttpActionResult ImportHomelocation(List<dynamic> homelocations)
        {
            InitDIRoot();
            //var gfactory = this.GetIdentityFactory();
            //var manager = gfactory.CreateGroupManager();
            //var factory = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            //factory.Importhomelocation(homelocations, manager);


            return this.Ok();
        }
        [HttpGet]
        [Authentication]
        [ActionName("TestInventorySync")]
        public IHttpActionResult TestInventorySync(int times, int interval)
        {
            InitDIRoot();
            var gfactory = this.GetIdentityFactory();
            var manager = gfactory.CreateGroupManager();
            var factory = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            factory.TestInventorySync(times, interval);

            return this.Ok();
        }
        [ActionName("TestAllocatedSync")]
        public IHttpActionResult TestAllocatedSync(int times, int interval)
        {
            InitDIRoot();
            var gfactory = this.GetIdentityFactory();
            var manager = gfactory.CreateGroupManager();
            var factory = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;


            return this.Ok();
        }
        [ActionName("TestReceivingSync")]
        public IHttpActionResult TestReceivingSync(int times, int interval)
        {
            InitDIRoot();
            var gfactory = this.GetIdentityFactory();
            var manager = gfactory.CreateGroupManager();
            var factory = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;


            return this.Ok();
        }
    }
}
