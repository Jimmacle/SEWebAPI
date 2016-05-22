using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIPlugin
{
    public abstract class APIHandler
    {
        public string Signature { get; private set; }
        private string[] signatureParts;

        public APIHandler(string signature = "")
        {
            signatureParts = signature.Split('/');
        }

        public bool CanHandle(string path)
        {
            string[] pathParts = path.Split('/');

            if (pathParts.Length != signatureParts.Length)
            {
                return false;
            }

            for (var i = 0; i < signatureParts.Length; i++)
            {
                if (signatureParts[i] != "*" && pathParts[i] != signatureParts[i])
                {
                    return false;
                }
            }

            return true;
        }

        public abstract void Get(HttpListenerContext context);

        public abstract void Put(HttpListenerContext context);
    }
}
