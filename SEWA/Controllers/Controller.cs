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
        public ITorchBase Torch { get; set; }
        public ValueBag ValueBag { get; set; }
        public Request Request { get; set; }

        protected static readonly Logger Log = LogManager.GetLogger("SEWA");

        public async Task HandleRequestAsync()
        {
            try
            {
                switch (Request.Method.Method)
                {
                    case "POST":
                        await PostAsync();
                        break;
                    case "PUT":
                        await PutAsync();
                        break;
                    case "GET":
                        await GetAsync();
                        break;
                    case "DELETE":
                        await DeleteAsync();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error handling request.");
                Log.Error(e);
                await Request.RespondAsync("Internal error.", HttpStatusCode.InternalServerError);
            }
        }

        public virtual async Task GetAsync()
        {
            await Request.RespondAsync("", HttpStatusCode.NotImplemented);
        }

        public virtual async Task PutAsync()
        {
            await Request.RespondAsync("", HttpStatusCode.NotImplemented);
        }

        public virtual async Task PostAsync()
        {
            await Request.RespondAsync("", HttpStatusCode.NotImplemented);
        }

        public virtual async Task DeleteAsync()
        {
            await Request.RespondAsync("", HttpStatusCode.NotImplemented);
        }
    }
}
