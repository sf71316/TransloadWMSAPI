using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using YAEP.Utilities;
using YAEP.WMS.Api.Models;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Model;

namespace YAEP.WMS.API.Controllers
{
    public partial class MobileController
    {
        private object file;

        /// <summary>
        /// 取得Attachment Type
        /// </summary>
        /// <returns></returns>
        [ConnectionLog]
        [HttpGet]
        [ActionName("GetAttachmentType")]
        public IHttpActionResult GetAttachmentType(int belongtotype)
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetAttachmentTypeList(belongtotype);
            var result = this.GetSuccessResult<dynamic>(rs);
            return this.Json<APIResult<dynamic>>(result);
        }
        //[Compression]
        [HttpGet]
        [ActionName("GetAttachmentList")]
        public IHttpActionResult GetAttachmentList(Guid belongtouid, int belongtotype, Guid? attachmenttypeuid)
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.GetAttachmentList(belongtouid, belongtotype, attachmenttypeuid);
            var result = this.GetSuccessResult<dynamic>(rs.Content);
            return this.Json<APIResult<dynamic>>(result);
        }
        [HttpPost]
        [ActionName("UploadAttachment")]
        public IHttpActionResult UploadAttachment()
        {
            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var queryString = HttpContext.Current.Request.QueryString;
                var param = new TicketUploadAttachmentParameters();
                Guid atuid, btu = Guid.Empty;
                if (Guid.TryParse(queryString["AttachmentType"], out atuid))
                    param.AttachmentTypeUID = atuid;
                if (Guid.TryParse(queryString["belongToUID"], out btu))
                    param.BelongToGuid = btu;
                param.BelongToType = Convert.ToInt32(queryString["belongToType"]);
                param.BelongToGuid = btu;
                param.File = HttpContext.Current.Request.Files[0];
                InitDIRoot();
                var _instance = DIContainer.ManifestFactory.CreateManger().TicketManager;
                var rs = _instance.UploadAttachment(param);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<dynamic>(rs.Content);
                    return this.Json<APIResult<dynamic>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }

            }
            else
            {
                return this.GetFailureResult(-1, "not find upload file.");
            }
        }
        [HttpGet]
        [ActionName("DownloadAttachment")]
        public HttpResponseMessage DownloadAttachment([FromUri] Guid attachmentuid)
        {
            HttpResponseMessage response = null;
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().TicketManager;
            var rs = _instance.DownloadAttachment(attachmentuid);
            if (rs.Success)
            {
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(Convert.FromBase64String(rs.Content.FileBase64)));
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = rs.Content.FileName
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(rs.Content.ContentType);
            }
            else
            {
                response = Request.CreateResponse<string>(HttpStatusCode.NotFound, rs.Message);
            }
            return response;
        }
    }
}
