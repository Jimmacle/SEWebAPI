using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.API;

namespace SEWA.Controllers
{
    public class ActionController : Controller
    {
        public ActionController(ITorchBase torch) : base(torch) { }

        /// <inheritdoc />
        public override Task PostAsync(Request request)
        {
            return base.PostAsync(request);
        }
    }
}
