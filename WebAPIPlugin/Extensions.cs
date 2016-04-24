using Newtonsoft.Json;
using Sandbox;
using System.Net;
using System.Text;

namespace WebAPIPlugin
{
    public static class Extensions
    {
        public static void Respond(this HttpListenerContext context, object data, int statusCode)
        {
            try
            {
                var response = context.Response;
                response.StatusCode = statusCode;

                string serialized = JsonConvert.SerializeObject(data, Formatting.Indented);
                byte[] bytes = Encoding.ASCII.GetBytes(serialized);
                response.ContentLength64 = bytes.Length;

                response.OutputStream.Write(bytes, 0, bytes.Length);
                response.OutputStream.Close();
            }
            catch
            {
                MySandboxGame.Log.WriteLineAndConsole("SEWA: Error writing stream");
            }
        }
    }
}
