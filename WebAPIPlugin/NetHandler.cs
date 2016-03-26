using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIPlugin
{
    public class NetHandler
    {
        HttpListener listener;

        public NetHandler()
        {
            listener = new HttpListener();
            listener.Start();
            listener.Prefixes.Add("http://+:80/api/");
        }

        public void HandleRequest()
        {
            var context = listener.GetContext();
            var response = context.Response;

            string responseString = "<html><body>" + context.Request.Url.ToString() + "</body></html>";
            byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);

            response.ContentLength64 = responseBytes.Length;

            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            response.OutputStream.Close();
        }
    }
}
