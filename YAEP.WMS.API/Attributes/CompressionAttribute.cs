using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace YAEP.WMS.API
{
    /// <summary>
    /// 實作資料壓縮
    /// </summary>
    public class CompressionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var content = actionExecutedContext.Response.Content;
            var bytes = content?.ReadAsByteArrayAsync().Result;
            if (bytes != null && bytes.Length > 0)
            {
                var acceptEncoding = actionExecutedContext.Request.Headers.AcceptEncoding.Where(x => x.Value == "gzip" || x.Value == "deflate").ToList();
                byte[] zlibbedContent;
                if (acceptEncoding.FirstOrDefault()?.Value == "deflate")
                {
                    zlibbedContent = DeflateByte(bytes);
                    actionExecutedContext.Response.Content = new ByteArrayContent(zlibbedContent);
                    actionExecutedContext.Response.Content.Headers.Add("Content-Encoding", "deflate");
                    actionExecutedContext.Response.Content.Headers.Add("ContentZip", "deflate");
                }
                else
                {
                    zlibbedContent = GZipByte(bytes);
                    actionExecutedContext.Response.Content = new ByteArrayContent(zlibbedContent);
                    actionExecutedContext.Response.Content.Headers.Add("Content-Encoding", "gzip");
                    actionExecutedContext.Response.Content.Headers.Add("ContentZip", "gzip");
                }
            }
            actionExecutedContext.Response.Content.Headers.Add("Content-Type", "application/json");

            base.OnActionExecuted(actionExecutedContext);
        }
        #region DotNetZip
        private static byte[] DeflateByte(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var compressor = new DeflateStream(output, CompressionMode.Compress))
                {
                    compressor.Write(data, 0, data.Length);
                    compressor.Close();
                }

                return output.ToArray();
            }
        }

        private byte[] GZipByte(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var compressor = new GZipStream(output, CompressionMode.Compress))
                {
                    compressor.Write(data, 0, data.Length);
                    compressor.Close();
                }

                return output.ToArray();
            }
        }
        #endregion
    }

}