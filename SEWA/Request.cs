using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Sandbox;

namespace SEWA
{
    public class Request
    {
        private readonly HttpListenerContext _context;
        private static Logger _log = LogManager.GetLogger("Request");

        public string[] PathSegments { get; }
        public NameValueCollection QueryString { get; }
        public HttpMethod Method { get; }
        public string ApiKey { get; }
        public string Content { get; }

        public Request(HttpListenerContext context)
        {
            _context = context;
            PathSegments = context.Request.Url.Segments;
            QueryString = context.Request.QueryString;
            Method = new HttpMethod(context.Request.HttpMethod);
            ApiKey = context.Request.Headers.Get("ApiKey") ?? "";
            Content = context.Request.InputStream.ReadString();
        }

        public async Task RespondAsync(object responseObject, HttpStatusCode statusCode)
        {
            var sw = Stopwatch.StartNew();
            var response = _context.Response;
            if (!response.OutputStream.CanWrite)
                return;

            await Task.Run(() =>
            {
                var jsonObj = JsonConvert.SerializeObject(responseObject, Formatting.Indented, new JsonSerializerSettings{ContractResolver = new SkipEmptyContractResolver()});
                var jsonBytes = Encoding.Unicode.GetBytes(jsonObj);
                response.ContentType = "application/json";
                response.StatusCode = (int)statusCode;
                response.ContentLength64 = jsonBytes.Length;
                response.OutputStream.Write(jsonBytes, 0, jsonBytes.Length);
                response.OutputStream.Close();
            });
            sw.Stop();
            _log.Debug($"Serialization/response took {sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}
