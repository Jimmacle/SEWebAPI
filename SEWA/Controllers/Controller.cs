using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Torch.API;

namespace SEWA.Controllers
{
    public abstract class Controller
    {
        protected ITorchBase Torch { get; }
        private static readonly Logger Log = LogManager.GetLogger("SEWA");
        public ValueBag ValueBag;

        protected Controller(ITorchBase torchBase)
        {
            Torch = torchBase;
        }

        public async Task HandleRequestAsync(Request request)
        {
            try
            {
                switch (request.Method.Method)
                {
                    case "POST":
                        await PostAsync(request);
                        break;
                    case "PUT":
                        await PutAsync(request);
                        break;
                    case "GET":
                        await GetAsync(request);
                        break;
                    case "DELETE":
                        await DeleteAsync(request);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error handling request.");
                Log.Error(e);
                await request.RespondAsync("Internal error.", HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task GetAsync(Request request)
        {
            await request.RespondAsync("", HttpStatusCode.NotImplemented);
        }

        public virtual async Task PutAsync(Request request)
        {
            await request.RespondAsync("", HttpStatusCode.NotImplemented);
        }

        public virtual async Task PostAsync(Request request)
        {
            await request.RespondAsync("", HttpStatusCode.NotImplemented);
        }

        public virtual async Task DeleteAsync(Request request)
        {
            await request.RespondAsync("", HttpStatusCode.NotImplemented);
        }
    }
}
